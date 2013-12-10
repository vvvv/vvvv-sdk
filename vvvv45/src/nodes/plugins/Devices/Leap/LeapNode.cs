#region usings
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Algorithm;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using Leap;

#endregion usings

namespace VVVV.Nodes.Devices
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
		
		//circle
		[Input("Enable Circle Gesture")]
		IDiffSpread<bool> FEnableCircleGesture;
		
		[Input("Circle Min Radius (mm)", DefaultValue = 5.0, Visibility = PinVisibility.OnlyInspector)]
		IDiffSpread<float> FCircleMinRadius;
		
		[Input("Circle Min Arc (radians)", DefaultValue = 1.5*Math.PI, Visibility = PinVisibility.OnlyInspector)]
		IDiffSpread<float> FCircleMinArc;
		
		//swipe
		[Input("Enable Swipe Gesture")]
		IDiffSpread<bool> FEnableSwipeGesture;
		
		[Input("Swipe Min Length (mm)", DefaultValue = 150.0, Visibility = PinVisibility.OnlyInspector)]
		IDiffSpread<float> FSwipeMinLength;
		
		[Input("Swipe Min Velocity (mm/s)", DefaultValue = 1000.0, Visibility = PinVisibility.OnlyInspector)]
		IDiffSpread<float> FSwipeMinVelocity;
		
		//key tab
		[Input("Enable KeyTab Gesture")]
		IDiffSpread<bool> FEnableKeyTabGesture;
		
		[Input("KeyTab Min Down Velocity (mm/s)", DefaultValue = 50.0, Visibility = PinVisibility.OnlyInspector)]
		IDiffSpread<float> FKeyTabMindDownVelo;
		
		[Input("KeyTab History Seconds (s)", DefaultValue = 0.1, Visibility = PinVisibility.OnlyInspector)]
		IDiffSpread<float> FKeyTabHistorySeconds;
		
		[Input("KeyTab Min Distance (mm)", DefaultValue = 5.0, Visibility = PinVisibility.OnlyInspector)]
		IDiffSpread<float> FKeyTabMindDistance;
		
		//screen tab
		[Input("Enable ScreenTab Gesture")]
		IDiffSpread<bool> FEnableScreenTabGesture;
		
		[Input("ScreenTab Min Forward Velocity (mm/s)", DefaultValue = 50.0, Visibility = PinVisibility.OnlyInspector)]
		IDiffSpread<float> FScreenTabMindForwardVelo;
		
		[Input("ScreenTab History Seconds (s)", DefaultValue = 0.1, Visibility = PinVisibility.OnlyInspector)]
		IDiffSpread<float> FScreenTabHistorySeconds;
		
		[Input("ScreenTab Min Distance (mm)", DefaultValue = 3.0, Visibility = PinVisibility.OnlyInspector)]
		IDiffSpread<float> FScreenTabMindDistance;
		
		//screen tab
		[Input("Reset", IsBang = true)]
		IDiffSpread<bool> FResetIn;

        [Output("Hand Position")]
        ISpread<Vector3D> FHandPosOut;
        
        [Output("Hand Direction")]
        ISpread<Vector3D> FHandDirOut;
        
        [Output("Hand Normal")]
        ISpread<Vector3D> FHandNormOut;
        
        [Output("Hand Ball Center", Visibility = PinVisibility.OnlyInspector)]
        ISpread<Vector3D> FHandBallCentOut;
        
        [Output("Hand Ball Radius", Visibility = PinVisibility.OnlyInspector)]
        ISpread<double> FHandBallRadOut;
        
        [Output("Hand Velocity", Visibility = PinVisibility.OnlyInspector)]
        ISpread<Vector3D> FHandVelOut;
        
        [Output("Hand ID")]
        ISpread<int> FHandIDOut;
        
        [Output("Finger Position")]
        ISpread<Vector3D> FFingerPosOut;
        
        [Output("Finger Direction")]
        ISpread<Vector3D> FFingerDirOut;
        
        [Output("Finger Velocity", Visibility = PinVisibility.OnlyInspector)]
        ISpread<Vector3D> FFingerVelOut;
        
        [Output("Is Tool")]
        ISpread<bool> FFingerIsToolOut;
        
        [Output("Finger Size", Visibility = PinVisibility.OnlyInspector)]
        ISpread<Vector2D> FFingerSizeOut;
        
        [Output("Finger ID")]
        ISpread<int> FFingerIDOut;
        
        [Output("Hand Slice", Visibility = PinVisibility.OnlyInspector)]
        ISpread<int> FHandSliceOut;
        
        [Output("Gestures")]
        ISpread<GestureList> FGestureOut;
        
		#pragma warning restore
		
		Controller FLeapController = new Controller();
		Frame FLastFrame;
		
		#endregion fields & pins

		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
//			if(FEnableGestures.IsChanged)
//			{
//				foreach (var gestureType in (Gesture.GestureType[])Enum.GetValues(typeof(Gesture.GestureType)))
//				{
//					if(gestureType != Gesture.GestureType.TYPEINVALID)
//						FLeapController.EnableGesture(gestureType, FEnableGestures[0]);
//				}
//			}
			
			if(FResetIn[0])
			{
				FLeapController.Dispose();
				FLeapController = new Controller();
			}
			
			ConfigureGestures();
			
			var frame = FLeapController.Frame();
			
			if(FLeapController.IsConnected && frame.IsValid)
			{
			
				var hands = frame.Hands;
				SpreadMax = hands.Count;
				
				FHandPosOut.SliceCount = SpreadMax;
				FHandDirOut.SliceCount = SpreadMax;
				FHandVelOut.SliceCount = SpreadMax;
				FHandNormOut.SliceCount = SpreadMax;
				FHandBallCentOut.SliceCount = SpreadMax;
				FHandBallRadOut.SliceCount = SpreadMax;
				FHandIDOut.SliceCount = SpreadMax;
				
				FFingerPosOut.SliceCount = 0;
				FFingerDirOut.SliceCount = 0;
				FFingerVelOut.SliceCount = 0;
				FFingerIsToolOut.SliceCount = 0;
				FFingerSizeOut.SliceCount = 0;
				FFingerIDOut.SliceCount = 0;
				FHandSliceOut.SliceCount = 0;
				
				for(int i=0; i < SpreadMax; i++)
				{
					var hand = hands[i];
					
					if(hand != null)
					{
						FHandPosOut[i] = hand.PalmPosition.ToVector3DPos();
						FHandDirOut[i] = hand.Direction.ToVector3DDir();
						FHandNormOut[i] = hand.PalmNormal.ToVector3DDir();
						FHandBallCentOut[i] = hand.SphereCenter.ToVector3DPos();
						FHandBallRadOut[i] = hand.SphereRadius * 0.001;
						FHandVelOut[i] = hand.PalmVelocity.ToVector3DPos();
						FHandIDOut[i] = hand.Id;
					}
					
					var pointables = hand.Pointables;
					
					for(int j=0; j < pointables.Count; j++)
					{
						var pointable = pointables[j];
						FFingerPosOut.Add(pointable.TipPosition.ToVector3DPos());
						FFingerDirOut.Add(pointable.Direction.ToVector3DDir());
						FFingerVelOut.Add(pointable.TipVelocity.ToVector3DPos());
						FFingerIsToolOut.Add(pointable.IsTool);
						FFingerSizeOut.Add(new Vector2D(pointable.Width * 0.001, pointable.Length * 0.001));
						FFingerIDOut.Add(pointable.Id);
						FHandSliceOut.Add(i);
					}
				}
				
				//gestures
				FGestureOut[0] = FLastFrame != null ? frame.Gestures(FLastFrame) : frame.Gestures();
				FLastFrame = frame;
			}
		}
		
		void ConfigureGestures()
		{
			var cfg = FLeapController.Config;
			
			//circle
			HandleEnableGesture(FEnableCircleGesture, Gesture.GestureType.TYPECIRCLE);
			HandleGestureConfig(new string[]{"Circle.MinRadius", "Circle.MinArc"}, FCircleMinRadius, FCircleMinArc);
			
			//swipe
			HandleEnableGesture(FEnableSwipeGesture, Gesture.GestureType.TYPESWIPE);
			HandleGestureConfig(new string[]{"Swipe.MinLength", "Swipe.MinVelocity"}, FSwipeMinLength, FSwipeMinVelocity);
			
			//key tab
			HandleEnableGesture(FEnableKeyTabGesture, Gesture.GestureType.TYPEKEYTAP);
			HandleGestureConfig(new string[]{"KeyTap.MinDownVelocity", "KeyTap.HistorySeconds", "KeyTap.MinDistance"}, 
			                    FKeyTabMindDownVelo, FKeyTabHistorySeconds, FKeyTabMindDistance);
			
			//screen tab
			HandleEnableGesture(FEnableScreenTabGesture, Gesture.GestureType.TYPESCREENTAP);
			HandleGestureConfig(new string[]{"ScreenTap.MinForwardVelocity", "ScreenTap.HistorySeconds", "ScreenTap.MinDistance"}, 
			                    FScreenTabMindForwardVelo, FScreenTabHistorySeconds, FScreenTabMindDistance);
		}
		
		//generic enable
		void HandleEnableGesture(IDiffSpread<bool> pin, Gesture.GestureType gestureType)
		{
			if(pin.IsChanged)
				FLeapController.EnableGesture(gestureType, pin[0]);
		}
		
		//generic gesture config
		void HandleGestureConfig(string[] keys, params IDiffSpread<float>[] pins)
		{
			for (int i = 0; i < keys.Length; i++) 
			{
				if(pins[i].IsChanged)
					FLeapController.Config.SetFloat("Gesture." + keys[i], pins[i][0]);
			}	
		}

		public void Dispose()
		{
			FLeapController.Dispose();
		}
		
	}
	
	public abstract class LeapSplitGestureNodeBase : IPluginEvaluate
	{
		#region fields & pins
		#pragma warning disable 0649
		
		//screen tab
		[Input("Gesture List")]
		IDiffSpread<GestureList> FGestureListIn;

        [Output("ID")]
        ISpread<int> FIDOut;
        
        [Output("Duration")]
        ISpread<float> FDurationOut;
        
        [Output("State")]
        ISpread<Gesture.GestureState> FStateOut;
        
		#pragma warning restore
		
		#endregion fields & pins
		
		public void Evaluate(int SpreadMax)
		{
			if(FGestureListIn.IsChanged)
			{
				ClearPins();
				var list = FGestureListIn[0];
				if(list == null) return;
				
				for (int i = 0; i < list.Count; i++)
				{
					var gesture = list[i];
					if(CheckGesture(gesture))
					{
						FIDOut.Add(gesture.Id);
						FDurationOut.Add(gesture.DurationSeconds);
						FStateOut.Add(gesture.State);
						SplitGesture(gesture);
					}
				}
			}
		}
		
		protected virtual void ClearPins()
		{
			FIDOut.SliceCount = 0;
			FDurationOut.SliceCount = 0;
			FStateOut.SliceCount = 0;
		}
		
		protected abstract bool CheckGesture(Gesture gesture);
		protected abstract void SplitGesture(Gesture gesture);
	}
	
	#region PluginInfo
	[PluginInfo(Name = "CircleGesture",
	Category = "Leap", 
	Help = "Returns the tracking data of the Leap circle gesture",
	Tags = "tracking, hand, finger",
	AutoEvaluate = true)]
	#endregion PluginInfo
	public class LeapSplitCircleGestureNode : LeapSplitGestureNodeBase
	{
		[Output("Center")]
        ISpread<Vector3D> FCenterOut;
        
        [Output("Normal")]
        ISpread<Vector3D> FNormalOut;
        
        [Output("Radius")]
        ISpread<float> FRadiusOut;
        
        [Output("Progress")]
        ISpread<float> FProgressOut;
        
        protected override void SplitGesture(Gesture gesture)
        {
        	var g = new CircleGesture(gesture);
        	FCenterOut.Add(g.Center.ToVector3DPos());
        	FNormalOut.Add(g.Normal.ToVector3DDir());
        	FRadiusOut.Add(g.Radius * 0.001f);
        	FProgressOut.Add(g.Progress);
        }
		
		protected override void ClearPins()
		{
			base.ClearPins();
			
			FCenterOut.SliceCount = 0;
			FNormalOut.SliceCount = 0;
			FRadiusOut.SliceCount = 0;
			FProgressOut.SliceCount = 0;
		}
		
		protected override bool CheckGesture(Gesture gesture)
		{
			return gesture.Type == Gesture.GestureType.TYPECIRCLE;
		}
	}
	
	#region PluginInfo
	[PluginInfo(Name = "SwipeGesture",
	Category = "Leap", 
	Help = "Returns the tracking data of the Leap swipe gesture",
	Tags = "tracking, hand, finger",
	AutoEvaluate = true)]
	#endregion PluginInfo
	public class LeapSplitSwipeGestureNode : LeapSplitGestureNodeBase
	{
		[Output("Direction")]
        ISpread<Vector3D> FDirectinOut;
        
        [Output("Position")]
        ISpread<Vector3D> FPositionOut;
        
        [Output("Speed")]
        ISpread<float> FSpeedOut;
        
        [Output("Start Position")]
        ISpread<Vector3D> FStartPositionOut;
        
        protected override void SplitGesture(Gesture gesture)
        {
        	var g = new SwipeGesture(gesture);
        	FDirectinOut.Add(g.Direction.ToVector3DDir());
        	FPositionOut.Add(g.Position.ToVector3DPos());
        	FSpeedOut.Add(g.Speed * 0.001f);
        	FStartPositionOut.Add(g.StartPosition.ToVector3DPos());
        }
		
		protected override void ClearPins()
		{
			base.ClearPins();
			
			FDirectinOut.SliceCount = 0;
			FPositionOut.SliceCount = 0;
			FSpeedOut.SliceCount = 0;
			FStartPositionOut.SliceCount = 0;
		}
		
		protected override bool CheckGesture(Gesture gesture)
		{
			return gesture.Type == Gesture.GestureType.TYPESWIPE;
		}
	}
	
	#region PluginInfo
	[PluginInfo(Name = "KeyTabGesture",
	Category = "Leap", 
	Help = "Returns the tracking data of the Leap key tab gesture",
	Tags = "tracking, hand, finger",
	AutoEvaluate = true)]
	#endregion PluginInfo
	public class LeapSplitKeyTabGestureNode : LeapSplitGestureNodeBase
	{
		[Output("Direction")]
        ISpread<Vector3D> FDirectinOut;
        
        [Output("Position")]
        ISpread<Vector3D> FPositionOut;
        
        [Output("Progress")]
        ISpread<float> FProgressOut;
        
        protected override void SplitGesture(Gesture gesture)
        {
        	var g = new KeyTapGesture(gesture);
        	FDirectinOut.Add(g.Direction.ToVector3DDir());
        	FPositionOut.Add(g.Position.ToVector3DPos());
        	FProgressOut.Add(g.Progress);
        }
		
		protected override void ClearPins()
		{
			base.ClearPins();
			
			FDirectinOut.SliceCount = 0;
			FPositionOut.SliceCount = 0;
			FProgressOut.SliceCount = 0;
		}
		
		protected override bool CheckGesture(Gesture gesture)
		{
			return gesture.Type == Gesture.GestureType.TYPEKEYTAP;
		}
	}
	
    #region PluginInfo
	[PluginInfo(Name = "ScreenTabGesture",
	Category = "Leap", 
	Help = "Returns the tracking data of the Leap screen tab gesture",
	Tags = "tracking, hand, finger",
	AutoEvaluate = true)]
	#endregion PluginInfo
	public class LeapSplitScreenTabGestureNode : LeapSplitGestureNodeBase
	{
		[Output("Direction")]
        ISpread<Vector3D> FDirectinOut;
        
        [Output("Position")]
        ISpread<Vector3D> FPositionOut;
        
        [Output("Progress")]
        ISpread<float> FProgressOut;
        
        protected override void SplitGesture(Gesture gesture)
        {
        	var g = new KeyTapGesture(gesture);
        	FDirectinOut.Add(g.Direction.ToVector3DDir());
        	FPositionOut.Add(g.Position.ToVector3DPos());
        	FProgressOut.Add(g.Progress);
        }
		
		protected override void ClearPins()
		{
			base.ClearPins();
			
			FDirectinOut.SliceCount = 0;
			FPositionOut.SliceCount = 0;
			FProgressOut.SliceCount = 0;
		}
		
		protected override bool CheckGesture(Gesture gesture)
		{
			return gesture.Type == Gesture.GestureType.TYPESCREENTAP;
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




