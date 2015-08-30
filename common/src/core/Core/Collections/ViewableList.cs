using System;
//using VVVV.Core.Collections.Internal;

namespace VVVV.Core.Collections
{
    [Serializable]
    public class ViewableList<T> : ViewableCollection<T>, IViewableList<T>, IViewableList
    {
        private readonly IEditableList<T> FInternalList;
        private readonly bool FOwnerOfInternalList;
        
        public ViewableList()
            :this(new EditableList<T>())
        {
            FOwnerOfInternalList = true;
        }
        
        public ViewableList(System.Collections.Generic.IList<T> list)
            :this(new EditableList<T>(list))
        {
            FOwnerOfInternalList = true;
        }
        
        public ViewableList(IEditableList<T> list)
            :base(list)
        {
            FInternalList = list;
            FInternalList.OrderChanged += InternalList_OrderChanged;
        }
        
		public override void Dispose()
		{
			FInternalList.OrderChanged -= InternalList_OrderChanged;
			OrderChanged = null;
			
			if (FOwnerOfInternalList)
			    (FInternalList as EditableList<T>).Dispose();
			
			base.Dispose();
		}

        void InternalList_OrderChanged(IViewableList<T> list)
        {
            OnOrderChanged();
        }
        
        protected virtual void OnOrderChanged()
        {
            if (OrderChanged != null) 
                OrderChanged(this);
            if (FOrderChanged != null)
                FOrderChanged(this);
        }
        
        #region IViewableList<T> Members
        
        public event OrderChangedHandler<T> OrderChanged;
        
        public virtual T this[int index]
        {
            get
            {
                return FInternalList[index];
            }
            set
            {
                throw new NotImplementedException();
            }
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
    }
}
