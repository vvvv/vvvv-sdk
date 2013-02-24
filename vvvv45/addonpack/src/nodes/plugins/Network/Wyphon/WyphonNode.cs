#region usings
using System;
using System.ComponentModel.Composition;
using System.Collections.Generic;
using System.Collections.Concurrent;

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
	[PluginInfo(Name = "Wyphon", Category = "Network", Author="ft", Help = "Share DX9ex shared textures", Tags = "")]
	#endregion PluginInfo
	public class WyphonNode : IPluginEvaluate, IDisposable
	{
		#region pins
		[Input("Name", StringType = StringType.String, IsSingle = true, DefaultString = "vvvv")]
		IDiffSpread<string> FNameIn;
		
		[Output("Wyphon Id", IsSingle = true, Visibility = PinVisibility.True)]
		ISpread<UInt32> FIdOut;

		[Output("Partner Id", IsSingle = false, Visibility = PinVisibility.True)]
		ISpread<UInt32> FPartnerIdOut;

		[Output("Partner Name", IsSingle = false, Visibility = PinVisibility.True)]
		ISpread<string> FPartnerNameOut;

		//for debugging
		[Output("Status", IsSingle = true, Visibility = PinVisibility.True)]
		ISpread<string> FStatusOut;
		
		#endregion pins

		
		#region fields
		[Import()]
		ILogger FLogger;
		
		private UInt32 previousPartnersUpdatedVersion = 0;
		
		private static string logMe = "";
		
		private static string wyphonPartnerForAllNodesName = null;

		private static WyphonPartner wyphonPartnerForAllNodes = null;


		public static Object partnersLock = new Object();
		public static ISpread<UInt32> PartnerIds = new Spread<UInt32>();
		public static ISpread<string> PartnerNames = new Spread<string>();
		private static UInt32 partnersUpdatedVersion = 0; //whenever this goes up, something changed
		
		
		public static Object sharedTexturesLock = new Object();
		public static Dictionary<UInt32, ISpread<SharedTextureInfo>> SharedTexturesPerPartner = new Dictionary<UInt32, ISpread<SharedTextureInfo>>();
//		static BlockingCollection<SharedTextureInfo> newSharedTexturesForMainLoop = new BlockingCollection<SharedTextureInfo>();
//		static BlockingCollection<SharedTextureInfo> obsoleteSharedTexturesForMainLoop = new BlockingCollection<SharedTextureInfo>();
		//static List<SharedTextureInfo> newSharedTexturesForMainLoop = new List<SharedTextureInfo>();
		//static List<SharedTextureInfo> obsoleteSharedTexturesForMainLoop = new List<SharedTextureInfo>();

		private static UInt32 texturesUpdatedVersion = 0; //whenever this goes up, something changed

		private bool disposed = false;
		
		#endregion fields

		#region properties
		
		public static string wyphonPartnerName {
			get { return wyphonPartnerForAllNodesName; }
			set { 
				wyphonPartnerForAllNodesName = value; 
				if (wyphonPartnerForAllNodes != null) {
					Log(LogType.Debug, "Disposing wyphonPartnerForAllNodes...");
					wyphonPartnerForAllNodes.Dispose();
				}
				wyphonPartnerForAllNodes = null;
			}
		}
		
		public static WyphonPartner wyphonPartner {
			get { 
				if ( wyphonPartnerForAllNodes == null && wyphonPartnerForAllNodesName != null ) {
					Log(LogType.Debug, "Creating NEW wyphonPartnerForAllNodes ");
					wyphonPartnerForAllNodes = new WyphonPartner( wyphonPartnerForAllNodesName );
					
					wyphonPartnerForAllNodes.WyphonPartnerJoinedEvent += WyphonPartnerJoined;
					wyphonPartnerForAllNodes.WyphonPartnerLeftEvent += WyphonPartnerLeft;
					wyphonPartnerForAllNodes.WyphonPartnerD3DTextureSharedEvent += WyphonD3DTextureShared;
					wyphonPartnerForAllNodes.WyphonPartnerD3DTextureUnsharedEvent += WyphonD3DTextureUnshared;

					if ( wyphonPartnerForAllNodes == null ) {
						Log(LogType.Error, "Returning wyphonPartnerForAllNodes = NULL ????");
					}
					else {
						UInt32 partnerId = 999999;
						try {
							partnerId = wyphonPartnerForAllNodes.PartnerId;
						}
						catch (Exception e) {
							Log(LogType.Error, "Getting partnerId seemed to be impossible\n" + e.Message + "\n" + e.StackTrace);
						}
						Log(LogType.Debug, "Returning wyphonPartnerForAllNodes " + partnerId);
					}
				}
				return wyphonPartnerForAllNodes; 
			}
		}

		public static UInt32 texturesVersion {
			get { return texturesUpdatedVersion; }
		}
		
		public bool Disposed {
			get { return disposed; }
		}

		#endregion properties
		
		#region WyphonCallbackDelegates
		
		private static void WyphonPartnerJoined(UInt32 partnerId, string partnerName) {
			try {
				Log(LogType.Debug, "WyphonPartner joined with id=" + partnerId + " and name=" + partnerName);
				
				lock (partnersLock) {
					PartnerIds.Add(partnerId);
					PartnerNames.Add(partnerName);
					
					Log(LogType.Debug, "partnersVersion is now " + partnersUpdatedVersion);
					updatePartnersVersion();
					Log(LogType.Debug, "partnersVersion has been updated to " + partnersUpdatedVersion);
				}
			} catch (Exception e) {
				Log(LogType.Error, "[WyphonPartnerJoined Exception] " + e.Message + "\n" + e.StackTrace);				
			}
		}

		private static void WyphonPartnerLeft(UInt32 partnerId) {
			try {
				Log(LogType.Debug, "WyphonPartner LEFT with id=" + partnerId);
	
				lock (partnersLock) {
					int index = PartnerIds.IndexOf(partnerId);
					if (index < 0) {
						Log(LogType.Error, "WHAT'S THIS? parter with id " + partnerId + " NOT FOUND IN OUR LIST?");
					}
					
					//should there be multiple instances (errors in the list), clean up all occurrences
					while (index > -1) {
						lock (sharedTexturesLock) {
							ISpread<SharedTextureInfo> spread;
							if ( SharedTexturesPerPartner.TryGetValue(partnerId, out spread) ) {
								SharedTexturesPerPartner[partnerId].SliceCount = 0;
								SharedTexturesPerPartner.Remove(partnerId);

								updateTexturesVersion();
							}
						}
						
						PartnerIds.RemoveAt(index);
						PartnerNames.RemoveAt(index);
						
						index = PartnerIds.IndexOf(partnerId);
					}
					
					//Log(LogType.Debug, "partnersVersion is now " + partnersUpdatedVersion);
					updatePartnersVersion();
					//Log(LogType.Debug, "partnersVersion has been updated to " + partnersUpdatedVersion);
				}
			} catch (Exception e) {
				Log(LogType.Error, "[WyphonPartnerLeft Exception] " + e.Message + "\n" + e.StackTrace);				
			}

		}
		
		private static void WyphonD3DTextureShared(UInt32 sendingPartnerId, UInt32 sharedTextureHandle, UInt32 width, UInt32 height, UInt32 format, UInt32 usage, string description) {
			try {
				Log(LogType.Debug, "WyphonPartner " + sendingPartnerId +  " shared a new texture with handle " + sharedTextureHandle + " " + width + "x" + height + " fmt=" + format + " usg=" + usage + " descr=" + description);
				
				SharedTextureInfo sharedTextureInfo = new SharedTextureInfo(sendingPartnerId, sharedTextureHandle, width, height, format, usage, description);
				
				lock (sharedTexturesLock) {
					ISpread<SharedTextureInfo> spread;
					if ( SharedTexturesPerPartner.TryGetValue(sendingPartnerId, out spread) ) {
						//remove same handle if already in spread
						RemoveFromTextureInfoSpread(spread, sharedTextureInfo.textureHandle);
	
						Log(LogType.Debug, "HEY I FOUND A shared textures SPREAD for partner " + sendingPartnerId +  " with slicecount " + spread.SliceCount + " !!!");
						spread.Add( sharedTextureInfo );
					}
					else {
						Log(LogType.Debug, "HEY I DIDN'T FIND A shared textures SPREAD for partner " + sendingPartnerId + " so I should create one !!!");
						SharedTexturesPerPartner[sendingPartnerId] = new Spread<SharedTextureInfo>();
						SharedTexturesPerPartner[sendingPartnerId].Add( sharedTextureInfo );
					}
					
					//for MainLoop
					//newSharedTexturesForMainLoop.Add(sharedTextureInfo);
					
					updateTexturesVersion();
				}
			} catch (Exception e) {
				Log(LogType.Error, "[WyphonD3DTextureShared Exception] " + e.Message + "\n" + e.StackTrace);				
			}
		}
				
		private static void WyphonD3DTextureUnshared(UInt32 sendingPartnerId, UInt32 sharedTextureHandle, UInt32 width, UInt32 height, UInt32 format, UInt32 usage, string description) {
			try {
				Log(LogType.Debug, "WyphonPartner " + sendingPartnerId +  " STOPPED sharing the texture with handle " + sharedTextureHandle);
	
				lock (sharedTexturesLock) {
					ISpread<SharedTextureInfo> spread;
					if ( SharedTexturesPerPartner.TryGetValue(sendingPartnerId, out spread) ) {
						//Log(LogType.Debug, "HEY I FOUND A shared textures SPREAD for partner " + sendingPartnerId +  " with slicecount " + spread.SliceCount + " !!!");
						
						RemoveFromTextureInfoSpread(spread, sharedTextureHandle);
					}
					
					updateTexturesVersion();
				}
			} catch (Exception e) {
				Log(LogType.Error, "[WyphonD3DTextureUNShared Exception] " + e.Message + "\n" + e.StackTrace);				
			}
		}
		
		#endregion WyphonCallbackDelegates

		#region helper functions
		static void updateTexturesVersion() {
			texturesUpdatedVersion = newVersion(texturesUpdatedVersion);
		}

		static void updatePartnersVersion() {
			partnersUpdatedVersion = newVersion(partnersUpdatedVersion);
		}
		
		static UInt32 newVersion(UInt32 currentVersion) {
			if (currentVersion < UInt32.MaxValue) {
				return currentVersion + 1;
			}
			else {
				return 0;
			}			
		}

		
		/// <summary>
		/// Removes the textureInfo from a spread that is about the given textureHandle
		/// </summary>
		/// <param name="spread"></param>
		/// <param name="sharedTextureHandle"></param>
		static void RemoveFromTextureInfoSpread(ISpread<SharedTextureInfo> spread, UInt32 sharedTextureHandle)
		{
			for ( int i = spread.SliceCount - 1; i >= 0; i--) {
				if (spread[i].textureHandle == sharedTextureHandle) {
					//Log(LogType.Debug, "HEY I FOUND THE UNSHARED TEXTURE " + sharedTextureHandle +  " IN THE LIST at position " + i + " !!!");

					//for MainLoop
					//obsoleteSharedTexturesForMainLoop.Add(spread[i]);

					//Does this give trouble maybe???
					spread.RemoveAt(i);
				}
			}
		}

		#endregion helper functions

		
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax) {

			try {
	            if (FNameIn.IsChanged && FNameIn.SliceCount > 0) {
					LogNow(LogType.Debug, "[Evaluate] FNameIn.IsChanged: creating a new WyphonPartner...");
					wyphonPartnerName = FNameIn[0];
					
					//Clean up old 'status' data
					FPartnerIdOut.SliceCount = 0;
					FPartnerNameOut.SliceCount = 0;
					lock (sharedTexturesLock) {
						SharedTexturesPerPartner.Clear();
						updateTexturesVersion();
					}
					lock(partnersLock) {
						PartnerIds.SliceCount = 0;
						PartnerNames.SliceCount = 0;
						updatePartnersVersion();
					}
					//Cleanup done...
					
					
					LogNow(LogType.Debug, "[Evaluate] WyphonPartner name set to " + FNameIn[0] + " !!!");
					FIdOut.SliceCount = 1;
					LogNow(LogType.Debug, "[Evaluate] slicecount set to 1 !!!");
					
					if ( wyphonPartner != null ) {
						LogNow(LogType.Debug, "[Evaluate] NEW WyphonPartner created successfully !!!");
						
						FIdOut[0] = wyphonPartner.PartnerId;
						LogNow(LogType.Debug, "[Evaluate] FIdOut[0] set to " + FIdOut[0]);
						
						FStatusOut[0] = "ok id=" + wyphonPartner.PartnerId.ToString();
						//LogNow(LogType.Debug, "NEW WyphonPartner !!!");						
					}
					else {
						LogNow(LogType.Error, "[Evaluate] NO WyphonPartner created: PROBLEM !!!");
	
						FStatusOut[0] = "PROBLEM";
						//LogNow(LogType.Error, "WyphonPartner IS NULL (shouldn't be)!!!");
					}
	            }
			} catch (Exception e) {
				Log(LogType.Error, "[WyphonNode Evaluate Exception] creating new wyphon partner: " + e.Message + "\n" + e.StackTrace);				
			}


//			FStatusOut[0] = "ok id=" + wyphonPartner.PartnerId.ToString();
			
			
			try {
				if (previousPartnersUpdatedVersion != partnersUpdatedVersion) {				
					lock (partnersLock) {
						
						LogNow(LogType.Debug, "[Evaluate] Something changed in WyphonPartners, so I will update the list of partners...");
		
	//					foreach (UInt32 pId in PartnerIds) {
	//						LogNow(LogType.Debug, "[Evaluate] found partner with id " + pId);					
	//					}
	//	
	//					foreach (string pName in PartnerNames) {
	//						LogNow(LogType.Debug, "[Evaluate] found partner with name " + pName);					
	//					}
		
		
						if (FPartnerIdOut == null) {
							LogNow(LogType.Error, "[Evaluate] FPartnerIdOut == null. PROBLEM !!!");
						}
						else {
							
							//LogNow(LogType.Debug, "[Evaluate] set partnerids slicecount = 0");
							FPartnerIdOut.SliceCount = 0;
							//LogNow(LogType.Debug, "[Evaluate] addrange to partnerids");
							FPartnerIdOut.AddRange(PartnerIds);
							//LogNow(LogType.Debug, "[Evaluate] set partnernames slicecount = 0");
							FPartnerNameOut.SliceCount = 0;
							//LogNow(LogType.Debug, "[Evaluate] addrange to slicecount");
							FPartnerNameOut.AddRange(PartnerNames);
							//Seems to fail FPartnerNameOut.AddRange( PartnerNames );
							LogNow(LogType.Debug, "[Evaluate] updating partners done...");
						}
						
						previousPartnersUpdatedVersion = partnersUpdatedVersion;
					}
				}
			} catch (Exception e) {
				Log(LogType.Error, "[WyphonNode Evaluate Exception] updating partners: " + e.Message + "\n" + e.StackTrace);				
			}
			
			try {
				lock (logMe) {
					if (logMe.Length > 0) {
						LogNow(LogType.Message, logMe);
						logMe = "";
					}
				}
			} catch (Exception e) {
				Log(LogType.Error, "[WyphonNode Evaluate Exception] while logging: " + e.Message + "\n" + e.StackTrace);				
			}
		}
		
		
		public void LogNow(LogType logType, string message) {
			FLogger.Log( logType, message);
		}
 
		public static void Log( LogType logType, string message)
		{
			lock (logMe) {
				logMe += "\n" + (logType == LogType.Error ? "ERR " : (logType == LogType.Warning ? "WARN " : "")) + message;
			}
		}

		public void Dispose() {
			if ( ! disposed ) {
				try {
					wyphonPartnerName = null;
				} catch {}
					
				// Take yourself off the Finalization queue
				// to prevent finalization code for this object
				// from executing a second time.
				GC.SuppressFinalize(this);
				
				disposed = true;
			}
		}

	}
}
