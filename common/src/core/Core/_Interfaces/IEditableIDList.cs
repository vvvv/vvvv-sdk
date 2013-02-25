using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.Core.Collections;
//using VVVV.Core.Collections.Internal;

namespace VVVV.Core
{
    public interface IEditableIDList : IEditableList, IViewableIDList
    {
        new IIDItem this[int index]
        {
            get;
            set;
        }
    }

    public interface IEditableIDList<T> : IEditableList<T>, IChangedSequence<T>, IViewableIDList<T>, IEditableIDList where T : IIDItem
    {
        new T this[int index]
        {
            get;
            set;
        }
    }
}
