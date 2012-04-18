using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace VVVV.Lang.View
{
    public partial class HDEForm : Form
    {
        public HDEForm(MenuStrip mainMenuStrip)
            : this()
        {
            MainMenuStrip = mainMenuStrip;
        }

        public HDEForm()
        {
            InitializeComponent();
            this.FormClosed += new FormClosedEventHandler(MyFormClosed);
        }

        private static void MyFormClosed(object sender, FormClosedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void HDEForm_Activated(object sender, EventArgs e)
        {
            //MainMenuStrip = MainMenu;

            //if (!(this is MainForm))
            //    MainMenuStrip.Visible = false;
        }

        //private void HDEForm_MouseDown(object sender, MouseEventArgs e)
        //{
        //    if (e.Button == System.Windows.Forms.MouseButtons.Middle)
        //    {
        //        MainMenuStrip.Location = e.Location;
        //        MainMenuStrip.Show();
        //    }
        //}

    }
}
