using System;
using System.Collections.Generic;

namespace VVVV.Core.Collections.Sync
{
    internal class SourceChangedEventArgs<T> : EventArgs
    {
        public T Item
        {
            get;
            private set;
        }
        
        public CollectionAction Action
        {
            get;
            private set;
        }

        public Source<T> SourceCollection 
        { 
            get; 
            private set; 
        }

        public SourceChangedEventArgs(CollectionAction action, Source<T> sourceCollection, T item)
        {
            Action = action;
            SourceCollection = sourceCollection;
            Item = item;
        }

        public SourceChangedEventArgs(CollectionAction action, Source<T> sourceCollection)
            : this(action, sourceCollection, default(T))
        {
        }
    }
}
