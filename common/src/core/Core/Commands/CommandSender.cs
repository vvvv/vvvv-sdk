using System;
using System.Threading;
using System.Xml.Linq;
using VVVV.Utils.Network;
using VVVV.Core.Model;
using System.Runtime.Remoting.Messaging;
using VVVV.Core.Serialization;
using System.Collections.Generic;

namespace VVVV.Core.Commands
{
    //sends a command to the remote server
    public class CommandSender : RemotingProxyManagerTCP<CommandHistory>
    {
        //port and remoting manager
        private IIDItem FIDItem;
        private readonly Serializer FSerializer;


        public CommandSender(string[] hosts, int port, IIDItem idItem)
            : base(hosts, port)
        {

            FIDItem = idItem;
            FSerializer = idItem.GetSerializer();

            foreach (var item in FHosts)
            {
                Console.WriteLine("Remote History: {0}", item);
            }
        }

        //proxy object creation for base class
        protected override CommandHistory GetProxyElement(string host, int port)
        {
            return FRemoter.GetRemoteObject<Shell>("Shell", host, port).GetAtID<ICommandHistory>(FIDItem.GetID()) as CommandHistory;
        }

        public void ExecuteAndInsert(string xml)
        {
            for (int i = 0; i < FHosts.Length; i++)
            {
                try
                {
                    //h is a proxy objec of the remote history
                    //the xml string gets sent as value and is executed on the remote host
                    var h = GetProxy(i);
                    h.ExecuteAndInsert(xml);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Command could not be inserted and executed on remote history: " + e.Message);
                }
            }
        }

        //try to send a command
        public void OnlyExecute(string xml)
        {
            for (int i = 0; i < FHosts.Length; i++)
            {
                try
                {
                    //h is a proxy objec of the remote history
                    //the xml string gets sent as value and is executed on the remote host
                    var h = GetProxy(i);
                    h.OnlyExecute(xml);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Command could not be executed on remote history: " + e.Message);
                }
            }
        }

        //try to call undo on remote history
        public void RemoteUndo()
        {
            for (int i = 0; i < FHosts.Length; i++)
            {
                try
                {
                    var h = GetProxy(i);
                    h.Undo();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Undo could not be executed on remote server: " + e.Message);
                }
            }
        }

        //try to call redo on remote history
        public void RemoteRedo()
        {
            for (int i = 0; i < FHosts.Length; i++)
            {
                try
                {
                    var h = GetProxy(i);
                    h.Redo();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Redo could not be executed on remote server: " + e.Message);
                }
            }
        }

        public void StartCompound()
        {
            for (int i = 0; i < FHosts.Length; i++)
            {
                try
                {
                    var h = GetProxy(i);
                    h.StartCompound();
                }
                catch (Exception e)
                {
                    Console.WriteLine("StartCompound could not be executed on remote server: " + e.Message);
                }
            }
        }

        public void StopCompound()
        {
            for (int i = 0; i < FHosts.Length; i++)
            {
                try
                {
                    var h = GetProxy(i);
                    h.StopCompound();
                }
                catch (Exception e)
                {
                    Console.WriteLine("StopCompound could not be executed on remote server: " + e.Message);
                }
            }
            
        }

        #region async
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

        #endregion async
    }
}
