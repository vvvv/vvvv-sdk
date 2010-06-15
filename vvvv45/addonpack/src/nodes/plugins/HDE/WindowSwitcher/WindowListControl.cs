using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

using VVVV.PluginInterfaces.V1;

namespace VVVV.Nodes
{
    /// <summary>
    /// Description of WindowListControl.
    /// </summary>
    public partial class WindowListControl : UserControl
    {
        private Color CHoverColor = Color.FromArgb(255, 216, 216, 216);
        private IWindow FWindow;
        public IWindow Window
        {
            get{return FWindow;}                
        }
                
        public bool Selected
        {
            set
            {
                if (value)
                {
                    labelCaption.BackColor = CHoverColor;
                    panel1.BackColor = CHoverColor;
                }
                else
        	    {
                    labelCaption.BackColor = Color.Silver;
                    panel1.BackColor = Color.Silver;
                }
            }
        }
        
        private int FCaptionWidth = 200;
        public int CaptionWidth
        {
            get{return FCaptionWidth;}
        }
        
        public WindowListControl(IWindow window)
        {
            //
            // The InitializeComponent() call is required for Windows Forms designer support.
            //
            InitializeComponent();
            
            FWindow = window;
        }
        
        public void UpdateCaption()
        {
            labelCaption.Text = FWindow.GetCaption();
            
            Size s = TextRenderer.MeasureText(labelCaption.Text, labelCaption.Font, new Size(1, 1));
            FCaptionWidth = Math.Max(FCaptionWidth, s.Width + 50);
        }        
        
        void WindowListControlMouseEnter(object sender, EventArgs e)
        {
        	Selected = true;
        }
        
        void WindowListControlMouseLeave(object sender, EventArgs e)
        {
        	Selected = false;
        }
    }
}
