using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Timers;

using VVVV.Utils.Animation;

namespace VVVV.Utils.Network
{
	/// <summary>
	/// Common interface for network time server and client
	/// </summary>
	public interface INetworkTimeSync
	{
		/// <summary>
		/// Start serving or requesting time data
		/// </summary>
		void Start();
		
		/// <summary>
		/// The elapsed seconds since the time server was started.
		/// </summary>
		double ElapsedSeconds
		{
			get;
		}
	}
	
	#region timing server
	/// <summary>
	/// UDP Time server
	/// </summary>
	public class UDPTimeServer : UDPServer, INetworkTimeSync
	{
		protected Stopwatch FStopWatch;

		/// <summary>
		/// Create the UDP server
		/// </summary>
		/// <param name="port">The listening port</param>
		public UDPTimeServer(int port)
			: base(port)
		{
			//init clock
			FStopWatch = new Stopwatch();
		}
		
		#region clock
	 	private double FOffset;

	 	/// <summary>
	 	/// The elapsed time in seconds.
	 	/// </summary>
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
		
		/// <summary>
		/// Clock offset
		/// </summary>
		public double Offset
		{
			get {return FOffset;}
			set {FOffset = value;}
		}
		
		/// <summary>
		/// Reset time
		/// </summary>
		/// <param name="offset">Optionally set the time to given value</param>
		public void ResetTime(double offset = 0)
		{
			FOffset = offset;
			FStopWatch.Restart();
		}
		#endregion clock
		
		//message callback
		protected override void ReceiveCallback(IAsyncResult ar)
		{
			base.ReceiveCallback(ar);
			
			if(FReceiveSuccess)
				FInternalServer.Send(BitConverter.GetBytes(ElapsedSeconds), 8, FRemoteSender);
		}
	}
	
	#endregion timing server
	
	#region timing client
	public class UDPTimeClient : UDPTimeServer
	{
		System.Timers.Timer FTimer;
		IPEndPoint FRemoteTimeServer;
		
		double FSendTime;
		IIRFilter FNetDelayFilter;
		IIRFilter FTimeOffsetFilter;
		
		/// <summary>
		/// Create UDP time client
		/// </summary>
		/// <param name="serverIP">Server IP</param>
		/// <param name="port">Server port (internal listening port is set to server port+1)</param>
		public UDPTimeClient(string serverIP, int port)
			: base(port+1)
		{
			FTimer = new System.Timers.Timer(1000);
			FTimer.Elapsed += OnTimer;
			FTimer.Start();
			
			FRemoteTimeServer = new IPEndPoint(IPAddress.Parse(serverIP), port);
			
			FNetDelayFilter.Alpha = 0.9;
			FNetDelayFilter.Thresh = 0.001;
			
			FTimeOffsetFilter.Alpha = 0.9;
			FTimeOffsetFilter.Thresh = 0.1;
		}
		
		private void OnTimer(object source, ElapsedEventArgs e)
		{
			var data = Encoding.ASCII.GetBytes("time");
			
			FSendTime = FStopWatch.Elapsed.TotalSeconds;
			Send(data, FRemoteTimeServer);
		}
		
		/// <summary>
		/// Set port of time server (internal listening port is set to server port+1)
		/// </summary>
		public override int Port
		{
			set
			{
				base.Port = value + 1;
				FRemoteTimeServer.Port = value;
			}
		}
		
		/// <summary>
		/// Set IP if time server
		/// </summary>
		public string IP
		{
			set
			{
				if (value == "localhost") value = "127.0.0.1";
				FRemoteTimeServer.Address = IPAddress.Parse(value);
			}
		}
		
		protected override void ReceiveCallback(IAsyncResult ar)
		{
			//check network delay
			var receiveTime = FStopWatch.Elapsed.TotalSeconds;
			var netDelay = (receiveTime - FSendTime) * 0.5;
			netDelay = FNetDelayFilter.Update(netDelay);
			
			base.ReceiveCallback(ar);
			
			if(FReceiveSuccess)
			{
				double serverTime = BitConverter.ToDouble(FReceivedBytes, 0);
				
				//estimate offset
				var offset = serverTime - (receiveTime - netDelay);
				
				Offset = FTimeOffsetFilter.Update(offset);
			}
		}
		
	}
	#endregion timing client
	
}
