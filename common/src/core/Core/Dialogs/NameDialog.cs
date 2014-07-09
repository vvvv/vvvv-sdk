using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace VVVV.Core.Dialogs
{
    //enter a name
    public partial class NameDialog : BaseDialog
    {
        //needed to check whether the textbox was edited
        protected string FInitialText;

        public NameDialog()
            : this("Name")
        {
        }

        public NameDialog(string initText)
        {
            InitializeComponent();
            TextBoxName.Text = initText;
            FInitialText = initText;
            StartPosition = FormStartPosition.CenterParent;
        }

        //create dialog with a position which will be the center
        public NameDialog(Point pos)
            : this("Name", pos)
        {
        }

        public NameDialog(string initText, Point pos)
            : this(initText)
        {
            StartPosition = FormStartPosition.Manual;
            Location = Point.Subtract(pos, new Size(Width / 2, Height / 2));
        }

        //empty textbox if it contains the initial text
        private void textBox1_Click(object sender, EventArgs e)
        {
            if (TextBoxName.Text == FInitialText)
                TextBoxName.Text = "";
        }

        //return the text
        public string EnteredText
        {
            get
            {
                return TextBoxName.Text;
            }
        }

        //check if input is valid
        private void TextBoxFunctionName_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!AllowCharacter(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        protected virtual bool AllowCharacter(char chr)
        {
            if (TextBoxName.Text == "" || TextBoxName.Text == FInitialText)
            {
                return Char.IsLetter(chr) ||
                    Char.IsControl(chr);
            }
            else
            {
                return Char.IsLetterOrDigit(chr) ||
                    chr == '_' ||
                	chr == '.' ||
                    Char.IsControl(chr);
            }

        }

        //check if pasted text is ok
        protected virtual void TextBoxFunctionName_KeyUp(object sender, KeyEventArgs e)
        {
            //temp text
            var text = TextBoxName.Text;
            
            //remove numbers and '_' from start
            while (text.Length > 0 && (Char.IsDigit(text[0]) || text[0] == '_'))
            {
                text = text.Remove(0, 1);
            }

            //remove special characters
            foreach (var chr in TextBoxName.Text)
            {
                if (!AllowCharacter(chr))
                {
                    text = text.Replace("" + chr, "");
                }
            }

            TextBoxName.Text = text;

            if (text != "") ButtonOK.Enabled = true;
        }
    }
}
