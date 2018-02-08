using System;

namespace VVVV.Core.View.Table
{
    public class Cell : ICell
    {
        public Cell(object value, Type valueType)
            : this(value, valueType, false)
        {
        }
        
        public Cell(object value, Type valueType, bool wrapContent)
        {
            Value = value;
            ValueType = valueType;
            WrapContent = wrapContent;
        }
        
        public event EventHandler ValueChanged;
        
        protected virtual void OnValueChanged(EventArgs e)
        {
            if (ValueChanged != null) 
            {
                ValueChanged(this, e);
            }
        }
        
        public object Value 
        {
            get;
            set;
        }
        
        public Type ValueType
        {
            get;
            private set;
        }
        
        public bool WrapContent
        {
            get;
            private set;
        }
        
        public bool ReadOnly 
        {
            get 
            {
                return true;
            }
        }
        
        public bool AcceptsValue(object newValue)
        {
            throw new NotImplementedException();
        }
    }
}
