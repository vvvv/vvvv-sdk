using System;
using System.Diagnostics;
using System.Collections.Generic;
using VVVV.Core.Logging;
using VVVV.Core.Model;
using VVVV.Core.Serialization;
using System.Xml.Linq;

namespace VVVV.Core.Commands
{
	/// <summary>
	/// The HDE implementation of ICommandHistory. Sends the comands to the runtime
	/// </summary>
    public class HDECommandHistory : CommandHistory
	{
        private CommandSender FCommandSender;
        private IIDItem FIdItem;

        public HDECommandHistory(IIDItem idItem)
            : base(idItem.Mapper.Map<Serializer>(), null)
        {
            FCommandSender = new CommandSender(Shell.Instance.CommandLineArguments.RemoteIPs, idItem);
            FIdItem = idItem;
        }

        /// <summary>
        /// Serializes a command and executes it on the remote vvvv and returns it.
        /// </summary>
        /// <param name="command">The command to be executed.</param>
        public override void ExecuteAndInsert(Command command)
        {
            var xCommand = FSerializer.Serialize(command);
            base.ExecuteAndInsert(command);
            DebugHelpers.CatchAndLog(
                () => FCommandSender.ExecuteAndInsertAsync(xCommand),
                string.Format("ExecuteAndInsertCommandAsync: {0}", command));

            Debug.WriteLine(string.Format("Executed and Inserted command {0} on HDECommandHistory for {1}", command, FIdItem.Name));
        }

        /// <summary>
        /// Serializes a command and executes it on the remote vvvv and returns it.
        /// </summary>
        /// <param name="command">The command to be executed.</param>
        public override XElement OnlyExecute(Command command)
        {
            var xCommand = FSerializer.Serialize(command);
            base.OnlyExecute(command);
            DebugHelpers.CatchAndLog(
                () => FCommandSender.OnlyExecute(xCommand.ToString()),
                string.Format("ExecuteCommandAsync: {0}", command));
            
            Debug.WriteLine(string.Format("Executed command {0} on HDECommandHistory for {1}", command, FIdItem.Name));

            return xCommand;
        }

        /// <summary>
        /// Serializes a command and inserts it on the remote vvvv.
        /// </summary>
        /// <param name="command">The command to be inserted.</param>
        public override void OnlyInsert(Command command)
        {
            var xCommand = FSerializer.Serialize(command);
            base.OnlyInsert(command);
            DebugHelpers.CatchAndLog(
                () => FCommandSender.OnlyInsert(xCommand.ToString()),
                string.Format("InsertCommandAsync: {0}", command));

            Debug.WriteLine(string.Format("Inserted command {0} on HDECommandHistory for {1}", command, FIdItem.Name));
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
