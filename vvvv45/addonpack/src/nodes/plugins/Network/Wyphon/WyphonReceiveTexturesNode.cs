/*
 * Created by SharpDevelop.
 * User: frederik
 * Date: 8/01/2013
 * Time: 17:32
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
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
	[PluginInfo(Name = "WyphonReceiveTextures", Category = "Network", 
	            AutoEvaluate = false, 
	            Author="ft", Help = "Share our DX9ex shared textures with other wyhon partner applications", Tags = "")]
	#endregion PluginInfo
	public class WyphonReceiveTexturesNode : IPluginEvaluate, IDisposable
	{
		#region pins
		[Input("Partner Id")]
		IDiffSpread<uint> FPartnerIdIn;

		[Input("Do Filter", IsSingle = true)]
		IDiffSpread<bool> FDoFilterIn;
		
		
		[Output("Description")]
		ISpread<string> FDescriptionOut;

		[Output("Width")]
		ISpread<uint> FWidthOut;

		[Output("Height")]
		ISpread<uint> FHeightOut;
		
		[Output("Format")]
		ISpread<uint> FFormatOut;

		[Output("Usage")]
		ISpread<uint> FUsageOut;
		
//		[Input("Format", EnumName = "TextureFormat")]
//        IDiffSpread<EnumEntry> FFormat;
//
//        [Input("Usage", EnumName = "TextureUsage")]
//        IDiffSpread<EnumEntry> FUsage;

		
		[Output("Handle")]
		ISpread<uint> FHandleOut;

		#endregion pins

		#region fields
		[Import()]
		ILogger FLogger;

		private uint previousTexturesVersion = 0;
						
		private string logMe = "";
		#endregion fields

		#region helper functions
		
		#endregion helper functions

		

		public WyphonReceiveTexturesNode()
		{
		}
		
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax) {
			//ignore spreadmax, use FHandle in as the reference spread !!!
			if (	( previousTexturesVersion != WyphonNode.texturesVersion ) 
					|| FPartnerIdIn.IsChanged
					|| FDoFilterIn.IsChanged
				) {
				
				LogNow(LogType.Debug, "Something happened with the textures shared by others, update our output !");
				
				lock (WyphonNode.sharedTexturesLock) {
					
					FDescriptionOut.SliceCount = 0;
					FWidthOut.SliceCount = 0;
					FHeightOut.SliceCount = 0;
					FFormatOut.SliceCount = 0;
					FUsageOut.SliceCount = 0;
					FHandleOut.SliceCount = 0;
					
					foreach ( uint partnerId in WyphonNode.SharedTexturesPerPartner.Keys ) {
						if ( ! (FDoFilterIn.SliceCount > 0 && FDoFilterIn[0]) || FPartnerIdIn.IndexOf(partnerId) >= 0 ) {
							//filter not enabled OR partnerId found in list => add this partner's textures to ouput

							ISpread<SharedTextureInfo> textureInfoSpread;
							if ( WyphonNode.SharedTexturesPerPartner.TryGetValue(partnerId, out textureInfoSpread) && textureInfoSpread != null) {
								
								foreach ( SharedTextureInfo textureInfo in textureInfoSpread ) {
									FDescriptionOut.Add(textureInfo.description);
									FWidthOut.Add(textureInfo.width);
									FHeightOut.Add(textureInfo.height);
									FFormatOut.Add(textureInfo.format);
									FUsageOut.Add(textureInfo.usage);
									FHandleOut.Add(textureInfo.textureHandle);
								}
								
							}
						}
					}
				}
				
				previousTexturesVersion = WyphonNode.texturesVersion;
			}
			
			if (logMe.Length > 0) {
				LogNow(LogType.Message, logMe);
				logMe = "";
			}			
		}
		
		public void LogNow(LogType logType, string message) {
			FLogger.Log( logType, message);
		}
 
		public void Log( LogType logType, string message)
		{
			logMe += "\n" + (logType == LogType.Error ? "ERR " : (logType == LogType.Warning ? "WARN " : "")) + message;
		}


		public void Dispose() {
			
			// Take yourself off the Finalization queue
			// to prevent finalization code for this object
			// from executing a second time.
			GC.SuppressFinalize(this);
		}
	}

}
