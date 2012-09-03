using System;
using System.Collections;
using System.Collections.Generic;
//using VVVV.Core.Collections.Internal;

namespace VVVV.Core.Collections
{
    /// <summary>
    /// A ViewableCollection is a readonly collection of T.
    /// </summary>    
    [Serializable]
    public class ViewableCollection<T> : IViewableCollection<T>, IViewableCollection, IDisposable
    {
        private readonly IEditableCollection<T> FInternalCollection;
        private readonly bool FOwnerOfInternalCollection;
        private int FUpdating;
        
        public ViewableCollection()
            : this((IEditableCollection<T>)(new EditableCollection<T>()))
        {
            FOwnerOfInternalCollection = true;
        }
        
        public ViewableCollection(System.Collections.Generic.ICollection<T> collection)
            : this((IEditableCollection<T>)(new EditableCollection<T>(collection)))
        {
            FOwnerOfInternalCollection = true;
        }
        
        public ViewableCollection(IEditableCollection<T> collection)
        {
            FInternalCollection = collection;

            FInternalCollection.Added += InternalCollection_Added;
            FInternalCollection.Removed += InternalCollection_Removed;
            FInternalCollection.Cleared += InternalCollection_Cleared;
            FInternalCollection.UpdateBegun += InternalCollection_UpdateBegun;
            FInternalCollection.Updated += InternalCollection_Updated;
        }

        void InternalCollection_Added(IViewableCollection<T> collection, T item)
        {
            OnAdded(item);
        }

        void InternalCollection_Removed(IViewableCollection<T> collection, T item)
        {
            OnRemoved(item);
        }
        
        void InternalCollection_Cleared(IViewableCollection<T> collection)
        {
            OnCleared();
        }

        void InternalCollection_UpdateBegun(IViewableCollection collection)
        {
            if (UpdateBegun != null)
                UpdateBegun(this);
        }

        void InternalCollection_Updated(IViewableCollection collection)
        {
            if (Updated != null)
                Updated(this);
        }

//        public bool OwnsItems
//        {
//            get
//            {
//                return InternalCollection.OwnsItems;
//            }
//            set
//            {
//                InternalCollection.OwnsItems = value;
//            }
//        }
        
        public virtual void Add(T item)
        {
            FInternalCollection.Add(item);
        }
        
        public virtual bool Remove(T item)
        {
            return FInternalCollection.Remove(item);
        }
        
        public void Clear()
        {
            FInternalCollection.Clear();
        }
        
        public void BeginUpdate()
        {
        	if (FUpdating == 0) 
        		OnUpdateBegun();
        	FUpdating++;
        }

        public void EndUpdate()
        {
        	FUpdating--;
        	if (FUpdating == 0) 
        		OnUpdated();
        }
        
        #region IViewableCollection<T> Members
        
        public event CollectionDelegate<T> Added;

        protected virtual void OnAdded(T item)
        {
            if (Added != null)
                Added(this, item);
            if (FAdded != null)
                FAdded(this, item);
        }

        public event CollectionDelegate<T> Removed;
        
        protected virtual void OnRemoved(T item)
        {
            if (Removed != null)
                Removed(this, item);
            if (FRemoved != null)
                FRemoved(this, item);
        }
        
        public event CollectionUpdateDelegate<T> Cleared;
        
        protected virtual void OnCleared()
        {
            if (Cleared != null)
                Cleared(this);
            if (FCleared != null)
                FCleared(this);
        }
        
        public virtual IEnumerator<T> GetEnumerator()
        {
            return FInternalCollection.GetEnumerator();
        }
        
        public virtual int Count 
        {
            get 
            {
                return FInternalCollection.Count;
            }
        }
        
        public virtual bool Contains(T item)
        {
            return FInternalCollection.Contains(item);
        }
        
        #endregion
        
        #region IViewableCollection Members
        
        protected CollectionDelegate FAdded;
        event CollectionDelegate IViewableCollection.Added
        {
            add
            {
                FAdded += value;
            }
            remove
            {
                FAdded -= value;
            }
        }

        protected CollectionDelegate FRemoved;
        event CollectionDelegate IViewableCollection.Removed
        {
            add
            {
                FRemoved += value;
            }
            remove
            {
                FRemoved -= value;
            }
        }

        protected CollectionUpdateDelegate FCleared;
        event CollectionUpdateDelegate IViewableCollection.Cleared
        {
            add
            {
                FCleared += value;
            }
            remove
            {
                FCleared -= value;
            }
        }

        public event CollectionUpdateDelegate UpdateBegun;
        
        protected virtual void OnUpdateBegun()
        {
            if (UpdateBegun != null) {
                UpdateBegun(this);
            }
        }
        
        public event CollectionUpdateDelegate Updated;
        
        protected virtual void OnUpdated()
        {
            if (Updated != null) {
                Updated(this);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) FInternalCollection).GetEnumerator();
        }
        
        bool IViewableCollection.Contains(object item)
        {
            return ((IViewableCollection) FInternalCollection).Contains(item);
        }
        
        #endregion
        
        #region IDisposable Members
        
        public virtual void Dispose()
        {
            FInternalCollection.Added -= InternalCollection_Added;
            FInternalCollection.Removed -= InternalCollection_Removed;
            FInternalCollection.Cleared -= InternalCollection_Cleared;
            FInternalCollection.UpdateBegun -= InternalCollection_UpdateBegun;
            FInternalCollection.Updated -= InternalCollection_Updated;
            
            if (FOwnerOfInternalCollection)
                (FInternalCollection as EditableCollection<T>).Dispose();
            else
            	OnCleared();
        }
        
        #endregion

    }
    
    public static class ViewableCollectionExtensionMethods
    {
    	public static void AddRange<T>(this ViewableCollection<T> collection, IEnumerable<T> items)
    	{
    		foreach (var item in items)
    			collection.Add(item);
    	}
    	
    	public static void AddRange<T>(this ViewableCollection<T> collection, IEnumerable items)
    	{
    		foreach (T item in items)
    			collection.Add(item);
    	}
    	
    	public static void RemoveRange<T>(this ViewableCollection<T> collection, IEnumerable<T> items)
    	{
    		foreach (var item in items)
    			collection.Remove(item);
    	}
    	
    	public static void RemoveRange<T>(this ViewableCollection<T> collection, IEnumerable items)
    	{
    		foreach (T item in items)
    			collection.Remove(item);
    	}
    }
}
