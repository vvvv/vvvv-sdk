using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VVVV.Utils.Collections
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> OfType<T, U>(this IEnumerable<U> enumerable) where T : class
        {
            return enumerable.Where(e => e is T).Select(e => e as T);
        }
    }
}
