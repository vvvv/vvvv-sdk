#region usings
using System;
using System.ComponentModel.Composition;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

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
	            Help = "Hand tracking",
	            Tags = "tracking, hand")]
	#endregion PluginInfo
	public class HandOpenNI: IPluginEvaluate, IPluginConnections, IDisposable
	{
		#region fields & pins
		[Input("Context", IsSingle=true)]
		Pin<Context> FContextIn;
		
		[Input("Start Position")]
		ISpread<Vector3D> FStartPositionIn;
		
		[Input("Enabled")]
		IDiffSpread<bool> FDoTrackStartPosition;
		
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
		private Dictionary<int, Point3D> FTrackedHands = new Dictionary<int, Point3D>();
		private Dictionary<int, Point3D> FTrackedStartPositions = new Dictionary<int, Point3D>();
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
			
			if (FHandGenerator != null)
			{
				if (FDoTrackStartPosition.IsChanged)
				{
					bool enable = false;
					for (int i = 0; i < FDoTrackStartPosition.SliceCount; i++)
						enable |= FDoTrackStartPosition[i];

					//enable if any slice is 1
					if (enable)
						FHandGenerator.StartGenerating();
					else
						FHandGenerator.StopGenerating();
				}
				
				if (FHandGenerator.IsDataNew)
				{
					FIsTrackedOut.SliceCount = FHandIdOut.SliceCount = FHandPositionOut.SliceCount = FStartPositionIn.SliceCount;
					//for every given StartPosition check if it is currently tracked
					for (int i = 0; i < FStartPositionIn.SliceCount; i++)
					{
						if (FDoTrackStartPosition[i])
						{
							//find userID in FTrackedStartPositions
							int userID = -1;
							foreach (var tracker in FTrackedStartPositions)
							{
								var p = new Point3D((float)(FStartPositionIn[i].x * 1000), (float)(FStartPositionIn[i].y * 1000), (float)(FStartPositionIn[i].z * 1000));
								if (tracker.Value.Equals(p))
								{
									userID = tracker.Key;
									break;
								}
							}
							
							//if present return tracking info
							if (userID > -1)
							{
								FIsTrackedOut[i] = true;
								FHandIdOut[i] = userID;
								
								var p = FTrackedHands[userID];
								FHandPositionOut[i] = new Vector3D(p.X / 1000, p.Y / 1000, p.Z / 1000);
							}
							//else start tracking
							else
							{
								FIsTrackedOut[i] = false;
								FHandIdOut[i] = -1;
								FHandPositionOut[i] = FStartPositionIn[i];
								
								var p = new Point3D((float)(FStartPositionIn[i].x * 1000), (float)(FStartPositionIn[i].y * 1000), (float)(FStartPositionIn[i].z * 1000));
								FHandGenerator.StartTracking(p);
							}
						}
						else
						{
							//find the handID corresponding to the StartPosition
							//and stop tracking it
							int userID = -1;
							foreach (var tracker in FTrackedStartPositions)
							{	
								var p = new Point3D((float)(FStartPositionIn[i].x * 1000), (float)(FStartPositionIn[i].y * 1000), (float)(FStartPositionIn[i].z * 1000));
								if (tracker.Value.Equals(p))
								{
									userID = tracker.Key;
									break;
								}
							}
							
							if (userID > -1)
								FHandGenerator.StopTracking(userID);
							
							FIsTrackedOut[i] = false;
							FHandIdOut[i] = -1;
							FHandPositionOut[i] = FStartPositionIn[i];
						}
					}
				}
			}
			else
			{
				FIsTrackedOut.SliceCount = FHandIdOut.SliceCount = FHandPositionOut.SliceCount = 0;
			}
		}

		void FHands_HandUpdate(object sender, HandUpdateEventArgs e)
		{
			//if this hand is updated for the first time
			//add it to TrackedStartPositions
			//with the original position of this hand which is found in FTrackedHands[e.UserID]
			//before this is updated!
			if (FTrackedHands.ContainsKey(e.UserID) && !FTrackedStartPositions.ContainsKey(e.UserID))
				FTrackedStartPositions.Add(e.UserID, FTrackedHands[e.UserID]);
			
			if (FTrackedHands.ContainsKey(e.UserID))
				FTrackedHands[e.UserID] = e.Position;//new Vector3D(e.Position.X / 1000, e.Position.Y / 1000, e.Position.Z / 1000);
		}

		void FHands_HandDestroy(object sender, HandDestroyEventArgs e)
		{
			if (FTrackedHands.ContainsKey(e.UserID))
				FTrackedHands.Remove(e.UserID);
			
			if (FTrackedStartPositions.ContainsKey(e.UserID))
				FTrackedStartPositions.Remove(e.UserID);
		}

		void FHands_HandCreate(object sender, HandCreateEventArgs e)
		{
//			var v = new Vector3D(e.Position.X / 1000, e.Position.Y / 1000, e.Position.Z / 1000);
			if (!FTrackedHands.ContainsValue(e.Position))
				FTrackedHands.Add(e.UserID, e.Position);
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
