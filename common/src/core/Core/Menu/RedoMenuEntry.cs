using System;
using System.Windows.Forms;
using VVVV.Core.Commands;

namespace VVVV.Core.Menu
{
    public class RedoMenuEntry : MenuEntry
    {
        public RedoMenuEntry(ICommandHistory commandHistory)
            :base(commandHistory, string.Format("Redo {0}", commandHistory.NextCommand), Keys.Control | Keys.Shift | Keys.Z)
        {
        }

        public override bool Enabled
        {
            get
            {
                return CommandHistory.NextCommand != null;
            }
        }
        
        public override void Click()
        {
            CommandHistory.Redo();
        }
    }
}
