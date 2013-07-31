using System;
using System.Xml.Linq;
using VVVV.Core.Serialization;

namespace VVVV.Core.Commands
{
    internal class RenameCommand : Command
    {
        #region Serialization
        internal class RenameCommandSerializer : ISerializer<RenameCommand>
        {
            public XElement Serialize(RenameCommand command, Serializer serializer)
            {
                var xElement = new XElement("RENAME");

                var idItem = command.FRenameable as IIDItem;
                xElement.Add(new XAttribute("Item", idItem.GetID()));
                xElement.Add(new XAttribute("Name", command.FNewName));
                
                return xElement;
            }
            
            public RenameCommand Deserialize(XElement data, Type type, Serializer serializer)
            {
                var itemAttribute = data.Attribute("Item");
                var nameAttribute = data.Attribute("Name");
                
                var item = Shell.Instance.GetIDItem(itemAttribute.Value);
                var name = nameAttribute.Value;
                
                return new RenameCommand(item as IRenameable, name);
            }
        }
        #endregion
        
        private readonly IRenameable FRenameable;
        private readonly string FNewName;
        private readonly string FOldName;
        
        public RenameCommand(IRenameable renameable, string newName)
        {
            FRenameable = renameable;
            FNewName = newName;
            FOldName = FRenameable.Name;
        }
        
        public override void Execute()
        {
            FRenameable.Name = FNewName;
        }
        
        public override void Undo()
        {
            FRenameable.Name = FOldName;
        }
        
        public override bool HasUndo
        {
            get
            {
                return true;
            }
        }
    }
}
