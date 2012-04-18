using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using VVVV.Core;
using System.Drawing;

namespace VVVV.Nodes.Windows
{
    public class LabelState
    {

        Label FLabel;

        public LabelState()
        {
            FLabel = new Label();
            //FLabel.Text = "STATIC TEXT"; 
        }

        [Node]
        public int Label(WindowState window, int x, int y, string text)
        {
            FLabel.Location = new Point(x, y);

            FLabel.Text = text;
            if (!window.Controls.Contains(FLabel))
                window.Controls.Add(FLabel);

            return 0;
        }
    }
}
