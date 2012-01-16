#region usings
using System;
using System.ComponentModel.Composition;
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;

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
	            Author = "Phlegma")]
	#endregion PluginInfo
	public class KinectContext: IPluginEvaluate, IDisposable
	{
		#region fields & pins
		//vvvv
		[Input("Mirrored", IsSingle = true, DefaultValue = 1)]
		IDiffSpread<bool> FMirrored;

		//[Input("Enabled", IsSingle = true, DefaultValue = 1)]
		//IDiffSpread<bool> FUpdateIn;

		[Output("Context")]
		ISpread<Context> FContextOut;
		
		[Output("Driver")]
		ISpread<string> FDriver;

		[Import()]
		ILogger FLogger;

		//Kinect
		private bool FRunning;
		private Context FContext;
		private ImageGenerator FImageGenerator;
		private DepthGenerator FDepthGenerator;
		private Device FDevice;
		private Thread FUpdater;
		private string FOpenNI;
		private string FSensor;
		private string FMiddleware;
		#endregion fields & pins
		
		public KinectContext()
		{
			try
			{
				OpenContext();
				
				FUpdater = new Thread(Update);
				FRunning = true;
				//FUpdater.Start();
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
			//writes the Context Object to the Output
			//as it is required for other generators
			FContextOut[0] = FContext;
			FDriver[0] = FOpenNI + "\n" + FMiddleware + "\n" + FSensor;
			
			if (FMirrored.IsChanged)
				FContext.GlobalMirror = FMirrored[0];
//			FContext.WaitNoneUpdateAll();
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
				
				FImageGenerator = (ImageGenerator) FContext.CreateAnyProductionTree(OpenNI.NodeType.Image, null);
				FDepthGenerator = (DepthGenerator) FContext.CreateAnyProductionTree(OpenNI.NodeType.Depth, null);
				FDepthGenerator.AlternativeViewpointCapability.SetViewpoint(FImageGenerator);

				FContext.StartGeneratingAll();
				
				//read out driver versions:
				var v = OpenNI.Version.Current;
				FOpenNI = "OpenNI: " + v.Major + "." + v.Minor + "." + v.Maintenance + "." + v.Build;
				
				//create a usergenerator here just for getting the NITE version
				var user = FContext.CreateAnyProductionTree(OpenNI.NodeType.User, null);
				v = user.Info.Description.Version;
				FMiddleware = user.Info.Description.Vendor + " " + user.Info.Description.Name + ": " + v.Major + "." + v.Minor + "." + v.Maintenance + "." + v.Build;
				user.Dispose();
				
				v = FImageGenerator.Info.Description.Version;
				FSensor = FImageGenerator.Info.Description.Vendor + " " + FImageGenerator.Info.Description.Name + ": " + v.Major + "." + v.Minor + "." + v.Maintenance + "." + v.Build;
			}
			catch (Exception e)
			{
				FLogger.Log(e);
			}
		}
		
		private void CloseContext()
		{
		/*	if (FUpdater != null && FUpdater.IsAlive)
			{
				//wait for threadloop to exit
				FRunning = false;
				FUpdater.Join();
			}*/

			if (FContext != null)
			{
				FContext.StopGeneratingAll();
				FContext.ErrorStateChanged -= FContext_ErrorStateChanged;
				
				if (FImageGenerator != null)
					FImageGenerator.Dispose();
				if (FDepthGenerator != null)
					FDepthGenerator.Dispose();
				
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

		#region Update Thread
		private void Update()
		{
			while (FRunning)
			{
				try
				{
					if (FContext != null)
					{
						lock(FContext)
						{
							FContext.GlobalMirror = FMirrored[0];
							FContext.WaitAndUpdateAll();
						}
					}
				}
				catch (Exception ex)
				{
					Debug.WriteLine(ex.Message);
				}
			}
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
