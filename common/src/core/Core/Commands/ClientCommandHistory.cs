using System;
using System.Diagnostics;
using System.Collections.Generic;
using VVVV.Core.Logging;
using VVVV.Core.Model;
using VVVV.Core.Serialization;

namespace VVVV.Core.Commands
{
	/// <summary>
	/// The client implementation of ICommandHistory.
	/// </summary>
    public class ClientCommandHistory : CommandHistory
	{
        private CommandSender FCommandSender;
        private IIDItem FIdItem;

        public ClientCommandHistory(IIDItem idItem)
            : base(idItem.Mapper.Map<Serializer>())
        {
            FCommandSender = new CommandSender("localhost", idItem);
            FIdItem = idItem;
        }

        /// <summary>
        /// Executes a command and adds it to the command history if the command
        /// is undoable and sends the command to the remote server.
        /// </summary>
        /// <param name="command">The command to be executed.</param>
        public override void Insert(Command command)
        {
            var xCommand = FSerializer.Serialize(command);
            base.Insert(command);
            DebugHelpers.CatchAndLog(
                () => FCommandSender.SendCommandAsync(xCommand), 
                string.Format("SendCommandAsync: {0}", command));
            Debug.WriteLine(string.Format("Executed on ClientCommandHistory for {0}", FIdItem.Name));
        }

        public override void Undo()
        {
            FCommandSender.RemoteUndoAsync();
            base.Undo();
        }

        public override void Redo()
        {
            FCommandSender.RemoteRedoAsync();
            base.Redo();
        }
	}
}
