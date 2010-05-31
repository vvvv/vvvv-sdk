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
                    labelCaption.BackColor = Color.LightGray;
                    panel1.BackColor = Color.LightGray;
                }
                else
        	    {
                    labelCaption.BackColor = SystemColors.Control;
                    panel1.BackColor = SystemColors.Control;
                }
            }
        }
        
        public WindowListControl(IWindow window)
        {
            //
            // The InitializeComponent() call is required for Windows Forms designer support.
            //
            InitializeComponent();
            
            FWindow = window;
            labelCaption.Text = FWindow.GetCaption();
        }
        
        public void UpdateCaption()
        {
            labelCaption.Text = FWindow.GetCaption();
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
