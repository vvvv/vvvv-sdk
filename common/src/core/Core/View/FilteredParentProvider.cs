using System;
using System.Collections;
using VVVV.Core;
using VVVV.Core.Collections;
using VVVV.Core.Collections.Sync;

namespace VVVV.Core.View
{
    public class FilteredParentProvider : IParent, IDisposable
    {
        private readonly IDisposable FSyncer;
        
        public FilteredParentProvider(ModelMapper mapper)
        {
            var defaultParentProvider = new DefaultParentProvider(mapper);
            var children = defaultParentProvider.Childs;
            
            if (children != null)
            {
                var collection = new EditableCollection<object>();
                FSyncer = collection.SyncWith(children, CreateTargetItem, DestroyTargetItem, FilterPredicate);
            
                Childs = collection.AsViewableCollection();
            }
        }
        
        public IEnumerable Childs 
        {
            get;
            private set;
        }
        
        object CreateTargetItem(object sourceItem)
        {
            return sourceItem;
        }
        
        void DestroyTargetItem(object targetItem)
        {
            // Do nothing
        }
        
        bool FilterPredicate(object sourceItem)
        {
            return !(sourceItem is IViewableProperty);
        }
        
        public void Dispose()
        {
            if (FSyncer != null)
            {
                FSyncer.Dispose();
            }
        }
    }
}
