#region usings
using System;
using System.ComponentModel.Composition;
using System.Collections.Generic;
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
	[PluginInfo(Name = "Gesture",
	            Category = "Kinect",
	            Version = "OpenNI",
	            Help = "Gesture recognition",
	            Tags = "tracking, hand",
	            Author = "Phlegma, joreg")]
	#endregion PluginInfo
	public class GestureOpenNI: IPluginEvaluate, IPluginConnections, IDisposable
	{
		#region fields & pins
		[Input("Context", IsSingle=true)]
		Pin<Context> FContextIn;
		
		[Input("Active Gesture", EnumName = "OpenNIGestures")]
		IDiffSpread<EnumEntry> FActiveGestureIn;
		
		[Input("Enabled", IsSingle = true, DefaultValue = 1)]
		IDiffSpread<bool> FEnabledIn;

		[Output("Recognized Gesture")]
		ISpread<string> FGesturesRegOut;

		[Output("Gesture Position")]
		ISpread<Vector3D> FGesturePositionOut;

		[Import()]
		ILogger FLogger;

		GestureGenerator FGestureGenerator;

		private bool FContextChanged = false;
		private bool FUpdateGestures = false;
		private SortedList<int,Vector3D> FPositions = new SortedList<int,Vector3D>();
		private Dictionary<Vector3D,string> FGestureRecognized = new Dictionary<Vector3D,string>();
		private List<Vector3D> FMovingHands = new List<Vector3D>();
		#endregion fields & pins

		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			if (FContextChanged)
			{
				if (FContextIn.PluginIO.IsConnected)
				{
					if (FContextIn[0] != null)
					{
						try
						{
							FGestureGenerator = new GestureGenerator(FContextIn[0]);
							
							FGestureGenerator.GestureRecognized += FGesture_GestureRecognized;
							FGestureGenerator.GestureProgress += FGesture_GestureProgress;

							//Write all possible gestures to the Output Pin
							var gestures = FGestureGenerator.EnumerateAllGestures();
							EnumManager.UpdateEnum("OpenNIGestures", gestures[0], gestures);
							
							FGestureGenerator.StartGenerating();

							FContextChanged = false;
						}
						catch (Exception ex)
						{
							FLogger.Log(ex);
						}
					}
				}
				else
				{
					CleanUp();
					FContextChanged = false;
				}
			}
			
			if (FActiveGestureIn.IsChanged)
				FUpdateGestures = true;
			
			if (FGestureGenerator != null)
			{
				if (FEnabledIn.IsChanged)
					if (FEnabledIn[0])
						FGestureGenerator.StartGenerating();
					else
						FGestureGenerator.StopGenerating();

				if (FGestureGenerator.IsDataNew)
				{
					//Check the Incomming gestures and try to add them to the Gesture generator
					if (FUpdateGestures)
					{
						//first remove all active gestures
						string[] ActiveGesture = FGestureGenerator.GetAllActiveGestures();
						if (ActiveGesture.Length > 0)
						{
							foreach (string Gesture in ActiveGesture)
							{
								try
								{
									FGestureGenerator.RemoveGesture(Gesture);
								}
								catch (StatusException ex)
								{
									FLogger.Log(LogType.Error, String.Format("Cannot Remove Gesture: {0}", Gesture));
									FLogger.Log(LogType.Message, ex.Message);
								}
							}
						}

						//add all gestures to the Gesture Generator
						foreach (var gesture in FActiveGestureIn)
						{
							try
							{
								FGestureGenerator.AddGesture(gesture.Name);
							}
							catch (StatusException ex)
							{
								FLogger.Log(LogType.Error, String.Format("Cannot Add Gesture: {0}", gesture));
								FLogger.Log(LogType.Message, ex.Message);
							}
						}
						
						FUpdateGestures = false;
					}

					// Handle the Output Pins
					FGesturesRegOut.SliceCount = FGestureRecognized.Count;
					FGesturePositionOut.SliceCount = FGestureRecognized.Count;
					int Slice = 0;

					// Write the received Gestures with their endposition to the output
					foreach (KeyValuePair<Vector3D, string> item in FGestureRecognized)
					{
						FGesturesRegOut[Slice] = item.Value;
						FGesturePositionOut[Slice] = item.Key / 1000;
						Slice++;
					}

					FGestureRecognized.Clear();
				}
			}
			else
			{
				FGesturesRegOut.SliceCount = 0;
				FGesturePositionOut.SliceCount = 0;
			}
		}

		void FGesture_GestureRecognized(object sender, GestureRecognizedEventArgs e)
		{
			Vector3D Vector = new Vector3D(e.EndPosition.X, e.EndPosition.Y, e.EndPosition.Z);
			FGestureRecognized.Add(Vector, e.Gesture);
		}
		
		void FGesture_GestureProgress(object sender, GestureProgressEventArgs e)
		{
			//not used
			//FLogger.Log(LogType.Debug, e.Gesture + " - " + e.Position + " - " + e.Progress);
		}
		
		#region Dispose
		public void Dispose()
		{
			CleanUp();
		}
		
		private void CleanUp()
		{
			if (FGestureGenerator != null)
			{
				FGestureGenerator.GestureRecognized -= FGesture_GestureRecognized;
				FGestureGenerator.GestureProgress -= FGesture_GestureProgress;
				
				FGestureGenerator.Dispose();
				FGestureGenerator = null;
			}
		}
		#endregion
		
		#region ContextConnect
		public void ConnectPin(IPluginIO pin)
		{
			if (pin == FContextIn.PluginIO)
				FContextChanged = true;
		}

		public void DisconnectPin(IPluginIO pin)
		{
			if (pin == FContextIn.PluginIO)
				FContextChanged = true;
		}
		#endregion
	}
}
