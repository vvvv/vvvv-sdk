using System;
using System.Collections.Generic;

namespace VVVV.Core.Collections
{
    [Serializable]
    public class SortedViewableList<T, TKey> : ViewableList<T>, System.Collections.IEnumerable
    {
        protected System.Collections.Generic.SortedList<TKey, T> FSortedList;
        protected Func<T, TKey> FKeySelector;
        
        public SortedViewableList(Func<T, TKey> keySelector)
            :this(new System.Collections.Generic.List<T>(), keySelector)
        {
        }
        
        public SortedViewableList(System.Collections.Generic.IList<T> list, Func<T, TKey> keySelector)
            :this(new EditableList<T>(list), keySelector)
        {
        }
        
        public SortedViewableList(IEditableList<T> list, Func<T, TKey> keySelector)
            :base(list)
        {
            FSortedList = new System.Collections.Generic.SortedList<TKey, T>();
            FKeySelector = keySelector;
            
            foreach (var item in list)
                FSortedList.Add(FKeySelector(item), item);
        }
        
        public void UpdateKey(TKey oldKey, TKey newKey)
        {
            var item = FSortedList[oldKey];
            FSortedList.Remove(oldKey);
            FSortedList.Add(newKey, item);
            
            OnOrderChanged();
        }
        
        protected override void OnAdded(T item)
        {
            FSortedList.Add(FKeySelector(item), item);
            base.OnAdded(item);
        }
        
        protected override void OnRemoved(T item)
        {
            FSortedList.Remove(FKeySelector(item));
            base.OnRemoved(item);
        }

        public override T this[int index]
        {
            get
            {
                return FSortedList.Values[index];
            }
        }
        
        public override IEnumerator<T> GetEnumerator()
        {
            return FSortedList.Values.GetEnumerator();
        }
        
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
