//////licence
//GNU Lesser General Public License (LGPL)
//english: http://www.gnu.org/licenses/lgpl.html
//german: http://www.gnu.de/lgpl-ger.html

#region usings
using System;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;
using VVVV.Utils.SlimDX;
using VVVV.Core.Logging;

using SlimDX;
using SlimDX.Direct3D9;

using FantastiqUINet;
using System.Diagnostics;
#endregion usings

namespace VVVV.Nodes
{
    [PluginInfo (Name = "Flash",
                 Category = "EX9",
                 Author = "chrismo",
                 Help = "Renders the Surface of a SWF File to a Direct3D Texture")]                 
    public class DrawFlash : IPluginEvaluate, IDisposable, IPluginDXLayer
    {
    	[DllImport("user32.dll", CharSet=CharSet.Auto)]
		internal static extern int MapVirtualKey(int uCode, int nMapType);
    	
    	//note: the binary version of this plugin shipping with vvvvs addonpack 
    	//is licensed by meso.net
    	//to build your own non-trial version enter your license key here:	
        const string LICENSENAME = "";
        const string LICENSENUMBER = "";
        
        #region pins & fields
        [Input ("Filename", IsSingle = true, StringType = StringType.Filename, FileMask = "Shockwave Flash (*.swf)|*.swf")]
        ISpread<string> FSWFPath;
        
        [Input ("Load", IsSingle = true)]
        IDiffSpread<bool> FLoadSWF;
            
        [Input ("Mouse X", IsSingle = true)]
        IDiffSpread<double> FMouseX;
        
        [Input ("Mouse Y", IsSingle = true)]
        IDiffSpread<double> FMouseY;
        
        [Input ("Mouse Left Button", IsSingle = true)]
        IDiffSpread<bool> FMouseLeftButton;
        
        [Input ("Key Code")]
        IDiffSpread<int> FKeyCodeIn;
        
        enum BufferMode {Single, Double};
        [Input ("Buffer Mode", IsSingle = true)]//, DefaultEnum = BufferMode.Single)]
        IDiffSpread<BufferMode> FBufferMode;
        
        enum Quality {Low, Medium, High, Best};
        [Input ("Quality", IsSingle = true)]//, DefaultEnum = BufferMode.Best)]
        IDiffSpread<Quality> FQuality;
        
        [Input ("Seek Frame", IsSingle = true, MinValue = 0)]
        IDiffSpread<int> FGoToFrame;

        [Input ("Enabled", IsSingle = true, DefaultValue = 1)]
        ISpread<bool> FEnabledInput;

        [Output ("Frame Rate", IsSingle = true)]
        ISpread<int> FFrameRateOutput;
        
        [Import]
    	private ILogger FLogger;
        
        //Track whether Dispose has been called.
        private bool FDisposed = false;
        private bool _DisposeAllowed = true;
        
        private IDXRenderStateIn FRenderStatePin;
        //private IDXSamplerStateIn FSamplerStatePin;
        private ITransformIn FTransformIn;
        private IDXLayerIO FLayerOutput;

        private List<int> FLastKeyState;        

        private Dictionary<Device, Sprite> FSprites = new Dictionary<Device, Sprite>();
        bool _NeedsUpdate;
        private int FSpreadCount;
        
        private FNUIMain _FNUIMain;
        private FNUIFlashPlayer _FNUIFlashPlayer;

        private Texture _Texture1, _Texture2;
        private int _Width = 0, _Height = 0;

        private float _FrameRate;
        private int _Frames;

        private int _BufferMode = 0;
        #endregion field declaration

        #region constructor/destructor
        [ImportingConstructor]
        public DrawFlash(IPluginHost host)
        {
            host.CreateRenderStateInput(TSliceMode.Single, TPinVisibility.True, out FRenderStatePin);
            FRenderStatePin.Order = -2;
            host.CreateTransformInput("Transform", TSliceMode.Single, TPinVisibility.True, out FTransformIn);
            FTransformIn.Order = -1;
            host.CreateLayerOutput("Layer", TPinVisibility.True, out FLayerOutput);
            FLayerOutput.Order = -1;

            FLastKeyState = new List<int>();
            
            _FNUIMain = new FNUIMain();
            _FNUIMain.SetLicenseKey(0, LICENSENAME, LICENSENUMBER);
            _FNUIMain.CreateUI("");
        }

        // Implementing IDisposable's Dispose method.
        // Do not make this method virtual.
        // A derived class should not be able to override this method.
        public void Dispose()
        {
            Dispose(true);
            // Take yourself off the Finalization queue
            // to prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        // Dispose(bool disposing) executes in two distinct scenarios.
        // If disposing equals true, the method has been called directly
        // or indirectly by a user's code. Managed and unmanaged resources
        // can be disposed.
        // If disposing equals false, the method has been called by the
        // runtime from inside the finalizer and you should not reference
        // other objects. Only unmanaged resources can be disposed.
        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!FDisposed)
            {
                if (disposing)
                {
                    //Debug.WriteLine("Dispose");

                    if (_FNUIFlashPlayer != null)
                    {
                        //Debug.WriteLine("Remove Player");

                        int tTimer = 0;

                        while (_DisposeAllowed == false && tTimer < Int32.MaxValue)
                            tTimer++;

                        //Debug.WriteLine("Timer :: " + tTimer);


                        //Debug.WriteLine("Remove Player :: DisableFlashRendering");
                        _FNUIFlashPlayer.DisableFlashRendering(true);

                        _FNUIFlashPlayer.SetDelegates(null, null, null, null);
                        _FNUIFlashPlayer.SetEventNotifier(null);

                        //Debug.WriteLine("Remove Player :: DeleteFlashPlayer");
                        _FNUIMain.DeleteFlashPlayer(_FNUIFlashPlayer);
                    }

                    _FNUIMain.Dispose();

                    _FNUIFlashPlayer = null;
                    _FNUIMain = null;

                    GC.Collect();
                    GC.SuppressFinalize(this);
                }
                // Release unmanaged resources. If disposing is false,
                // only the following code is executed.

                //FHost.Log(TLogType.Debug, "PluginMeshTemplate is being deleted");

                // Note that this is not thread safe.
                // Another thread could start disposing the object
                // after the managed resources are disposed,
                // but before the disposed flag is set to true.
                // If thread safety is necessary, it must be
                // implemented by the client.
            }
            FDisposed = true;
        }

        // Use C# destructor syntax for finalization code.
        // This destructor will run only if the Dispose method
        // does not get called.
        // It gives your base class the opportunity to finalize.
        // Do not provide destructors in types derived from this class.
        ~DrawFlash()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }

        #endregion constructor/destructor
   
        #region mainloop
        public void Evaluate(int pSpreadMax)
        {
            if (_FNUIFlashPlayer == null)
                return;

            try
            {
                FSpreadCount = pSpreadMax;

                if (FGoToFrame.IsChanged)
                    _FNUIFlashPlayer.GotoFrame(FGoToFrame[0]);

                if (FQuality.IsChanged)
                    _FNUIFlashPlayer.SetQualityString(FQuality[0].ToString());

                if (FEnabledInput[0])
                {
                    Matrix4x4 world;

                    if (FMouseX.IsChanged || FMouseY.IsChanged)
                    {
                        FTransformIn.GetRenderWorldMatrix(0, out world);

						var mouse = new Vector3D(FMouseX[0], FMouseY[0],0);
						//  getting the transformed stage
						mouse = !world*mouse;
						
						// scale to swf coordinates
						mouse.x = (mouse.x + 0.5) * _Width / 1.0;
						mouse.y = (-1 * mouse.y + 0.5) * _Height / 1.0;
						
						_FNUIFlashPlayer.UpdateMousePosition((int)mouse.x, (int)mouse.y);
                    }

                    if (FMouseLeftButton.IsChanged)
                    {
                    	if (FMouseLeftButton[0])
                    		_FNUIFlashPlayer.UpdateMouseButton(0, true);
                    	else
                    		_FNUIFlashPlayer.UpdateMouseButton(0, false);
                    }
                    
                    if(FKeyCodeIn.IsChanged)
                    {
                    	List<int> currentKeyState = new List<int>();
                    	int count = FKeyCodeIn.SliceCount;
                    	
                    	for(int i = 0; i<count; i++)
                    	{
                    	    int v = FKeyCodeIn[i];
                    		
                    		if (v > 0) 
                    		    currentKeyState.Add(v);
                    	}
                    	
                    	count = FLastKeyState.Count;
                    	for (int i=0; i<count; i++)
                    	{
                    		if(currentKeyState.IndexOf(FLastKeyState[i]) < 0)
                    		{
                    			_FNUIFlashPlayer.SendKey(false, FLastKeyState[i], 0);
                    		}
                    	}
                    	
                    	count = currentKeyState.Count;
                    	bool isShift = currentKeyState.IndexOf(16) >= 0;
                    	
                    	for (int i=0; i<count; i++)
                    	{
                    		if(FLastKeyState.IndexOf(currentKeyState[i]) < 0)
                    		{
                    			int key = currentKeyState[i];
                    			
                    			_FNUIFlashPlayer.SendKey(true, key, 0);
                    
                    			//char
                    			if(!isShift && (key >= 65 && key <= 90))
                    			{
                    				_FNUIFlashPlayer.SendChar(MapVirtualKey(key, 2)+32, 0);
                    			}
                    			
                    			//Ä
                    			else if (key == 222)
                    			{
                    				if(isShift)
                    				{
                    					_FNUIFlashPlayer.SendChar(196, 0);
                    				}else{
                    					_FNUIFlashPlayer.SendChar(228, 0);
                    				}
                    			}
                    			
                    			//Ö
                    			else if (key == 192)
                    			{
                    				if(isShift)
                    				{
                    					_FNUIFlashPlayer.SendChar(214, 0);
                    				}else{
                    					_FNUIFlashPlayer.SendChar(246, 0);
                    				}
                    			}
                    			
                    			//Ü
                    			else if (key == 186)
                    			{
                    				if(isShift)
                    				{
                    					_FNUIFlashPlayer.SendChar(220, 0);
                    				}else{
                    					_FNUIFlashPlayer.SendChar(252, 0);
                    				}
                    			}
                    			
                    			//ß
                    			else if (key == 219)
                    			{
                    				_FNUIFlashPlayer.SendChar(223, 0);
                    			}
                    			
                    			else
                    			{
                    				_FNUIFlashPlayer.SendChar(MapVirtualKey(key, 2), 0);
                    			}
                    			
                    		}
                    	}
                    	
                    	FLastKeyState = currentKeyState;
                    }
                }
            }
            catch
            {
            }
        }
        #endregion mainloop

        #region DXLayer
        private void LoadSWF(Device tDevice)
        {
            if (_FNUIFlashPlayer != null)
            {
                int tTimer = 0;

                while (_DisposeAllowed == false && tTimer < Int32.MaxValue)
                    tTimer++;

                //Debug.WriteLine("Remove Player :: DisableFlashRendering");
                _FNUIFlashPlayer.DisableFlashRendering(true);

                _FNUIFlashPlayer.SetDelegates(null, null, null, null);
                _FNUIFlashPlayer.SetEventNotifier(null);

                //Debug.WriteLine("Remove Player :: DeleteFlashPlayer");
                _FNUIMain.DeleteFlashPlayer(_FNUIFlashPlayer);
            }

            //Debug.WriteLine("Create Player");

            //Debug.WriteLine("Create Player :: CreateFlashPlayer");
            _FNUIFlashPlayer = _FNUIMain.CreateFlashPlayer();

            //Debug.WriteLine("Create Player :: SetDelegates");
            _FNUIFlashPlayer.SetDelegates(ResizeTexture, LockRectangle, UnlockRectangle, AddDirtyRectangle);
            //Debug.WriteLine("Create Player :: SetEventNotifier");
            _FNUIFlashPlayer.SetEventNotifier(EventNotifier);

            string tPath = FSWFPath[0];
            if (System.IO.File.Exists(tPath) == false)
            {
                //System.Windows.Forms.MessageBox.Show("Invalid swf path: " + tPath);
                return;
            }

            //Debug.WriteLine("Create Player :: LoadFlashHeader");
            _FNUIMain.LoadFlashHeader(tPath, ref _Width, ref _Height, ref _FrameRate, ref _Frames);

            //FHost.Log(TLogType.Debug, "swf Width: " + _Width);
            //FHost.Log(TLogType.Debug, "swf Height: " + _Height);
            //FHost.Log(TLogType.Debug, "swf Framerate: " + _FrameRate);
            //FHost.Log(TLogType.Debug, "swf Frames: " + _Frames);

            FFrameRateOutput[0] = (int)_FrameRate;
            
            var pool = Pool.Managed;
            var usage = Usage.None;
			if (tDevice is DeviceEx)
			{
				pool = Pool.Default;
				usage = Usage.Dynamic;
			}

            _Texture1 = new Texture(tDevice, _Width, _Height, 1, usage, Format.A8R8G8B8, pool);
            _Texture2 = new Texture(tDevice, _Width, _Height, 1, usage, Format.A8R8G8B8, pool);

            try
            {
                //Debug.WriteLine("Create Player :: CreateFlashControl");
                _FNUIFlashPlayer.CreateFlashControl(2, _Width, _Height, (IntPtr)0, (IntPtr)_BufferMode, false, false);

                _FNUIFlashPlayer.DisableFlashRendering(true);

                //Debug.WriteLine("Create Player :: LoadMovie");
                _FNUIFlashPlayer.LoadMovie(tPath);
            }
            catch (Exception ex)
            {
                FLogger.Log(LogType.Debug, "Exception in FantastiqUI: " + ex.Message);

                if (ex.InnerException != null)
                    FLogger.Log(LogType.Debug, "inner: " + ex.InnerException.Message);
            }
        }

        private void RemoveResource(Device OnDevice)
        {
            //Debug.WriteLine("RemoveResource");

            if (FSprites.ContainsKey(OnDevice) == false)
                return;

            Sprite tSprite = FSprites[OnDevice];
            FSprites.Remove(OnDevice);

            tSprite.Dispose();

            _NeedsUpdate = true;
        }

        public void UpdateResource(IPluginOut ForPin, Device OnDevice)
        {
            try
            {
                if (FLoadSWF.IsChanged)
                {
                    if (FLoadSWF[0])
                    {
                        //Debug.WriteLine("FLoadSWF.PinIsChanged");
                        _NeedsUpdate = true;
                    }
                }

                if (FBufferMode.IsChanged)
                {
                    //Debug.WriteLine("FBufferMode.PinIsChanged");

                    switch (FBufferMode[0])
                    {
                        case BufferMode.Single:
                            _BufferMode = 0;
                            break;

                        case BufferMode.Double:
                            _BufferMode = 1;
                            break;
                    }

                    _NeedsUpdate = true;
                }
            }
            catch (Exception ex)
            {
                FLogger.Log(LogType.Debug, "Exception in UpdateResource: " + ex.Message);

                if (ex.InnerException != null)
                    FLogger.Log(LogType.Debug, "inner: " + ex.InnerException.Message);

                //if resource is not yet created on given Device, create it now
                _NeedsUpdate = true;
            }

            if (_NeedsUpdate)
            {
                RemoveResource(OnDevice);

                Sprite tSprite = new Sprite(OnDevice);

                FSprites.Add(OnDevice, tSprite);

                LoadSWF(OnDevice);

                _FNUIFlashPlayer.SetQualityString(FQuality[0].ToString());

                _NeedsUpdate = false;
            }
        }

        public void DestroyResource(IPluginOut ForPin, Device OnDevice, bool OnlyUnManaged)
        {
            //Debug.WriteLine("DestroyResource");
            //Called by the PluginHost whenever a resource for a specific pin needs to be destroyed on a specific device. 
            //This is also called when the plugin is destroyed, so don't dispose dxresources in the plugins destructor/Dispose()

            RemoveResource(OnDevice);
        }

		public void SetStates()
		{
			FRenderStatePin.SetRenderState(RenderState.AlphaTestEnable, 1);
			FRenderStatePin.SetRenderState(RenderState.SourceBlend, (int) Blend.SourceAlpha);
			FRenderStatePin.SetRenderState(RenderState.DestinationBlend, (int) Blend.InverseSourceAlpha);
		}

        public void Render(IDXLayerIO ForPin, Device tDevice)
        {
            //Called by the PluginHost everytime the plugin is supposed to render itself.
            //This is called from the PluginHost from within DirectX BeginScene/EndScene,
            //therefore the plugin shouldn't be doing much here other than some drawing calls.

            if (_FNUIFlashPlayer == null || FSprites.Count < 1)
                return;

            if (FEnabledInput[0])
                _FNUIFlashPlayer.DisableFlashRendering(false);            
            else
            {
                _FNUIFlashPlayer.DisableFlashRendering(true);
                return;
            }
                
            try
            {
                tDevice.SetTransform(TransformState.World, Matrix.Identity);
                Sprite tSprite = FSprites[tDevice];
                FTransformIn.SetRenderSpace();

                FRenderStatePin.SetSliceStates(0);
                tSprite.Begin(SpriteFlags.DoNotAddRefTexture | SpriteFlags.ObjectSpace);

                Matrix4x4 tTransformMatrix;

                for (int i = 0; i < FSpreadCount; i++)
                {
                    FTransformIn.GetRenderWorldMatrix(i, out tTransformMatrix);
                    tSprite.Transform = (VMath.Scale(1.0 / _Width, -1.0 / _Height, 1) * tTransformMatrix).ToSlimDXMatrix();

                    int t = _FNUIFlashPlayer.GetTexture().ToInt32();

                    if (t == 0)
                    {
                        if (_Texture1 != null)
                            tSprite.Draw(_Texture1, new Rectangle(0, 0, _Width, _Height), new Vector3(_Width / 2, _Height / 2, -0.001f), new Vector3(0, 0, 0), new Color4(Color.White.ToArgb()));
                    }
                    else
                    {
                        if (_Texture2 != null)
                            tSprite.Draw(_Texture2, new Rectangle(0, 0, _Width, _Height), new Vector3(_Width / 2, _Height / 2, -0.001f), new Vector3(0, 0, 0), new Color4(Color.White.ToArgb()));
                    }

                    _FNUIFlashPlayer.ReleaseTexture();
                }
                tSprite.End();
            }
            catch
            {
            }
        }

        #endregion

        #region FantastiqUI
        /// <summary>
        /// In case you resize the flash player, this function is called to tell you to
        /// actually resize the textures used, as return value you can provide a new
        /// texture pointer that will be used from that moment
        /// </summary>
        public IntPtr ResizeTexture(IntPtr pTexture, Int32 pSizeX, Int32 pSizeY, Int32 pReserved)
        {
            return (IntPtr)0;
        }

        /// <summary>
        /// Requests from you a pointer to a surface to which flash has to be written
        /// for texture pTexture, so here we lock the texture and return the surface pointer
        /// </summary>
        public IntPtr LockRectangle(IntPtr pTexture)
        {
            _DisposeAllowed = false;

            DataRectangle tStream;

            //lock the texture, and return the surface pointer
            if ((Int32)pTexture == 0)
                tStream = _Texture1.LockRectangle(0, LockFlags.NoDirtyUpdate);
            else
                tStream = _Texture2.LockRectangle(0, LockFlags.NoDirtyUpdate);

            return tStream.Data.DataPointer;
        }

        /// <summary>
        /// DirtyRect is a callback that passes us regions of an updated part
        /// of the texture, we pass this to the dirty texture class as
        /// dirty rectangles so directx updates them
        /// </summary>
        public void AddDirtyRectangle(IntPtr pTexture, Int32 pX, Int32 pY, Int32 pX1, Int32 pY1)
        {
            System.Drawing.Rectangle tRectangle = new System.Drawing.Rectangle(pX, pY, pX1 - pX, pY1 - pY);

            //set the rectangle to dirty
            if ((Int32)pTexture == 0)
                _Texture1.AddDirtyRectangle(tRectangle);
            else
                _Texture2.AddDirtyRectangle(tRectangle);

        }

        /// <summary>
        /// Fantastiqui calls this function when texture editing is complete, and the
        /// texture can be unlocked again
        /// </summary>
        public Int32 UnlockRectangle(IntPtr pTexture, IntPtr pPointer)
        {
            //unlock the texture and return
            if ((Int32)pTexture == 0)
                _Texture1.UnlockRectangle(0);
            else
                _Texture2.UnlockRectangle(0);

            _DisposeAllowed = true;

            return 0;
        }




        //WARNING , NOT CALLED FROM MAIN THREAD!!! EG, MULTITHREADING CAREFULNESS NEEDED
        //FANTASTIQUI event functions should be safe for this.
        public void EventNotifier()
        {
            return;

            //get the number of events, this call also locks the
            //event list, clearevents() always needs to be called close
            //after to unlock it
            Int32 inum = _FNUIFlashPlayer.GetNumEvents();
            for (Int32 i = 0; i < inum; i++)
            {
                //grab the first event
                FNUIFlashEvent ev = _FNUIFlashPlayer.GetEvent(i);
                //make a string of the function name called and the value of the
                //first argument
                String blaat = ev.GetFunctionName() + " " + ev.GetValueString(0);
                Debug.WriteLine(blaat);
                _FNUIFlashPlayer.DeleteEvent(ev);
            }
            //unlock the event list
            _FNUIFlashPlayer.ClearEvents();
        }

        #endregion
    }
}
