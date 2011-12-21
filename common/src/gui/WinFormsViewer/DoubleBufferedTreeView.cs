
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace VVVV.HDE.Viewer.WinFormsViewer
{
    public partial class DoubleBufferedTreeView : TreeView
    {
        public DoubleBufferedTreeView()
        {
            //
            // The InitializeComponent() call is required for Windows Forms designer support.
            //
            InitializeComponent();
            
             this.DoubleBuffered = true;
        }
        
        private const int WM_ERASEBKGND = 0x0014;

        protected override void WndProc(ref Message msg)
        {
            if (msg.Msg == WM_ERASEBKGND)
            {
                msg.Msg = (int) 0x0000; //reset message to null

            }
            base.WndProc(ref msg);
        }
    }
}
