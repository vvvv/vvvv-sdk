using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace VVVV.Core.Dialogs
{
    public partial class NamespaceNameDialog : NameDialog
    {
        public NamespaceNameDialog(string initText = "Name", Func<string, string, char, bool> isValidChar = null)
            : base(initText, isValidChar)
        {
        }

        //check if pasted text is ok
        protected override void TextBoxFunctionName_KeyUp(object sender, KeyEventArgs e)
        {
            //temp text
            var text = TextBoxName.Text;

            //remove numbers and '_' or '.' from start
            while (text.Length > 0 && (Char.IsDigit(text[0]) || text[0] == '_' || text[0] == '.'))
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

            //remove double '.'
            for (int i = 0; i < text.Length - 1; i++)
            {
                if(text[i] == '.' && text[i+1] == '.')
                    text = text.Remove(i, 1);
    
            }

            TextBoxName.Text = text;
            TextBoxName.SelectionStart = TextBoxName.Text.Length;

            if (text != "") ButtonOK.Enabled = true;
        }
    }
}
