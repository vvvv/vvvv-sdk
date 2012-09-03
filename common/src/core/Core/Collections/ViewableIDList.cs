using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.Core;
using VVVV.Core.Model;

namespace VVVV.Core.Collections
{
    // items named
    // therefore lookup works through [string name]
    // since it is an editablecollection it provides easy acces to 
    // IViewableCollection and IEditableCollection

    [Serializable]
    public class ViewableIDList<T> : ViewableList<T>, IViewableIDList<T>, IViewableIDList
        where T : IIDItem
    {
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

        private readonly IEditableIDList<T> FInternalIDList;
        private readonly bool FOwnerOfInternalIDList;

        public ViewableIDList(string name)
            :this(new EditableIDList<T>(name))
        {
            FOwnerOfInternalIDList = true;
        }
        
        public ViewableIDList(KeyedIDCollection<T> collection, string name)
            :this(new EditableIDList<T>(collection, name))
        {
            FOwnerOfInternalIDList = true;
        }
        
        public ViewableIDList(IEditableIDList<T> idList)
            :base(idList)
        {
            FInternalIDList = idList;
            OwnsItems = true;

            FInternalIDList.RootingChanged += FInternalIDList_RootingChanged;
        }

        void FInternalIDList_RootingChanged(object sender, RootingChangedEventArgs args)
        {
            OnRootingChanged(args);
        }
        
        public override void Dispose()
        {
            FInternalIDList.RootingChanged -= FInternalIDList_RootingChanged;

            if (FOwnerOfInternalIDList)
                (FInternalIDList as EditableIDList<T>).Dispose();
            
            base.Dispose();
        }
        
        public bool Contains(string name)
        {
            return FInternalIDList.Contains(name);
        }

        protected override void OnAdded(T item)
        {
            if (OwnsItems)
                item.Owner = this;
            base.OnAdded(item);
        }

        protected override void OnRemoved(T item)
        {
            if (OwnsItems)
                item.Owner = null;
            base.OnRemoved(item);
        }

        #region IViewableIDList<T> Members
        
        #endregion
        
        #region IViewableIDList Members
        
        IIDItem IViewableIDList.this[int index]
        {
            get
            {
                return ((IViewableIDList) FInternalIDList)[index];
            }
        }
        
        #endregion
        
        #region IIDContainer<T> Members
        
        public event RenamedHandler ItemRenamed
        {
            add
            {
                FInternalIDList.ItemRenamed += value;
            }
            remove
            {
                FInternalIDList.ItemRenamed -= value;
            }
        }
        
        public T this[string name]
        {
            get
            {
                return FInternalIDList[name];
            }
        }
        
        #endregion
        
        #region IIDContainer Members
        
        IIDItem IIDContainer.this[string name]
        {
            get
            {
                return ((IIDContainer) FInternalIDList)[name];
            }
        }
        
        public bool IsRooted
        {
            get
            {
                return FInternalIDList.IsRooted;
            }
        }
        
        #endregion
        
        #region IIDItem Members
        
        public IIDContainer Owner
        {
            get
            {
                return FInternalIDList.Owner;
            }
            set
            {
                FInternalIDList.Owner = value;
            }
        }

        public ModelMapper Mapper
        {
            get
            {
                return FInternalIDList.Mapper;
            }
        }

        public event RootingChangedEventHandler RootingChanged;
        
        public virtual void Dispatch(IVisitor visitor)
        {
            visitor.Visit(this);
        }

        protected virtual void OnRootingChanged(RootingChangedEventArgs args)
        {
            if (RootingChanged != null)
            {
                RootingChanged(this, args);
            }
        }

        public void MarkChanged()
        {
            throw new Exception("viewable lists should never be changed. so don't mark them as changed");
        }

        public void AcknowledgeChanges()
        {
            throw new Exception("viewable lists should never be changed. so don't acknowledge changes");
        }
        
        public bool Changed { get { return false; } }

        #endregion
        
        #region INamed Members
        
        public string Name
        {
            get
            {
                return FInternalIDList.Name;
            }
        }
        
        // as long as Name is private we don't fire this (name is only set on construction)
        public event RenamedHandler Renamed
        {
            add
            {
                FInternalIDList.Renamed += value;
            }
            remove
            {
                FInternalIDList.Renamed -= value;
            }
        }
        
        #endregion

        public override string ToString()
        {
            return string.Format("{0}: {1}", Name, this.GetType().Name);
        }
    }
}
