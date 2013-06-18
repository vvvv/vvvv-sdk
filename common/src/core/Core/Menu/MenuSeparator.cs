using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;

namespace VVVV.Core.Menu
{
    /// <summary>
    /// A MenuSeparator should be rendered by a viewer in a way to reflect the
    /// separation of two menu entries.
    /// </summary>
    public class MenuSeparator : IMenuEntry
    {
        private static readonly List<IMenuEntry> FEmptyList = new List<IMenuEntry>();
        
        public MenuSeparator()
        {
        }
        
        public string Name
        {
            get 
            {
                return null;
            }
        }
        
        public void Click()
        {
            
        }
        
        public IEnumerator<IMenuEntry> GetEnumerator()
        {
            return FEmptyList.GetEnumerator();
        }
        
        IEnumerator IEnumerable.GetEnumerator()
        {
            return FEmptyList.GetEnumerator();
        }
        
        public Keys ShortcutKeys
        {
            get
            {
                return Keys.None;
            }
        }
        
        public bool Enabled
        {
            get 
            {
                return true;
            }
        }
    }
}
