/*
 * Created by SharpDevelop.
 * User: frederik
 * Date: 8/01/2013
 * Time: 15:32
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
	[PluginInfo(Name = "WyphonSendTextures", Category = "Network", 
	            AutoEvaluate = true, 
	            Author="ft", Help = "Receieves DX9ex shared textures by other Wyphon partner applications", Tags = "")]
	#endregion PluginInfo
	public class WyphonSendTexturesNode : IPluginEvaluate, IDisposable
	{
		#region pins
		[Input("Description", StringType = StringType.String, IsSingle = false, DefaultString = "vvvv texture")]
		IDiffSpread<string> FDescriptionIn;

		[Input("Width")]
		IDiffSpread<uint> FWidthIn;

		[Input("Height")]
		IDiffSpread<uint> FHeightIn;
		
		[Input("Format")]
		IDiffSpread<uint> FFormatIn;

		[Input("Usage")]
		IDiffSpread<uint> FUsageIn;
		
		[Input("Handle")]
		IDiffSpread<uint> FHandleIn;

		#endregion pins

		#region fields
		[Import()]
		ILogger FLogger;

		private WyphonPartner wyphon = null;

		private ISpread<uint> SharedTextureHandles = new Spread<uint>(0);
		//private Dictionary<uint, ISpread<SharedTextureInfo>> SharedTexturesByHandle = new Dictionary<uint, ISpread<SharedTextureInfo>>();
		
		private int previousSpreadMax = 0;
		
		private string logMe = "";
		#endregion fields

		#region helper functions
		
		#endregion helper functions

		

		public WyphonSendTexturesNode()
		{
		}
				
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax) {
			//ignore spreadmax, use FHandle in as the reference spread !!!
			if ( (wyphon != WyphonNode.wyphonPartner || FHandleIn.IsChanged) && WyphonNode.wyphonPartner != null ) {
				wyphon = WyphonNode.wyphonPartner;
				
				//Share textures we didn't share before yet
				for (int i = 0; i < FHandleIn.SliceCount; i++) {
					uint handle = FHandleIn[i];

					LogNow(LogType.Debug, "checking if handle " + handle + " already shared");

					if ( SharedTextureHandles.IndexOf(handle) > -1 ) {
						LogNow(LogType.Debug, "YES : " + handle + " already shared");
						//already shared
					}
					else if (handle == 0) {
						LogNow(LogType.Debug, "Ignore handle = 0, we will not share a texture with handle=0");
					}
					else {
						LogNow(LogType.Debug, "NO : " + handle + " NOT SHARED YET, we will try to share it now");
						bool success = wyphon.ShareD3DTexture(handle, FWidthIn[i], FHeightIn[i], FFormatIn[i], FUsageIn[i], FDescriptionIn[i]);
						if (success) {
							SharedTextureHandles.Add(handle);
							LogNow(LogType.Debug, "Successfully shared texture with handle " + handle + " ");
						}
						else {
							LogNow(LogType.Debug, "Sharing texture with handle " + handle + " FAILED !!!");
						}
					}
				}
				
				//Unshare textures that have disappeared from the spread
				foreach (uint handle in SharedTextureHandles) {
					LogNow(LogType.Debug, "Should the texture with handle " + handle + " still be shared?");
					if ( FHandleIn.IndexOf(handle) == -1) {
						LogNow(LogType.Debug, "NO : stop sharing " + handle + "");
						wyphon.UnshareD3DTexture(handle);
						SharedTextureHandles.Remove(handle);
					}
					else {
						LogNow(LogType.Debug, "YES : keep sharing " + handle + "");
					}
				}
				
				previousSpreadMax = SpreadMax;						
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
			foreach (uint handle in SharedTextureHandles) {
				wyphon.UnshareD3DTexture(handle);
			}
			SharedTextureHandles.SliceCount = 0;
			
			// Take yourself off the Finalization queue
			// to prevent finalization code for this object
			// from executing a second time.
			GC.SuppressFinalize(this);
		}
	}

}
