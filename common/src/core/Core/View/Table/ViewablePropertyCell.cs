
using System;

namespace VVVV.Core.View.Table
{
    public class ViewablePropertyCell : ICell, IDisposable
    {
        private readonly IViewableProperty FProperty;
        
        public ViewablePropertyCell(IViewableProperty property)
        {
            FProperty = property;
            FProperty.ValueObjectChanged += HandleValueObjectChanged;
        }
        
        public void Dispose()
        {
            FProperty.ValueObjectChanged -= HandleValueObjectChanged;
        }

        void HandleValueObjectChanged(IViewableProperty property, object newValue, object oldValue)
        {
            OnValueChanged(EventArgs.Empty);
        }
        
        public event EventHandler ValueChanged;
        
        protected virtual void OnValueChanged(EventArgs e)
        {
            if (ValueChanged != null)
            {
                ValueChanged(this, e);
            }
        }
        
        public virtual object Value
        {
            get
            {
                return FProperty.ValueObject;
            }
            set
            {
                throw new InvalidOperationException();
            }
        }
        
        public Type ValueType
        {
            get
            {
                return FProperty.ValueType;
            }
        }
        
        public bool WrapContent
        {
            get;
            set;
        }
        
        public virtual bool ReadOnly
        {
            get
            {
                return true;
            }
        }
        
        public virtual bool AcceptsValue(object newValue)
        {
            throw new InvalidOperationException();
        }
    }
}
