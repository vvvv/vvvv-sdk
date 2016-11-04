#region usings
using System;
using System.ComponentModel.Composition;
using System.Collections.Generic;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using VVVV.Core.Logging;
using VVVV.Utils.Animation;
#endregion usings

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "OneEuroFilter", 
				Category = "Animation", 
				Help = "Simple speed-based low-pass filter for noisy input in interactive systems", 
				Credits = "http://www.lifl.fr/~casiez/1euro and Mitsuru Furuta for the c# implementation",
				Tags = "1€",
			    AutoEvaluate = true)]
	#endregion PluginInfo
	public class OneEuroFilterNode : IPluginEvaluate, IPartImportsSatisfiedNotification
	{
		#region fields & pins
		[Input("Input", DefaultValue = 1.0)]
		public ISpread<double> FInput;
		
		[Input("Minimum Cutoff", DefaultValue = 1.0, MinValue = 0.0, MaxValue = 10.0)]
		public ISpread<double> FMinCutoff;
		
		[Input("Beta", DefaultValue = 0.7, MinValue = 0.0, MaxValue = 10.0)]
		public ISpread<double> FBeta;
		
		[Input("Cutoff for Derivative", DefaultValue = 1.0, MinValue = 0.0, MaxValue = 10.0, Visibility = PinVisibility.OnlyInspector)]
		public ISpread<double> FCutoffDerivative;

		[Output("Output")]
		public ISpread<double> FOutput;
		
		Spread<OneEuroFilter> FFilters = new Spread<OneEuroFilter>(0);

		[Import()]
		public ILogger FLogger;
		
		[Import()]
		public IHDEHost FHDEHost;
		
		double FLastFrameTime;
		#endregion fields & pins
		
        public void OnImportsSatisfied()
        {
            FLastFrameTime = FHDEHost.FrameTime;
        }

		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
            //resize spread
            FFilters.ResizeAndDismiss(SpreadMax, i => new OneEuroFilter(FMinCutoff[i], FBeta[i]));
			
			FOutput.SliceCount = SpreadMax;

			var rate = 1 / (FHDEHost.FrameTime - FLastFrameTime);
			FLastFrameTime = FHDEHost.FrameTime;
			
			for (int i = 0; i < SpreadMax; i++)
			{
				FFilters[i].MinCutoff = FMinCutoff[i];
				FFilters[i].Beta = FBeta[i];
				FFilters[i].CutoffDerivative = FCutoffDerivative[i];
				
				FOutput[i] = FFilters[i].Filter(FInput[i], rate);
			}
		}
	}
}
