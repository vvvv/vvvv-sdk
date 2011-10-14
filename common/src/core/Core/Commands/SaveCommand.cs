
using System;
using System.Xml.Linq;
using VVVV.Core.Model;
using VVVV.Core.Serialization;

namespace VVVV.Core.Commands
{
    class SaveCommand<TPersistent> : Command
        where TPersistent : IPersistent, IIDItem
    {
        #region Serialization
        internal class SaveCommandSerializer : ISerializer<SaveCommand<TPersistent>>
        {
            public XElement Serialize(SaveCommand<TPersistent> command, Serializer serializer)
            {
                var xElement = new XElement("SAVE");
                
                TPersistent persitentItem = command.FPersistent;
                xElement.Add(new XAttribute("Item", persitentItem.GetID()));
                
                return xElement;
            }
            
            public SaveCommand<TPersistent> Deserialize(XElement data, Type type, Serializer serializer)
            {
                // This should not happen
                throw new NotImplementedException();
            }
        }
        #endregion
        
        private readonly TPersistent FPersistent;
        
        public SaveCommand(TPersistent persistent)
        {
            FPersistent = persistent;
        }
        
        public override void Undo()
        {
            // Do nothing
        }
        
        public override bool HasUndo
        {
            get
            {
                return false;
            }
        }
        
        public override void Execute()
        {
            FPersistent.Save();
        }
    }
}
