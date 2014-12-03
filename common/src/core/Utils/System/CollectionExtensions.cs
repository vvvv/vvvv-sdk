using System;
using System.Linq;
using System.Text;

namespace System.Collections.Generic
{
    public static class CollectionExtensions
    {
        public static void Swap<T>(this IList<T> list, int i, int j)
        {
            var tmp = list[i];
            list[i] = list[j];
            list[j] = tmp;
        }

        public static void AddRange<TEnumerable, TItem>(this HashSet<TItem> hashSet, TEnumerable values)
            where TEnumerable : IEnumerable<TItem>
        {
            foreach (var value in values)
                hashSet.Add(value);
        }
    }
}
