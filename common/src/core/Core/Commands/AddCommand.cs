using System;
using System.Collections;
using System.Xml.Linq;

using VVVV.Core;
using VVVV.Core.Serialization;

namespace VVVV.Core.Commands
{
    internal class AddCommand<TOwner, TItem> : Command
        where TOwner : IEditableCollection, IIDItem
        where TItem : IIDItem
    {
        #region Serialization
        internal class AddCommandSerializer : ISerializer<AddCommand<TOwner, TItem>>
        {
            public XElement Serialize(AddCommand<TOwner, TItem> command, Serializer serializer)
            {
                var xElement = new XElement("ADD");
                
                TOwner owner = command.FOwner;
                TItem item = command.FItem;
                
                xElement.Add(new XAttribute("Owner", owner.GetID()));
                
                if (item.IsRooted)
                {
                    xElement.Add(new XAttribute("Item", item.GetID()));
                }
                else
                {
                    var serializedItem = serializer.Serialize(item);
                    xElement.Add(serializedItem);
                    xElement.AddTypeAttribute(item.GetType());
                }
                
                return xElement;
            }
            
            public AddCommand<TOwner, TItem> Deserialize(XElement data, Type type, Serializer serializer)
            {
                // This should not happen
                throw new NotImplementedException();
            }
        }
        #endregion
        
        protected TOwner FOwner;
        protected TItem FItem;
        
        public AddCommand(TOwner owner, TItem item)
        {
            FOwner = owner;
            FItem = item;
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
            FOwner.Add(FItem);
        }

        public override void Undo()
        {
            FOwner.Remove(FItem);
        }
        
        public override string ToString()
        {
            return string.Format("Add {0}", FItem);
        }
    }
}
