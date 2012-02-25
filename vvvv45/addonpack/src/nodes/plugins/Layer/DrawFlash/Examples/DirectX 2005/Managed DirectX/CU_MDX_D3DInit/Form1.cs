using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using FantastiqUINet;

namespace CUnit
{
    public partial class Form1 : Form
    {
        private Device m_device;
        private CustomVertex.TransformedColoredTextured[] verts;

        //fantastiqui main class
        public FNUIMain fmain;
        //flash player instance
        public FNUIFlashPlayer fplayer;
        Texture tex1; Texture tex2;

        /// <summary>
        /// DirtyRect is a callback that passes us regions of an updated part
        /// of the texture, we pass this to the ditry texture class as
        /// dirty rectangles so directx updates them
        /// </summary>
        public void DirtyRect(IntPtr pTexture, Int32 x, Int32 y, Int32 x1, Int32 y1)
        {
            //figure which texture to use
            Texture tex;
            if ((Int32)pTexture == 0)
                tex = tex1;
            else
                tex = tex2;
            //set the rectangle to dirty
            System.Drawing.Rectangle r = new System.Drawing.Rectangle(x, y, x1 - x, y1 - y);
            tex.AddDirtyRectangle(r);
        }

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
        public IntPtr GetTextureSurfacePointer(IntPtr pTexture)
        {
            //figure which texture to use
            Texture tex;
            if ((Int32)pTexture == 0)
                tex = tex1;
            else
                tex = tex2;
            //lock the texture, and return the surface pointer
            GraphicsStream stream = tex.LockRectangle(0, LockFlags.NoDirtyUpdate);
            return stream.InternalData;
        }
        /// <summary>
        /// Fantastiqui calls this function when texture editing is complete, and the
        /// texture can be unlocked again
        /// </summary>
        public Int32 ReleaseTextureSurfacePointer(IntPtr pTexture, IntPtr pPointer)
        {
            //figure which texture to use
            Texture tex;
            if ((Int32)pTexture == 0)
                tex = tex1;
            else
                tex = tex2;
            //unlock the texture and return
            tex.UnlockRectangle(0);
            return 0;
        }

        //WARNING , NOT CALLED FROM MAIN THREAD!!! EG, MULTITHREADING CAREFULNESS NEEDED
        //FANTASTIQUI event functions should be safe for this.
        public void EventNotifier()
        {
            //get the number of events, this call also locks the
            //event list, clearevents() always needs to be called close
            //after to unlock it
            Int32 inum = fplayer.GetNumEvents();
            for (Int32 i = 0; i < inum; i++)
            {
                //grab the first event
                FNUIFlashEvent ev = fplayer.GetEvent(i);
                //make a string of the function name called and the value of the
                //first argument
                String blaat = ev.GetFunctionName() + " " + ev.GetValueString(0);
                MessageBox.Show(blaat);
                fplayer.DeleteEvent(ev);
            }
            //unlock the event list
            fplayer.ClearEvents();
        }

        public Form1()
        {
            InitializeComponent();
        }

        /// <summary>Initializes DirectX graphics</summary>
        /// <returns>true on success, false on failure</returns>
        public bool InitializeGraphics()
        {
            PresentParameters pp = new PresentParameters();
            pp.SwapEffect = SwapEffect.Discard;
            //pp.IsWindowed = true;
            pp.Windowed = true;
            try
            {
                m_device = new Device( 0, DeviceType.Hardware, this.Handle, CreateFlags.SoftwareVertexProcessing, pp );

                verts = new CustomVertex.TransformedColoredTextured[4];
                verts[0] = new CustomVertex.TransformedColoredTextured(
                    0, 0, 0, 1.0f,Color.White.ToArgb(),0.0f,0.0f);
                verts[1] = new CustomVertex.TransformedColoredTextured(
                   511, 0, 0, 1.0f, Color.White.ToArgb(), 1.0f, 0.0f);
                verts[2] = new CustomVertex.TransformedColoredTextured(
                    511, 511, 0, 1.0f, Color.White.ToArgb(), 1.0f, 1.0f);
                verts[3] = new CustomVertex.TransformedColoredTextured(
                0, 511, 0, 1.0f, Color.White.ToArgb(), 0.0f, 1.0f);

                tex1 = new Texture(m_device, 512, 512, 1, 0, Format.A8R8G8B8, Pool.Managed);
                tex2 = new Texture(m_device, 512, 512, 1, 0, Format.A8R8G8B8, Pool.Managed);
                //create main fui class
                fmain = new FNUIMain();
                fmain.CreateUI("");
                //create a new flash player instance
                fplayer = fmain.CreateFlashPlayer();
                //set the callback delegates and the event notifier
                fplayer.SetDelegates(ResizeTexture, GetTextureSurfacePointer, ReleaseTextureSurfacePointer, DirtyRect);
                fplayer.SetEventNotifier(EventNotifier);
                //create the flash control at resolution 512x512 and with tex1 and tex2 as textures,
                //eg. doublebuffering, for singlebuffering, just provide 1 texture
                fplayer.CreateFlashControl(0, 512, 512, (IntPtr)0, (IntPtr)1, true);
                //load a movie to test
                fplayer.LoadMovie(Application.StartupPath + "\\SiemensContent\\index.swf");

                return true;
            }
            catch ( DirectXException )
            {
                return false;
            }

        }

        /// <summary>Application idle event. Updates and renders frames.</summary>
        /// <param name="sender">Sending object</param>
        /// <param name="e">Event arguments</param>
        public void OnApplicationIdle( object sender, EventArgs args )
        {
            while ( AppStillIdle )
            {
                // UpdateFrame();
                RenderFrame();
            }
        }

        /// <summary>Renders the current frame</summary>
        private void RenderFrame()
        {
            m_device.Clear( ClearFlags.Target, Color.Navy, 1.0f, 0 );
            m_device.BeginScene();

            // Set render states since GUI and font changes them when they render 
            m_device.SetRenderState(RenderStates.ZEnable, true);

            m_device.SetRenderState(RenderStates.ZBufferWriteEnable, true);
            m_device.SetRenderState(RenderStates.AlphaBlendEnable, true);

            // use alpha channel in texture for alpha
            /*
            m_device.SetTextureStageState(0, TextureD3DTSS_ALPHAARG1, D3DTA_TEXTURE);
            m_device.SetTextureStageState(0, D3DTSS_ALPHAOP, D3DTOP_SELECTARG1);

            m_device.SetRenderState(RenderStates.SourceBlend, D3DBLEND_SRCALPHA);
            m_device.SetRenderState(RenderStates.DestinationBlend, D3DBLEND_INVSRCALPHA);		//alpha blending enabled
            */

            // Render triangle 
            if(fplayer.GetTexture().ToInt32() == 0)
                m_device.SetTexture(0, tex1);
            else
                m_device.SetTexture(0, tex2);

            m_device.VertexFormat = CustomVertex.TransformedColoredTextured.Format;
            m_device.DrawUserPrimitives(PrimitiveType.TriangleFan, 2, verts);

            fplayer.ReleaseTexture();

            m_device.EndScene();
            m_device.Present();
        }

        /// <summary>Returns whether the application is currently idle.</summary>
        private bool AppStillIdle
        {
            get
            {

                NativeMethods.Message msg;
                bool peek = NativeMethods.PeekMessage(out msg, IntPtr.Zero, 0, 0, 0);
                return !peek;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            fplayer.UpdateMousePosition(e.X, e.Y);
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            fplayer.UpdateMouseButton(0, true);
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            fplayer.UpdateMouseButton(0, false);
        }
    }
}