using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VVVV.Core.Menu
{
    public interface IAddMenuProvider 
    {
        IEnumerable<IMenuEntry> GetEnumerator();
    }
}
