using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace VVVV.Core.Dialogs
{
    //enter a name
    public partial class NameAndTypeDialog : BaseDialog
    {
        //needed to check whether the textbox was edited
        protected string FInitialText;
        private readonly Func<string, string, char, bool> FIsValidChar;

        public NameAndTypeDialog(string initName, string initType, Func<string, string, char, bool> isValidChar = null)
        {
            InitializeComponent();
            NameTextBox.Text = initName;
            TypeTextBox.Text = initType;
            FInitialText = initName;
            StartPosition = FormStartPosition.CenterParent;
            FIsValidChar = isValidChar ?? DefaultIsValidChar;
            IsValidName = DefaultIsValidName;
        }

        public Point Position
        {
            set
            {
                StartPosition = FormStartPosition.Manual;
                Location = Point.Subtract(value, new Size(Width / 2, Height / 2));
            }
        }

        public Func<string, bool> IsValidName { get; set; }

        private static bool DefaultIsValidName(string name)
        {
            return true;
        }

        //empty textbox if it contains the initial text
        private void textBox1_Click(object sender, EventArgs e)
        {
            if (NameTextBox.Text == FInitialText)
                NameTextBox.Text = "";
        }

        //return the text
        public string EnteredText
        {
            get
            {
                return NameTextBox.Text;
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

        private static bool DefaultIsValidChar(string currentText, string initialText, char chr)
        {
            if (string.IsNullOrEmpty(currentText) || currentText == initialText)
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

        protected bool AllowCharacter(char chr)
        {
            return FIsValidChar(NameTextBox.Text, FInitialText, chr);
        }

        //check if pasted text is ok
        protected virtual void TextBoxFunctionName_KeyUp(object sender, KeyEventArgs e)
        {
            //temp text
            var text = NameTextBox.Text;
            
            //remove numbers and '_' from start
            while (text.Length > 0 && (Char.IsDigit(text[0]) || text[0] == '_'))
            {
                text = text.Remove(0, 1);
            }

            //remove special characters
            foreach (var chr in NameTextBox.Text)
            {
                if (!AllowCharacter(chr))
                {
                    text = text.Replace("" + chr, "");
                }
            }

            NameTextBox.Text = text;

            if (text != "") ButtonOK.Enabled = IsValidName(text);
        }
    }
}
