/*
 * Created by SharpDevelop.
 * User: frederik
 * Date: 7/12/2012
 * Time: 18:19
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

#region usings

using System;
using System.Collections.Generic;

using System.ComponentModel.Composition;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V2.EX9;
using VVVV.Core.Logging;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;
//using VVVV.Utils.SlimDX;

#endregion usings

namespace VVVV.Nodes.Network
{
	/// <summary>
	/// Description of MyClass.
	/// </summary>
	#region PluginInfo
	[PluginInfo(Name = "WyphonPartner", Category = "Network", Version = "", Author = "ft", Help = "Share DX9ex shared textures", Tags = "Syphon, Wyphon, DX9ex", Credits = "Frederik Tilkin", Bugs = "definitely", Warnings = "")]
	#endregion PluginInfo
	public class WyphonPartnerNode : IPluginEvaluate, IDisposable
	{

		[Import()]
		IPluginHost FHost;

		[Import()]
		VVVV.Core.Logging.ILogger FLogger;


		#region pins

//		[Input("Play", DefaultValue = 1)]
//		IDiffSpread<bool> FPlayIn;
//
//		[Input("Loop", DefaultValue = 0)]
//		IDiffSpread<bool> FLoopIn;
//
//        [Input("Loop Start Time", DefaultValue = 0)]
//        IDiffSpread<float> FLoopStartIn;
//
//        [Input("Loop End Time", DefaultValue = 999999)]
//        IDiffSpread<float> FLoopEndIn;
//
//		[Input("Do Seek", DefaultValue = 0, IsBang = true)]
//		IDiffSpread<bool> FDoSeekIn;
//
//		[Input("Seek Time", DefaultValue = 0)]
//		IDiffSpread<float> FSeekTimeIn;
//
//		[Input("Speed", DefaultValue = 1)]
//		IDiffSpread<float> FSpeedIn;        
//
//		[Input("Volume", DefaultValue = 1)]
//		IDiffSpread<float> FVolumeIn;
//
//		[Input("Forced Width", DefaultValue = 0, Visibility = PinVisibility.OnlyInspector)]
//		IDiffSpread<int> FWidthIn;
//
//		[Input("Forced Height", DefaultValue = 0, Visibility = PinVisibility.OnlyInspector)]
//		IDiffSpread<int> FHeightIn;
//
//		[Input("Rotate", DefaultValue = 0, Visibility = PinVisibility.False)]
//		IDiffSpread<int> FRotateIn;

		[Input("Name", /*StringType = StringType.Filename, */ DefaultString = "vvvv")]
		IDiffSpread<string> FFileNameIn;


		[Output("Partner Name")]
		ISpread<string> FPartnerNameOut;

		[Output("Width", Visibility = PinVisibility.OnlyInspector)]
		ISpread<int> FWidthOut;

		[Output("Height", Visibility = PinVisibility.OnlyInspector)]
		ISpread<int> FHeightOut;
		
		
//		[Output("FrameCount")]
//		ISpread<int> FFrameCountOut;
//
//		
//		[Output("Position")]
//		ISpread<float> FPositionOut;
//
//
//		[Output("Width", Visibility = PinVisibility.OnlyInspector)]
//		ISpread<int> FWidthOut;
//
//		[Output("Height", Visibility = PinVisibility.OnlyInspector)]
//		ISpread<int> FHeightOut;
//
//		[Output("Texture Aspect Ratio", DefaultValue = 1, Visibility = PinVisibility.OnlyInspector)]
//		ISpread<float> FTextureAspectRatioOut;
//
//		[Output("Pixel Aspect Ratio", DefaultValue = 1, Visibility = PinVisibility.OnlyInspector)]
//		ISpread<float> FPixelAspectRatioOut;
//
//		[Output("Next Ready", Visibility = PinVisibility.Hidden)]
//		ISpread<bool> FNextReadyOut;

		#endregion pins
		

		public void Evaluate(int spreadMax) {
			LogNow(LogType.Debug, "Hello, spreadmax=" + spreadMax);
		}
		
		
		public void LogNow(LogType logType, string message)
		{
			FLogger.Log( logType, message);
		}

		public void Dispose() {
			
		}

	}
}