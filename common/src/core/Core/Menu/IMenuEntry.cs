using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace VVVV.Core.Menu
{
    public interface IMenuEntry
    {
        /// <summary>
        /// The Name to be displayed in the menu.
        /// </summary>
        string Name
        {
            get;
        }
        
        /// <summary>
        /// The Keys to be pressed in order to trigger the Click method.
        /// </summary>
        Keys ShortcutKeys
        {
            get;
        }
        
        /// <summary>
        /// Gets executed if the menu entry is clicked by the user or
        /// the keys matching the KeyData property are pressed.
        /// </summary>
        void Click();
        
        /// <summary>
        /// Whether this entry is enabled or disabled.
        /// </summary>
        bool Enabled
        {
            get;
        }

        /// <summary>
        /// Whether this entry is checked or not. This property should be ignored if null.
        /// </summary>
        bool? Checked { get; set; }

        /// <summary>
        /// Gets all the sub entries (if any).
        /// </summary>
        IEnumerable<IMenuEntry> Entries { get; }
    }
}
