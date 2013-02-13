using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

using VVVV.Core.Collections;

namespace VVVV.Core
{
    public delegate void CollectionDelegate(IViewableCollection collection, object item);
    public delegate void CollectionUpdateDelegate(IViewableCollection collection);

    /// <summary>
    /// A IViewableCollection is a readonly collection of items.
    /// </summary>
    public interface IViewableCollection : IEnumerable//, INotifyCollectionChanged
    {
        int Count
        {
            get;
        }
        
        bool Contains(object item);

        event CollectionDelegate Added;
        event CollectionDelegate Removed;
        event CollectionUpdateDelegate Cleared;
        event CollectionUpdateDelegate UpdateBegun;
        event CollectionUpdateDelegate Updated;
    }
    
    public delegate void CollectionDelegate<T>(IViewableCollection<T> collection, T item);
    public delegate void CollectionUpdateDelegate<T>(IViewableCollection<T> collection);
    
    /// <summary>
    /// A IViewableCollection is a readonly collection of T.
    /// </summary>
    public interface IViewableCollection<T> : IEnumerable<T>, IViewableCollection//, INotifyCollectionChanged<T>
    {
        bool Contains(T item);
        
        new event CollectionDelegate<T> Added;
        new event CollectionDelegate<T> Removed;
        new event CollectionUpdateDelegate<T> Cleared;
    }


    public static class ViewableCollectionExtensions
    {
//        public static IViewableCollection<T> AsViewableCollection<T>(this ICollection<T> collection)
//        {
//            return new ViewableCollection<T>(collection);
//        }
    }
    
//    public class NotifyCollectionChangedEventArgs<T> : EventArgs
//    {
//        NotifyCollectionChangedAction Action
//        {
//            get;
//            private set;
//        }
//        
//        IEnumerable<T> NewItems
//        {
//            get;
//            private set;
//        }
//        
//        int NewStartingIndex
//        {
//            get;
//            private set;
//        }
//        
//        IEnumerable<T> OldItems
//        {
//            get;
//            private set;
//        }
//        
//        int OldStartingIndex
//        {
//            get;
//            private set;
//        }
//        
//        public NotifyCollectionChangedEventArgs()
//        {
//            Action = NotifyCollectionChangedAction.Reset;
//        }
//    }
//    
//    public delegate void NotifyCollectionChangedEventHandler<T>(object sender, NotifyCollectionChangedEventArgs<T> args);
    
//    public interface INotifyCollectionChanged<T>
//    {
//        event NotifyCollectionChangedEventHandler<T> CollectionChanged;
//    }
}
