using System;
using System.Xml.Linq;
using VVVV.Core.Serialization;
using VVVV.Utils.Reflection;

namespace VVVV.Core.Commands
{
    [Serializable]
    internal class SetPropertyCommand<TProperty> : Command
        where TProperty : IEditableProperty, IIDItem
    {
        #region Serialization
        internal class SetPropertyCommandSerializer : ISerializer<SetPropertyCommand<TProperty>>
        {
            public XElement Serialize(SetPropertyCommand<TProperty> command, Serializer serializer)
            {
                var xElement = new XElement("SET");
                
                TProperty property = command.FProperty;
                object value = command.FValue;
                
                xElement.Add(new XAttribute("Property", property.GetID()));
                xElement.AddTypeAttribute(value.GetType());
                
                var serializedValue = serializer.Serialize(value);
                xElement.Add(serializedValue);
                
                return xElement;
            }
            
            public SetPropertyCommand<TProperty> Deserialize(XElement data, Type type, Serializer serializer)
            {
                // This should not happen
                throw new NotImplementedException();
            }
        }
        #endregion
        
        protected TProperty FProperty;
        protected object FValue;
        protected object FOldValue;

        public SetPropertyCommand(TProperty property, object newValue)
        {
            FProperty = property;
            FValue = newValue;
        }

        public override bool HasUndo 
        {
            get 
            {
                return true;
            }
        }

        public override void Execute()
        {
            FOldValue = FProperty.ValueObject;
            FProperty.ValueObject = FValue;
        }

        public override void Undo()
        {
            FProperty.ValueObject = FOldValue;
        }  

        public override string ToString()
        {
            return string.Format("Set {0} = {1}", FProperty.GetType().GetCSharpName(true), FValue);
        }
        
    }

    public static class EditableProperty
    {
        public static void SetByCommand(this IEditableProperty property, object newValue, ICommandHistory history)
        {
            if (property.AcceptValueObject(newValue))
            {
                var command = Command.Set(property, newValue);
                history.Insert(command);
            }
        }

        public static void SetByCommand(this IEditableProperty property, object newValue, ModelMapper mapper)
        {
            if (mapper.CanMap<ICommandHistory>())
                property.SetByCommand(newValue, mapper.Map<ICommandHistory>());
        }
    }
}
