using System;
using VVVV.Core.Commands;
using VVVV.Core.Dialogs;
using System.Windows.Forms;

namespace VVVV.Core.Menu
{
    public class SetPropertyMenuEntry : MenuEntry
    {
        public SetPropertyMenuEntry(ICommandHistory commandHistory, IEditableProperty property)
            : base(commandHistory, "Change Value")
        {
            Property = property;
        }

        public IEditableProperty Property
        {
            get;
            private set;
        }

        public override sealed void Click()
        {
            var diag = new NameDialog();

            var nameDialog = new NameDialog();

            nameDialog.Text = Property.ValueObject.ToString();

            var result = nameDialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                Property.SetByCommand(nameDialog.EnteredText, CommandHistory);
            }
        }
    }
}
