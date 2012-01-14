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
	[PluginInfo(Name = "Hand",
	            Category = "Kinect",
	            Version = "OpenNI",
	            Help = "Hand recognition from the Kinect",
	            Tags = "tracking, hand")]
	#endregion PluginInfo
	public class HandOpenNI: IPluginEvaluate, IPluginConnections, IDisposable
	{
		#region fields & pins
		[Input("Context", IsSingle=true)]
		Pin<Context> FContextIn;
		
		[Input("Start Position")]
		ISpread<Vector3D> FStartPositionIn;
		
		[Input("Initialize")]
		ISpread<bool> FTrackIn;
		
		[Input("Enabled", IsSingle = true, DefaultValue = 1)]
		ISpread<bool> FEnabledIn;
		
		[Output("Position")]
		ISpread<Vector3D> FHandPositionOut;
		
		[Output("Start Position Is Tracked")]
		ISpread<bool> FIsTrackedOut;
		
		[Output("ID")]
		ISpread<int> FHandIdOut;

		[Import()]
		ILogger FLogger;

		HandsGenerator FHandGenerator;

		private bool FContextChanged = false;
		private Dictionary<int, Vector3D> FTrackedHands = new Dictionary<int, Vector3D>();
		private Dictionary<int, Vector3D> FTrackedStartPositions = new Dictionary<int, Vector3D>();
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
							// Create and Hands generator
							FHandGenerator = new HandsGenerator(FContextIn[0]);
							FHandGenerator.HandCreate += FHands_HandCreate;
							FHandGenerator.HandDestroy += FHands_HandDestroy;
							FHandGenerator.HandUpdate += FHands_HandUpdate;
							
							//Start the Nodes to generate data
							FHandGenerator.StartGenerating();
							
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
			
			if (FHandGenerator != null && FEnabledIn[0])
			{
				FIsTrackedOut.SliceCount = FStartPositionIn.SliceCount;
				//for every given StartPosition check if it is currently tracked
				for (int i = 0; i < FStartPositionIn.SliceCount; i++)
				{
					if (FTrackIn[i])
					{
						if (FTrackedStartPositions.ContainsValue(FStartPositionIn[i]))
							FIsTrackedOut[i] = true;
						else
						{
							FIsTrackedOut[i] = false;
							var p = new Point3D((float)(FStartPositionIn[i].x * 1000), (float)(FStartPositionIn[i].y * 1000), (float)(FStartPositionIn[i].z * 1000));
							FHandGenerator.StartTracking(p);
						}
					}
					else
					{
						//find the userID corresponding to the StartPosition 
						//and stop tracking it
						int userID = -1;
						foreach (var tracker in FTrackedStartPositions)
							if (tracker.Value == FStartPositionIn[i])
						{
							userID = tracker.Key;
							break;
						}
						if (userID > -1)
						{
							FHandGenerator.StopTracking(userID);
//							FTrackedStartPositions.Remove(userID);
						}
						FIsTrackedOut[i] = false;
					}
				}
				
				//Write all tracked hands to the output
				FHandIdOut.SliceCount = FHandPositionOut.SliceCount = FTrackedHands.Count;
				int slice = 0;
				lock(FTrackedHands)
					foreach (var item in FTrackedHands)
				{
					FHandIdOut[slice] = item.Key;
					FHandPositionOut[slice] = item.Value;
					slice++;
				}
			}
			else
			{
				FHandIdOut.SliceCount = FHandPositionOut.SliceCount = 0;
			}
		}

		void FHands_HandUpdate(object sender, HandUpdateEventArgs e)
		{
			//if this hand is updated for the first time
			//add it to TrckedStartPositions 
			//with the original position of this hand which is found in FTrackedHands[e.UserID]
			//before this is updated!
			if (FTrackedHands.ContainsKey(e.UserID) && !FTrackedStartPositions.ContainsKey(e.UserID))
				FTrackedStartPositions.Add(e.UserID, FTrackedHands[e.UserID]);
			
			lock(FTrackedHands)
				if (FTrackedHands.ContainsKey(e.UserID))
					FTrackedHands[e.UserID] = new Vector3D(e.Position.X / 1000, e.Position.Y / 1000, e.Position.Z / 1000);
		}

		void FHands_HandDestroy(object sender, HandDestroyEventArgs e)
		{
			lock(FTrackedHands)
				if (FTrackedHands.ContainsKey(e.UserID))
					FTrackedHands.Remove(e.UserID);
			FTrackedStartPositions.Remove(e.UserID);
		}

		void FHands_HandCreate(object sender, HandCreateEventArgs e)
		{
			var v = new Vector3D(e.Position.X / 1000, e.Position.Y / 1000, e.Position.Z / 1000);
			lock(FTrackedHands)
				if (!FTrackedHands.ContainsValue(v))
					FTrackedHands.Add(e.UserID, v);
			//FTrackedStartPositions.Add(e.UserID, v);
		}
		
		#region Dispose
		public void Dispose()
		{
			CleanUp();
		}
		
		private void CleanUp()
		{
			if (FHandGenerator != null)
			{
				FHandGenerator.HandCreate -= FHands_HandCreate;
				FHandGenerator.HandDestroy -= FHands_HandDestroy;
				FHandGenerator.HandUpdate -= FHands_HandUpdate;
				
				FHandGenerator.Dispose();
				FHandGenerator = null;
			}
			
			FTrackedStartPositions.Clear();
			FTrackedHands.Clear();
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
