using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Collections;
using VVVV.Core.Collections;

namespace VVVV.Core.Model
{
    [Serializable]
    public class KeyedIDCollection<T> : KeyedCollection<string, T>, IList<T> where T : IIDItem
    {
        protected override string GetKeyForItem(T item)
        {
            return item.Name;
        }

        public void ChangeKey(T item, string newName) 
        {
            base.ChangeItemKey(item, newName);
        }
        
        public bool IsReadOnly 
        { 
            get
            {
                return true;
            }
        }
        
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IList) Items).GetEnumerator();
        }
    	
		public void Sort(Comparison<T> comparison)
		{
			var list = base.Items as List<T>;
			list.Sort(comparison);
		}
    }


    public class IDContainer : IDItem, IIDContainer, IEnumerable<IIDItem>
    {
        protected KeyedIDCollection<IIDItem> FItems;
        protected Property<string> FNameProperty;
        
        public event RenamedHandler ItemRenamed;
        
        public IDContainer(string name, bool isRooted = false)
            : base(name, isRooted)
        {
            FItems = new KeyedIDCollection<IIDItem>();
            
            if (IsRenameable())
                FNameProperty = new EditableProperty<string>("Name", name);
            else
                FNameProperty = new ViewableProperty<string>("Name", name);
            FNameProperty.ValueChanged += FNameProperty_ValueChanged;
        }
        
        protected override void OnRootingChanged(RootingAction rooting)
        {
            switch (rooting) 
            {
                case RootingAction.Rooted:
                    Add(FNameProperty);
                    break;
                case RootingAction.ToBeUnrooted:
                    Remove(FNameProperty);
                    break;
            }
            base.OnRootingChanged(rooting);
        }

        void FNameProperty_ValueChanged(IViewableProperty<string> property, string newValue, string oldValue)
        {
            Name = newValue;
        }
        
        public virtual IEnumerator<IIDItem> GetEnumerator()
        {
            return FItems.GetEnumerator();
        }
        
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        
        public void Add(IIDItem item)
        {
            FItems.Add(item);
            item.Owner = this;
            item.Renamed += item_Renamed;
        }
        
        public void Remove(IIDItem item)
        {
            item.Renamed -= item_Renamed;
            item.Owner = null;
            FItems.Remove(item);
        }

        void item_Renamed(INamed sender, string newName)
        {
            FItems.ChangeKey(this, newName);
            OnItemRenamed(sender, newName);
        }
        
        protected virtual void OnItemRenamed(INamed item, string newName)
        {
            if (ItemRenamed != null) {
                ItemRenamed(item, newName);
            }
        }
        
        protected override void OnRenamed(string newName)
        {
            base.OnRenamed(newName);
            FName = newName;
            FNameProperty.Value = newName;
        }

        #region IIDContainer Members

        public IIDItem this[string name]
        {
            get 
            {
                return FItems[name];
            }
        }

        #endregion
        
        protected override void DisposeManaged()
        {
            FNameProperty.ValueChanged -= FNameProperty_ValueChanged;
			FNameProperty.Dispose();

            foreach (var item in FItems) 
            {
                item.Renamed -= item_Renamed;
            }
            
            base.DisposeManaged();
        }

        public override void AcknowledgeChanges()
        {
            base.AcknowledgeChanges();

            foreach (var item in this)
                item.AcknowledgeChanges();
        }

        //public override string ToString()
        //{
        //    return string.Format("IDContainer {0}", Name);
        //}
    }
}
