using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace VVVV.Core.Menu
{
    public interface IMenuEntry : IEnumerable<IMenuEntry>
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
    }
}
