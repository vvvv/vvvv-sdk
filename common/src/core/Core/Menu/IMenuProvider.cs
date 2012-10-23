using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VVVV.Core.Menu
{
    public interface IMenuProvider
    {
        /// <summary>
        /// Returns all the menu entries this menu is composed of.
        /// </summary>
        IEnumerable<IMenuEntry> MenuEntries { get; }
    }
}
