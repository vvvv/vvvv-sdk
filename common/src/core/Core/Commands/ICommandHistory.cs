using System;
using VVVV.Utils;

namespace VVVV.Core.Commands
{
	
    /// <summary>
    /// a command history accepts commands, allows redo and undo
    /// </summary>
	public interface ICommandHistory
	{
		/// <summary>
		/// Execute a command and add to history.
		/// </summary>
		/// <param name="command">The command to be executed.</param>
		void Insert(Command command);
		
		/// <summary>
		/// Adds a command to history but does not execute it. (e.g. if the command was executed already)
		/// </summary>
		/// <param name="command">The command to be added to the history.</param>
		void InsertOnly(Command command);
		
		/// <summary>
		/// Undo last command.
		/// </summary>
		void Undo();
		
		/// <summary>
		/// Redo last command.
		/// </summary>
		void Redo();
		
		/// <summary>
		/// The command which will be executed by a redo.
		/// </summary>
		Command NextCommand
		{
		    get;
		}
		
		/// <summary>
		/// The command which will be undone by an undo.
		/// </summary>
		Command PreviousCommand
		{
		    get;
		}
		
		event EventHandler<EventArgs<Command>> CommandInserted;
		event EventHandler<EventArgs<Command>> Undone;
		event EventHandler<EventArgs<Command>> Redone;
	}
}
