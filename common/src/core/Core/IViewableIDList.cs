using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VVVV.Core
{
    public interface IViewableIDList : IViewableList, IIDContainer
    {
        new IIDItem this[int index]
        {
            get;
        }
        
        bool Contains(string name);
    }

    public interface IViewableIDList<T> : IViewableList<T>, IIDContainer<T>, IViewableIDList where T : IIDItem
    {
        //bool Contains(string name);
    }
}
