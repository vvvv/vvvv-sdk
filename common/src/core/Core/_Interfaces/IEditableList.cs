using System;
//using VVVV.Core.Collections.Internal;

namespace VVVV.Core
{
    public interface IEditableList : IEditableCollection, IViewableList//, IEditableList<object>
    {
        new object this[int index]
        {
            get;
            set;
        }
    }

    public interface IEditableList<T> : IEditableCollection<T>, IViewableList<T>, IEditableList
    {
        new T this[int index]
        {
            get;
            set;
        }
//        List<T> InternalList
//        {
//            get;
//        }
    }
}
