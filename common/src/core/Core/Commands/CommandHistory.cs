using System;
using System.Diagnostics;
using VVVV.Core.Logging;
using System.Xml.Linq;
using VVVV.Core.Serialization;
using System.Threading;
using System.Collections.Generic;

namespace VVVV.Core.Commands
{
    /// <summary>
    /// A basic implementation of ICommandHistory.
    /// </summary>
    public class CommandHistory : MarshalByRefObject, ICommandHistory
    {
        #region Node class

        class Node<T>
        {
            public T Value { get; set; }
            public Node<T> Next { get; set; }
            public Node<T> Previous { get; set; }

            public Node()
            {
            }

            public Node(T value)
            {
                Value = value;
            }
        }

        #endregion

        private readonly Node<Command> FFirstNode = new Node<Command>();
        protected readonly Serializer FSerializer;

        // Position in command list of last executed command.
        private Node<Command> FCurrentNode;
        private SynchronizationContext FMainThread;

        /// <summary>
        /// The command which will be executed on redo.
        /// </summary>
        public Command NextCommand
        {
            get
            {
                if (FCurrentNode.Next != null)
                    return FCurrentNode.Next.Value;
                return null;
            }
        }

        /// <summary>
        /// The command which will be undone on undo.
        /// </summary>
        public Command PreviousCommand
        {
            get
            {
                return FCurrentNode.Value;
            }
        }

        public CommandHistory(Serializer serializer, SynchronizationContext syncContext)
        {
            FCurrentNode = FFirstNode;
            FSerializer = serializer;
            FMainThread = syncContext;
        }

        public virtual void Insert(Command command)
        {
            if (command is CompoundCommand)
            {
                HandleCompoundCommand(command as CompoundCommand);
            }
            else
            {
                ExecuteAndInsert(command);
            }

        }

        private void HandleCompoundCommand(CompoundCommand command)
        {
            StartCompound();
            foreach (var subCom in command.Commands)
            {
                if (subCom is CompoundCommand)
                {
                    HandleCompoundCommand(subCom as CompoundCommand);
                }
                else
                    OnlyExecute(subCom);
            }
            StopCompound();
        }

        public virtual void ExecuteAndInsert(Command command)
        {
            //insert command
            if (command != Command.Empty)
            {
                DebugHelpers.CatchAndLog(() =>
                {
                    command.Execute();

                    if (command.HasUndo)
                    {
                        var newNode = new Node<Command>(command);
                        newNode.Previous = FCurrentNode;
                        FCurrentNode.Next = newNode;
                        FCurrentNode = newNode;
                    }
                    else
                    {
                        FFirstNode.Next = null;
                        FCurrentNode = FFirstNode;
                    }

                    Debug.WriteLine(string.Format("Command {0} executed and inserted.", command));
                },
                string.Format("Innsertion and execution of command {0}", command));

                if (OnChange != null)
                    OnChange();
            }
            
        }

        public virtual void OnlyExecute(Command command)
        {
            //execute command
            if (command != Command.Empty)
            {
                DebugHelpers.CatchAndLog(() =>
                {
                    command.Execute();

                    Debug.WriteLine(string.Format("Command {0} executed.", command));
                },
                string.Format("Execution of command {0}", command));

                if (OnChange != null)
                    OnChange();
            }

            //Debug.WriteLine("StackSize: " + FCompoundStack.Count);

            if (FCompoundStack.Count > 0)
            {
                FCompoundStack.Peek().Append(command);
                //Debug.WriteLine("Append");
            }
        }

        public virtual void OnlyInsert(Command command)
        {
            //insert command
            if (command != Command.Empty)
            {
                DebugHelpers.CatchAndLog(() =>
                {
                    if (command.HasUndo)
                    {
                        var newNode = new Node<Command>(command);
                        newNode.Previous = FCurrentNode;
                        FCurrentNode.Next = newNode;
                        FCurrentNode = newNode;
                    }
                    else
                    {
                        FFirstNode.Next = null;
                        FCurrentNode = FFirstNode;
                    }

                    Debug.WriteLine(string.Format("Command {0} inserted.", command));
                },
                string.Format("Insertion of command {0}", command));
            }
        }


        private Stack<CompoundCommand> FCompoundStack = new Stack<CompoundCommand>();

        public virtual void StartCompound()
        {
            //Debug.WriteLine("StartCompound: " + FCompoundStack.Count);
            FCompoundStack.Push(new CompoundCommand());
        }

        public virtual void StopCompound()
        {
            //Debug.WriteLine("StopCompound " + FCompoundStack.Count);
            var comp = FCompoundStack.Pop();

            if (FCompoundStack.Count == 0)
                OnlyInsert(comp);
            else
                FCompoundStack.Peek().Append(comp);

        }


        //xml from HDE
        public virtual void ExecuteAndInsert(string xml)
        {
            var x = XElement.Parse(xml);

            if (FMainThread != null)
                FMainThread.Send((state) => ExecuteAndInsert(FSerializer.Deserialize<Command>(x)), null);
            else
                ExecuteAndInsert(FSerializer.Deserialize<Command>(x));
        }

        public virtual void OnlyExecute(string xml)
        {
            var x = XElement.Parse(xml);

            if (FMainThread != null)
                FMainThread.Send((state) => OnlyExecute(FSerializer.Deserialize<Command>(x)), null);
            else
                OnlyExecute(FSerializer.Deserialize<Command>(x));
        }

        /// <summary>
        /// Undo last command.
        /// </summary>
        public virtual void Undo()
        {
            var command = PreviousCommand;
            if (command != null)
            {
                DebugHelpers.CatchAndLog(() =>
                {
                    command.Undo();
                    FCurrentNode = FCurrentNode.Previous;
                    Debug.WriteLine(string.Format("Command {0} undone.", command));
                },
                string.Format("Undo of command {0}", command));

                if (OnChange != null)
                    OnChange();
            }
        }

        /// <summary>
        /// Redo last command.
        /// </summary>
        public virtual void Redo()
        {
            var command = NextCommand;
            if (command != null)
            {
                DebugHelpers.CatchAndLog(() =>
                {
                    if (command is CompoundCommand)
                    {
                        RedoCompoundCommand(command as CompoundCommand);
                    }
                    else
                    {
                        command.Redo();
                    }
                    FCurrentNode = FCurrentNode.Next;
                    Debug.WriteLine(string.Format("Command {0} redone.", command));
                },
                string.Format("Redo of command {0}", command));
            	
            	if (OnChange != null) 
                    OnChange();
            }
        }

        //redo copund does not build new commands
        private void RedoCompoundCommand(CompoundCommand command)
        {
            foreach (var subCom in command.Commands)
            {
                if (subCom is CompoundCommand)
                {
                    RedoCompoundCommand(subCom as CompoundCommand);
                }
                else
                    OnlyExecute(subCom);
            }
        }
        
        public Action OnChange {get;set;}

        //IIDItem Interface
        public IIDContainer Parent { get; set; }

        public string Name
        {
            get { return "History"; }
        }
    }
}
