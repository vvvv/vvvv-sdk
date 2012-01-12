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
	            Help = "Gesture recognition from the Kinect",
	            Tags = "tracking, hand",
	            Author = "Phlegma")]
	#endregion PluginInfo
	public class GestureOpenNI: IPluginEvaluate, IPluginConnections, IDisposable
	{
		#region fields & pins
		[Input("Context", IsSingle=true)]
		Pin<Context> FContextIn;
		
		[Input("Gestures Names", DefaultString = "RaiseHand")]
		IDiffSpread<string> FGestureNamesIn;

		[Input("Reset", IsSingle = true, IsBang = true)]
		IDiffSpread<bool> FStopTrackingIn;

		[Input("Enabled", IsSingle = true, DefaultValue = 1)]
		ISpread<bool> FEnabledIn;

		[Output("Possible Gestures")]
		ISpread<string> FGesturesPosOut;

		[Output("Active Gestures")]
		ISpread<string> FGestureActOut;

		[Output("Recognized Gestures")]
		ISpread<string> FGesturesRegOut;

		[Output("Gesture Position")]
		ISpread<Vector3D> FGesturePositionOut;

		[Output("Hand ID")]
		ISpread<int> FUserIdOut;

		[Output("Tracking Position")]
		ISpread<Vector3D> FPositionOut;

		[Import()]
		ILogger FLogger;

		GestureGenerator FGestureGenerator;

		private bool FContextChanged = false;
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
							FGesturesPosOut.SliceCount = gestures.Length;
							for (int i = 0; i < gestures.Length; i++)
								FGesturesPosOut[i] = gestures[i];

							//Start the Nodes to generate data
							FGestureGenerator.StartGenerating();
							
							FContextChanged = false;
						}
						catch (StatusException ex)
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
			
			if (FGestureGenerator != null && FEnabledIn[0])
			{
				//Check the Incomming gestures and try to add them to the Gesture generator
				if (FGestureNamesIn.IsChanged)
				{
					//check which getsures are active and delete them
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
					foreach (string Gesture in FGestureNamesIn)
					{
						try
						{
							FGestureGenerator.AddGesture(Gesture);
						}
						catch (StatusException ex)
						{
							FLogger.Log(LogType.Error, String.Format("Cannot Add Gesture: {0}", Gesture));
							FLogger.Log(LogType.Message, ex.Message);
						}
					}

					//check with gestures are active and write them to the output
					ActiveGesture = null;
					ActiveGesture = FGestureGenerator.GetAllActiveGestures();
					FGestureActOut.SliceCount = ActiveGesture.Length;
					for (int i = 0; i < ActiveGesture.Length; i++)
					{
						FGestureActOut[i] = ActiveGesture[i];
					}
				}

				// Handle the Output Pins
				if (true)
				{
					if (FGestureRecognized.Count > 0)
					{
						//lock the List that it its not changed  by the OpenNI Thread
						lock (FGestureRecognized)
						{
							FGesturesRegOut.SliceCount = FGestureRecognized.Count;
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
					}

					//write tracked hand id and position to the output
					if (FPositions.Count > 0)
					{
						lock (FPositions)
						{
							IList<Vector3D> Vectors = FPositions.Values;
							IList<int> UserIds = FPositions.Keys;

							FUserIdOut.SliceCount = UserIds.Count;

							for (int i = 0; i < Vectors.Count; i++)
							{
								try
								{
									FPositionOut[i] = Vectors[i] / 1000;
									FUserIdOut[i] = UserIds[i];
								}
								catch (ArgumentOutOfRangeException Exception)
								{
									FLogger.Log(Exception);
								}
							}
						}
					}
					else
					{
						FUserIdOut.SliceCount = 0;
						FPositionOut.SliceCount = 0;
					}
				}
				else
				{
					FGesturesRegOut.SliceCount = FPositionOut.SliceCount = FUserIdOut.SliceCount = 0;
				}
			}
		}

		void FGesture_GestureRecognized(object sender, GestureRecognizedEventArgs e)
		{
			try
			{
				lock (FGestureRecognized)
				{
					Vector3D Vector = new Vector3D(e.EndPosition.X, e.EndPosition.Y, e.EndPosition.Z);
					FGestureRecognized.Add(Vector, e.Gesture);
				}
			}
			catch (ArgumentException)
			{
			}
			catch (InvalidOperationException)
			{
			}
		}
		
		void FGesture_GestureProgress(object sender, GestureProgressEventArgs e)
		{
			
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
