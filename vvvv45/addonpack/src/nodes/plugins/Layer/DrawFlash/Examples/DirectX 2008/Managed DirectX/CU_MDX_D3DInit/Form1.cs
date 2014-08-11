using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using System.IO;

using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

using FantastiqUINet;


namespace CUnit
{
    public partial class Form1 : Form
    {
        private Device _Device;
        private CustomVertex.TransformedColoredTextured[] _Vertices;

        public FNUIMain _FNUIMain;
        public FNUIFlashPlayer _FNUIFlashPlayer;
        
        Texture _Texture1; Texture _Texture2;

        int _Width = 0, _Height = 0;
        string _Path = Application.StartupPath + @"\SiemensContent\index.swf";



        /// <summary>Returns whether the application is currently idle.</summary>
        private bool AppStillIdle
        {
            get
            {
                NativeMethods.Message msg;

                bool tPeek = NativeMethods.PeekMessage(out msg, IntPtr.Zero, 0, 0, 0);

                return !tPeek;
            }
        }


        public Form1()
        {
            InitializeComponent();
        }



        /// <summary>Initializes DirectX graphics</summary>
        /// <returns>true on success, false on failure</returns>
        public bool InitializeGraphics()
        {
            PresentParameters tPresentParas = new PresentParameters();
            tPresentParas.SwapEffect = SwapEffect.Discard;
            tPresentParas.Windowed = true;

            try
            {
                _Device = new Device(0, DeviceType.Hardware, this.Handle, CreateFlags.SoftwareVertexProcessing, tPresentParas);

                //create main fui class
                _FNUIMain = new FNUIMain();
                _FNUIMain.CreateUI("");


                float tFramerate = 0;
                int tFrames = 0;
                
                _FNUIMain.LoadFlashHeader(_Path, ref _Width, ref _Height, ref tFramerate, ref tFrames);

                this.ClientSize = new Size(_Width, _Height);
            
                _Vertices = new CustomVertex.TransformedColoredTextured[4];
                _Vertices[0] = new CustomVertex.TransformedColoredTextured(0, 0, 0, 1.0f, Color.White.ToArgb(), 0.0f, 0.0f);
                _Vertices[1] = new CustomVertex.TransformedColoredTextured(_Width-1, 0, 0, 1.0f, Color.White.ToArgb(), 1.0f, 0.0f);
                _Vertices[2] = new CustomVertex.TransformedColoredTextured(_Width-1, _Height-1, 0, 1.0f, Color.White.ToArgb(), 1.0f, 1.0f);
                _Vertices[3] = new CustomVertex.TransformedColoredTextured(0, _Height-1, 0, 1.0f, Color.White.ToArgb(), 0.0f, 1.0f);


                _Texture1 = new Texture(_Device, _Width, _Height, 1, 0, Format.A8R8G8B8, Pool.Managed);
                _Texture2 = new Texture(_Device, _Width, _Height, 1, 0, Format.A8R8G8B8, Pool.Managed);



                //create a new flash player instance
                _FNUIFlashPlayer = _FNUIMain.CreateFlashPlayer();
                //set the callback delegates and the event notifier
                _FNUIFlashPlayer.SetDelegates(ResizeTexture, LockRectangle, UnlockRectangle, AddDirtyRectangle);
                _FNUIFlashPlayer.SetEventNotifier(EventNotifier);
                //create the flash control at resolution 512x512 and with tex1 and tex2 as textures,
                //eg. doublebuffering, for singlebuffering, just provide 1 texture
                _FNUIFlashPlayer.CreateFlashControl(0, _Width, _Height, (IntPtr)0, (IntPtr)1, false);
                //load a movie
                _FNUIFlashPlayer.LoadMovie(_Path);

                return true;
            }
            catch (DirectXException)
            {
                return false;
            }

        }

        /// <summary>Application idle event. Updates and renders frames.</summary>
        /// <param name="sender">Sending object</param>
        /// <param name="e">Event arguments</param>
        public void OnApplicationIdle(object sender, EventArgs args)
        {
            while (AppStillIdle)
            {
                // UpdateFrame();
                RenderFrame();

                System.Threading.Thread.Sleep(10);
            }
        }

        /// <summary>Renders the current frame</summary>
        private void RenderFrame()
        {
            _Device.Clear(ClearFlags.Target, Color.Navy, 1.0f, 0);
            _Device.BeginScene();

            //// Set render states since GUI and font changes them when they render 
            //_Device.SetRenderState(RenderStates.ZEnable, true);

            //_Device.SetRenderState(RenderStates.ZBufferWriteEnable, true);
            //_Device.SetRenderState(RenderStates.AlphaBlendEnable, true);

            //// use alpha channel in texture for alpha

            //_Device.SetTextureStageState(0, TextureD3DTSS_ALPHAARG1, D3DTA_TEXTURE);
            //_Device.SetTextureStageState(0, D3DTSS_ALPHAOP, D3DTOP_SELECTARG1);

            //_Device.SetRenderState(RenderStates.SourceBlend, D3DBLEND_SRCALPHA);
            //_Device.SetRenderState(RenderStates.DestinationBlend, D3DBLEND_INVSRCALPHA);		//alpha blending enabled
            

            // Render triangle 
            if (_FNUIFlashPlayer.GetTexture().ToInt32() == 0)
            {
                _Device.SetTexture(0, _Texture1);
                Console.WriteLine("Device.SetTexture(Texture 1)");
            }
            else
            {
                _Device.SetTexture(0, _Texture2);
                Console.WriteLine("Device.SetTexture(Texture 2)");
            }

            _Device.VertexFormat = CustomVertex.TransformedColoredTextured.Format;
            _Device.DrawUserPrimitives(PrimitiveType.TriangleFan, 2, _Vertices);

            _FNUIFlashPlayer.ReleaseTexture();

            _Device.EndScene();
            _Device.Present();
        }













        #region Callbacks

        /// <summary>
        /// In case you resize the flash player, this function is called to tell you to
        /// actually resize the textures used, as return value you can provide a new
        /// texture pointer that will be used from that moment
        /// </summary>
        public IntPtr ResizeTexture(IntPtr _pTexture, Int32 iSizeX, Int32 iSizeY, Int32 iReserved)
        {
            return (IntPtr)0;
        }
        
        /// <summary>
        /// Requests from you a pointer to a surface to which flash has to be written
        /// for texture pTexture, so here we lock the texture and return the surface pointer
        /// </summary>
        public IntPtr LockRectangle(IntPtr pTexture)
        {
            GraphicsStream tStream;

            //lock the texture, and return the surface pointer

            if ((Int32)pTexture == 0)
            {
                tStream = _Texture1.LockRectangle(0, LockFlags.NoDirtyUpdate);
                Console.WriteLine("Texture1.LockRectangle()");
            }
            else
            {
                tStream = _Texture2.LockRectangle(0, LockFlags.NoDirtyUpdate);
                Console.WriteLine("Texture2.LockRectangle()");
            }

            return tStream.InternalData;
        }

        /// <summary>
        /// DirtyRect is a callback that passes us regions of an updated part
        /// of the texture, we pass this to the dirty texture class as
        /// dirty rectangles so directx updates them
        /// </summary>
        public void AddDirtyRectangle(IntPtr pTexture, Int32 x, Int32 y, Int32 x1, Int32 y1)
        {
            System.Drawing.Rectangle tRectangle = new System.Drawing.Rectangle(x, y, x1 - x, y1 - y);

            //set the rectangle to dirty
            if ((Int32)pTexture == 0)
            {
                _Texture1.AddDirtyRectangle(tRectangle);
                Console.WriteLine("Texture1.AddDirtyRectangle()");
            }
            else
            {
                _Texture2.AddDirtyRectangle(tRectangle);
                Console.WriteLine("Texture2.AddDirtyRectangle()");
            }
        }
                
        /// <summary>
        /// Fantastiqui calls this function when texture editing is complete, and the
        /// texture can be unlocked again
        /// </summary>
        public Int32 UnlockRectangle(IntPtr pTexture, IntPtr pPointer)
        {
            //unlock the texture and return
            if ((Int32)pTexture == 0)
            {
                _Texture1.UnlockRectangle(0);
                Console.WriteLine("Texture1.UnlockRectangle()");
            }
            else
            {
                _Texture2.UnlockRectangle(0);
                Console.WriteLine("Texture2.UnlockRectangle()");
            }

            return 0;
        }

        //WARNING , NOT CALLED FROM MAIN THREAD!!! EG, MULTITHREADING CAREFULNESS NEEDED
        //FANTASTIQUI event functions should be safe for this.
        public void EventNotifier()
        {
            ////get the number of events, this call also locks the
            ////event list, clearevents() always needs to be called close
            ////after to unlock it
            //Int32 inum = _FNUIFlashPlayer.GetNumEvents();
            //for (Int32 i = 0; i < inum; i++)
            //{
            //    //grab the first event
            //    FNUIFlashEvent ev = _FNUIFlashPlayer.GetEvent(i);
            //    //make a string of the function name called and the value of the
            //    //first argument
            //    String blaat = ev.GetFunctionName() + " " + ev.GetValueString(0);
            //    //MessageBox.Show(blaat);
            //    _FNUIFlashPlayer.DeleteEvent(ev);
            //}
            ////unlock the event list
            //_FNUIFlashPlayer.ClearEvents();
        }

        #endregion











        #region Events

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            _FNUIFlashPlayer.UpdateMousePosition(e.X, e.Y);
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            _FNUIFlashPlayer.UpdateMouseButton(0, true);
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            _FNUIFlashPlayer.UpdateMouseButton(0, false);
        }

        #endregion
    }
}