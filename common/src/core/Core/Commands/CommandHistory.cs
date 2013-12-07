using System;
using System.Diagnostics;
using VVVV.Core.Logging;
using System.Xml.Linq;
using VVVV.Core.Serialization;
using VVVV.Utils;

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
        private readonly Serializer FSerializer;

        // Position in command list of last executed command.
        private Node<Command> FCurrentNode;

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

        public CommandHistory(Serializer serializer)
        {
            FCurrentNode = FFirstNode;
            FSerializer = serializer;
        }

        /// <summary>
        /// Executes a command and adds it to the command history if the command
        /// is undoable.
        /// </summary>
        /// <param name="command">The command to be executed.</param>
        public virtual void Insert(Command command)
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
        	
        	OnCommandInserted(command);
        }

        public virtual void Insert(string xml)
        {
            var x = XElement.Parse(xml);
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
            }
            
            OnUndone(command);
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
            }
            
            OnRedone(command);
        }

        //IIDItem Interface
        public IIDContainer Parent { get; set; }

        public string Name
        {
            get { return "History"; }
        }
    	
		public event EventHandler<EventArgs<Command>> CommandInserted;
		
		protected void OnCommandInserted(Command command)
		{
			var handler = CommandInserted;
			if(handler != null)
			{
				handler(this, handler.CreateArgs(command));
			}
		}
    	
		public event EventHandler<EventArgs<Command>> Undone;
		
		protected void OnUndone(Command command)
		{
			var handler = Undone;
			if(handler != null)
			{
				handler(this, handler.CreateArgs(command));
			}
		}
    	
		public event EventHandler<EventArgs<Command>> Redone;
		
		protected void OnRedone(Command command)
		{
			var handler = Redone;
			if(handler != null)
			{
				handler(this, handler.CreateArgs(command));
			}
		}
    }
}
