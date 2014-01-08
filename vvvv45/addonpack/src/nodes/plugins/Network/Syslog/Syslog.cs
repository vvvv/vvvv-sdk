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
	[PluginInfo(Name = "Syslog", Category = "VVVV", Help = "log TTY messages to a specified Syslog Server", Tags = "Network, Udp")]
	#endregion PluginInfo
	public class SyslogServerNode : IPluginEvaluate
	{
		#region fields & pins		
		[Input("Mac Adress", DefaultString = "00:00:00:00:00:00")]
		ISpread<string> FMac;
		
		[Input("Port", DefaultValue = 50000)]
		ISpread<int> FPort;

		
		[Output("Status")]
		ISpread<string> FStatus;
		
//		[Import()]
//		ILogger FLogger;
		#endregion fields & pins	
		
		
		public void Evaluate(int SpreadMax)
		{
			FStatus.SliceCount = 1;
			
			
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
