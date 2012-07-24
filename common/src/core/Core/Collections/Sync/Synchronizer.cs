using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using VVVV.Utils;

namespace VVVV.Core.Collections.Sync
{
    public delegate void SyncEventHandler<Ta, Tb>(object sender, SyncEventArgs<Ta, Tb> args);
    
    internal delegate void EventHandler<TSender>(TSender sender);
    internal delegate void EventHandler<TSender, TItem>(TSender sender, TItem item);
    
    public class Synchronizer<Ta, Tb> : IDisposable
    {
        private readonly Target<Ta> FTarget;
        private readonly Source<Tb> FSource;
        private readonly Func<Tb, Ta> FCreateTargetItem;
        private readonly Action<Ta> FDestroyTargetItem;
        private readonly Predicate<Tb> FFilterPredicate;
        private readonly Dictionary<Tb, List<Ta>> FBToAMap;
        private readonly List<Action> FStashedActions;
        
        private bool FUpdating;
        
        internal Synchronizer(Target<Ta> target, Source<Tb> source, Func<Tb, Ta> createTargetItem, Action<Ta> destroyTargetItem, Predicate<Tb> filterPredicate, SyncEventHandler<Ta, Tb> syncEventHandler = null)
        {
            FTarget = target;
            FSource = source;
            FCreateTargetItem = createTargetItem;
            FDestroyTargetItem = destroyTargetItem;
            FFilterPredicate = filterPredicate;
            FBToAMap = new Dictionary<Tb, List<Ta>>();
            FStashedActions = new List<Action>();
            if (syncEventHandler != null)
                Synced += syncEventHandler;
            
            Reload();
            
            FSource.SourceChanged += HandleSourceChanged;
        }

        public void Dispose()
        {
            Synced = null;
            
            FSource.SourceChanged -= HandleSourceChanged;
            
            FSource.Dispose();
            
            foreach (var item in FTarget)
            {
                FDestroyTargetItem(item);
            }
            
            FTarget.Clear();
        }
        
        void HandleSourceChanged(object sender, SourceChangedEventArgs<Tb> args)
        {
            switch (args.Action)
            {
                case CollectionAction.Added:
                    HandleAdded(args.SourceCollection, args.Item);
                    break;
                case CollectionAction.Removed:
                    HandleRemoved(args.SourceCollection, args.Item);
                    break;
                case CollectionAction.Cleared:
                    HandleCleared(args.SourceCollection);
                    break;
                case CollectionAction.OrderChanged:
                    HandleOrderChanged(args.SourceCollection);
                    break;
                case CollectionAction.Updating:
                    HandleUpdating(args.SourceCollection);
                    break;
                case CollectionAction.Updated:
                    HandleUpdated(args.SourceCollection);
                    break;
                default:
                    throw new Exception("Invalid value for CollectionAction");
            }
        }
        
        void HandleAdded(Source<Tb> source, Tb item)
        {
            if (FFilterPredicate(item))
            {
                if (FUpdating)
                {
                    FStashedActions.Add(() => DoAdd(item));
                }
                else
                {
                    DoAdd(item);
                }
            }
        }
        
        void DoAdd(Tb item)
        {
            var targetItem = FCreateTargetItem(item);
            FTarget.Add(targetItem);
            
            if (!FBToAMap.ContainsKey(item))
            {
                FBToAMap[item] = new List<Ta>();
            }
            FBToAMap[item].Add(targetItem);
            
            OnSynced(new SyncEventArgs<Ta, Tb>(targetItem, item, CollectionAction.Added, FTarget, FSource));
        }
        
        void HandleRemoved(Source<Tb> source, Tb item)
        {
            if (FFilterPredicate(item))
            {
                if (FUpdating)
                {
                    FStashedActions.Add(() => DoRemove(item));
                }
                else
                {
                    DoRemove(item);
                }
            }
        }
        
        void DoRemove(Tb item)
        {
            var targetItems = FBToAMap[item];
            var targetItem = targetItems[0];

            FTarget.Remove(targetItem);
            
            targetItems.RemoveAt(0);
            if (targetItems.Count == 0)
            {
                FBToAMap.Remove(item);
            }
            
            OnSynced(new SyncEventArgs<Ta, Tb>(targetItem, item, CollectionAction.Removed, FTarget, FSource));
            FDestroyTargetItem(targetItem);
        }
        
        void HandleCleared(Source<Tb> source)
        {
            if (FUpdating)
            {
                FStashedActions.Clear();
                FStashedActions.Add(DoClear);
            }
            else
            {
                DoClear();
            }
        }
        
        void DoClear()
        {
            foreach (var targetItem in FTarget)
            {
                FDestroyTargetItem(targetItem);
            }
            
            FBToAMap.Clear();
            FTarget.Clear();
            
            OnSynced(new SyncEventArgs<Ta, Tb>(CollectionAction.Cleared, FTarget, FSource));
        }
        
        void HandleOrderChanged(Source<Tb> source)
        {
            if (FUpdating)
            {
                FStashedActions.Add(UpdateOrder);
            }
            else
            {
                UpdateOrder();
            }
        }
        
        void HandleUpdating(Source<Tb> source)
        {
            try
            {
                FTarget.BeforeBatchUpdate();
            }
            finally
            {
                FUpdating = true;
                OnSynced(new SyncEventArgs<Ta, Tb>(CollectionAction.Updating, FTarget, FSource));
            }
        }
        
        void HandleUpdated(Source<Tb> source)
        {
            FUpdating = false;
            
            try
            {
                // Execute all stashed actions
                foreach (var action in FStashedActions)
                {
                    action();
                }
                
                // Tell target about this batch update.
                FTarget.AfterBatchUpdate();
            }
            finally
            {
                FStashedActions.Clear();
                OnSynced(new SyncEventArgs<Ta, Tb>(CollectionAction.Updated, FTarget, FSource));
            }
        }
        
        protected void UpdateOrder()
        {
            FTarget.BeforeBatchUpdate();
            try
            {
                FTarget.Clear();
                
                var index = new Dictionary<Tb, int>();
                foreach (var itemB in FSource)
                {
                    index[itemB] = 0;
                }
                
                foreach (var itemB in FSource)
                {
                    List<Ta> bToAList = null;
                    if (FBToAMap.TryGetValue(itemB, out bToAList))
                    {
                        var itemA = bToAList[index[itemB]];
                        FTarget.Add(itemA);
                        index[itemB]++;
                    }
                }
            }
            finally
            {
                FTarget.AfterBatchUpdate();
                
                OnSynced(new SyncEventArgs<Ta, Tb>(CollectionAction.OrderChanged, FTarget, FSource));
            }
        }
        
        protected void Reload()
        {
            FTarget.BeforeBatchUpdate();
            try
            {
                foreach (var item in FTarget)
                {
                    FDestroyTargetItem(item);
                }
                
                FTarget.Clear();
                FBToAMap.Clear();
                
                foreach (var item in FSource)
                {
                    HandleAdded(FSource, item);
                }
            }
            finally
            {
                FTarget.AfterBatchUpdate();
                
                OnSynced(new SyncEventArgs<Ta, Tb>(CollectionAction.OrderChanged, FTarget, FSource));
            }
        }
        
        public event SyncEventHandler<Ta, Tb> Synced;
        
        protected virtual void OnSynced(SyncEventArgs<Ta, Tb> args)
        {
            if (Synced != null)
            {
                Synced(this, args);
            }
        }
    }
    
    /// <summary>
    /// Adds SyncWith extension methods to various collection types.
    /// </summary>
    public static class SyncExtensions
    {
        static void RemoveAndDispose(object item)
        {
            if (item is IDisposable)
                (item as IDisposable).Dispose();
        }

        static void RemoveAndDispose<Ta>(Ta item)
        {
            if (item is IDisposable)
                (item as IDisposable).Dispose();
        }
        
        static bool DefaultFilterPredicate<Tb>(Tb b)
        {
            return true;
        }
        
        static Source<T> CreateGenericSource<T>(IEnumerable<T> enumerable)
        {
            if (enumerable is IViewableList<T>)
            {
                return new ViewableListSource<T>(enumerable as IViewableList<T>);
            }
            else if (enumerable is IViewableCollection<T>)
            {
                return new ViewableCollectionSource<T>(enumerable as IViewableCollection<T>);
            }
            else
            {
                return new EnumerableSource<T>(enumerable);
            }
        }
        
        static Source<object> CreateNonGenericSource(IEnumerable enumerable)
        {
            if (enumerable is IViewableList)
            {
                return new ViewableListSource(enumerable as IViewableList);
            }
            else if (enumerable is IViewableCollection)
            {
                return new ViewableCollectionSource(enumerable as IViewableCollection);
            }
            else
            {
                return new EnumerableSource(enumerable);
            }
        }

        // Add overloads as needed.
        // Generic sources
        public static Synchronizer<Ta, Tb> SyncWith<Ta, Tb>(this IEditableCollection<Ta> collectionA, IEnumerable<Tb> collectionB, Func<Tb, Ta> createA, Action<Ta> destroyA = null, Predicate<Tb> filterPredicate = null, SyncEventHandler<Ta, Tb> syncEventHandler = null)
        {
            var target = new EditableCollectionTarget<Ta>(collectionA);
            var source = CreateGenericSource(collectionB);
            destroyA = destroyA ?? RemoveAndDispose;
            filterPredicate = filterPredicate ?? DefaultFilterPredicate;
            return new Synchronizer<Ta, Tb>(target, source, createA, destroyA, filterPredicate, syncEventHandler);
        }
        
        public static Synchronizer<Ta, Tb> SyncWith<Ta, Tb>(this ViewableCollection<Ta> collectionA, IEnumerable<Tb> collectionB, Func<Tb, Ta> createA, Action<Ta> destroyA = null, Predicate<Tb> filterPredicate = null, SyncEventHandler<Ta, Tb> syncEventHandler = null)
        {
            var target = new ViewableCollectionTarget<Ta>(collectionA);
            var source = CreateGenericSource(collectionB);
            destroyA = destroyA ?? RemoveAndDispose;
            filterPredicate = filterPredicate ?? DefaultFilterPredicate;
            return new Synchronizer<Ta, Tb>(target, source, createA, destroyA, filterPredicate, syncEventHandler);
        }

        public static Synchronizer<object, Tb> SyncWith<Tb>(this IList listA, IEnumerable<Tb> collectionB, Func<Tb, object> createA, Action<object> destroyA = null, Predicate<Tb> filterPredicate = null, SyncEventHandler<object, Tb> syncEventHandler = null)
        {
            var target = new NonGenericListTarget(listA);
            var source = CreateGenericSource(collectionB);
            destroyA = destroyA ?? RemoveAndDispose;
            filterPredicate = filterPredicate ?? DefaultFilterPredicate;
            return new Synchronizer<object, Tb>(target, source, createA, destroyA, filterPredicate, syncEventHandler);
        }
        
        // Non generic sources
        public static Synchronizer<Ta, object> SyncWith<Ta>(this IEditableCollection<Ta> collectionA, IEnumerable collectionB, Func<object, Ta> createA, Action<Ta> destroyA = null, Predicate<object> filterPredicate = null, SyncEventHandler<Ta, object> syncEventHandler = null)
        {
            var target = new EditableCollectionTarget<Ta>(collectionA);
            var source = CreateNonGenericSource(collectionB);
            destroyA = destroyA ?? RemoveAndDispose;
            filterPredicate = filterPredicate ?? DefaultFilterPredicate;
            return new Synchronizer<Ta, object>(target, source, createA, destroyA, filterPredicate, syncEventHandler);
        }
        
        public static Synchronizer<object, object> SyncWith(this IList listA, IEnumerable collectionB, Func<object, object> createA, Action<object> destroyA = null, Predicate<object> filterPredicate = null, SyncEventHandler<object, object> syncEventHandler = null)
        {
            var target = new NonGenericListTarget(listA);
            var source = CreateNonGenericSource(collectionB);
            destroyA = destroyA ?? RemoveAndDispose;
            filterPredicate = filterPredicate ?? DefaultFilterPredicate;
            return new Synchronizer<object, object>(target, source, createA, destroyA, filterPredicate, syncEventHandler);
        }
    }
}
