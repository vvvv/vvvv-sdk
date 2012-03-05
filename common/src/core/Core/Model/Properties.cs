using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.Core.View;

namespace VVVV.Core.Model
{
    [Serializable]
    public class Property<T> : IDItem, IDescripted //where T : IEquatable<T>
    {
        public Property(string name)
            : base(name)
        { }

        public Property(string name, T value)
            : base(name)
        {
            FValue = value;
        }

        #region IEditableProperty<T> Members

        protected T FValue;
        public T Value
        {
            get 
            { 
                return FValue;
            }
            set
            {
                if (AcceptValue(value)) //(!FValue.Equals(value) && (AcceptValue(value)))
                {
                    var oldvalue = FValue;

                    FValue = value;

                    if (ValueChanged != null)
                        ValueChanged((IViewableProperty<T>)this, FValue, oldvalue);
                    if (ValueObjectChanged != null)
                        ValueObjectChanged((IViewableProperty)this, FValue, oldvalue);
                }
            }
        }

        public virtual bool AcceptValue(T newValue)
        {
            return true;
        }

        public void SetValueByCommand(T newValue)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IViewableProperty<T> Members

        public event PropertyDelegate<T> ValueChanged;

        #endregion

        #region IEditableProperty Members

        public object ValueObject
        {
            get
            {
                return Value;
            }
            set
            {
                Value = (T)value;
            }
        }

        public bool AcceptValueObject(object newValue)
        {
            return ((newValue is T) && AcceptValue((T)newValue));
        }

        public void SetValueObjectByCommand(object newValue)
        {
            if (newValue is T)
                SetValueByCommand((T)newValue);
        }

        #endregion

        #region IViewableProperty Members


        public event PropertyDelegate ValueObjectChanged;
        
        public Type ValueType
        {
            get
            {
                return typeof(T);
            }
        }

        #endregion


        #region IDescripted Members

        public string Description
        {
            get
            {
                return Value.ToString();
            }
        }

        #endregion
    }

    [Serializable]    
    public class ViewableProperty<T> : Property<T>, IViewableProperty, IViewableProperty<T> //where T : IEquatable<T>
    {
        public ViewableProperty(string name)
            : base(name)
        { }

        public ViewableProperty(string name, T value)
            : base(name, value)
        { }

    }

    [Serializable]    
    public class EditableProperty<T> : Property<T>, IEditableProperty<T>, IEditableProperty //where T : IEquatable<T>
    {
        public EditableProperty(string name)
            : base(name)
        { }

        public EditableProperty(string name, T value)
            : base(name, value)
        { }

        public override bool AcceptValue(T newValue)
        {
            if (AllowChange != null)
                return AllowChange(this, newValue);
            return true;
        }

        public PropertyPredicate<T> AllowChange;
    }
}
