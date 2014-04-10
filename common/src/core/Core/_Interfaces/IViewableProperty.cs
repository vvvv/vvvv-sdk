using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VVVV.Core
{
	public delegate void PropertyDelegate(IViewableProperty property, object newValue, object oldValue);
	public delegate void PropertyDelegate<T>(IViewableProperty<T> property, T newValue, T oldValue);

	public interface IViewableProperty : IIDItem
    {
        object ValueObject
        {
            get;
        }
        
        Type ValueType
        {
            get;
        }

        event PropertyDelegate ValueObjectChanged;
    }

    public interface IViewableProperty<T> : IIDItem
    {
        T Value
        {
            get;
        }

        event PropertyDelegate<T> ValueChanged;
    }
}
