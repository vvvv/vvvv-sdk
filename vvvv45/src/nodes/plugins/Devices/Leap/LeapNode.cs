#region usings
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

using VVVV.Core.Logging;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Algorithm;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using Leap;

#endregion usings

namespace VVVV.Nodes.Devices.Leap
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
        
        [Output("Hand Normal")]
        ISpread<Vector3D> FHandNormOut;
        
        [Output("Hand Ball Center")]
        ISpread<Vector3D> FHandBallCentOut;
        
        [Output("Hand Ball Radius")]
        ISpread<double> FHandBallRadOut;
        
        [Output("Hand Velocity")]
        ISpread<Vector3D> FHandVelOut;
        
        [Output("Hand ID")]
        ISpread<int> FHandIDOut;
        
        [Output("Finger Is Tool")]
        ISpread<bool> FFingerIsToolOut;
        
        [Output("Finger Position")]
        ISpread<Vector3D> FFingerPosOut;
        
        [Output("Finger Direction")]
        ISpread<Vector3D> FFingerDirOut;
        
        [Output("Finger Velocity")]
        ISpread<Vector3D> FFingerVelOut;
        
        [Output("Finger Size")]
        ISpread<Vector2D> FFingerSizeOut;
        
        [Output("Finger ID")]
        ISpread<int> FFingerIDOut;
        
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
			FHandNormOut.SliceCount = FCurrentHands.Count;
			FHandBallCentOut.SliceCount = FCurrentHands.Count;
			FHandBallRadOut.SliceCount = FCurrentHands.Count;
			FHandIDOut.SliceCount = FCurrentHands.Count;
			
			FFingerPosOut.SliceCount = 0;
			FFingerDirOut.SliceCount = 0;
			FFingerVelOut.SliceCount = 0;
			FFingerIsToolOut.SliceCount = 0;
			FFingerSizeOut.SliceCount = 0;
			FFingerIDOut.SliceCount = 0;
			FHandSliceOut.SliceCount = 0;
			
			for(int i=0; i < FCurrentHands.Count; i++)
			{
				var hand = FCurrentHands[i];
				
				if(hand.palm() != null)
				{
					FHandPosOut[i] = hand.palm().position.ToVector3DPos();
					FHandDirOut[i] = hand.palm().direction.ToVector3DDir();
					FHandNormOut[i] = hand.normal().ToVector3DDir();
					FHandBallCentOut[i] = hand.ball().position.ToVector3DPos();
					FHandBallRadOut[i] = hand.ball().radius * 0.001;
					FHandVelOut[i] = hand.velocity().ToVector3DPos();
				}

				FHandIDOut[i] = hand.id();
				
				FCurrentFingers = hand.fingers();
				
				for(int j=0; j < FCurrentFingers.Count; j++)
				{
					var finger = FCurrentFingers[j];
					FFingerPosOut.Add(finger.tip().position.ToVector3DPos());
					FFingerDirOut.Add(finger.tip().direction.ToVector3DDir());
					FFingerVelOut.Add(finger.velocity().ToVector3DPos());
					FFingerIsToolOut.Add(finger.isTool());
					FFingerSizeOut.Add(new Vector2D(finger.width() * 0.001, finger.length() * 0.001));
					FFingerIDOut.Add(finger.id());
					FHandSliceOut.Add(i);
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
		public static Vector3D ToVector3DPos(this Vector v)
		{
			return new Vector3D(v.x * 0.001, v.y * 0.001, v.z * -0.001);
		}
		
		public static Vector3D ToVector3DDir(this Vector v)
		{
			return new Vector3D(v.x, v.y, -v.z);
		}
	}
}




