using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VVVV.Utils.Collections
{
    public class ReferenceEqualityComparer<T> : EqualityComparer<T>
    {
        public override bool Equals(T x, T y)
        {
            return ReferenceEquals(x, y);
        }

        public override int GetHashCode(T obj)
        {
            return obj.GetHashCode();
        }
    }
}
