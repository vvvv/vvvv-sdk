using System;
using System.Linq;
using System.Text;

using VVVV.Core;
using VVVV.Core.Model;
using System.Collections.Generic;

namespace VVVV.Core.Collections
{
    // items named
    // therefore lookup works through [string name]
    // since it is an editablecollection it provides easy acces to
    // IViewableCollection and IEditableCollection

    [Serializable]
    public class EditableIDList<T> : EditableList<T>,
    IViewableIDList<T>, IEditableIDList<T>,
    IViewableIDList, IEditableIDList where T: IIDItem
    {
        protected KeyedIDCollection<T> FItems;

        private bool FOwnsItems;
        public bool OwnsItems
        {
            get
            {
                return FOwnsItems;
            }
            set
            {
                if (Count == 0)
                    FOwnsItems = value;
                else
                    throw new Exception("OwnsItems property should be set on startup");
            }
        }

        public bool AllowRenameOnAdd { get; set; }

        public ModelMapper Mapper
        {
            get;
            private set;
        }

        public EditableIDList(KeyedIDCollection<T> collection, string name)
            : base(collection)
        {
            Name = name;
            FItems = collection;
            OwnsItems = true;
        }

        public EditableIDList(KeyedIDCollection<T> collection, string name, bool allowRenameOnAdd)
            : this(collection, name)
        {
            AllowRenameOnAdd = allowRenameOnAdd;
        }

        public EditableIDList(string name)
            : this(new KeyedIDCollection<T>(), name)
        {
        }
        public EditableIDList(string name, bool allowRenameOnAdd)
            : this(new KeyedIDCollection<T>(), name, allowRenameOnAdd)
        {
        }

        protected EditableIDList(IList<T> list, KeyedIDCollection<T> collection, string name, bool allowRenameOnAdd)
            : base(list)
        {
            Name = name;
            FItems = collection;
            OwnsItems = true;
            AllowRenameOnAdd = allowRenameOnAdd;
        }
        
        public override void Dispose()
        {
            if (FOwner != null)
            {
                FOwner.RootingChanged -= FOwner_RootingChanged;
            }

            ItemRenamed = null;
            Renamed = null;
            base.Dispose();
        }

        public bool Contains(string name)
        {
            return FItems.Contains(name);
        }

        public ViewableIDList<T> AsViewableIDList()
        {
            return new ViewableIDList<T>(this);
        }
        
        protected override void AddToInternalCollection(T item)
        {
            if (Contains(item))
                if (AllowRenameOnAdd && (item is IRenameable) && (!item.Equals(this[item.Name])))
                    (item as IRenameable).Name = GetNewQualifyingName(item.Name);
                else
                    throw new Exception("Item can't be added since there is already an item with that name in the list. Either it is not renameable, or rename on add is not allowed by the list, or that item exists already in the list.");

            base.AddToInternalCollection(item);
            if (FItems != FList)
                FItems.Add(item);
            if (OwnsItems)
                item.Owner = this;
            item.Renamed += item_Renamed;
        }

        private string GetNewQualifyingName(string name)
        {
            var x = 2;

            while (Contains(name + x.ToString()))
                x++;

            return name + x.ToString();
        }
        
        protected override bool RemoveFromInternalCollection(T item)
        {
            TearDownItem(item);
            return base.RemoveFromInternalCollection(item);
        }
        
        private void TearDownItem(T item)
        {
            if (OwnsItems)
                item.Owner = null;
            if (FItems != FList)
                FItems.Remove(item);
            item.Renamed -= item_Renamed;
        }
        
        protected override void ClearInternalCollection()
        {
            foreach (var item in FCollection)
            {
                TearDownItem(item);
            }
            base.ClearInternalCollection();
        }
        
        private void item_Renamed(INamed sender, string newName)
        {
            FItems.ChangeKey((T)sender, newName);
            OnItemRenamed(sender, newName);
        }

        public override void MarkChanged()
        {
            base.MarkChanged();

            if (Owner != null)
            {
                Owner.MarkChanged();
            }
        }

        public override void AcknowledgeChanges()
        {
            base.AcknowledgeChanges();

            foreach (IIDItem item in this)
            {
                item.AcknowledgeChanges();
            }
        }
        
        #region IEditableIDList<T> Members
        
        #endregion
        
        #region IEditableIDList Members
        
        IIDItem IEditableIDList.this[int index]
        {
            get
            {
                return this[index];
            }
            set
            {
                this[index] = (T) value;
            }
        }
        
        #endregion
        
        #region IViewableIDList<T> Members
        
        #endregion
        
        #region IViewableIDList Members
        
        IIDItem IViewableIDList.this[int index]
        {
            get
            {
                return this[index];
            }
        }
        
        #endregion
        
        #region IIDContainer<T> Members
        
        public event RenamedHandler ItemRenamed;
        
        public T this[string name]
        {
            get
            {
                if (FItems.Contains(name))
                    return FItems[name];
                return default(T);
            }
        }
        
        protected virtual void OnItemRenamed(INamed item, string newName)
        {
            if (ItemRenamed != null) {
                ItemRenamed(item, newName);
            }
        }
        
        #endregion
        
        #region IIDContainer Members
        
        IIDItem IIDContainer.this[string name]
        {
            get
            {
                return this[name];
            }
        }
        
        #endregion
        
        #region IIDItem Members
        
        IIDContainer FOwner;
        public IIDContainer Owner
        {
            get
            {
                return FOwner;
            }
            set
            {
                if (FOwner != null)
                {
                    if (value != null)
                    {
                        throw new Exception(string.Format("ID item {0} ('{1}') has owner already.", this, Name));
                    }
                    
                    // Unsubscribe from old owner
                    FOwner.RootingChanged -= FOwner_RootingChanged;
                    
                    CheckIfRootingChanged(new RootingChangedEventArgs(RootingAction.ToBeUnrooted));
                }
                
                FOwner = value;

                if (FOwner != null)
                {
                    // Subscribe to new owner
                    FOwner.RootingChanged += FOwner_RootingChanged;
                    
                    CheckIfRootingChanged(new RootingChangedEventArgs(RootingAction.Rooted));
                }
            }
        }
        
        public bool IsRooted
        {
            get;
            private set;
        }

        public event RootingChangedEventHandler RootingChanged;
        
        public virtual void Dispatch(IVisitor visitor)
        {
            visitor.Visit(this);
        }

        private void CheckIfRootingChanged(RootingChangedEventArgs args)
        {
            switch (args.Rooting)
            {
                case RootingAction.Rooted:
                    if (FOwner.IsRooted && !IsRooted)
                    {
                        IsRooted = true;
                        OnRootingChanged(args);
                    }
                    break;
                case RootingAction.ToBeUnrooted:
                    if (FOwner.IsRooted && IsRooted)
                    {
                        OnRootingChanged(args);
                        IsRooted = false;
                    }
                    break;
            }
        }
        
        private void OnRootingChanged(RootingChangedEventArgs args)
        {
            if (args.Rooting == RootingAction.Rooted)
            {
                Mapper = FOwner.Mapper.CreateChildMapper(this);
                OnRootingChanged(RootingAction.Rooted);
            }
            
            if (RootingChanged != null)
            {
                RootingChanged(this, args);
            }
            
            if (args.Rooting == RootingAction.ToBeUnrooted)
            {
                OnRootingChanged(RootingAction.ToBeUnrooted);
                Mapper.Dispose();
            }
        }
        
        protected virtual void OnRootingChanged(RootingAction rooting)
        {

        }
        
        void FOwner_RootingChanged(object sender, RootingChangedEventArgs args)
        {
            // Propagate the event down further in the object graph
            CheckIfRootingChanged(args);
        }
        
        #endregion
        
        #region INamed Members
        
        public string Name
        {
            get;
            private set;
        }
        
        // as long as Name is private we don't fire this (name is only set on construction)
        public event RenamedHandler Renamed;
        
        protected virtual void OnRenamed(string newName)
        {
            if (Renamed != null) {
                Renamed(this, newName);
            }
        }
        
        #endregion
        
        #region Overwritten ViewableCollection<T> Members
        
        new public bool Contains(T item)
        {
            return Contains(item.Name);
        }
        
        #endregion
        
        #region Overwritten EditableCollection<T> Members
        
        public override bool CanAdd(T item)
        {
            if (Contains(item))
                if (this[item.Name].Equals(item))
                    // don't allow to add same object twice (it only can have one name)
                    return false;
                else
                    // allow to add item when name is already used and AllowRenameOnAdd == true
                    return (AllowRenameOnAdd && (item is IRenameable));
                else
                    return base.CanAdd(item);
        }
        
        #endregion
        
        #region Overwritten EditableList<T> Members
        
        public override T this[int index]
        {
            get
            {
                return base[index];
            }
            set
            {
                base[index] = value;
                
                if (OwnsItems)
                    value.Owner = this;
                value.Renamed += item_Renamed;
            }
        }
        
        #endregion

        public override string ToString()
        {
            return string.Format("{0}: {1}", Name, this.GetType().Name);
        }
    }
}
