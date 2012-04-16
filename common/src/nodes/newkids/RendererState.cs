using System;
using System.Drawing;
using System.Windows.Forms;
using OpenTK.Graphics.OpenGL;
using VVVV.Core;
using VVVV.Lang.View;

namespace VVVV.Nodes.Graphics.OpenGL
{
    /// <summary>
    /// Description of OpenGLWindow.
    /// </summary>
    public partial class RendererState : HDEForm
    {
        private bool loaded = false;
        
        public RendererState()
        {
            //
            // The InitializeComponent() call is required for Windows Forms designer support.
            //
            InitializeComponent();
            
            //
            // TODO: Add constructor code after the InitializeComponent() call.
            //
        }
        
        private Action<RendererState> FDrawAction;
        
        [Node]
        public RendererState Renderer(Action<RendererState> drawAction)
        {
            FDrawAction = drawAction;
            Visible = true;
            glControl.Invalidate();
            return this;
        }
        
        [Node]
        public static Action<RendererState> Quad(double x, double y, double width, double height)
        {
            return (renderer) =>
            {
                GL.Begin(BeginMode.Quads);
                GL.Vertex2(x, y);
                GL.Vertex2(x + width, y);
                GL.Vertex2(x + width, y + height);
                GL.Vertex2(x, y + height);
                GL.End();
            };
        }
        
        void GlControlLoad(object sender, EventArgs e)
        {
            loaded = true;
            GL.ClearColor(Color.SkyBlue);
            SetupViewport();
        }
        
        private void SetupViewport()
        {
            int w = glControl.Width;
            int h = glControl.Height;
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(-w/2, w/2, -h/2, h/2, -1, 1); // Bottom-left corner pixel has coordinate (0, 0)
            GL.Viewport(0, 0, w, h); // Use all of the glControl painting area
        }
        
        void GlControlPaint(object sender, PaintEventArgs e)
        {
            if (!loaded) return;
            
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            GL.Color3(Color.Yellow);
            
            if (FDrawAction != null)
            {
                FDrawAction(this);
            }
            
            glControl.SwapBuffers();
        }
        
        void GlControlResize(object sender, EventArgs e)
        {
            SetupViewport();
            glControl.Invalidate();
        }
    }
}
