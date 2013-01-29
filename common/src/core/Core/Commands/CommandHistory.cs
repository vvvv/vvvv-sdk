using System;
using System.Diagnostics;
using VVVV.Core.Logging;
using System.Xml.Linq;
using VVVV.Core.Serialization;
using System.Threading;

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

        /// <summary>
        /// Executes a command and adds it to the command history if the command
        /// is undoable.
        /// </summary>
        /// <param name="command">The command to be executed.</param>
        public virtual void Insert(Command command)
        {
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

                    Debug.WriteLine(string.Format("Command {0} executed.", command));
                },
                string.Format("Execution of command {0}", command));

                if (OnChange != null)
                    OnChange();
            }
            else
            {
                Debug.WriteLine("Skipped empty command.");
            }
        }

        public virtual void Insert(string xml)
        {
            var x = XElement.Parse(xml);
            if (FMainThread != null)
                FMainThread.Post((state) => Insert(FSerializer.Deserialize<Command>(x)), null);
            else
                Insert(FSerializer.Deserialize<Command>(x));
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
                    command.Redo();
                    FCurrentNode = FCurrentNode.Next;
                    Debug.WriteLine(string.Format("Command {0} redone.", command));
                },
                string.Format("Redo of command {0}", command));
            	
            	if (OnChange != null) 
                    OnChange();
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
