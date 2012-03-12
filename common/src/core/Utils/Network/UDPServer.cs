using System;
using System.Net;
using System.Net.Sockets;

namespace VVVV.Utils.Network
{
	/// <summary>
	/// UDP message received event argument
	/// </summary>
	public class UDPReceivedEventArgs
    {
		public UDPReceivedEventArgs(IPEndPoint s, byte[] data)
        { 
        	RemoteSender = s;
        	Data = data;
        }
        
		/// <summary>
		/// The remote sender IPEndPoint
		/// </summary>
        public IPEndPoint RemoteSender {get; private set;}
        
        /// <summary>
        /// The incoming data
        /// </summary>
        public byte[] Data {get; private set;}
    }
	
	/// <summary>
	/// Class to handle UDP connections
	/// </summary>
	public class UDPServer
	{
		protected UdpClient FServer;
		protected IPEndPoint FRemoteSender;
		protected bool FReceiveSuccess;
		protected byte[] FReceivedBytes;
		
		/// <summary>
		/// Create the UDP server
		/// </summary>
		/// <param name="port">The listening port</param>
		public UDPServer(int port)
		{
			//setup listening port		
			CreateServer(port);
			
			//create empty sender object
			FRemoteSender = new IPEndPoint(IPAddress.Any, 0);
		}
		
		//declare the delegate
        public delegate void MessageReceivedEventHandler(object sender, UDPReceivedEventArgs e);
        
        /// <summary>
        /// This event is rised on incoming UPD data
        /// </summary>
		public event MessageReceivedEventHandler MessageReceived;
		
		//setup server
		protected void CreateServer(int port)
		{
			if(FServer != null)
			{
				FServer.Close();
			}
			
			//server socket
			IPEndPoint ipep = new IPEndPoint(IPAddress.Any, port);
			FServer = new UdpClient(ipep);
		}
		
		/// <summary>
		/// Set port and restart server
		/// </summary>
		public virtual int Port
		{
			set
			{
				CreateServer(value);
				Start();
			}
		}
		
		//message callback
		protected virtual void ReceiveCallback(IAsyncResult ar)
		{
			try
			{
				FReceivedBytes = FServer.EndReceive(ar, ref FRemoteSender);
				
				if (MessageReceived != null)
					MessageReceived(this, new UDPReceivedEventArgs(FRemoteSender, FReceivedBytes));
				
				FReceiveSuccess = true;
			}
			catch
			{
				FReceiveSuccess = false;
			}
			
			//restart
			Start();
		}
		
		/// <summary>
		/// Starts the listening callback loop on the configurated port.
		/// </summary>
		public virtual void Start()
		{
			try
			{
				FServer.BeginReceive(new AsyncCallback(ReceiveCallback), null);
			}
			catch
			{}
		}
		
		/// <summary>
		/// Send UPD data
		/// </summary>
		/// <param name="data">The bytes to send</param>
		/// <param name="receiver">The remote receiver</param>
		public virtual void Send(byte[] data, IPEndPoint receiver)
		{
			try
			{
				FServer.Send(data, data.Length, receiver);
			}
			catch
			{}
		}
		
		/// <summary>
		/// Close socket
		/// </summary>
		public void Close()
		{
			FServer.Close();
		}
	}
}
