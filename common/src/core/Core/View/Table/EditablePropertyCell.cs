
using System;
using VVVV.Core.Commands;

namespace VVVV.Core.View.Table
{
    public class EditablePropertyCell : ViewablePropertyCell
    {
        private readonly IEditableProperty FProperty;
        
        public EditablePropertyCell(IEditableProperty property)
            : base(property)
        {
            FProperty = property;
        }
        
        public override object Value
        {
            get
            {
                return base.Value;
            }
            set
            {
                var command = Command.Set(FProperty, value);
                FProperty.GetCommandHistory().Insert(command);
            }
        }
        
        public override bool ReadOnly
        {
            get
            {
                return false;
            }
        }
        
        public override bool AcceptsValue(object newValue)
        {
            return FProperty.AcceptValueObject(newValue);
        }
    }
}
