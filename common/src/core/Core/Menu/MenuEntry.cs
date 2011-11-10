using System;
using System.Collections.Generic;
using System.Windows.Forms;

using VVVV.Core.Commands;

namespace VVVV.Core.Menu
{
    /// <summary>
    /// Abstract implemention of IMenuEntry.
    /// </summary>
    public class MenuEntry : List<IMenuEntry>, IMenuEntry
    {
        protected ICommandHistory CommandHistory
        {
            get;
            set;
        }

        public string Name 
        { 
            get; 
            private set; 
        }
        
        public Keys ShortcutKeys 
        {
            get;
            private set;
        }
        
        public bool Enabled
        {
            get;
            protected set;
        }

        Action<MenuEntry> ClickCB;

        public MenuEntry(ICommandHistory commandHistory, string name)
            : this(commandHistory, name, Keys.None)
        {
        }

        public MenuEntry(string name, Action<MenuEntry> clickCB)
        {
            Name = name;
            ClickCB = clickCB;
            Enabled = true;
            ShortcutKeys = Keys.None;
        }

        public MenuEntry(ICommandHistory commandHistory, string name, Keys shortcutKeys)
        {
            CommandHistory = commandHistory;
            Name = name;
            ShortcutKeys = shortcutKeys;
            Enabled = true;
        }

        public bool HasSubMenuEntries
        {
            get
            {
                return Count > 0;;
            }
        }

        public virtual void Click()
        {
            if (ClickCB != null)
                ClickCB(this);
        }
    }
}
