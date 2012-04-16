using System;

namespace VVVV.Core
{
    public delegate void OrderChangedHandler(IViewableList list);
    
    public interface IViewableList : IViewableCollection
    {
        object this[int index]
        {
            get;
        }

        event OrderChangedHandler OrderChanged;        
    }
    
    public delegate void OrderChangedHandler<T>(IViewableList<T> list);

    public interface IViewableList<T> : IViewableCollection<T>, IViewableList
    {
        new T this[int index]
        {
            get;
        }
        
        new event OrderChangedHandler<T> OrderChanged;
    }
}
