using System;
using System.Collections.Generic;
using System.Text;

namespace MrVux.Comparers
{
    public class DefaultComparer<P> : IComparer<P>
        where P : IComparable<P>
    {
        public int Compare(P x, P y)
        {
            return x.CompareTo(y);
        }
    }
}
