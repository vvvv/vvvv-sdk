using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VVVV.Core.Collections.Sync
{
    public static class ImmutableSynchronizers
    {
        //public static IEnumerable<U> Sync<T, U>(EditableCollection<T> source, IEnumerable<U> old, Func<T, U> lookup, Func<T, U> creator) where T : IIDItem
        //{
        //    if (source.Changed)
        //    {
        //        foreach (var el in source)
        //            if (el.Changed)
        //                yield return creator(el);
        //            else
        //                yield return lookup(el);
        //    }
        //    else
        //        foreach (var ol in old)
        //            yield return ol;
        //}

        //public static IEnumerable<U> Sync<T, U>(EditableCollection<T> source, Func<T, U> lookup, Func<T, U> creator) where T : IIDItem
        //{
        //    if (source.Changed)
        //    {
        //        foreach (var el in source)
        //            if (el.Changed)
        //                yield return creator(el);
        //            else
        //                yield return lookup(el);
        //    }
        //    else
        //        foreach (var el in source)
        //            yield return lookup(el);
        //}

        //public static IEnumerable<U> Sync<T, U>(EditableCollection<T> source, Func<T, U> creator) where T : IIDItem
        //{
        //    if (source.Changed)
        //    {
        //        foreach (var el in source)
        //            if (el.Changed)
        //                yield return creator(el);
        //            else
        //                yield return (U)el.Symbol;
        //    }
        //    else
        //        foreach (var el in source)
        //            yield return (U)el.Symbol;
        //}

        private static IEnumerable<U> SyncCo<T, U>(IEnumerable<T> source, Func<T, U> creator, Func<T, U> lookup) 
            where T : IIDItem 
        {
            foreach (var el in source)
                if (el.Changed)
                    yield return creator(el);
                else
                    yield return lookup(el);
        }

        //public static IEnumerable<U> Sync<T, U>(IViewableIDList<T> source, IEnumerable<U> old, Func<T, U> creator, Func<T, U> lookup)
        //    where T : IIDItem
        //{
        //    if (source.Changed || (old==null))
        //        return SyncCo(source, creator, lookup);
        //    else
        //        return old;
        //}

        public static IEnumerable<U> Sync<T, U>(bool changed, IEnumerable<T> source, IEnumerable<U> old, Func<T, U> creator, Func<T, U> lookup)
            where T : IIDItem
        {
            if (changed || (old == null))
                return SyncCo(source, creator, lookup).ToArray();
            else
                return old;
        }
    }
}
