using System;
using VVVV.Core.Commands;
using VVVV.Core.Dialogs;
using System.Windows.Forms;
using VVVV.Core.Viewer;

namespace VVVV.Core.Menu
{
    public class RenameMenuEntry : MenuEntry
    {
        public IRenameable Renameable
        {
            get;
            private set;
        }
        
        public ILabelEditor LabelEditor
        {
            get;
            private set;
        }
        
        public RenameMenuEntry(ICommandHistory commandHistory, IRenameable renameable)
            : this(commandHistory, renameable, null)
        {
        }
        
        public RenameMenuEntry(ICommandHistory commandHistory, IRenameable renameable, ILabelEditor labelEditor)
            : base(commandHistory, "Rename", Keys.F2)
        {
            Renameable = renameable;
            LabelEditor = labelEditor;
        }

        public override sealed void Click()
        {
            if (LabelEditor != null)
            {
            	LabelEditor.BeforeLabelEdit += LabelEditor_BeforeLabelEdit;
                LabelEditor.AfterLabelEdit += LabelEditor_AfterLabelEdit;
                if (!LabelEditor.BeginEdit(Renameable))
                {
                	LabelEditor.BeforeLabelEdit -= LabelEditor_BeforeLabelEdit;
                    LabelEditor.AfterLabelEdit -= LabelEditor_AfterLabelEdit;
                }
            }
            else
            {
                var diag = new NameDialog();
    
                var nameDialog = new NameDialog();
                nameDialog.Text = Renameable.Name;
                var result = nameDialog.ShowDialog();
    
                if (result == DialogResult.OK)
                {
                    if (Renameable.CanRenameTo(nameDialog.EnteredText))
                    {
                        CommandHistory.Insert(Command.Rename(Renameable, nameDialog.EnteredText));
                    }
                }
            }
        }

        void LabelEditor_BeforeLabelEdit(object sender, VVVV.Core.Viewer.LabelEditEventArgs args)
        {
            //args.CancelEdit = !args.Model.Equals(Renameable);
        }

        void LabelEditor_AfterLabelEdit(object sender, VVVV.Core.Viewer.LabelEditEventArgs args)
        {
            //if (args.Model.Equals(Renameable))
            {
                var label = args.Label;
                if (Renameable.CanRenameTo(label))
                {
                    var command = Command.Rename(Renameable, label);
                    CommandHistory.Insert(command);
                    LabelEditor.AfterLabelEdit -= LabelEditor_AfterLabelEdit;
                }
                else
                    args.CancelEdit = true;
            }
        }
    }
}
