/*
 * Created by SharpDevelop.
 * User: frederik
 * Date: 1/02/2013
 * Time: 11:13
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
#region usings
using System;
using System.Collections.Generic;
using System.IO;

using System.ComponentModel.Composition;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;
using VVVV.Utils.Streams;

using VVVV.Core.Logging;

using LocalMessageBroadcast;

#endregion usings

/*
namespace InterProcessSendReceiveNodes
{
	#region PluginInfo
	[PluginInfo(Name = "R", Category = "Network.Interprocess.Raw", 
	            AutoEvaluate = false, 
	            Author="ft", Help = "S/R nodes that allow to communicate between different vvvv instances", Tags = "")]
	#endregion PluginInfo
	public class RRawNode : IPluginEvaluate, IDisposable
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
//		IDiffSpread<UInt32> FPartnerIdIn;
//
//		[Input("Do Filter", IsSingle = true, Visibility = PinVisibility.OnlyInspector)]
//		IDiffSpread<bool> FDoFilterIn;

		
		[Output("Output Value")]
		IOutStream<Stream> FStreamOut;
		
//		[Output("Found", IsSingle = true)]
//		ISpread<bool> FFoundOut;


//		[Output("Test")]
//		ISpread<Stream> FTestOut;
		
		#endregion pins

		#region fields
		[Import()]
		ILogger FLogger;
				
		private string logMe = "";
		
		private LocalMessageBroadcastPartner localMessageBroadcastPartner = null;
		
		private bool received = false;
		IOutStream<Stream> receivedStream;
		private Dictionary<UInt32, UInt32> lastVersionPerPartner = new Dictionary<UInt32, UInt32>();
		
//		ISpread<Stream> receivedStreamA = new Spread<Stream>();
//		ISpread<Stream> receivedStreamB = new Spread<Stream>();
//		private bool selectSpread = false;
		
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

//		public RRawNode() {
//			receivedStreamA.SliceCount = 1;
//			receivedStreamA[0] = 0;
//			receivedStreamB.SliceCount = 1;
//			receivedStreamB[0] = 1;
//		}
		
		private void OnMessageHandler(UInt32 sendingPartnerId, IntPtr msgData, UInt32 msgLength) {
//			Log(LogType.Debug, "[R.Raw] Received new message from " + sendingPartnerId + " of size " + msgLength);
			
			byte[] bytes = new byte[msgLength];
			System.Runtime.InteropServices.Marshal.Copy( msgData, bytes, 0, (int)msgLength );

			try {
				//correct type: not really necessary anymore since different nodes will send on different channels
				if (Utils.GetMessageType(bytes) == MessageTypeEnum.rawSpread) {
					//only parse the whole message if it's a version we have never received before from that partner
					UInt32 currVersion = Utils.GetVersion(bytes);
					UInt32 lastVersion;
					
					if ( ! lastVersionPerPartner.TryGetValue(sendingPartnerId, out lastVersion) || (currVersion != lastVersion) ) {
						Utils.ProcessMessage( bytes, receivedStream );
						received = true;
						lastVersionPerPartner[sendingPartnerId] = currVersion;
					}
				}
			} catch (Exception e) {
				Log(LogType.Debug, "[R Exception] while trying to processMessage.\n" + e.Message + "\n" + e.StackTrace);
			}

			//Log(LogType.Debug, "[R.Raw] New spread has " + receivedStream.SliceCount + " slices.");

//			try {
//				string debug = "";
//				for ( int i = 0; i < spread.SliceCount; i++ ) {
//					debug += ( i > 0 ? " | " : "" ) + spread[i];
//				}
//				Log(LogType.Debug, "[R.Raw] New spread contains: " + debug );
//			} catch (Exception e) {
//				Log(LogType.Debug, "[R Exception] while trying to build debug message" );				
//			}
		}

		
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax) {
//			FTestOut = selectSpread ? receivedStreamB : receivedStreamA;
//			selectSpread = ! selectSpread;
//			LogNow(LogType.Debug, "Test spread = " + (FTestOut == receivedStreamB ? "SPREAD 1" : "Spread 0" ) + " slice 0 = " + FTestOut[0] + (FTestOut.IsChanged ? "CHANGED" : "not changed" ) );
//			FTestOut[0] = FTestOut[0];
//			LogNow(LogType.Debug, (FTestOut.IsChanged ? "CHANGED" : "not changed" ) );
//			
			if (FChannelIn.IsChanged) {
				if ( localMessageBroadcastPartner != null ) {
					localMessageBroadcastPartner.Dispose();
					localMessageBroadcastPartner = null;
				}
				localMessageBroadcastPartner = new LocalMessageBroadcastPartner("vvvv", Utils.GetChannelPrefix(FStreamOut) + FChannelIn[0]);
				LogNow(LogType.Debug, "[R.Raw] New LocalMessageBroadcastPartner created with id = " + localMessageBroadcastPartner.PartnerId);
				localMessageBroadcastPartner.OnMessage += OnMessageHandler;
			}
			
			if ( received ) {
				received = false;
				
				//LogNow(LogType.Debug, "[R.Raw] Try to update output" );
				
				StreamUtils.GetSpreadMax(FStreamOut);
				FStreamOut.SliceCount = receivedStream.SliceCount;
				for (int i = 0; i < receivedStream.SliceCount; i++ ) {
					FValueOut[i] = receivedStream[i];
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
*/