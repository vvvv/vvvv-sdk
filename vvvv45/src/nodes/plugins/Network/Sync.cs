#region usings
using System;
using System.Timers;
using System.Net;
using System.Text;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;
using VVVV.Utils.Network;
using VVVV.Utils.Animation;

using VVVV.Core.Logging;

#endregion usings

namespace VVVV.Nodes
{
	
	#region PluginInfo
	[PluginInfo(Name = "Sync", 
	Category = "Network", 
	Version = "FileStream",
	Help = "Syncronizes a FileStream node over network in a boygroup setup", 
	Tags = "",
	AutoEvaluate = true)]
	#endregion PluginInfo
	public class FileStreamNetworkSyncNode : IPluginEvaluate, IDisposable
	{
		#region fields & pins
		[Input("Time", IsSingle = true)]
		ISpread<double> FTime;
		
		[Input("Clock", IsSingle = true)]
		ISpread<double> FClock;

		[Input("Port", DefaultValue = 3336, IsSingle = true)]
		IDiffSpread<int> FPort;
		
		[Output("Do Seek")]
		ISpread<bool> FDoSeekOut;
		
		[Output("Seek Position")]
		ISpread<double> FSeekTimeOut;
		
		[Output("Adjust System Time")]
		ISpread<int> FAdjustTimeOut;
		
		[Output("Offset")]
		ISpread<double> FOffsetOut;
		
		[Output("Stream Offset")]
		ISpread<double> FStreamOffsetOut;
		
		object FLock = new object();
		double FStreamTime;
		double FTimeStamp;
		double FReceivedStreamTime;
		double FReceivedTimeStamp;
		double FLastUpdateTime;
		IIRFilter FAdjustTimeFilter;
		IIRFilter FStreamDiffFilter;
		UDPServer FServer;
		IPEndPoint FRemoteServer;
		Timer FTimer;
		
		[Import]
		IHDEHost FHost;

		[Import]
		ILogger FLogger;
		#endregion fields & pins
		
		public FileStreamNetworkSyncNode()
		{
			FAdjustTimeFilter.Value = 0;
			FAdjustTimeFilter.Thresh = 500;
			FAdjustTimeFilter.Alpha = 0.9;
			
			FStreamDiffFilter.Value = 0;
			FStreamDiffFilter.Thresh = 1;
			FStreamDiffFilter.Alpha = 0.9;
			
			FTimer = new Timer(500);
			FTimer.Elapsed += FTimer_Elapsed;
			FTimer.Start();
		}

		void FTimer_Elapsed(object sender, ElapsedEventArgs e)
		{
			//request sync data
			FServer.Send(Encoding.ASCII.GetBytes("videosync"), FRemoteServer);
		}
			
		
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			//set server and port
			if(FPort.IsChanged)
			{
				if(FServer == null)
				{
					FServer = FHost.IsBoygroupClient ? new UDPServer(FPort[0] + 1) : new UDPServer(FPort[0]);
					FServer.MessageReceived += FServer_MessageReceived;
					FServer.Start();
				}
				else
				{
					FServer.Port = FHost.IsBoygroupClient ? FPort[0] + 1 : FPort[0];
				}
				
				if(FHost.IsBoygroupClient)
					FRemoteServer = new IPEndPoint(IPAddress.Parse(FHost.BoygroupServerIP), FPort[0]);
			}
			
			//read stream time
			lock(FLock)
			{
				FStreamTime = FTime[0];
				FTimeStamp = FClock[0];
			}
			
			//do the evaluation for client or server
			if(FHost.IsBoygroupClient)
			{
				ClientEvaluate();
			}
			else
			{
				ServerEvaluate();
			}
		}

		//respond to udp message
		void FServer_MessageReceived(object sender, UDPReceivedEventArgs e)
		{
			if(FHost.IsBoygroupClient)
			{
				ReceiveServerAnswer(e.Data);
			}
			else //server code
			{
				lock(FLock)
				{
					FServer.Send(Encoding.ASCII.GetBytes(FStreamTime.ToString() + ";" + FTimeStamp.ToString()), e.RemoteSender);
					
					FLogger.Log(LogType.Debug, FStreamTime.ToString() + ";" + FHost.RealTime.ToString());
				}
			}
		}
		
		#region client code
		void ReceiveServerAnswer(byte[] data)
		{
			var s = Encoding.ASCII.GetString(data).Split(';');
			
			lock(FLock)
			{
				FReceivedStreamTime = Double.Parse(s[0]);
				FReceivedTimeStamp = Double.Parse(s[1]);
			}
		}
		
		protected void ClientEvaluate()
		{
			lock(FLock)
			{
				var offset = FTimeStamp - FReceivedTimeStamp;
				var streamDiff = (FReceivedStreamTime - offset) - FStreamTime;
				var doSeek = Math.Abs(streamDiff) > 2;
				
				FStreamDiffFilter.Update(streamDiff);
				
				FDoSeekOut[0] = doSeek;
				FSeekTimeOut[0] = FReceivedStreamTime + offset + 0.05;
				
				if(!doSeek)
				{
					FAdjustTimeOut[0] = Math.Sign(FStreamDiffFilter.Value) * 1;
				}
				else
				{
					FAdjustTimeOut[0] = 0;
					FAdjustTimeFilter.Value = 0;
				}
				
				FOffsetOut[0] = offset;
				FStreamOffsetOut[0] = streamDiff;
			}
			
		}
		#endregion client code
		
		#region server code
		protected void ServerEvaluate()
		{
			
		}
		#endregion server code
		
		public void Dispose()
		{
			FServer.MessageReceived -= FServer_MessageReceived;
			FTimer.Elapsed -= FTimer_Elapsed;
			FServer.Close();
		}
		
	}
	
}
