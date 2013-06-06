using System;
using System.Collections.Generic;

namespace VVVV.Core.Collections.Sync
{
    public class SyncEventArgs<Ta, Tb> : EventArgs
    {
        public Ta TargetItem
        {
            get;
            protected set;
        }
        
        public Tb SourceItem
        {
            get;
            protected set;
        }
        
        public CollectionAction Action
        {
            get;
            protected set;
        }

        public IEnumerable<Ta> TargetItems { get; protected set; }
        public IEnumerable<Tb> SourceItems { get; protected set; }

        public SyncEventArgs(Ta targetItem, Tb sourceItem, CollectionAction action, IEnumerable<Ta> targetItems, IEnumerable<Tb> sourceItems)
        {
            TargetItem = targetItem;
            SourceItem = sourceItem;
            Action = action;
            TargetItems = targetItems;
            SourceItems = sourceItems;
        }

        public SyncEventArgs(CollectionAction action, IEnumerable<Ta> targetItems, IEnumerable<Tb> sourceItems)
            : this(default(Ta), default(Tb), action, targetItems, sourceItems)
        {
            
        }
    }
}
