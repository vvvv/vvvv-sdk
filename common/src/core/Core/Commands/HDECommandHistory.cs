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
            : base(idItem.ServiceProvider)
        {
            FCommandSender = new CommandSender(Shell.Instance.CommandLineArguments.ClientIPs, Shell.Instance.CommandLineArguments.Port, idItem);
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
                () => FCommandSender.ExecuteAndInsert(xCommand.ToString()),
                string.Format("ExecuteAndInsertCommand: {0}", command));

            //Debug.WriteLine(string.Format("Executed and Inserted command {0} on HDECommandHistory for {1}", command, FIdItem.Name));
        }

        /// <summary>
        /// Serializes a command and executes it on the remote vvvv and returns it.
        /// </summary>
        /// <param name="command">The command to be executed.</param>
        public override void OnlyExecute(Command command)
        {
            var xCommand = FSerializer.Serialize(command);
            base.OnlyExecute(command);
            FCommandSender.OnlyExecute(xCommand.ToString());

            //Debug.WriteLine(string.Format("Executed command {0} on HDECommandHistory for {1}", command, FIdItem.Name));
        }

        public override void StartCompound()
        {
            base.StartCompound();
            FCommandSender.StartCompound();
        }

        public override void StopCompound()
        {
            base.StopCompound();
            FCommandSender.StopCompound();
        }

        public override void Undo()
        {
            FCommandSender.RemoteUndo();
            base.Undo();
        }

        public override void Redo()
        {
            FCommandSender.RemoteRedo();
            base.Redo();
        }
	}
}
