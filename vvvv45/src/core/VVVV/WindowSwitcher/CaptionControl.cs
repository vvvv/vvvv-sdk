
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace VVVV.Nodes
{
    /// <summary>
    /// Description of CaptionControl.
    /// </summary>
    public partial class CaptionControl : UserControl
    {
        public CaptionControl(string caption)
        {
            //
            // The InitializeComponent() call is required for Windows Forms designer support.
            //
            InitializeComponent();
            
            labelCaption.Text = caption;
        }
    }
}
