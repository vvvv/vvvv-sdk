using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VVVV.Core
{
    public delegate bool PropertyPredicate<T>(IEditableProperty<T> property, T newValue);

    public interface IEditableProperty : IViewableProperty
    {
        new object ValueObject
        {
            get;
            set;
        }

        bool AcceptValueObject(object newValue);
    }

    public interface IEditableProperty<T> : IViewableProperty<T>
    {
        new T Value
        {
            get;
            set;
        }

        bool AcceptValue(T newValue);
    }
}
