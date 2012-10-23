using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

using VVVV.Core.Serialization;

namespace VVVV.Core.Commands
{
    /// <summary>
    /// A compound command wraps several single commands and implements generic undo and redo.
    /// </summary>
    public class CompoundCommand : Command
    {
        #region Serialization
        public class CompoundCommandSerializer : ISerializer
        {
            public XElement Serialize(object value, Serializer serializer)
            {
                var command = value as CompoundCommand;
                var xElement = new XElement("COMPOUND");
                
                foreach (var subCmd in command.FCommands)
                {
                    xElement.Add(serializer.Serialize(subCmd));
                }
                
                return xElement;
            }
            
            public object Deserialize(XElement data, Type type, Serializer serializer)
            {
                var commands = new List<Command>();
                
                foreach (var element in data.Elements())
                {
                    commands.Add(serializer.Deserialize<Command>(element));
                }
                
                return new CompoundCommand(commands);
            }
        }
        #endregion
        
        private readonly IList<Command> FCommands;
        
        public CompoundCommand()
            :this(new List<Command>())
        {
        }
        
        public CompoundCommand(IEnumerable<Command> commands)
        {
            FCommands = new List<Command>(commands);
        }

        public CompoundCommand(params Command[] commands)
        {
            FCommands = new List<Command>(commands);
        }
        
        public void Append(Command command)
        {
            FCommands.Add(command);
        }
        
        public override bool HasUndo 
        {
            get 
            {
                foreach (var cmd in FCommands) 
                {
                    if (!cmd.HasUndo)
                    {
                        return false;
                    }
                }
                
                return true;
            }
        }

        public override sealed void Execute()
        {
            foreach (var cmd in FCommands) 
            {
                cmd.Execute();
            }
        }

        public override sealed void Undo()
        {
            if (HasUndo)
            {
                foreach (var cmd in FCommands.Reverse())
                {
                    cmd.Undo();
                }
            }
        }
        
        public override string ToString()
        {
            return string.Join(", ", (from cmd in FCommands select cmd.ToString()).ToArray());
        }

    }
}
