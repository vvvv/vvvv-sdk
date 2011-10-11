
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace VVVV.Nodes
{
    /// <summary>
    /// Description of DoubleBufferedPanel.
    /// </summary>
    public partial class DoubleBufferedPanel : Panel
    {
        public DoubleBufferedPanel()
        {
            //
            // The InitializeComponent() call is required for Windows Forms designer support.
            //
            InitializeComponent();
            
            //
            // TODO: Add constructor code after the InitializeComponent() call.
            //
            this.DoubleBuffered = true;// SetStyle(ControlStyles.OptimizedDoubleBuffer + ControlStyles.AllPaintingInWmPaint);
        }
    }
}
