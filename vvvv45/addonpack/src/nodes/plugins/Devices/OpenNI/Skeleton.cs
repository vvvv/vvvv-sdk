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
	[PluginInfo(Name = "Skeleton",
	            Category = "Kinect",
	            Version = "OpenNI",
	            Help = "Returns skeleton information for each recognized user",
	            Tags = "tracking, person, user, people",
	            Author = "Phlegma, joreg")]
	#endregion PluginInfo
	public class Skeleton2OpenNI: IPluginEvaluate, IPluginConnections, IDisposable
	{
		#region fields & pins
		[Input("Context", IsSingle=true)]
		Pin<Context> FContextIn;
		
		[Input("Skeleton Profile", DefaultEnumEntry = "All", IsSingle = true)]
		IDiffSpread<SkeletonProfile> FSkeletonProfileIn;
		
		[Input("Smoothing", DefaultValue = 0.5, MinValue = 0, MaxValue = 1, IsSingle = true)]
		IDiffSpread<float> FSmoothingIn;
		
		[Input("Confidence CutOff", DefaultValue = 0.5, MinValue = 0, MaxValue = 1)]
		ISpread<float> FConfidenceIn;
		
		[Input("Joint", DefaultEnumEntry = "Head")]
		ISpread<ISpread<SkeletonJoint>> FJointIn;
		
		[Input("Enabled", IsSingle = true, DefaultValue = 1)]
		IDiffSpread<bool> FEnabledIn;

		[Output("Position")]
		ISpread<ISpread<Vector3D>> FJointPositionOut;
		
		[Output("Orientation X ")]
		ISpread<ISpread<Vector3D>> FJointOrientationXOut;

		[Output("Orientation Y ")]
		ISpread<ISpread<Vector3D>> FJointOrientationYOut;

		[Output("Orientation Z ")]
		ISpread<ISpread<Vector3D>> FJointOrientationZOut;
		
		[Output("User ID")]
		ISpread<int> FUserIdOut;
		
		[Output("Status")]
		ISpread<string> FStatusOut;

		[Import()]
		ILogger FLogger;

		UserGenerator FUserGenerator;
		SkeletonCapability FSkeletonCapability;
		PoseDetectionCapability FPoseDetectionCapability;
		string FCalibPose;
		
		private bool FContextChanged = false;
		private Dictionary<int, Dictionary<SkeletonJoint, SkeletonJointTransformation>> FJoints;
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
							FUserGenerator = new UserGenerator(FContextIn[0]);
							FSkeletonCapability = FUserGenerator.SkeletonCapability;
							FPoseDetectionCapability = FUserGenerator.PoseDetectionCapability;
							FCalibPose = FSkeletonCapability.CalibrationPose;
							
							FUserGenerator.NewUser += userGenerator_NewUser;
							FUserGenerator.LostUser += userGenerator_LostUser;
							FPoseDetectionCapability.PoseDetected += poseDetectionCapability_PoseDetected;
							FSkeletonCapability.CalibrationComplete += skeletonCapbility_CalibrationComplete;
							
							FSkeletonCapability.SetSkeletonProfile(FSkeletonProfileIn[0]);
							FJoints = new Dictionary<int, Dictionary<SkeletonJoint, SkeletonJointTransformation>>();
							
							FUserGenerator.StartGenerating();
							
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
			
			if (FUserGenerator != null)
			{
				if (FEnabledIn.IsChanged)
					if (FEnabledIn[0])
						FUserGenerator.StartGenerating();
					else
						FUserGenerator.StopGenerating();
				
				if (FUserGenerator.IsDataNew)
				{
					if (FSkeletonProfileIn.IsChanged)
						FSkeletonCapability.SetSkeletonProfile(FSkeletonProfileIn[0]);
					
					if (FSmoothingIn.IsChanged)
						FSkeletonCapability.SetSmoothing(FSmoothingIn[0]);
					
					FUserIdOut.SliceCount = FUserGenerator.NumberOfUsers;
					FStatusOut.SliceCount = FUserGenerator.NumberOfUsers;
					FJointPositionOut.SliceCount = FUserGenerator.NumberOfUsers;
					FJointOrientationXOut.SliceCount = FUserGenerator.NumberOfUsers;
					FJointOrientationYOut.SliceCount = FUserGenerator.NumberOfUsers;
					FJointOrientationZOut.SliceCount = FUserGenerator.NumberOfUsers;
					
					if (FUserGenerator.NumberOfUsers > 0)
					{
						//get all Users and sort them
						int[] users = FUserGenerator.GetUsers();
						Array.Sort(users);

						int slice = 0;
						foreach (int user in users)
						{
							FUserIdOut[slice] = user;
							if (FSkeletonCapability.IsTracking(user))
							{
								FStatusOut[slice] = "Tracking user " + user;
								
								int u = user - 1;
								int binSize = FJointIn[u].SliceCount;
								FJointPositionOut[u].SliceCount = binSize;
								FJointOrientationXOut[u].SliceCount = FJointOrientationYOut[u].SliceCount = FJointOrientationZOut[u].SliceCount = binSize;
								for (int i = 0; i < binSize; i++)
								{
									var j = GetJoint(user, FJointIn[u][i]);
									
									var p = j.Position.Position;
									if (j.Position.Confidence > 0.5)
										FJointPositionOut[u][i] = new Vector3D(p.X, p.Y, p.Z) / 1000;
									
									var o = j.Orientation;
									if (o.Confidence > FConfidenceIn[slice])
									{
										FJointOrientationXOut[u][i] = new Vector3D(o.X1, o.Y1, o.Z1);
										FJointOrientationYOut[u][i] = new Vector3D(o.X2, o.Y2, o.Z2);
										FJointOrientationZOut[u][i] = new Vector3D(o.X3, o.Y3, o.Z3);
									}
								}
							}
							else if (FSkeletonCapability.IsCalibrating(user))
								FStatusOut[slice] = "Calibrating user " + user;
							else
								FStatusOut[slice] = "Looking for pose on user " + user;
							
							slice++;
						}
					}
				}
			}
			else
			{
				FUserIdOut.SliceCount = 0;
				FStatusOut.SliceCount = 0;
				FJointPositionOut.SliceCount = 0;
				FJointOrientationXOut.SliceCount = 0;
				FJointOrientationYOut.SliceCount = 0;
				FJointOrientationZOut.SliceCount = 0;
			}
		}
		
		private SkeletonJointTransformation GetJoint(int user, SkeletonJoint joint)
		{
			if (FSkeletonCapability.IsJointAvailable(joint))
			{
				if (!FSkeletonCapability.IsJointActive(joint))
					FSkeletonCapability.SetJointActive(joint, true);
				
				return FSkeletonCapability.GetSkeletonJoint(user, joint);
			}
			else
				return new SkeletonJointTransformation();
		}
		
		void skeletonCapbility_CalibrationComplete(object sender, CalibrationProgressEventArgs e)
		{
			if (e.Status == CalibrationStatus.OK)
			{
				FSkeletonCapability.StartTracking(e.ID);
				FJoints.Add(e.ID, new Dictionary<SkeletonJoint, SkeletonJointTransformation>());
			}
			else if (e.Status != CalibrationStatus.ManualAbort)
			{
				if (FSkeletonCapability.DoesNeedPoseForCalibration)
					FPoseDetectionCapability.StartPoseDetection(FCalibPose, e.ID);
				else
					FSkeletonCapability.RequestCalibration(e.ID, true);
			}
		}

		void poseDetectionCapability_PoseDetected(object sender, PoseDetectedEventArgs e)
		{
			FPoseDetectionCapability.StopPoseDetection(e.ID);
			FSkeletonCapability.RequestCalibration(e.ID, true);
		}

		void userGenerator_NewUser(object sender, NewUserEventArgs e)
		{
			if (FSkeletonCapability.DoesNeedPoseForCalibration)
				FPoseDetectionCapability.StartPoseDetection(FCalibPose, e.ID);
			else
				FSkeletonCapability.RequestCalibration(e.ID, true);
		}

		void userGenerator_LostUser(object sender, UserLostEventArgs e)
		{
			FJoints.Remove(e.ID);
		}
		
		#region Dispose
		private void CleanUp()
		{
			if (FUserGenerator != null)
			{
				FUserGenerator.StopGenerating();
				
				FSkeletonCapability.CalibrationComplete -= skeletonCapbility_CalibrationComplete;
				FPoseDetectionCapability.PoseDetected -= poseDetectionCapability_PoseDetected;
				FUserGenerator.LostUser -= userGenerator_LostUser;
				FUserGenerator.NewUser -= userGenerator_NewUser;
				
				FUserGenerator.Dispose();
				FUserGenerator = null;
				
				FJoints = null;
			}
		}
		
		public void Dispose()
		{
			CleanUp();
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
