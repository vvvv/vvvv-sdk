using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VVVV.HDE.CodeEditor.Gui
{
    public partial class ReloadBar : UserControl
    {
        private CodeEditor editor;

        public ReloadBar()
        {
            InitializeComponent();
        }

        public ReloadBar(CodeEditor codeEditor) : this()
        {
            editor = codeEditor;
            Hide();
        }

        public void ShowBar()
        {
            if (Visible) return;
            
            var document = editor.TextDocument;
            if (document.IsDirty)
            {
                Show();
                BringToFront();

                reloadButton.Focus();
            }
        }

        private void reloadButton_Click(object sender, EventArgs e)
        {
            Hide();
            var document = editor.TextDocument;
            document.Content = document.ContentOnDisk;
        }
    }
}
