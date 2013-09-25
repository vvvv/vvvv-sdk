using System;
using System.Xml.Linq;
using VVVV.Core;
using VVVV.Core.Serialization;

namespace VVVV.Core.Commands
{
    internal class RemoveCommand<TOwner, TItem> : Command
        where TOwner : IEditableCollection, IIDItem
        where TItem : IIDItem
    {
        #region Serialization
        internal class RemoveCommandSerializer : ISerializer<RemoveCommand<TOwner, TItem>>
        {
            public XElement Serialize(RemoveCommand<TOwner, TItem> command, Serializer serializer)
            {
                var xElement = new XElement("REMOVE");
                
                TOwner owner = command.FOwner;
                TItem item = command.FItem;
                
                xElement.Add(new XAttribute("Owner", owner.GetID()));
                xElement.Add(new XAttribute("Item", item.GetID()));
                
                return xElement;
            }
            
            public RemoveCommand<TOwner, TItem> Deserialize(XElement data, Type type, Serializer serializer)
            {
                // This should not happen
                throw new NotImplementedException();
            }
        }
        #endregion
        
        private readonly TOwner FOwner;
        private readonly TItem FItem;
        
        public RemoveCommand(TOwner owner, TItem item)
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
            FOwner.Remove(FItem);
        }

        public override void Undo()
        {
            FOwner.Add(FItem);
        }
        
        public override string ToString()
        {
            return string.Format("Remove {0}", FItem);
        }
    }
}
