using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.Core.View.GraphicalEditor;
using UMD.HCIL.Piccolo.Event;
using System.Windows.Forms;



namespace VVVV.HDE.GraphicalEditing
{
    public static class Helpers
    {
        internal static Mouse_Buttons GetButton(PInputEventArgs e)
        {
            Mouse_Buttons mb;

            switch (e.Button)
            {
                case MouseButtons.Right: mb = Mouse_Buttons.Right; break;
                case MouseButtons.Middle: mb = Mouse_Buttons.Middle; break;
                default: mb = Mouse_Buttons.Left; break;
            }

            return mb;
        }
    }
}
