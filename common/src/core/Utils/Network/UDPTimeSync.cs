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
		
		/// <summary>
		/// If on server, this sets the time to a given value
		/// </summary>
		/// <param name="time"></param>
		void SetTime(double time = 0);
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
			FStopWatch.Start();
			
			Debug.WriteLine(Stopwatch.Frequency.ToString());
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
		/// <param name="time">Optionally set the time to given value</param>
		public virtual void SetTime(double time = 0)
		{
			FOffset = time;
			FStopWatch.Restart();
		}
		#endregion clock
		
		//message callback
		protected override void ReceiveCallback(IAsyncResult ar)
		{
			base.ReceiveCallback(ar);
			
			if(FReceiveSuccess && !(this is UDPTimeClient))
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
			
			FNetDelayFilter.Alpha = 0.95;
			FNetDelayFilter.Thresh = 0.01;
			FNetDelayFilter.Value = -1;
			
			FTimeOffsetFilter.Alpha = 0.98;
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
				
				Debug.WriteLine(serverTime);
				
				//estimate offset
				var offset = serverTime - (receiveTime - netDelay);
				
				FTimeOffsetFilter.Update(offset);
				Debug.WriteLine("offset: " + FTimeOffsetFilter.Value);
				
				if(Math.Abs(FTimeOffsetFilter.Value) > 0.01)
					Offset = FTimeOffsetFilter.Value;
			}
		}
		
		public override void SetTime(double time = 0)
		{
			//client can't change the time offset
		}
		
	}
	#endregion timing client
	
}
