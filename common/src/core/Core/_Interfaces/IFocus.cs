using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VVVV.Core
{
    public interface IFocus<T> 
    {
        T FocusedItem
        {
            get;
            set;
        }
    }
}
