using System;
using System.Windows.Forms;
using VVVV.Core.Commands;

namespace VVVV.Core.Menu
{
    public class UndoMenuEntry : MenuEntry
    {
        public UndoMenuEntry(ICommandHistory commandHistory)
            :base(commandHistory, string.Format("Undo {0}", commandHistory.PreviousCommand), Keys.Control | Keys.Z)
        {
        }

        public override bool Enabled
        {
            get
            {
                var prevCommand = CommandHistory.PreviousCommand;
                return prevCommand != null && prevCommand.HasUndo;
            }
        }
        
        public override void Click()
        {
            CommandHistory.Undo();
        }
    }
}
