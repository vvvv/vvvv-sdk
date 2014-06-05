using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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
        
        public void Append(Command command)
        {
//        	if(command is CompoundCommand)
//        	{
//        		var comp = (command as CompoundCommand);
//        		if(comp.IsEmptyRecursive())
//        		{
//        			Debug.WriteLine("Empty compound command appended");
//        			Debug.WriteLine(new StackTrace());
//        		}
//        	}
        	
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
        
        /// <summary>
        /// The number of subcommands in ths compound command
        /// </summary>
        public int CommandCount
        {
        	get
        	{
        		return FCommands.Count;
        	}
        }
        
        List<CompoundCommand> FCompoundCommandsToCheck = new List<CompoundCommand>();
        /// <summary>
        /// Checks this and all sub commands for empty commands
        /// </summary>
        /// <returns></returns>
        public bool IsEmptyRecursive()
        {
        	if(FCommands.Count > 0)
        	{
        		FCompoundCommandsToCheck.Clear();
        		
        		//test for normal command
        		foreach (var com in FCommands) 
        		{
        			if(com is CompoundCommand)
        			{
        				FCompoundCommandsToCheck.Add(com as CompoundCommand);
        			}
        			else
        			{
        				return false;
        			}
        		}
        		
        		//if all sub commands are compound commands check them too
        		foreach (var comp in FCompoundCommandsToCheck) 
        		{
        			//return false if any is not empty
        			if(!comp.IsEmptyRecursive())
        				return false;
        		}
        		
        		//all is empty return true
        		return true;
        	}
        	else
        	{
        		return true;
        	}
        }
    }
}
