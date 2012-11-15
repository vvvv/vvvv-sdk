#region usings
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;
using VVVV.Utils.Algorithm;

using VVVV.Core.Logging; 

#endregion usings

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "Leap",
	Category = "Devices", 
	Help = "Returns the tracking data of the Leap device",
	Tags = "tracking, hand, finger",
	AutoEvaluate = true)]
	#endregion PluginInfo
	public class LeapNode : IPluginEvaluate, IDisposable
	{
		#region fields & pins
		#pragma warning disable 0649

        [Output("Hand Position")]
        ISpread<Vector3D> FHandPosOut;
        
        [Output("Hand Direction")]
        ISpread<Vector3D> FHandDirOut;
        
        [Output("Hand Velocity")]
        ISpread<Vector3D> FHandVelOut;
        
        [Output("Finger Position")]
        ISpread<Vector3D> FFingerPosOut;
        
        [Output("Finger Direction")]
        ISpread<Vector3D> FFingerDirOut;
        
        [Output("Finger Velocity")]
        ISpread<Vector3D> FFingerVelOut;
        
        [Output("Hand Slice")]
        ISpread<int> FHandSliceOut;
		#pragma warning restore
		
		Controller FLeapController = new Controller();
		Frame FCurrentFrame;
		HandArray FCurrentHands;
		FingerArray FCurrentFingers;
		
		#endregion fields & pins
		
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			FCurrentFrame = FLeapController.frame();
			FCurrentHands = FCurrentFrame.hands();
			
			FHandPosOut.SliceCount = FCurrentHands.Count;
			FHandDirOut.SliceCount = FCurrentHands.Count;
			FHandVelOut.SliceCount = FCurrentHands.Count;
			
			FFingerPosOut.SliceCount = 0;
			FFingerDirOut.SliceCount = 0;
			FFingerVelOut.SliceCount = 0;
			FHandSliceOut.SliceCount = 0;
			
			for(int i=0; i < FCurrentHands.Count; i++)
			{
				var hand = FCurrentHands[i];
				
				FHandPosOut[i] = hand.palm().position.ToVector3D();
				FHandDirOut[i] = hand.palm().direction.ToVector3D();
				FHandVelOut[i] = hand.velocity().ToVector3D();
				
				FCurrentFingers = hand.fingers();
				
				for(int j=0; j < FCurrentFingers.Count; j++)
				{
					var finger = FCurrentFingers[j];
					FFingerPosOut.Add(finger.tip().position.ToVector3D());
					FFingerDirOut.Add(finger.tip().direction.ToVector3D());
					FFingerVelOut.Add(finger.velocity().ToVector3D());
					FHandSliceOut.Add(j);
				}
			}
			
		}
		

		public void Dispose()
		{
			FLeapController.Dispose();
		}
		
	}
	
	public static class LeapExtensions
	{
		public static Vector3D ToVector3D(this Vector v)
		{
			return new Vector3D(v.x, v.y, v.z);
		}
	}
}




