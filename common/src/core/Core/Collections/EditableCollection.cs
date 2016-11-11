using System;
using System.Collections;
using System.Collections.Generic;
//using VVVV.Core.Collections.Internal;

namespace VVVV.Core.Collections
{
    [Serializable]
    public class EditableCollection<T> : 
        IViewableCollection<T>, IEditableCollection<T>, 
        IViewableCollection, IEditableCollection,
        ICollection<T>,
        IDisposable
    {
        protected ICollection<T> FCollection;
        protected int FUpdating;
            
        public EditableCollection()
            : this(new System.Collections.Generic.List<T>())
        {
        }

        public EditableCollection(ICollection<T> collection)
        {
            FCollection = collection;
        }
        
        protected virtual void AddToInternalCollection(T item)
        {
            FCollection.Add(item);
        }
        
        protected virtual bool RemoveFromInternalCollection(T item)
        {
            return FCollection.Remove(item);
        }
        
        protected virtual void ClearInternalCollection()
        {
            FCollection.Clear();
        }

        protected virtual void OnUpdateBegun()
        {
        	if (UpdateBegun != null)
        		UpdateBegun(this);
        }
        
        protected virtual void OnUpdated()
        {
        	if (Updated != null)
        		Updated(this);
        }

        public bool Changed { get; private set; }

        public virtual void MarkChanged()
        {
            Changed = true;
        }

        public virtual void AcknowledgeChanges()
        {
            Changed = false;
        }

        #region IEditableCollection<T> Members
        
        public CollectionPredicate<T> AllowAdd;
        public CollectionPredicate<T> AllowRemove;
        
        public void Add(T item)
        {
            if (!CanAdd(item))
                throw new Exception(string.Format("Can't add {0} to collection {1}.", item, this));
            
            AddToInternalCollection(item);
            MarkChanged();
            OnAdded(item);
        }
        
        public bool Remove(T item)
        {
            if (!CanRemove(item))
                throw new Exception(string.Format("Can't remove {0} from collection {1}.", item, this));
            
            var removed = RemoveFromInternalCollection(item);
            if (removed)
            {
                MarkChanged();
                OnRemoved(item);
            }
            return removed;
        }
        
        public virtual bool CanAdd(T item)
        {
            if (AllowAdd != null)
                return AllowAdd(this, item);
            return true;
        }

        public virtual bool CanRemove(T item)
        {
            if (AllowRemove != null)
                return AllowRemove(this, item);
            return true;
        }
        
        #endregion
        
        #region IEditableCollection Members
        
        void IEditableCollection.Add(object item)
        {
            Add((T) item);
        }

        void IEditableCollection.Remove(object item)
        {
            Remove((T) item);
        }
        
        public void Clear()
        {
            BeginUpdate();
            
            foreach (var item in FCollection)
            {
                OnRemoved(item);
            }
            
            ClearInternalCollection();
            MarkChanged();
            OnCleared();
            
            EndUpdate();
        }

        bool IEditableCollection.CanAdd(object item)
        {
            return (item is T) && CanAdd((T) item);
        }

        bool IEditableCollection.CanRemove(object item)
        {
            return (item is T) && CanRemove((T) item);
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
        
        #endregion
        
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
            return FCollection.GetEnumerator();
        }
        
        public bool Contains(T item)
        {
            return FCollection.Contains(item);
        }
        
        public int Count
        {
            get 
            {
                return FCollection.Count;
            }
        }
        
        #endregion
         
        #region IViewableCollection Members
        
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        
        bool IViewableCollection.Contains(object item)
        {
            return Contains((T) item);
        }
        
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
        public event CollectionUpdateDelegate Updated;

        #endregion

        #region IDisposable Members
        
        public virtual void Dispose()
        {
        	Clear();
            FRemoved = null;
            FAdded = null;
            FCleared = null;
            Removed = null;
            Added = null;
            Cleared = null;
            UpdateBegun = null;
            Updated = null;
        }
        
        #endregion

        #region ICollection<T>

        public bool IsReadOnly
        {
            get
            {
                return FCollection.IsReadOnly;
            }
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            FCollection.CopyTo(array, arrayIndex);
        }

        #endregion
        
        public IViewableCollection<T> AsViewableCollection()
        {
            return new ViewableCollection<T>(this as IEditableCollection<T>);
        }

    }
        
    public static class EditableCollectionExtensionMethods
    {
    	public static void AddRange<T>(this IEditableCollection<T> collection, IEnumerable<T> items)
    	{
    	    collection.BeginUpdate();
    		try 
    		{
    		    foreach (var item in items)
        			collection.Add(item);
    		} 
    		finally 
    		{
    		    collection.EndUpdate();
    		}
    	}
    	
    	public static void AddRange<T>(this IEditableCollection<T> collection, IEnumerable items)
    	{
    	    collection.BeginUpdate();
    		try 
    		{
    		    foreach (T item in items)
        			collection.Add(item);
    		} 
    		finally 
    		{
    		    collection.EndUpdate();
    		}
    	}
    	
    	public static void RemoveRange<T>(this IEditableCollection<T> collection, IEnumerable<T> items)
    	{
    	    collection.BeginUpdate();
    		try 
    		{
    		    foreach (var item in items)
        			collection.Remove(item);
    		} 
    		finally 
    		{
    		    collection.EndUpdate();
    		}
    	}
    	
    	public static void RemoveRange<T>(this IEditableCollection<T> collection, IEnumerable items)
    	{
    	    collection.BeginUpdate();
    		try 
    		{
    		    foreach (T item in items)
        			collection.Remove(item);
    		} 
    		finally 
    		{
    		    collection.EndUpdate();
    		}
    	}
    }
}
