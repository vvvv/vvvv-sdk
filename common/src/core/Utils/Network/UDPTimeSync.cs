#region usings
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Timers;

using VVVV.Utils.Animation;

#endregion usings

namespace VVVV.Utils.Network
{
	public interface INetworkTimeSync
	{
		void Start();
		
		double ElapsedSeconds
		{
			get;
		}
	}
	
	#region timing server
	public class UDPTimeServer : INetworkTimeSync
	{
		protected Stopwatch FStopWatch;
		protected UdpClient FServer;
		
		#region clock
	 	private double FOffset;

		public double ElapsedSeconds
		{
			get
			{
				lock(FStopWatch)
				{
					return FStopWatch.Elapsed.TotalSeconds + FOffset;
				}
			}
		}
		
		public double Offset
		{
			get {return FOffset;}
			set {FOffset = value;}
		}
		
		public void ResetTime(double offset)
		{
			FOffset = offset;
			FStopWatch.Restart();
		}
		#endregion clock
		
		public UDPTimeServer(int port)
		{
			
			//init clock
			FStopWatch = new Stopwatch();
			FStopWatch.Start();
			
			CreateServer(port);
		}
		
		//setup server
		protected void CreateServer(int port)
		{
			if(FServer != null)
			{
				FServer.Close();
			}
			
			//FLogger.Log(LogType.Message, "Creating new socket on port: " + port);
			
			//server socket
			IPEndPoint ipep = new IPEndPoint(IPAddress.Any, port);
			FServer = new UdpClient(ipep);
		}
		
		//set port and restart server
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
			var sender = new IPEndPoint(IPAddress.Any, 0);
			
			try
			{
				Byte[] receiveBytes = FServer.EndReceive(ar, ref sender);
				//string receiveString = Encoding.ASCII.GetString(receiveBytes);
				//FLogger.Log(LogType.Message, "Received: {0} from {1}", receiveString, sender);
				
				FServer.Send(BitConverter.GetBytes(ElapsedSeconds), 8, sender);
				
			} 
			catch (Exception e)
			{
				//FLogger.Log(LogType.Message, "End Receive Aborted");
			}
			
			
			//restart
			Start();
		}
		
		//start callback loop
		public virtual void Start()
		{
			try
			{
				FServer.BeginReceive(new AsyncCallback(ReceiveCallback), null);
			}
			catch (Exception e)
			{
				//FLogger.Log(LogType.Message, "Start Receive Aborted");
			}
		}
		
		public void Close()
		{
			FServer.Close();
		}
		
	}
	#endregion timing server
	
	#region timing client
	public class UDPTimeClient : UDPTimeServer
	{
		System.Timers.Timer FTimer;
		IPEndPoint FRemoteServer;
		
		double FSendTime;
		
		public UDPTimeClient(string serverIP, int port)
			: base(port+1)
		{
			FTimer = new System.Timers.Timer(1000);
			FTimer.Elapsed += OnTimer;
			FTimer.Start();
			
			FRemoteServer = new IPEndPoint(IPAddress.Parse(serverIP), port);
			
			FNetDelay.Alpha = 0.9;
			FNetDelay.Thresh = 0.001;
			
			FOffset.Alpha = 0.9;
			FOffset.Thresh = 0.1;
		}
		
		private void OnTimer(object source, ElapsedEventArgs e)
		{
			var data = Encoding.ASCII.GetBytes("time");
			
			FSendTime = FStopWatch.Elapsed.TotalSeconds;
			FServer.Send(data, data.Length, FRemoteServer);
		}
		
		//internal upd client has server port + 1
		public override int Port
		{
			set
			{
				base.Port = value + 1;
				FRemoteServer.Port = value;
			}
		}
		
		public string IP
		{
			set
			{
				if (value == "localhost") value = "127.0.0.1";
				FRemoteServer.Address = IPAddress.Parse(value);
			}
		}
		
		IIRFilter FNetDelay;
		IIRFilter FOffset;
		
		protected override void ReceiveCallback(IAsyncResult ar)
		{
			//check network delay
			var receiveTime = FStopWatch.Elapsed.TotalSeconds;
			var netDelay = (receiveTime - FSendTime) * 0.5;
			netDelay = FNetDelay.Update(netDelay);
			
			var sender = new IPEndPoint(IPAddress.Any, 0);
			
			try
			{
				Byte[] receiveBytes = FServer.EndReceive(ar, ref sender);
				double serverTime = BitConverter.ToDouble(receiveBytes, 0);
				
				//estimate offset
				var offset = serverTime - (receiveTime - netDelay);
				
				offset = FOffset.Update(offset);
				
				Offset = offset;
				
				//FLogger.Log(LogType.Message, "Offset {0} from {1} with network delay of {2}", offset, sender, netDelay);
				
			} 
			catch (Exception e)
			{
				//FLogger.Log(LogType.Message, "End Receive Aborted");
			}
			
			//restart
			Start();
		}
		
	}
	#endregion timing client
	
}
