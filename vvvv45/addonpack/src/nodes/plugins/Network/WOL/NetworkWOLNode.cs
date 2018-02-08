#region usings
using System;
using System.ComponentModel.Composition;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Linq;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

using VVVV.Core.Logging;

#endregion usings

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "WOL", 
                Category = "Network", 
                Help = "Wake On Lan by a given MAC adress", 
                Tags = "",
                AutoEvaluate = true,
                Author = "sebl")]
	#endregion PluginInfo
	public class NetworkWOLNode : IPluginEvaluate
	{
		#region fields & pins		
		[Input("Mac Adress", DefaultString = "00:00:00:00:00:00")]
		ISpread<string> FMac;
		
		[Input("Port", DefaultValue = 50000)]
		ISpread<int> FPort;
		
		[Input("Wake", IsBang=true)]
		IDiffSpread<bool> FWake;
		
		[Output("Status")]
		ISpread<string> FStatus;
		
//		[Import()]
//		ILogger FLogger;
		#endregion fields & pins	
		
		
		public void Evaluate(int SpreadMax)
		{
			FStatus.SliceCount = SpreadMax;
			
			bool ON = false;
			
			// ------------------------------------------------- Only Wake em once
			if (FWake.IsChanged)
			{
				for (int i = 0; i < SpreadMax; i++)
				{
					if (FWake[i] == true) ON = true;
					//FLogger.Log(LogType.Debug, i + " = " + FWake[i]);
				}
			}
			
			if (ON)
			{
				ON = false;
				for (int i = 0; i < SpreadMax; i++)
				{
					if (FWake[i])
					{
						FStatus[i] = ""; // reset status msg
						
						//FLogger.Log(LogType.Debug, "iteration: " + i);
						
						// clear variables
						string[] macDigits = null;
						byte[] WOLSignal = new byte[102];
						
						// parse Mac address
						if (FMac[i].Contains("-"))
						{
							macDigits = FMac[i].Split('-');
						}
						else if (FMac[i].Contains(":"))
						{
							macDigits = FMac[i].Split(':');
						}else
						{
							macDigits =  Split(FMac[i],2).ToArray();
						}
						
						bool isValid = Regex.IsMatch(string.Join(":", macDigits), "^([0-9A-F]{2}[:-]){5}([0-9A-F]{2})$");
						
						if (isValid)
						{							
//							FStatus[i] = i + " - " + FMac[i] + " - Wake UP";
							FStatus[i] = "Wake UP";
							
							// ------------------------------------------------- Magic Packet Header
							for (int h = 0; h < 6; h++)
							{
								WOLSignal[h] = 0xFF;
							}
							
							// ------------------------------------------------- Magic Packet Body
							int start = 6;
							for (int j = 0; j < 16; j++)
							{
								for (int x = 0; x < 6; x++)
								{
									int index = start + j * 6 + x;
									WOLSignal[index] = Convert.ToByte(macDigits[x], 16);
									//FLogger.Log(LogType.Debug, "WOLsignal: " +Idx + " "+ WOLSignal[start + j * 6 + x]);
								}
							}
							
							// ------------------------------------------------- send Magic Packet 
							BroadcastUdpClient WOLclient = new BroadcastUdpClient(); 
							WOLclient.Connect(IPAddress.Broadcast, FPort[i]);
//							FLogger.Log(LogType.Debug, "wake " + string.Join(":", macDigits) + " via " + IPAddress.Broadcast + ":" + FPort[i]);
							WOLclient.Send(WOLSignal, WOLSignal.Length);
							
//							FStatus[i] = i + " - " + FMac[i] + " - Wake UP";
							FStatus[i] = "Wake UP";
							
						}else
						{
//							FStatus[i] = i + " - " + FMac[i] + " - incorrect MAC address!";
							FStatus[i] = "incorrect MAC address!";
							
						}
					}else
					{
//						FStatus[i] = i + " - " + FMac[i] + " - skipped";
						FStatus[i] = "skipped";
						//FLogger.Log(LogType.Debug, "iteration: " + i + " skipped");
					}
				}
			}
		}
		
		
		static IEnumerable<string> Split(string str, int chunkSize)
		{
			return Enumerable.Range(0, str.Length / chunkSize)
			.Select(i => str.Substring(i * chunkSize, chunkSize));
		}
		
		
		public class BroadcastUdpClient:UdpClient
		{
			public BroadcastUdpClient():base()
			{ }
			//this is needed to send broadcast packet ?
			public void SetClientToBrodcastMode()
			{
				if(this.Active)
				this.Client.SetSocketOption(SocketOptionLevel.Socket,
				SocketOptionName.Broadcast,0);
			}
		}
		
		
		
	}
}
