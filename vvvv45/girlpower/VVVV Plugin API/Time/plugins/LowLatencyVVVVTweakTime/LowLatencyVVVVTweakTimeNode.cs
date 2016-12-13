#region usings
using System;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using VVVV.Core.Logging;
#endregion usings

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "TweakTime", Category = "VVVV", Version = "LowLatency", AutoEvaluate = true)]
	#endregion PluginInfo
	public class LowLatencyVVVVTweakTimeNode : IPluginEvaluate, IPartImportsSatisfiedNotification, IDisposable
		//, ITimeProvider
	{
		[Input("Time")]
		public ISpread<double> FInput;

		[Input("Enabled")]
		public ISpread<bool> FEnabledPin;

		[Import()]
		public IHDEHost FHost;
		
		public void OnImportsSatisfied()
		{
			// we can turn on time provider before evaluation of first frame
			Enabled = true;			
		}	
				
		public void Evaluate(int SpreadMax)
		{
			// if we set the time here, it will only be able to affect the next frame. 
			// the current frame is already evaluating...

			Time = FInput[0];
			Enabled = FEnabledPin[0];
			
			// if we however would set our Time property asynchronously we 			
			// should be able to reduce the latency about up to a frame.
		}
		
		double Time;

//   	outcomment when you want to implement the interface ITimeProvider
//      public double GetTime(double originalNewFrameTime) 
//		{
//			return Time;
//		}
		
		bool enabled;
		public bool Enabled
		{
			get{
				return enabled;
			}
			set{
				if (value != enabled)
				{
					enabled = value;
					if (value)
					{
						// we can implement the interface or just hand a delegate 
						// pointing to our time property. 
						
						//FHost.SetFrameTimeProvider(this);
						FHost.SetFrameTimeProvider(_ => Time);

						// after setting the time provider vvvv will call us back each time
						// it needs to pin down the frame time of the frame it is about to evaluate.
						// we can hand any time we want. in our example we even let the patcher decide.					
					}
					else
						FHost.SetFrameTimeProvider((ITimeProvider)null);
				}
			}
		}
		
		public void Dispose()
		{
			Enabled = false;
		}
	}
}
