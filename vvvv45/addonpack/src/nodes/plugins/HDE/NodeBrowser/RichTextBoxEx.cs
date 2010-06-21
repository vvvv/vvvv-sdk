using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.Runtime.InteropServices;


namespace VVVV.Nodes
{
    /// <summary>
    /// Description of RichTextBoxEx.
    /// </summary>
    public class RichTextBoxEx: RichTextBox

    {
        public event EventHandler OnPaint;
        const int WM_PAINT = 0x000F;
                
        public RichTextBoxEx(): base()
        {
        }
        
        protected override void WndProc(ref Message m)
        {
            bool handled = false;
            switch (m.Msg)
            {
                case WM_PAINT:
                    {
                       
                        base.WndProc(ref m);                      
                        OnPaint(this, null);
                        handled = true;
                        break;
                    }
            }

            if (!handled)
                base.WndProc(ref m);
        }
    }
}
