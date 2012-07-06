using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VVVV.Core.Collections.Sync
{
    internal abstract class Target<T> : IEnumerable<T>
    {
        public abstract void Add(T item);
        public abstract void Remove(T item);
        public abstract void Clear();
        public abstract IEnumerator<T> GetEnumerator();
        
        public virtual void BeforeBatchUpdate()
        {
            
        }
        
        public virtual void AfterBatchUpdate()
        {
            
        }
        
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
    
    internal class EditableCollectionTarget<T> : Target<T>
    {
        private readonly IEditableCollection<T> FCollection;
        
        public EditableCollectionTarget(IEditableCollection<T> collection)
        {
            FCollection = collection;
        }
        
        public override void Add(T item)
        {
            FCollection.Add(item);
        }
        
        public override void Remove(T item)
        {
            FCollection.Remove(item);
        }
        
        public override IEnumerator<T> GetEnumerator()
        {
            return FCollection.GetEnumerator();
        }
        
        public override void Clear()
        {
            FCollection.Clear();
        }
        
        public override void BeforeBatchUpdate()
        {
            FCollection.BeginUpdate();
        }
        
        public override void AfterBatchUpdate()
        {
            FCollection.EndUpdate();
        }
    }
    
    internal class ViewableCollectionTarget<T> : Target<T>
    {
        private readonly ViewableCollection<T> FCollection;
        
        public ViewableCollectionTarget(ViewableCollection<T> collection)
        {
            FCollection = collection;
        }
        
        public override void Add(T item)
        {
            FCollection.Add(item);
        }
        
        public override void Remove(T item)
        {
            FCollection.Remove(item);
        }
        
        public override IEnumerator<T> GetEnumerator()
        {
            return FCollection.GetEnumerator();
        }
        
        public override void Clear()
        {
            FCollection.Clear();
        }
        
        public override void BeforeBatchUpdate()
        {
            FCollection.BeginUpdate();
        }
        
        public override void AfterBatchUpdate()
        {
            FCollection.EndUpdate();
        }
    }
    
    internal class NonGenericListTarget : Target<object>
    {
        private readonly IList FList;
        
        public NonGenericListTarget(IList list)
        {
            FList = list;
        }
        
        public override void Add(object item)
        {
            FList.Add(item);
        }
        
        public override void Remove(object item)
        {
            FList.Remove(item);
        }
        
        public override void Clear()
        {
            FList.Clear();
        }
        
        public override IEnumerator<object> GetEnumerator()
        {
            foreach (object item in FList)
            {
                yield return item;
            }
        }
    }
    
    internal class GenericCollectionTarget<T> : Target<T>
    {
        private readonly ICollection<T> FCollection;
        
        public GenericCollectionTarget(ICollection<T> collection)
        {
            FCollection = collection;
        }
        
        public override void Add(T item)
        {
            FCollection.Add(item);
        }
        
        public override void Remove(T item)
        {
            FCollection.Remove(item);
        }
        
        public override void Clear()
        {
            FCollection.Clear();
        }
        
        public override IEnumerator<T> GetEnumerator()
        {
            return FCollection.GetEnumerator();
        }
    }
}
