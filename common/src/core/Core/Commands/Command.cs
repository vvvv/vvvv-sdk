using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Xml.Linq;

using VVVV.Core.Model;
using VVVV.Core.Serialization;

namespace VVVV.Core.Commands
{
    /// <summary>
    /// Base class for all commands.
    /// </summary>
    public abstract class Command
    {
        #region Serialization
        public class CommandSerializer : ISerializer<Command>
        {
            public XElement Serialize(Command cmd, Serializer serializer)
            {
                // This should not happen
                throw new NotImplementedException();
            }
            
            public virtual Command Deserialize(XElement data, Type type, Serializer serializer)
            {
                var itemResolver = Shell.Instance;
                
                string cmdName = data.Name.LocalName;
                switch (cmdName) {
                    case "ADD":
                        {
                            var ownerAttribute = data.Attribute("Owner");
                            var itemAttribute = data.Attribute("Item");

                            var owner = itemResolver.GetIDItem(ownerAttribute.Value);
                            object item = null;
                            if (itemAttribute != null)
                            {
                                item = itemResolver.GetIDItem(itemAttribute.Value);
                            }
                            else
                            {
                                item = serializer.Deserialize(data.Elements().First(), data.GetAttributedType());
                            }
                            
                            var cmdType = typeof(AddCommand<,>).MakeGenericType(owner.GetType(), item.GetType());
                            return Activator.CreateInstance(cmdType, owner, item) as Command;
                        }
                    case "REMOVE":
                        {
                            var ownerAttribute = data.Attribute("Owner");
                            var itemAttribute = data.Attribute("Item");

                            var owner = itemResolver.GetIDItem(ownerAttribute.Value);
                            var item = itemResolver.GetIDItem(itemAttribute.Value);
                            
                            var cmdType = typeof(RemoveCommand<,>).MakeGenericType(owner.GetType(), item.GetType());
                            return Activator.CreateInstance(cmdType, owner, item) as Command;
                        }
                    case "SET":
                        {
                            var propertyAttribute = data.Attribute("Property");
                            var valueElement = data.Elements().First();
                            
                            var property = itemResolver.GetIDItem(propertyAttribute.Value);
                            var value = serializer.Deserialize(valueElement, data.GetAttributedType());
                            
                            var cmdType = typeof(SetPropertyCommand<>).MakeGenericType(property.GetType());
                            return Activator.CreateInstance(cmdType, property, value) as Command;
                        }
                    case "RENAME":
                        {
                            var itemAttribute = data.Attribute("Item");
                            var nameAttribute = data.Attribute("Name");
                            
                            var item = itemResolver.GetIDItem(itemAttribute.Value);
                            
                            var cmdType = typeof(RenameCommand);
                            return Activator.CreateInstance(cmdType, item, nameAttribute.Value) as Command;
                        }
                    case "COMPOUND":
                        {
                            return serializer.Deserialize<CompoundCommand>(data);
                        }
                    default:
                        return null;
                }
            }
        }
        #endregion

        #region EmptyCommand

        class EmptyCommand : Command
        {
            public override void Execute()
            {
                // Do nothing
            }

            public override void Undo()
            {
                // Do nothing
            }

            public override bool HasUndo
            {
                get { return true; }
            }
        }

        #endregion

        /// <summary>
        /// Execute this command.
        /// </summary>
        public abstract void Execute();

        /// <summary>
        /// Undo this command.
        /// </summary>
        public abstract void Undo();

        /// <summary>
        /// Redo this command.
        /// Override this method if you need a special Redo operation.
        /// </summary>
        public virtual void Redo()
        {
            Execute();
        }

        /// <summary>
        /// Determines if this command is undoable.
        /// </summary>
        public abstract bool HasUndo
        {
            get;
        }
        
        public static Command Add<TOwner, TItem>(TOwner owner, TItem item)
            where TOwner : IEditableCollection, IIDItem
            where TItem : IIDItem
        {
            return new AddCommand<TOwner, TItem>(owner, item);
        }
        
        public static Command Remove<TOwner, TItem>(TOwner owner, TItem item)
            where TOwner : IEditableCollection, IIDItem
            where TItem : IIDItem
        {
            return new RemoveCommand<TOwner, TItem>(owner, item);
        }
        
        public static Command Set<TProperty>(TProperty property, object newValue)
            where TProperty : IEditableProperty, IIDItem
        {
            return new SetPropertyCommand<TProperty>(property, newValue);
        }
        
        public static Command Rename(IRenameable renameable, string name)
        {
            return new RenameCommand(renameable, name);
        }
        
        public static Command Move<TSource, TDestination, TItem>(TSource source, TDestination destination, TItem item)
            where TSource : IEditableCollection, IIDItem
            where TDestination : IEditableCollection, IIDItem
            where TItem : IIDItem
        {
            var moveCommand = new CompoundCommand();
            moveCommand.Append(new RemoveCommand<TSource, TItem>(source, item));
            moveCommand.Append(new AddCommand<TDestination, TItem>(destination, item));
            return moveCommand;
        }

        public static readonly Command Empty = new EmptyCommand();
    }
}
