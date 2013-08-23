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
        
        [Output("Is Tool")]
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
        
        [Output("Gestures")]
        ISpread<GestureList> FGestureOut;
        
		#pragma warning restore
		
		Controller FLeapController = new Controller();
		Frame FLastFrame = new Frame();
		
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
				FGestureOut[0] = frame.Gestures();
				
				var gestures = FLeapController.Frame().Gestures();
				for (int i = 0; i < gestures.Count; i++) 
				{
					var gesture = gestures[i];

					switch (gesture.Type) 
					{
						case Gesture.GestureType.TYPECIRCLE:
							CircleGesture circle = new CircleGesture (gesture);

							break;
						case Gesture.GestureType.TYPESWIPE:
							SwipeGesture swipe = new SwipeGesture (gesture);
//							SafeWriteLine ("Swipe id: " + swipe.Id
//							               + ", " + swipe.State
//							               + ", position: " + swipe.Position
//							               + ", direction: " + swipe.Direction
//							               + ", speed: " + swipe.Speed);
							break;
						case Gesture.GestureType.TYPEKEYTAP:
							KeyTapGesture keytap = new KeyTapGesture (gesture);
//							SafeWriteLine ("Tap id: " + keytap.Id
//							               + ", " + keytap.State
//							               + ", position: " + keytap.Position
//							               + ", direction: " + keytap.Direction);
							break;
						case Gesture.GestureType.TYPESCREENTAP:
							ScreenTapGesture screentap = new ScreenTapGesture (gesture);
//							SafeWriteLine ("Tap id: " + screentap.Id
//							               + ", " + screentap.State
//							               + ", position: " + screentap.Position
//							               + ", direction: " + screentap.Direction);
							break;
						default:
//							SafeWriteLine ("Unknown gesture type.");
							break;
					}
				}
				
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




