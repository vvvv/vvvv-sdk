using System;
using System.Threading;
using System.Xml.Linq;
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
        private string[] FHosts;
        private IIDItem FIDItem;
        private RemotingManagerTCP FRemoter;
        private readonly Serializer FSerializer;

        public CommandSender(string[] hosts, IIDItem idItem)
        {
            FRemoter = new RemotingManagerTCP();
            FHosts = hosts == null ? new string[]{"localhost"} : hosts;
            FIDItem = idItem;
            FSerializer = idItem.Mapper.Map<Serializer>();

            GetHistory();

            foreach (var item in FHosts)
            {
                Console.WriteLine("Remote: {0}", item);
            }
        }

        //async execute
        private delegate void SendAsync(string xml);

        public void ExecuteAndInsertAsync(XElement xCommand)
        {
            var sender = new SendAsync(ExecuteAndInsert);
            sender.BeginInvoke(xCommand.ToString(), new AsyncCallback(AfterSend), null);
        }

        public void OnlyExecuteAsync(XElement xCommand)
        {
            var sender = new SendAsync(OnlyExecute);
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
        ICommandHistory FHistoryProxy;
        private ICommandHistory GetHistory()
        {
            if (FHistoryProxy == null)
            {
                var remoteShell = FRemoter.GetRemoteObject<Shell>("Shell", FHosts[0], FPort);
                FHistoryProxy = remoteShell.GetAtID<ICommandHistory>(FIDItem.GetID());
            }

            return FHistoryProxy;
        }

        public void ExecuteAndInsert(string xml)
        {
            try
            {
                //h is a proxy objec of the remote history
                //the xml string gets sent as value and is executed on the remote host
                var h = GetHistory() as CommandHistory;
                h.ExecuteAndInsert(xml);
            }
            catch (Exception e)
            {
                Console.WriteLine("Command could not be inserted and executed on remote history: " + e.Message);
            }
        }

        //try to send a command
        public void OnlyExecute(string xml)
        {
            try
            {
                //h is a proxy objec of the remote history
                //the xml string gets sent as value and is executed on the remote host
                var h = GetHistory() as CommandHistory;
                h.OnlyExecute(xml);
            }
            catch (Exception e)
            {
                Console.WriteLine("Command could not be executed on remote history: " + e.Message);
                //Console.WriteLine("Command could not be executed on remote server");
                //throw e;
            }
        }

        //try to call undo on remote history
        public void RemoteUndo()
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
        public void RemoteRedo()
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

        public void StartCompound()
        {
            try
            {
                var h = GetHistory() as CommandHistory;
                h.StartCompound();
            }
            catch (Exception)
            {
                //Console.WriteLine("Redo could not be executed on remote server: " + e.Message);
                Console.WriteLine("StartCompound could not be executed on remote server");
            }
        }

        public void StopCompound()
        {
            try
            {
                var h = GetHistory() as CommandHistory;
                h.StopCompound();
            }
            catch (Exception)
            {
                //Console.WriteLine("Redo could not be executed on remote server: " + e.Message);
                Console.WriteLine("StopCompound could not be executed on remote server");
            }
        }
    }
}
