using System;

namespace VVVV.Core.View.Table
{
    public interface ICell
    {
        object Value
        {
            get;
            set;
        }
        
        Type ValueType
        {
            get;
        }
        
        event EventHandler ValueChanged;
        
        bool AcceptsValue(object newValue);
        
        bool WrapContent
        {
            get;
        }
        
        bool ReadOnly
        {
            get;
        }
    }
}
