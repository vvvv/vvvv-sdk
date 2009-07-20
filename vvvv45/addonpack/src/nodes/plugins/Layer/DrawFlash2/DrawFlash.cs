#region licence/info

//////project name
//vvvv draw flash

//////description
//basic vvvv node plugin template.
//Copy this an rename it, to write your own plugin node.

//////licence
//GNU Lesser General Public License (LGPL)
//english: http://www.gnu.org/licenses/lgpl.html
//german: http://www.gnu.de/lgpl-ger.html

//////language/ide
//C# sharpdevelop

//////dependencies
//VVVV.PluginInterfaces.V1;
//VVVV.Utils.VColor;
//VVVV.Utils.VMath;

//////initial author
//vvvv group

#endregion licence/info

//use what you need
using System;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Collections.Generic;
using System.Collections;

using VVVV.PluginInterfaces.V1;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;
using VVVV.Shared.VSlimDX;

using SlimDX;
using SlimDX.Direct3D9;

using FantastiqUINet;
using System.Diagnostics;

//the vvvv node namespace
namespace VVVV.Nodes
{





    //class definition
    public class DrawFlash : IPlugin, IDisposable, IPluginDXLayer
    {

        public class HiPerfTimer
        {
            [DllImport("Kernel32.dll")]
            private static extern bool QueryPerformanceCounter(out long lpPerformanceCount);

            [DllImport("Kernel32.dll")]
            private static extern bool QueryPerformanceFrequency(out long lpFrequency);

            private long startTime, stopTime, freq;

            public double Duration { get { return (double)(stopTime - startTime) / (double)freq; } }

            // Constructor
            public HiPerfTimer()
            {
                startTime = 0;
                stopTime = 0;

                if (QueryPerformanceFrequency(out freq) == false)
                    Debug.WriteLine("HiPerfTimer::NotSupported");
            }

            // Start the timer
            public long Start()
            {
                QueryPerformanceCounter(out startTime);

                return startTime;
            }

            // Stop the timer
            public long Stop()
            {
                QueryPerformanceCounter(out stopTime);

                return stopTime;
            }

            // ms
            public double CurrentTime()
            {
                Stop();

                return Duration * 1000.0;
            }
        }



        private FNUIMain _FNUIMain;
        private FNUIFlashPlayer _FNUIFlashPlayer;


        private Texture _Texture1, _Texture2;
        private int _Width = 0, _Height = 0;


        private float _FrameRate;
        private int _Frames;

        private int _BufferMode = 0;



        #region field declaration

        //the host (mandatory)
        public IPluginHost FHost;
        //Track whether Dispose has been called.
        private bool FDisposed = false;

        private int FSpreadCount;



        private HiPerfTimer _Timer = new HiPerfTimer();
        private System.IO.StreamWriter swVVVV = new System.IO.StreamWriter(@"C:\logVVVV.txt");
        private System.IO.StreamWriter swFant = new System.IO.StreamWriter(@"C:\logFant.txt");
        private double _MarkVVVV, _CurrentVVVV, _MarkFant, _CurrentFant;

        private ITransformIn FTranformIn;
        private IStringIn FSWFPath;
        private IValueIn FLoadSWF;
        private IValueIn FMouseX;
        private IValueIn FMouseY;
        private IValueIn FMouseLeftButton;
        private IEnumIn FBufferMode;
        private IValueIn FEnabledInput;
        private IValueIn FResetSiemensSWF;


        //a layer output pin
        private IDXLayerIO FLayerOutput;
        private IValueOut FFrameRateOutput;


        private Dictionary<int, Sprite> FSprites = new Dictionary<int, Sprite>();
        bool _NeedsUpdate;

        #endregion field declaration


        #region constructor/destructor

        public DrawFlash()
        {
            //the nodes constructor
            PresentParameters tPresentParas = new PresentParameters();
            tPresentParas.SwapEffect = SwapEffect.Discard;
            tPresentParas.Windowed = true;

            _FNUIMain = new FNUIMain();
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
                    // Dispose managed resources.

                    _FNUIMain.DeleteFlashPlayer(_FNUIFlashPlayer);
                    _FNUIMain.Dispose();

                    _FNUIFlashPlayer = null;
                    _FNUIMain = null;

                    GC.Collect();
                    GC.SuppressFinalize(this);

                    swVVVV.Flush();
                    swVVVV.Close();

                    swFant.Flush();
                    swFant.Close();
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


        #region node name and info

        //provide node infos
        private static IPluginInfo FPluginInfo;
        public static IPluginInfo PluginInfo
        {
            get
            {
                if (FPluginInfo == null)
                {
                    //fill out nodes info
                    //see: http://www.vvvv.org/tiki-index.php?page=Conventions.NodeAndPinNaming
                    FPluginInfo = new PluginInfo();

                    //the nodes main name: use CamelCaps and no spaces
                    FPluginInfo.Name = "Flash";
                    //the nodes category: try to use an existing one
                    FPluginInfo.Category = "EX9";
                    //the nodes version: optional. leave blank if not
                    //needed to distinguish two nodes of the same name and category
                    FPluginInfo.Version = "Layer";

                    //the nodes author: your sign
                    FPluginInfo.Author = "chrismo";
                    //describe the nodes function
                    FPluginInfo.Help = "Renders the Surface of a SWF File to a Direct3D Texture";
                    //specify a comma separated list of tags that describe the node
                    FPluginInfo.Tags = "";

                    //give credits to thirdparty code used
                    FPluginInfo.Credits = "";
                    //any known problems?
                    FPluginInfo.Bugs = "";
                    //any known usage of the node that may cause troubles?
                    FPluginInfo.Warnings = "";

                    //leave below as is
                    System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(true);
                    System.Diagnostics.StackFrame sf = st.GetFrame(0);
                    System.Reflection.MethodBase method = sf.GetMethod();
                    FPluginInfo.Namespace = method.DeclaringType.Namespace;
                    FPluginInfo.Class = method.DeclaringType.Name;
                    //leave above as is
                }
                return FPluginInfo;
            }
        }

        public bool AutoEvaluate
        {
            //return true if this node needs to calculate every frame even if nobody asks for its output
            get { return false; }
        }

        #endregion node name and info


        #region pin creation

        //this method is called by vvvv when the node is created
        public void SetPluginHost(IPluginHost pHost)
        {
            Debug.WriteLine("SetPluginHost");

            //assign host
            FHost = pHost;

            //create inputs
            FHost.CreateTransformInput("Transform", TSliceMode.Dynamic, TPinVisibility.True, out FTranformIn);

            FHost.CreateStringInput("SWF Path", TSliceMode.Dynamic, TPinVisibility.True, out FSWFPath);
            FSWFPath.SetSubType("", true);

            FHost.CreateValueInput("Load", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FLoadSWF);
            FLoadSWF.SetSubType(0, 1, 1, 0, true, false, false);

            FHost.CreateValueInput("Mouse X", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FMouseX);
            FHost.CreateValueInput("Mouse Y", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FMouseY);
            FHost.CreateValueInput("Mouse Left Button", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FMouseLeftButton);

            FHost.UpdateEnum("Buffer Mode", "Single Buffering", new string[] { "Single Buffering", "Double Buffering" });
            FHost.CreateEnumInput("Buffer Mode", TSliceMode.Dynamic, TPinVisibility.True, out FBufferMode);
            FBufferMode.SetSubType("Buffer Mode");


            FHost.CreateValueInput("Enabled", 1, null, TSliceMode.Single, TPinVisibility.True, out FEnabledInput);
            FEnabledInput.SetSubType(0, 1, 1, 1, false, true, false);

            // temp, only for siemens project
            FHost.CreateValueInput("Reset", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FResetSiemensSWF);
            FResetSiemensSWF.SetSubType(0, 1, 1, 0, true, false, false);



            //create outputs
            FHost.CreateLayerOutput("Layer", TPinVisibility.True, out FLayerOutput);

            FHost.CreateValueOutput("Frame Rate", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FFrameRateOutput);
        }

        #endregion pin creation


        #region mainloop

        public void Configurate(IPluginConfig pInput)
        {
            //nothing to configure in this plugin
            //only used in conjunction with inputs of type cmpdConfigurate
        }

        //here we go, thats the method called by vvvv each frame
        //all data handling should be in here
        public void Evaluate(int pSpreadMax)
        {
            if (_FNUIFlashPlayer == null)
                return;

            try
            {
                FSpreadCount = pSpreadMax;



                // temp for siemens project
                // no possibility to jump to a certain frame
                // ********************************************************
                if (FResetSiemensSWF.PinIsChanged)
                {
                    double tReset;

                    FResetSiemensSWF.GetValue(0, out tReset);

                    if (tReset == 1.0)
                    {
                        _FNUIFlashPlayer.UpdateMousePosition(90, 100);
                        _FNUIFlashPlayer.UpdateMouseButton(0, true);
                        _FNUIFlashPlayer.UpdateMouseButton(0, false);
                    }
                }
                // ********************************************************






                Matrix4x4 world;

                if (FMouseX.PinIsChanged || FMouseY.PinIsChanged)
                {
                    double x, y;

                    FMouseX.GetValue(0, out x);
                    FMouseY.GetValue(0, out y);

                    FTranformIn.GetRenderWorldMatrix(0, out world);

                    // getting the transformed stage
                    x = (x - world.m41) / world.m11;
                    y = (y - world.m42) / world.m22;

                    // scale to swf coordinates
                    x = (x + 0.5) * _Width / 1.0;
                    y = (-1 * y + 0.5) * _Height / 1.0;

                    _FNUIFlashPlayer.UpdateMousePosition((int)x, (int)y);
                }

                if (FMouseLeftButton.PinIsChanged)
                {
                    double tLB;

                    FMouseLeftButton.GetValue(0, out tLB);

                    if (tLB == 1.0)
                        _FNUIFlashPlayer.UpdateMouseButton(0, true);
                    else
                        _FNUIFlashPlayer.UpdateMouseButton(0, false);
                }

            }
            catch
            {
            }
        }

        #endregion mainloop





        #region DXLayer

        private void LoadSWF(int pOnDevice)
        {
            if (_FNUIFlashPlayer != null)
                _FNUIMain.DeleteFlashPlayer(_FNUIFlashPlayer);

            _FNUIFlashPlayer = _FNUIMain.CreateFlashPlayer();
            _FNUIFlashPlayer.SetDelegates(ResizeTexture, LockRectangle, UnlockRectangle, AddDirtyRectangle);
            _FNUIFlashPlayer.SetEventNotifier(EventNotifier);

            string tPath = "";
            FSWFPath.GetString(0, out tPath);

            if (System.IO.File.Exists(tPath) == false)
            {
                System.Windows.Forms.MessageBox.Show("Invalid swf path: " + tPath);
                return;
            }

            _FNUIMain.LoadFlashHeader(tPath, ref _Width, ref _Height, ref _FrameRate, ref _Frames);

            FHost.Log(TLogType.Debug, "swf Width: " + _Width);
            FHost.Log(TLogType.Debug, "swf Height: " + _Height);
            FHost.Log(TLogType.Debug, "swf Framerate: " + _FrameRate);
            FHost.Log(TLogType.Debug, "swf Frames: " + _Frames);


            FFrameRateOutput.SetValue(0, (double)_FrameRate);

            IntPtr tPointer = new IntPtr(pOnDevice);
            Device tDevice = Device.FromPointer(tPointer);

            _Texture1 = new Texture(tDevice, _Width, _Height, 1, 0, Format.A8R8G8B8, Pool.Managed);
            _Texture2 = new Texture(tDevice, _Width, _Height, 1, 0, Format.A8R8G8B8, Pool.Managed);

            try
            {
                _FNUIFlashPlayer.CreateFlashControl(0, _Width, _Height, (IntPtr)0, (IntPtr)_BufferMode, false);
                FHost.Log(TLogType.Debug, "Switch To " + (_BufferMode == 0 ? "Single" : "Double") + " Buffering");
                _FNUIFlashPlayer.LoadMovie(tPath);
                
                Debug.WriteLine(_Timer.Start());
            }
            catch (Exception ex)
            {
                FHost.Log(TLogType.Debug, "Exception in FantastiqUI: " + ex.Message);

                if (ex.InnerException != null)
                    FHost.Log(TLogType.Debug, "inner: " + ex.InnerException.Message);
            }
        }

        private void RemoveResource(int OnDevice)
        {
            FHost.Log(TLogType.Debug, "Destroying Resource..." + OnDevice);

            if (FSprites.ContainsKey(OnDevice) == false)
                return;

            Sprite tSprite = FSprites[OnDevice];
            FSprites.Remove(OnDevice);

            tSprite.Dispose();

            _NeedsUpdate = true;
        }

        public void UpdateResource(IPluginOut ForPin, int OnDevice)
        {
            try
            {
                if (FLoadSWF.PinIsChanged)
                {
                    double tIsOne = 0;
                    FLoadSWF.GetValue(OnDevice, out tIsOne);

                    if (tIsOne == 1.0)
                    {
                        RemoveResource(OnDevice);

                        _NeedsUpdate = true;
                    }
                }

                if (FBufferMode.PinIsChanged)
                {
                    string tBufferMode = "";
                    FBufferMode.GetString(OnDevice, out tBufferMode);

                    switch (tBufferMode)
                    {
                        case "Single Buffering":
                            _BufferMode = 0;
                            break;

                        case "Double Buffering":
                            _BufferMode = 1;
                            break;
                    }

                    RemoveResource(OnDevice);

                    _NeedsUpdate = true;
                }
            }
            catch (Exception ex)
            {
                FHost.Log(TLogType.Debug, "Exception in UpdateResource: " + ex.Message);

                if (ex.InnerException != null)
                    FHost.Log(TLogType.Debug, "inner: " + ex.InnerException.Message);

                //if resource is not yet created on given Device, create it now
                _NeedsUpdate = true;
            }

            if (_NeedsUpdate)
            {
                FHost.Log(TLogType.Debug, "Creating Resource...");
                Device tDevice = Device.FromPointer(new IntPtr(OnDevice));

                Sprite tSprite = new Sprite(tDevice);

                FSprites.Add(OnDevice, tSprite);

                //dispose device
                tDevice.Dispose();

                LoadSWF(OnDevice);

                _NeedsUpdate = false;
            }
        }

        public void DestroyResource(IPluginOut ForPin, int OnDevice, bool OnlyUnManaged)
        {
            //Called by the PluginHost whenever a resource for a specific pin needs to be destroyed on a specific device. 
            //This is also called when the plugin is destroyed, so don't dispose dxresources in the plugins destructor/Dispose()

            RemoveResource(OnDevice);
        }



        public void Render(IDXLayerIO ForPin, IPluginDXDevice DXDevice)
        {
            //Called by the PluginHost everytime the plugin is supposed to render itself.
            //This is called from the PluginHost from within DirectX BeginScene/EndScene,
            //therefore the plugin shouldn't be doing much here other than some drawing calls.

            double tEnabled;
            FEnabledInput.GetValue(0, out tEnabled);

            if (tEnabled < 0.5 || _FNUIFlashPlayer == null || FSprites.Count < 1)
                return;


            try
            {
                Device tDevice = Device.FromPointer(new IntPtr(DXDevice.DevicePointer()));
                tDevice.SetTransform(TransformState.World, Matrix.Identity);

                int pointer = DXDevice.DevicePointer();
                Sprite tSprite = FSprites[pointer];
                FTranformIn.SetRenderSpace();


                tSprite.Begin(SpriteFlags.DoNotAddRefTexture | SpriteFlags.ObjectSpace | SpriteFlags.AlphaBlend);

                Matrix4x4 tTranformMatrix;

                for (int i = 0; i < FSpreadCount; i++)
                {
                    FTranformIn.GetRenderWorldMatrix(i, out tTranformMatrix);
                    tSprite.Transform = VSlimDXUtils.Matrix4x4ToSlimDXMatrix(VMath.Scale(1.0 / _Width, -1.0 / _Height, 1) * tTranformMatrix);




                    //_MarkVVVV = _Timer.CurrentTime();

                    

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



                    //_CurrentVVVV = _Timer.CurrentTime();
                    //swVVVV.WriteLine(_CurrentVVVV + "\t" + (_CurrentVVVV - _MarkVVVV));





                
                
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
            //Debug.WriteLine("Lock");
                
            _MarkFant = _Timer.CurrentTime();
            
            
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
            //Debug.WriteLine("Dirty");
            //Debug.WriteLine(String.Format("{0}, {1}, {2}, {3}", pX, pY, pX1 - pX, pY1 - pY));

            System.Drawing.Rectangle tRectangle = new System.Drawing.Rectangle(pX, pY, pX1 - pX, pY1 - pY);
            //System.Drawing.Rectangle tRectangle = new System.Drawing.Rectangle(0, 200, 550, 100);

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
            //Debug.WriteLine("Unlock");
            
            //unlock the texture and return
            if ((Int32)pTexture == 0)
                _Texture1.UnlockRectangle(0);
            else
                _Texture2.UnlockRectangle(0);



            _CurrentFant = _Timer.CurrentTime();
            swFant.WriteLine(_CurrentFant + "\t" + (_CurrentFant - _MarkFant));





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
