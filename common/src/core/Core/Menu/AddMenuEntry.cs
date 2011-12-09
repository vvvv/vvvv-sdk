using System;
using VVVV.Core.Commands;

namespace VVVV.Core.Menu
{
    public abstract class AddMenuEntry : MenuEntry
    {
        public AddMenuEntry(ICommandHistory commandHistory)
            : base(commandHistory, "Add")
        {
        
        }

        public override sealed void Click()
        {
            // Do nothing
        }
    }
}
