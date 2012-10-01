using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Linq;

namespace VVVV.Core.Menu
{
    /// <summary>
    /// A MenuSeparator should be rendered by a viewer in a way to reflect the
    /// separation of two menu entries.
    /// </summary>
    public class MenuSeparator : IMenuEntry
    {
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

        public IEnumerable<IMenuEntry> Entries { get { return Enumerable.Empty<IMenuEntry>(); } }
        
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

        public bool? Checked { get; set; }
    }
}
