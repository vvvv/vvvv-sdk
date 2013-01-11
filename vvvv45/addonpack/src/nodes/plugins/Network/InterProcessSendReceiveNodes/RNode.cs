/*
 * Created by SharpDevelop.
 * User: frederik
 * Date: 11/01/2013
 * Time: 14:33
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
	[PluginInfo(Name = "R", Category = "Network Interprocess", 
	            AutoEvaluate = false, 
	            Author="ft", Help = "S/R nodes that allow to communicate between different vvvv instances", Tags = "")]
	#endregion PluginInfo
	public class RNode : IPluginEvaluate, IDisposable
	{
		#region pins
		[Input("ReceiveString", IsSingle = true)]
		IDiffSpread<string> FChannelIn;

//		[Input("Default Value")]
//		IDiffSpread<string> FDefaultValueIn;
		
		[Output("Output Value")]
		ISpread<string> FValueOut;
		
		[Output("Found", IsSingle = true)]
		ISpread<bool> FFoundOut;
		
		#endregion pins

		#region fields
		[Import()]
		ILogger FLogger;
				
		private string logMe = "";
		
		private LocalMessageBroadcastPartner localMessageBroadcastPartner = null;
		
		private bool received = false;
		ISpread<string> receivedSpread = new Spread<string>();
		private uint lastVersion = 0;
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
//			Log(LogType.Debug, "[R] Received new message from " + sendingPartnerId + " of size " + msgLength);
			
			byte[] bytes = new byte[msgLength];
			System.Runtime.InteropServices.Marshal.Copy( msgData, bytes, 0, (int)msgLength );

			try {
				//only parse the message if it's a version we have never received before?
				//but in that case we need to know which partner sent it !!!
				
				//SO currently this can only work for 1 S and 1 R node
//				if ( GetVersion(bytes) != lastVersion || lastVersion == 0 ) {
					Utils.ProcessMessage( bytes, receivedSpread );
					received = true;
//				}
			} catch (Exception e) {
				Log(LogType.Debug, "[R Exception] while trying to processMessage." );				
			}

			Log(LogType.Debug, "[R] New spread has " + receivedSpread.SliceCount + " slices.");

//			try {
//				string debug = "";
//				for ( int i = 0; i < spread.SliceCount; i++ ) {
//					debug += ( i > 0 ? " | " : "" ) + spread[i];
//				}
//				Log(LogType.Debug, "[R] New spread contains: " + debug );
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
				localMessageBroadcastPartner = new LocalMessageBroadcastPartner("vvvv", FChannelIn[0]);
				LogNow(LogType.Debug, "[R] New LocalMessageBroadcastPartner created with id = " + localMessageBroadcastPartner.PartnerId);
				localMessageBroadcastPartner.OnMessage += OnMessageHandler;
			}
			
			if ( received ) {
				received = false;
				
				LogNow(LogType.Debug, "[R] Try to update output" );
				
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