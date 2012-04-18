using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using VVVV.Core;
using VVVV.Lang.View;


namespace VVVV.Nodes.Windows
{
    public partial class WindowState : HDEForm
    {
        public WindowState()
        {
            InitializeComponent();
            Visible = true;
        }

        [Node]
        public WindowState Window(int x, int y)
        {
            Location = new Point(x, y);
            return this;
        }
    }
}
