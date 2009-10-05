/*
 * Created by SharpDevelop.
 * User: admin
 * Date: 04.09.2009
 * Time: 14:27
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Net.Sockets;

namespace VVVV.Nodes
{
	public enum TConnectStatus
	{
			NeverConnected,
			Connecting,
			Connected,
			Disconnected
	};
	
	/// <summary>
	/// Extends the TcpClient class to allow it to store a copy of the relevant
	/// VVVV node inputs for the slice that the TcpClient is associated with.  Also
	/// implements additional helpful behaviors such as keeping track of when the
	/// TcpClient is in the middle of an ansychronous connection attempt.
	/// </summary>
	public class TV4TcpClient: TcpClient
	{
		//these members match the inputs of the TCPDotNet VVVV node that are not
		//already recorded using the same data type in the TcpClient class
		public String FInput = "";
		public bool FEnabled = true;
		public bool FDoSend = false;
		public String FRemoteHost = null;
		public int FRemotePort = 0;
		//if true, we keep the last read data on the output pin until something new is read
		//if false, on every frame where nothing is read, we clear the output pin
		public bool FHoldOutput = false;	
		//if true, we keep reading in a given frame until all available data has been read
		//if false, we issue only one read command, which will read up to the number of bytes in our buffer
		public bool FReadGreedy = true;
		
		//We need to keep track of the status of this connection a little bit more
		//carefully than the .NET TcpClient does by default
		public TConnectStatus FConnectStatus = TConnectStatus.NeverConnected;
		
		//this is where we record if any connection attempts failed since the last Evaluate() call
		//We need to store the information because we can't inform a consumer until the
		//next time the node's Evaluate() function is called
		public bool FConnectFailSinceLastEvaluate = false;
		
		//starts an asynchronous attempt to connect to the specified server and port
		public void BeginConnectAndTrackStatus()
		{
			//make sure we have a server string and a valid port number
			if (FRemoteHost != null && FRemotePort >= 0 && FRemotePort <= 65535)
			{
				FConnectStatus = TConnectStatus.Connecting;
				try
        		{
        			base.BeginConnect(FRemoteHost, FRemotePort, new AsyncCallback(this.ConnectCallback), this);
        		}
        		catch (Exception e)
        		{
        			FConnectStatus = TConnectStatus.NeverConnected;
        			FConnectFailSinceLastEvaluate = true;
        		}
				
			}
		}
		
		protected void ConnectCallback(IAsyncResult asyncResult)
		{
			TConnectStatus connectStatus = TConnectStatus.Connected;
			
			try
			{
				EndConnect(asyncResult);
			}
			catch (Exception e)
			{
				connectStatus = TConnectStatus.NeverConnected;
				FConnectFailSinceLastEvaluate = true;
			}
			
			FConnectStatus = connectStatus;
		}
		
		public void Close(bool trackConnectStatus)
		{
			if (trackConnectStatus)
			{
				FConnectStatus = TConnectStatus.Disconnected;
			}
			base.Close();
		}
		
		public TV4TcpClient() : base()
		{
		}
		
		public TV4TcpClient(TV4TcpClient toCopy) : base()
		{
			FInput = toCopy.FInput;
			FEnabled = toCopy.FEnabled;
			FDoSend = toCopy.FDoSend;
			FRemoteHost = toCopy.FRemoteHost;
			FRemotePort = toCopy.FRemotePort;
			FHoldOutput = toCopy.FHoldOutput;
			FReadGreedy = toCopy.FReadGreedy;
			ReceiveBufferSize = toCopy.ReceiveBufferSize;
			ReceiveTimeout = toCopy.ReceiveTimeout;
			SendBufferSize = toCopy.SendBufferSize;
			SendTimeout = toCopy.SendTimeout;
		}
	}
}
