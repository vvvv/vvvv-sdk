using System;
using System.Threading;
using VVVV.Utils.Network;
using VVVV.Core.Model;
using System.Runtime.Remoting.Messaging;
using VVVV.Core.Serialization;

namespace VVVV.Core.Commands
{
    //sends a command to the remote server
    public class CommandSender
    {
        //port and remoting manager
        private int FPort = 3344;
        private string FHost;
        private IIDItem FIDItem;
        private RemotingManagerTCP FRemoter;
        private readonly Serializer FSerializer;

        public CommandSender(string host, IIDItem idItem)
        {
            FRemoter = new RemotingManagerTCP();
            FHost = host;
            FIDItem = idItem;
            FSerializer = idItem.Mapper.Map<Serializer>();
        }

        //async execute
        private delegate void SendAsync(string xml);

        public void SendCommandAsync(Command command)
        {
            var xCommand = FSerializer.Serialize(command);

            var sender = new SendAsync(SendCommand);
            sender.BeginInvoke(xCommand.ToString(), new AsyncCallback(AfterSend), null);

        }

        //async undo
        private delegate void CallAsync();

        public void RemoteUndoAsync()
        {
            var sender = new CallAsync(RemoteUndo);
            sender.BeginInvoke(new AsyncCallback(AfterSend), null);

        }

        //async redo
        public void RemoteRedoAsync()
        {
            var sender = new CallAsync(RemoteRedo);
            sender.BeginInvoke(new AsyncCallback(AfterSend), null);

        }

        //gets called when async is finished
        private void AfterSend(IAsyncResult result)
        {
            Console.WriteLine("Async remote call completed.");
        }

        //get remote history
        private CommandHistory GetHistory()
        {
            var remoteShell = FRemoter.GetRemoteObject<Shell>("Shell", FHost, FPort);
            return remoteShell.GetAtID<CommandHistory>(FIDItem.GetID());
        }

        //try to send a command
        private void SendCommand(string xml)
        {
      
            try
            {
                var h = GetHistory();
                h.Insert(xml);
            }
            catch (Exception e)
            {
                Console.WriteLine("Command could not be executed on remote server: " + e.Message);
                //Console.WriteLine("Command could not be executed on remote server");
                //throw e;
            }
        }

        //try to call undo on remote history
        private void RemoteUndo()
        {
            try
            {
                var h = GetHistory();
                h.Undo();
            }
            catch (Exception)
            {
                //Console.WriteLine("Undo could not be executed on remote server: " + e.Message);
                Console.WriteLine("Undo could not be executed on remote server");
            }
        }

        //try to call redo on remote history
        private void RemoteRedo()
        {
            try
            {
                var h = GetHistory();
                h.Redo();
            }
            catch (Exception)
            {
                //Console.WriteLine("Redo could not be executed on remote server: " + e.Message);
                Console.WriteLine("Redo could not be executed on remote server");
            }
        }
    }
}
