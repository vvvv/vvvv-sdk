/*
 * Created by SharpDevelop.
 * User: frederik
 * Date: 15/01/2013
 * Time: 9:39
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
/*
 * Created by SharpDevelop.
 * User: frederik
 * Date: 15/01/2013
 * Time: 8:57
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
#region usings
using System;
using System.Collections.Generic;

using System.ComponentModel.Composition;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using VVVV.Core.Logging;

using LocalMessageBroadcast;

#endregion usings


namespace InterProcessSendReceiveNodes
{
	#region PluginInfo
	[PluginInfo(Name = "R", Category = "Network.Interprocess.Color", 
	            AutoEvaluate = false, 
	            Author="ft", Help = "S/R nodes that allow to communicate between different vvvv instances", Tags = "")]
	#endregion PluginInfo
	public class RColorNode : IPluginEvaluate, IDisposable
	{
		#region pins
		[Input("ReceiveString", IsSingle = true, DefaultString = "vvvv")]
		IDiffSpread<string> FChannelIn;

//		[Input("Default Value")]
//		IDiffSpread<string> FDefaultValueIn;

//		I don't think we need filtering on partnedId: 
//		use good names and you can easily filter for the messages you are interested in
///////////////////////////////////////////////////////////////////////////////////////
//		[Input("Partner Id", Visibility = PinVisibility.OnlyInspector)]
//		IDiffSpread<uint> FPartnerIdIn;
//
//		[Input("Do Filter", IsSingle = true, Visibility = PinVisibility.OnlyInspector)]
//		IDiffSpread<bool> FDoFilterIn;

		
		[Output("Output Value")]
		ISpread<RGBAColor> FValueOut;
		
		[Output("Found", IsSingle = true)]
		ISpread<bool> FFoundOut;
		
		#endregion pins

		#region fields
		[Import()]
		ILogger FLogger;
				
		private string logMe = "";
		
		private LocalMessageBroadcastPartner localMessageBroadcastPartner = null;
		
		private bool received = false;
		ISpread<RGBAColor> receivedSpread = new Spread<RGBAColor>();
		private Dictionary<uint, uint> lastVersionPerPartner = new Dictionary<uint, uint>();
		
		#endregion fields

		#region helper functions
		public void LogNow(LogType logType, string message) {
			FLogger.Log( logType, message);
		}
 
		public void Log( LogType logType, string message)
		{
			logMe += "\n" + (logType == LogType.Error ? "ERR " : (logType == LogType.Warning ? "WARN " : "")) + message;
		}

		#endregion helper functions

		private void OnMessageHandler(uint sendingPartnerId, IntPtr msgData, uint msgLength) {
//			Log(LogType.Debug, "[R.Color] Received new message from " + sendingPartnerId + " of size " + msgLength);
			
			byte[] bytes = new byte[msgLength];
			System.Runtime.InteropServices.Marshal.Copy( msgData, bytes, 0, (int)msgLength );

			try {
				//correct type: not really necessary anymore since different nodes will send on different channels
				if (Utils.GetMessageType(bytes) == MessageTypeEnum.colorSpread) {
					//only parse the whole message if it's a version we have never received before from that partner
					uint currVersion = Utils.GetVersion(bytes);
					uint lastVersion;
					
					if ( ! lastVersionPerPartner.TryGetValue(sendingPartnerId, out lastVersion) || (currVersion != lastVersion) ) {
						Utils.ProcessMessage( bytes, receivedSpread );
						received = true;
						lastVersionPerPartner[sendingPartnerId] = currVersion;
					}
				}
			} catch (Exception e) {
				Log(LogType.Debug, "[R Exception] while trying to processMessage.\n" + e.Message + "\n" + e.StackTrace);
			}

			//Log(LogType.Debug, "[R.Color] New spread has " + receivedSpread.SliceCount + " slices.");

//			try {
//				string debug = "";
//				for ( int i = 0; i < spread.SliceCount; i++ ) {
//					debug += ( i > 0 ? " | " : "" ) + spread[i];
//				}
//				Log(LogType.Debug, "[R.Color] New spread contains: " + debug );
//			} catch (Exception e) {
//				Log(LogType.Debug, "[R Exception] while trying to build debug message" );				
//			}
		}

		
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax) {
			if (FChannelIn.IsChanged) {
				if ( localMessageBroadcastPartner != null ) {
					localMessageBroadcastPartner.Dispose();
					localMessageBroadcastPartner = null;
				}
				localMessageBroadcastPartner = new LocalMessageBroadcastPartner("vvvv", Utils.GetChannelPrefix(FValueOut) + FChannelIn[0]);
				LogNow(LogType.Debug, "[R.Color] New LocalMessageBroadcastPartner created with id = " + localMessageBroadcastPartner.PartnerId);
				localMessageBroadcastPartner.OnMessage += OnMessageHandler;
			}
			
			if ( received ) {
				received = false;
				
				//LogNow(LogType.Debug, "[R.Color] Try to update output" );
				
				FValueOut.SliceCount = receivedSpread.SliceCount;
				for (int i = 0; i < receivedSpread.SliceCount; i++ ) {
					FValueOut[i] = receivedSpread[i];
				}
	
			}
			
			
			if (logMe.Length > 0) {
				LogNow(LogType.Message, logMe);
				logMe = "";
			}

		}

		public void Dispose() {
			
			// Take yourself off the Finalization queue
			// to prevent finalization code for this object
			// from executing a second time.
			GC.SuppressFinalize(this);
		}

	}
}
