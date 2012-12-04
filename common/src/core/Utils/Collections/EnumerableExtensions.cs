using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace VVVV.Utils.Collections
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> OfType<T, U>(this IEnumerable<U> enumerable) where T : class
        {
            return enumerable.Where(e => e is T).Select(e => e as T);
        }

        //public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        //{
        //    foreach (var x in enumerable)
        //        action(x);
        //}

        public static bool All(this IEnumerable<bool> enumerable)
        {
            return enumerable.All(booleanValue => booleanValue);
        }
        
        public static ReadOnlyCollection<T> ToReadOnlyCollection<T>(this IEnumerable<T> enumerable)
        {
            return new ReadOnlyCollection<T>(enumerable.ToList());
        }        

        public static IEnumerable<T> Append<T>(this IEnumerable<T> seq, T item)
        {
            return seq.Concat(new T[] { item });
        } 	

    }
}
