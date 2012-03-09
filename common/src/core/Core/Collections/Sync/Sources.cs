using System;
using System.Collections;
using System.Collections.Generic;

namespace VVVV.Core.Collections.Sync
{
    internal delegate void SourceChangedEventHandler<T>(object sender, SourceChangedEventArgs<T> args);
    
    internal abstract class Source<T> : IEnumerable<T>, IDisposable
    {
        public event SourceChangedEventHandler<T> SourceChanged;
        
        protected virtual void OnSourceChanged(SourceChangedEventArgs<T> args)
        {
            if (SourceChanged != null) 
            {
                SourceChanged(this, args);
            }
        }
        
        public abstract IEnumerator<T> GetEnumerator();
        
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        
        public abstract void Dispose();
    }
    
    
    
    internal class ViewableCollectionSource<T> : Source<T>
    {
        private readonly IViewableCollection<T> FCollection;
        
        internal ViewableCollectionSource(IViewableCollection<T> collection)
        {
            FCollection = collection;
            
            FCollection.Added += HandleAdded;
            FCollection.Removed += HandleRemoved;
            FCollection.Cleared += HandleCleared;
            FCollection.UpdateBegun += HandleUpdateBegun;
            FCollection.Updated += HandleUpdated;
        }

        public override void Dispose()
        {
            FCollection.Added -= HandleAdded;
            FCollection.Removed -= HandleRemoved;
            FCollection.Cleared -= HandleCleared;
            FCollection.UpdateBegun -= HandleUpdateBegun;
            FCollection.Updated -= HandleUpdated;
        }
        
        public override IEnumerator<T> GetEnumerator()
        {
            return FCollection.GetEnumerator();
        }
        
        void HandleAdded(IViewableCollection<T> collection, T item)
        {
            OnSourceChanged(new SourceChangedEventArgs<T>(CollectionAction.Added, this, item));
        }
        
        void HandleRemoved(IViewableCollection<T> collection, T item)
        {
            OnSourceChanged(new SourceChangedEventArgs<T>(CollectionAction.Removed, this, item));
        }
        
        void HandleCleared(IViewableCollection<T> collection)
        {
            OnSourceChanged(new SourceChangedEventArgs<T>(CollectionAction.Cleared, this));
        }
        
        void HandleUpdateBegun(IViewableCollection collection)
        {
            OnSourceChanged(new SourceChangedEventArgs<T>(CollectionAction.Updating, this));
        }
        
        void HandleUpdated(IViewableCollection collection)
        {
            OnSourceChanged(new SourceChangedEventArgs<T>(CollectionAction.Updated, this));
        }
    }
    
    internal class ViewableCollectionSource : Source<object>
    {
        private readonly IViewableCollection FCollection;
        
        internal ViewableCollectionSource(IViewableCollection collection)
        {
            FCollection = collection;
            
            FCollection.Added += HandleAdded;
            FCollection.Removed += HandleRemoved;
            FCollection.Cleared += HandleCleared;
            FCollection.UpdateBegun += HandleUpdateBegun;
            FCollection.Updated += HandleUpdated;
        }
        
        public override void Dispose()
        {
            FCollection.Added -= HandleAdded;
            FCollection.Removed -= HandleRemoved;
            FCollection.Cleared -= HandleCleared;
            FCollection.UpdateBegun -= HandleUpdateBegun;
            FCollection.Updated -= HandleUpdated;
        }
        
        public override IEnumerator<object> GetEnumerator()
        {
            foreach (object item in FCollection)
            {
                yield return item;
            }
        }
        
        void HandleAdded(IViewableCollection collection, object item)
        {
            OnSourceChanged(new SourceChangedEventArgs<object>(CollectionAction.Added, this, item));
        }
        
        void HandleRemoved(IViewableCollection collection, object item)
        {
            OnSourceChanged(new SourceChangedEventArgs<object>(CollectionAction.Removed, this, item));
        }
        
        void HandleCleared(IViewableCollection collection)
        {
            OnSourceChanged(new SourceChangedEventArgs<object>(CollectionAction.Cleared, this));
        }
        
        void HandleUpdateBegun(IViewableCollection collection)
        {
            OnSourceChanged(new SourceChangedEventArgs<object>(CollectionAction.Updating, this));
        }
        
        void HandleUpdated(IViewableCollection collection)
        {
            OnSourceChanged(new SourceChangedEventArgs<object>(CollectionAction.Updated, this));
        }
    }
    
    internal class EnumerableSource<T> : Source<T>
    {
        private readonly IEnumerable<T> FEnumerable;
        
        internal EnumerableSource(IEnumerable<T> enumerable)
        {
            FEnumerable = enumerable;
        }
        
        public override void Dispose()
        {
        }
        
        public override IEnumerator<T> GetEnumerator()
        {
            return FEnumerable.GetEnumerator();
        }
    }
    
    internal class EnumerableSource : Source<object>
    {
        private readonly IEnumerable FEnumerable;
        
        internal EnumerableSource(IEnumerable enumerable)
        {
            FEnumerable = enumerable;
        }
        
        public override void Dispose()
        {
        }
        
        public override IEnumerator<object> GetEnumerator()
        {
            foreach (object item in FEnumerable)
            {
                yield return item;
            }
        }
    }
    
    internal class ViewableListSource<T> : ViewableCollectionSource<T>
    {
        private readonly IViewableList<T> FList;
        
        internal ViewableListSource(IViewableList<T> list)
            : base(list)
        {
            FList = list;
            
            FList.OrderChanged += HandleOrderChanged;
        }
        
        public override void Dispose()
        {
            FList.OrderChanged -= HandleOrderChanged;
            
            base.Dispose();
        }
        
        void HandleOrderChanged(IViewableList<T> list)
        {
            OnSourceChanged(new SourceChangedEventArgs<T>(CollectionAction.OrderChanged, this));
        }
    }
    
    internal class ViewableListSource : ViewableCollectionSource
    {
        private readonly IViewableList FList;
        
        internal ViewableListSource(IViewableList list)
            : base(list)
        {
            FList = list;
            
            FList.OrderChanged += HandleOrderChanged;
        }
        
        public override void Dispose()
        {
            FList.OrderChanged -= HandleOrderChanged;
            
            base.Dispose();
        }
        
        void HandleOrderChanged(IViewableList list)
        {
            OnSourceChanged(new SourceChangedEventArgs<object>(CollectionAction.OrderChanged, this));
        }
    }
}
