/*
 * Created by SharpDevelop.
 * User: frederik
 * Date: 6/01/2013
 * Time: 9:36
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
#region usings
using System;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using VVVV.Core.Logging;
using Wyphon;

#endregion usings

namespace VVVV.Nodes.Network.Wyphon
{
	#region PluginInfo
	[PluginInfo(Name = "WyphonPartners", Category = "Network", Author="ft", Help = "Share DX9ex shared textures", Tags = "")]
	#endregion PluginInfo
	public class WyphonPartnersNode : IPluginEvaluate, IDisposable
	{
		#region pins
//		[Input("Name", StringType = StringType.String, IsSingle = true, DefaultString = "vvvv")]
//		IDiffSpread<string> FFileNameIn;
		
		[Output("Partner Id", IsSingle = false, Visibility = PinVisibility.True)]
		ISpread<UInt32> FPartnerIdOut;

		[Output("Partner Name", IsSingle = false, Visibility = PinVisibility.True)]
		ISpread<string> FPartnerNameOut;
		
		#endregion pins

		
		#region fields
		[Import()]
		ILogger FLogger;

		private WyphonPartner wyphon = null;
		
		private bool partnersChanged = false;
		private bool texturesChanged = false;
		
		private string logMe;
		#endregion fields
		
		
		#region WyphonCallbackDelegates
		
//		private static void WyphonPartnerJoined(UInt32 partnerId, string partnerName) {
//			//Log(LogType.Debug, "WyphonPartner joined with id=" + partnerId + " and name=" + partnerName);
//			
//			//partnersChanged
//		}
//
//		private static void WyphonPartnerLeft(UInt32 partnerId) {
//			//Log(LogType.Debug, "WyphonPartner LEFT with id=" + partnerId);
//
//		}
//		
//		private static void WyphonD3DTextureShared(UInt32 sendingPartnerId, UInt32 sharedTextureHandle, UInt32 width, UInt32 height, UInt32 usage, string description) {
//			Log(LogType.Debug, "WyphonPartner " + sendingPartnerId +  " shared a new texture with handle " + sharedTextureHandle);
//			
//			SharedTextureInfo sharedTextureInfo = new SharedTextureInfo(sendingPartnerId, sharedTextureHandle, width, height, usage, description);
//			
//			lock (sharedTexturesLock) {
//				ISpread<SharedTextureInfo> spread;
//				if ( SharedTexturesPerPartner.TryGetValue(sendingPartnerId, out spread) ) {
//					spread.Add( sharedTextureInfo );
//				}
//				else {
//					spread = new Spread<SharedTextureInfo>();
//					spread.Add( sharedTextureInfo );
//					
//					SharedTexturesPerPartner[sendingPartnerId] = spread;
//				}
//				
//				//for MainLoop
//				newSharedTexturesForMainLoop.Add(sharedTextureInfo);
//			}
//		}
//		
//		private static void WyphonD3DTextureUnshared(UInt32 sendingPartnerId, UInt32 sharedTextureHandle, UInt32 width, UInt32 height, UInt32 usage, string description) {
//			Log(LogType.Debug, "WyphonPartner " + sendingPartnerId +  " STOPPED sharing the texture with handle " + sharedTextureHandle);
//
//			lock (sharedTexturesLock) {
//				ISpread<SharedTextureInfo> spread;
//				if ( SharedTexturesPerPartner.TryGetValue(sendingPartnerId, out spread) ) {
//					for ( int i = spread.SliceCount - 1; i >= 0; i--) {
//						if (spread[i].textureHandle == sharedTextureHandle) {
//							//for MainLoop
//							obsoleteSharedTexturesForMainLoop.Add(spread[i]);
//	
//							spread.RemoveAt(i);
//						}
//					}
//				}
//			}
//		}
		#endregion WyphonCallbackDelegates

		public WyphonPartnersNode() {
		}
		
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			if (wyphon != WyphonNode.wyphonPartner) {
				LogNow(LogType.Debug, "New WyphonPartner. Regster event handlers !!! ");

				wyphon = WyphonNode.wyphonPartner;
			}
			
			if ( WyphonNode.PartnerIds != null ) {
				
				//WyphonNode.PartnerIds
//				FPartnerIdOut = WyphonNode.FPartnerIdOut;
//				FPartnerNameOut = WyphonNode.FPartnerNameOut;
//				FPartnerIdOut.SliceCount = 0;
//				foreach (UInt32 pId in WyphonNode.FPartnerIdOut) {
//					FPartnerIdOut.Add(pId);
//				}
			}
			
//			if (WyhonNode.partnersUpdated) {
//				lock (WyhonNode.partnersLock) {
//	
//					WyhonNode.partnersUpdated = false;
//					
//					LogNow(LogType.Error, "[Evaluate] Something changed in WyphonPartners, so I will update the list of partners...");
//	
//					foreach (UInt32 pId in PartnerIds) {
//						LogNow(LogType.Error, "[Evaluate] found partner with id " + pId);					
//					}
//	
//					foreach (string pName in PartnerNames) {
//						LogNow(LogType.Error, "[Evaluate] found partner with name " + pName);					
//					}
//	
//	
//					LogNow(LogType.Error, "[Evaluate] set partnerids slicecount = 0");
//					FPartnerIdOut.SliceCount = 0;
//					LogNow(LogType.Error, "[Evaluate] addrange to partnerids");
//					FPartnerIdOut.AddRange( PartnerIds );
//					LogNow(LogType.Error, "[Evaluate] set partnernames slicecount = 0");
//					FPartnerNameOut.SliceCount = 0;
//					LogNow(LogType.Error, "[Evaluate] addrange to slicecount");
//					FPartnerNameOut.AddRange( PartnerNames );
//					LogNow(LogType.Error, "[Evaluate] updating partners done...");
//				}
//			}

			
			LogNow(LogType.Debug, logMe);
		}
		
		public void Log( LogType logType, string message)
		{
			logMe += "\n" + (logType == LogType.Error ? "ERR " : (logType == LogType.Warning ? "WARN " : "")) + message;
		}

		public void LogNow(LogType logType, string message) {
			FLogger.Log( logType, message);
		}
 
		public void Dispose() {
			
			// Take yourself off the Finalization queue
			// to prevent finalization code for this object
			// from executing a second time.
			GC.SuppressFinalize(this);
		}

	}
}
