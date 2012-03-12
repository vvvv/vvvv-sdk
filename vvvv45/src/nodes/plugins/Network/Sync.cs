#region usings
using System;
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

		[Input("Port", DefaultValue = 3336, IsSingle = true)]
		IDiffSpread<int> FPort;
		
		[Output("Do Seek")]
		ISpread<bool> FDoSeekOut;
		
		[Output("Seek Position")]
		ISpread<double> FSeekTimeOut;
		
		[Output("Adjust System Time")]
		ISpread<int> FAdjustTimeOut;
		
		object FLock = new object();
		double FStreamTime;
		double FReceivedStreamTime;
		double FReceivedTimeStamp;
		double FLastUpdateTime;
		IIRFilter FAdjustTimeFilter;
		UDPServer FServer;
		IPEndPoint FRemoteServer; 
		
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
				}
				else
				{
					FServer.Port = FHost.IsBoygroupClient ? FPort[0] + 1 : FPort[0];
				}
				
				FRemoteServer = new IPEndPoint(IPAddress.Parse(FHost.BoygroupServerIP), FPort[0]);
			}
			
			//read stream time
			lock(FLock)
			{
				FStreamTime = FTime[0];
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
					FServer.Send(Encoding.ASCII.GetBytes(FStreamTime.ToString() + ";" + FHost.RealTime.ToString()), e.RemoteSender);
					
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
				
				FLogger.Log(LogType.Debug, "Received stream time = {0} and time stamp = {1}", FReceivedStreamTime, FReceivedTimeStamp);
			}
		}
		
		protected void ClientEvaluate()
		{
						
			//request sync data
			if((FHost.RealTime - FLastUpdateTime) > 0.5)
			{
				FLastUpdateTime = FHost.RealTime;
				FServer.Send(Encoding.ASCII.GetBytes("videosync"), FRemoteServer);
			}
			
			lock(FLock)
			{
				var offset = FHost.RealTime - FReceivedTimeStamp;
				var streamDiff = FStreamTime - (FReceivedStreamTime + offset);
				var doSeek = Math.Abs(streamDiff) > 2;
				
				FDoSeekOut[0] = doSeek;
				FSeekTimeOut[0] = FReceivedStreamTime + offset + 0.05;
				
				if(!doSeek)
				{
					FAdjustTimeOut[0] = (int)FAdjustTimeFilter.Update(streamDiff * 1000);
				}
				else
				{
					FAdjustTimeOut[0] = 0;
					FAdjustTimeFilter.Value = 0;
				}
			}
			
		}
		#endregion client code
		
		#region server code
		protected void ServerEvaluate()
		{
			lock(FLock)
			{
				FStreamTime = FTime[0];
			}
		}
		#endregion server code
		
		public void Dispose()
		{
			FServer.Close();
		}
		
	}
	
}
