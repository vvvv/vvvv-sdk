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
	[PluginInfo(Name = "S", Category = "Network.Interprocess.String", 
	            AutoEvaluate = true, 
	            Author="ft", Help = "S/R nodes that allow to communicate between different vvvv instances", Tags = "")]
	#endregion PluginInfo
	public class SStringNode : IPluginEvaluate, IDisposable
	{
		#region pins
		[Input("Input Value", DefaultString = "")]
		IDiffSpread<string> FValueIn;

		[Input("SendString", IsSingle = true, DefaultString = "vvvv")]
		IDiffSpread<string> FChannelIn;
		
		#endregion pins

		#region fields
		[Import()]
		ILogger FLogger;
				
		private string logMe = "";
		
		private LocalMessageBroadcastPartner localMessageBroadcastPartner = null;
		
		private bool newPartner = false;
	
		private UInt32 messageVersion = 1; //never 0, so 0 can be used to know someone has never received a msg yet
		
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
				localMessageBroadcastPartner = new LocalMessageBroadcastPartner("vvvv", Utils.GetChannelPrefix(FValueIn) + FChannelIn[0]);
				LogNow(LogType.Debug, "[S.String] New LocalMessageBroadcastPartner created with id = " + localMessageBroadcastPartner.PartnerId);
				if (localMessageBroadcastPartner != null) {
					localMessageBroadcastPartner.OnPartnerJoined += 
						delegate(UInt32 partnerId, string partnerName) { 
							newPartner = true;
						};
				}
				else {
					LogNow(LogType.Debug, "[S.Color] Creating new LocalMessageBroadcastPartner FAILED ");
				}
			}
			
			if ( (FValueIn.IsChanged || newPartner) && localMessageBroadcastPartner != null) {
				newPartner = false;
				
				if (FValueIn.IsChanged) {
					//never return 0, because 0 means uninitialized
					if (messageVersion == UInt32.MaxValue) {
						messageVersion = 0;
					}
					messageVersion++;
				}
				
				//create a message that holds all slices
				byte[] msg = Utils.GenerateMessage(FValueIn, messageVersion);
				
				//LogNow(LogType.Debug, "[S.String] Trying to send message of size " + msg.Length);
				localMessageBroadcastPartner.BroadcastMessage(msg);
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