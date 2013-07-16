#region usings
using System;
using System.ComponentModel.Composition;
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;
using System.Xml.Linq;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;
using VVVV.Core.Logging;

using OpenNI;
#endregion usings

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "Kinect",
	            Category = "Devices",
	            Version = "OpenNI",
	            Help = "Provides access to a Kinect through the OpenNI API",
	            Tags = "EX9",
	            Author = "Phlegma, joreg")]
	#endregion PluginInfo
	public class KinectContext: IPluginEvaluate, IDisposable
	{
		#region fields & pins
		//vvvv
		[Input("Mirrored", IsSingle = true, DefaultValue = 1)]
		IDiffSpread<bool> FMirrored;
		
//		[Input("Device", IsSingle = true, EnumName = "OpenNIDevices")]
//		IDiffSpread<EnumEntry> FDevice;
		
		[Output("Context")]
		ISpread<Context> FContextOut;
		
		[Output("Driver")]
		ISpread<string> FDriver;

		[Import()]
		ILogger FLogger;

		//Kinect
		private Context FContext;
		private DepthGenerator FDepthGenerator;
		private ImageGenerator FImageGenerator;

		private string FOpenNI;
		private string FSensor;
		private string FMiddleware;
		
//		private Dictionary<string, NodeInfo> FDevices = new Dictionary<string, NodeInfo>();
		#endregion fields & pins
		
		public KinectContext()
		{
			try
			{
				OpenContext();
			}
			catch
			{
				FOpenNI = "Unable to connect to Device!";
			}
		}

		#region Evaluate

		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			if (FContext != null)
			{
				//writes the Context Object to the Output
				//as it is required for other generators
				FContextOut[0] = FContext;
				FDriver[0] = FOpenNI + "\n" + FMiddleware + "\n" + FSensor;
				
				if (FMirrored.IsChanged)
					FContext.GlobalMirror = FMirrored[0];
				
				FContext.WaitNoneUpdateAll();
			}
		}
		
		#endregion
		
		#region Open and close Context
		private void OpenContext()
		{
			//try to open Kinect Context
			try
			{
				FContext = new Context();
				FContext.ErrorStateChanged += FContext_ErrorStateChanged;
				
			/*	var devices = FContext.EnumerateProductionTrees(OpenNI.NodeType.Depth, null);

				foreach (var device in devices)
					FDevices.Add(device.CreationInfo, device);
				
				string[] deviceNames = new string[FDevices.Count];
				FDevices.Keys.CopyTo(deviceNames, 0);
				EnumManager.UpdateEnum("OpenNIDevices", deviceNames[0], deviceNames);
				
				Query q = new Query();
				q.SetCreationInfo(deviceNames[0]);
				var depths = FContext.EnumerateProductionTrees(OpenNI.NodeType.Depth, q);
				
				//here is one central depth generator that is used by downstream nodes like depth, user, hand, skeleton,..
				//since this is one central generator it should not be allowed to disable it downstream
				foreach (var depth in depths)*/
				FDepthGenerator = (DepthGenerator) FContext.CreateAnyProductionTree(OpenNI.NodeType.Depth, null);// .CreateProductionTree(depth);
				FDepthGenerator.StartGenerating();
				
				//creation of usergenerators requires generation of depthgenerator
				//depth node needs imagegenerator to adaptview
				
				//read out driver versions:
				var v = OpenNI.Version.Current;
				FOpenNI = "OpenNI: " + v.Major + "." + v.Minor + "." + v.Maintenance + "." + v.Build;
				
				//create a usergenerator here just for getting the NITE version
				var user = FContext.CreateAnyProductionTree(OpenNI.NodeType.User, null);
				v = user.Info.Description.Version;
				FMiddleware = user.Info.Description.Vendor + " " + user.Info.Description.Name + ": " + v.Major + "." + v.Minor + "." + v.Maintenance + "." + v.Build;
				user.Dispose();
				
				v = FDepthGenerator.Info.Description.Version;
				FSensor = FDepthGenerator.Info.Description.Vendor + " " + FDepthGenerator.Info.Description.Name + ": " + v.Major + "." + v.Minor + "." + v.Maintenance + "." + v.Build;
			}
			catch (Exception e)
			{
				FLogger.Log(e);
			}
		}
		
		private void CloseContext()
		{
			if (FContext != null)
			{
				FContext.StopGeneratingAll();
				FContext.ErrorStateChanged -= FContext_ErrorStateChanged;
				
				if (FDepthGenerator != null)
				{
					FDepthGenerator.Dispose();
					FDepthGenerator = null;
				}
				
				FContext.Release();
				FContext = null;
			}
		}
		#endregion

		#region Error Event
		void FContext_ErrorStateChanged(object sender, ErrorStateEventArgs e)
		{
			FLogger.Log(LogType.Error, "Global Kinect Error: " + e.CurrentError);
		}
		#endregion

		#region Dispose
		public void Dispose()
		{
			CloseContext();
		}
		#endregion Dispose
	}
}
