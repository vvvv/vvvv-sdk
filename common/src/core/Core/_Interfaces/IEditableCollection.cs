using System;
using System.Collections.Generic;
using VVVV.Core.Collections;

namespace VVVV.Core
{
    public delegate bool CollectionPredicate(IEditableCollection collection, object item);
    
    /// <summary>
    /// An IEditableCollection is an editable collection of items.
    /// </summary>
    public interface IEditableCollection : IViewableCollection
    {
        void Add(object item);
        void Remove(object item);
        bool CanAdd(object item);
        bool CanRemove(object item);
        void Clear();
        void BeginUpdate();
        void EndUpdate();
    }

    public delegate bool CollectionPredicate<T>(IEditableCollection<T> collection, T item);

    /// <summary>
    /// An IEditableCollection is an editable collection of T.
    /// </summary>
    public interface IEditableCollection<T> : IViewableCollection<T>, IEditableCollection
    {
        void Add(T item);
        bool Remove(T item);
        bool CanAdd(T item);
        bool CanRemove(T item);
    }


    public static class EditableCollectionExtensions
    {
        public static IEditableCollection<T> AsEditableCollection<T>(this ICollection<T> collection)
        {
            return new EditableCollection<T>(collection);
        }
        
        public static T[] ToArray<T>(this IEditableCollection<T> collection)
        {
            var arr = new T[collection.Count];
            
            int i = 0;
            foreach (var item in collection)
                arr[i++] = item;
            
            return arr;
        }
    }

}
