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
		protected UdpClient FInternalServer;
		protected IPEndPoint FLocalIPEndPoint;
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
			try 
			{
				if(FInternalServer != null)
				{
					FInternalServer.Close();
				}
				
				//server socket
				FLocalIPEndPoint = new IPEndPoint(IPAddress.Any, port);
				FInternalServer = new UdpClient(FLocalIPEndPoint);
				
				//avoid WSAECONNRESET error when sending to a closed/invalid end point, SIO_UDP_CONNRESET
				FInternalServer.Client.IOControl(-1744830452, new byte[]{0, 0, 0, 0}, new byte[]{0, 0, 0, 0});
				
			} 
			catch (Exception e)
			{
				System.Diagnostics.Debug.WriteLine(e.Message);
			}
				
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
				FReceivedBytes = FInternalServer.EndReceive(ar, ref FRemoteSender);
				
				//restart listening
				Start();
				
				//rise message received event
				if (MessageReceived != null)
					MessageReceived(this, new UDPReceivedEventArgs(FRemoteSender, FReceivedBytes));
				
				FReceiveSuccess = true;
			}
			catch
			{
				//restart
				Start();
				FReceiveSuccess = false;
			}

		}
		
		/// <summary>
		/// Starts the listening callback loop on the configurated port
		/// </summary>
		public virtual void Start()
		{
			try
			{
				FInternalServer.BeginReceive(new AsyncCallback(ReceiveCallback), null);
			}
			catch
			{}
		}
		
		/// <summary>
		/// Stop the listening callback loop
		/// </summary>
		public virtual void Stop()
		{
			try
			{
				CreateServer(FLocalIPEndPoint.Port);
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
				FInternalServer.Send(data, data.Length, receiver);
			}
			catch
			{}
		}
		
		/// <summary>
		/// Close socket
		/// </summary>
		public void Close()
		{
			if(FInternalServer != null)
				FInternalServer.Close();
		}
	}
}
