using System;
using System.Windows.Forms;
using VVVV.Core.Commands;

namespace VVVV.Core.Menu
{
    public class RemoveMenuEntry<TOwner, TItem> : MenuEntry
        where TOwner : IEditableCollection, IIDItem
        where TItem : IIDItem
    {
        protected TOwner FOwner;
        protected TItem FItem;
        
        public RemoveMenuEntry(ICommandHistory commandHistory, TOwner owner, TItem item)
            :this(commandHistory, owner, item, "Remove")
        {
            
        }
        
        public RemoveMenuEntry(ICommandHistory commandHistory, TOwner owner, TItem item, string name)
            :base(commandHistory, name, Keys.Delete)
        {
            FOwner = owner;
            FItem = item;
        }
        
        public override void Click()
        {
            var command = new RemoveCommand<TOwner, TItem>(FOwner, FItem);
            CommandHistory.Insert(command);
        }
    }
}
