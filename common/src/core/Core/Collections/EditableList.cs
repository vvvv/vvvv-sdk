using System;
using System.Collections.Generic;
using System.Collections;
//using VVVV.Core.Collections.Internal;

namespace VVVV.Core.Collections
{
    [Serializable]
    public class EditableList<T> : EditableCollection<T>,
        IViewableList<T>, IEditableList<T>,
        IViewableList, IEditableList
    {
        protected IList<T> FList;

        public EditableList(IList<T> list)
            : base(list)
        {
            FList = list;
        }

        public EditableList()
            : this(new System.Collections.Generic.List<T>())
        {
        }

        public ViewableList<T> AsViewableList()
        {
            return new ViewableList<T>(this);
        }

        public virtual void Sort(Comparison<T> comparison)
        {
            if (FList is System.Collections.Generic.List<T>)
            {
                (FList as System.Collections.Generic.List<T>).Sort(comparison);
                OnOrderChanged();
            }
        }

        public override void Dispose()
        {
            OrderChanged = null;
            base.Dispose();
        }

        #region IEditableList<T> Members

        public virtual T this[int index]
        {
            get
            {
                return FList[index];
            }
            set
            {
                if (!CanAdd(value))
                    throw new Exception(string.Format("Can't add {0} to list {1}.", value, this));

                var currentItem = FList[index];
                if (currentItem != null)
                    if (!Remove(currentItem))
                        throw new Exception(string.Format("Can't remove {0} from list {1}.", currentItem, this));

                MarkChanged();
                FList[index] = value;
            }
        }

        #endregion

        #region IEditableList Members

        object IEditableList.this[int index]
        {
            get
            {
                return this[index];
            }
            set
            {
                this[index] = (T)value;
            }
        }

        #endregion

        #region IViewableList<T> Members

        public event OrderChangedHandler<T> OrderChanged;

        protected virtual void OnOrderChanged()
        {
            if (OrderChanged != null)
                OrderChanged(this);
            if (FOrderChanged != null)
                FOrderChanged(this);
        }

        #endregion

        #region IViewableList Members

        object IViewableList.this[int index]
        {
            get
            {
                return this[index];
            }
        }

        protected OrderChangedHandler FOrderChanged;
        event OrderChangedHandler IViewableList.OrderChanged
        {
            add
            {
                FOrderChanged += value;
            }
            remove
            {
                FOrderChanged -= value;
            }
        }

        #endregion

        //        #region IList<T> Members
        //        
        //        public int IndexOf(T item)
        //        {
        //            return FList.IndexOf(item);
        //        }
        //        
        //        public void Insert(int index, T item)
        //        {
        //            var oldItem = this[index];
        //            if (Remove(oldItem))
        //            {
        //                FList.Insert(index, item);
        //                OnAdded(item);
        //            }
        //        }
        //        
        //        public void RemoveAt(int index)
        //        {
        //            Remove(this[index]);
        //        }
        //        
        //        #endregion
    }
}
