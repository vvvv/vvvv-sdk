using System;
using System.Collections.Generic;
using System.Windows.Forms;

using VVVV.Core.Commands;

namespace VVVV.Core.Menu
{
    /// <summary>
    /// Abstract implemention of IMenuEntry.
    /// </summary>
    public class MenuEntry : IMenuEntry
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

        private Func<bool> enabledFunc;
        public virtual bool Enabled
        {
            get
            {
                if (enabledFunc != null)
                {
                    return enabledFunc();
                }
                return true;
            }
        }

        public bool? Checked
        {
            get;
            set;
        }

        Action<MenuEntry> ClickCB;
        List<IMenuEntry> entries = new List<IMenuEntry>();

        public MenuEntry(string name)
            : this(name, null)
        {
        }

        public MenuEntry(string name, Action<MenuEntry> clickCB)
            : this(name, Keys.None, clickCB)
        {
        }

        public MenuEntry(string name, Keys shortcutKeys)
            : this(name, shortcutKeys, null)
        {
        }

        public MenuEntry(string name, Keys shortcutKeys, Action<MenuEntry> clickCB, Func<bool> enabledFunc = null)
        {
            Name = name;
            ClickCB = clickCB;
            ShortcutKeys = shortcutKeys;
            this.enabledFunc = enabledFunc;
        }

        public MenuEntry(ICommandHistory commandHistory, string name)
            : this(commandHistory, name, Keys.None)
        {
        }

        public MenuEntry(ICommandHistory commandHistory, string name, Keys shortcutKeys)
        {
            CommandHistory = commandHistory;
            Name = name;
            ShortcutKeys = shortcutKeys;
        }

        public bool HasSubMenuEntries
        {
            get
            {
                return entries.Count > 0;;
            }
        }

        public virtual void Click()
        {
            if (ClickCB != null)
                ClickCB(this);
        }

        public IEnumerable<IMenuEntry> Entries { get { return entries.AsReadOnly(); } }

        public void AddEntry(IMenuEntry entry)
        {
            entries.Add(entry);
        }
    }
}
