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

        public static void AddRange<T>(this IList<T> list, IEnumerable<T> items)
        {
            foreach (var item in items)
                list.Add(item);
        }

        public static void AssignFrom<T>(this IList<T> list, IEnumerable<T> items)
        {
            list.Clear();
            list.AddRange(items);
        }

        public static void Sort<T>(this IList<T> list, Comparison<T> comparison)
        {
            var mutableList = list as List<T>;
            if (mutableList != null)
            {
                mutableList.Sort(comparison);
                return;
            }
            throw new NotSupportedException();
        }

        public static void Sort<T>(this IList<T> list, IComparer<T> comparer)
        {
            var mutableList = list as List<T>;
            if (mutableList != null)
            {
                mutableList.Sort(comparer);
                return;
            }
            throw new NotSupportedException();
        }
    }
}
