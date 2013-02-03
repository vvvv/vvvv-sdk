/*
 * Created by SharpDevelop.
 * User: frederik
 * Date: 1/02/2013
 * Time: 9:39
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
using VVVV.Utils.Streams;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;
using VVVV.Core.Logging;
using LocalMessageBroadcast;

#endregion usings


namespace InterProcessSendReceiveNodes
{
	#region PluginInfo
	[PluginInfo(Name = "S", Category = "Network.Interprocess.Raw", 
	            AutoEvaluate = true, 
	            Author="ft", Help = "S/R nodes that allow to communicate between different vvvv instances", Tags = "")]
	#endregion PluginInfo
	public class SRawNode : IPluginEvaluate, IDisposable
	{
		#region pins
		[Input("Input Value", DefaultValue = 0.0)]
		IInStream<Stream> FStreamIn;

		[Input("SendString", IsSingle = true, DefaultString = "vvvv")]
		IDiffSpread<string> FChannelIn;
		
		#endregion pins

		#region fields
		[Import()]
		ILogger FLogger;
				
		private string logMe = "";
		
		private LocalMessageBroadcastPartner localMessageBroadcastPartner = null;
		
		private bool newPartner = false;
	
		private uint messageVersion = 1; //never 0, so 0 can be used to know someone has never received a msg yet
		
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
		
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax) {
			if (FChannelIn.IsChanged) {
				if ( localMessageBroadcastPartner != null ) {
					localMessageBroadcastPartner.Dispose();
					localMessageBroadcastPartner = null;
				}
				localMessageBroadcastPartner = new LocalMessageBroadcastPartner("vvvv", Utils.GetChannelPrefix(FStreamIn) + FChannelIn[0]);
				LogNow(LogType.Debug, "[S.Raw] New LocalMessageBroadcastPartner created with id = " + localMessageBroadcastPartner.PartnerId);
				localMessageBroadcastPartner.OnPartnerJoined += 
					delegate(uint partnerId, string partnerName) { 
						newPartner = true;
					};
			}
			
			if ( (FStreamIn.IsChanged || newPartner) && localMessageBroadcastPartner != null) {
				newPartner = false;
				
				if (FStreamIn.IsChanged) {
					//never return 0, because 0 means uninitialized
					if (messageVersion == uint.MaxValue) {
						messageVersion = 0;
					}
					messageVersion++;
				}
				
				//create a message that holds all slices
				LogNow(LogType.Debug, "[S.Raw] Trying to create message");
				byte[] msg = Utils.GenerateMessage(FStreamIn, messageVersion);
				
				LogNow(LogType.Debug, "[S.Raw] Trying to send message of size " + msg.Length);
				localMessageBroadcastPartner.BroadcastMessage(msg);
				LogNow(LogType.Debug, "[S.Raw] Message sent ");
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
