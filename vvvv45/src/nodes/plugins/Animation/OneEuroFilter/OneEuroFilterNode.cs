#region usings
using System;
using System.ComponentModel.Composition;
using System.Collections.Generic;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using VVVV.Core.Logging;
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
		
		[Input("Beta", StepSize = 0.001, DefaultValue = 0.007, MinValue = 0.0, MaxValue = 1.0)]
		public ISpread<double> FBeta;
		
		[Input("Cutoff for Derivative", DefaultValue = 1.0, MinValue = 0.0, MaxValue = 10.0)]
		public ISpread<double> FCutoffDerivative;

		[Output("Output")]
		public ISpread<double> FOutput;
		
		List<OneEuroFilter> FFilters = new List<OneEuroFilter>();

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
			//add new slices
			for (int i=FFilters.Count; i<=SpreadMax; i++)
				FFilters.Add(new OneEuroFilter(FMinCutoff[i], FBeta[i]));
			
			//remove old slices
			for (int i=FFilters.Count-1; i == SpreadMax-1; i--)
				FFilters.RemoveAt(i);
			
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
	
	//via http://www.lifl.fr/~casiez/1euro/OneEuroFilter.cs
	public class OneEuroFilter
    {
        public OneEuroFilter(double minCutoff, double beta)
        {
            firstTime = true;
            this.minCutoff = minCutoff;
            this.beta = beta;

            xFilt = new LowpassFilter();
            dxFilt = new LowpassFilter();
            dcutoff = 1;
        }

        protected bool firstTime;
        protected double minCutoff;
        protected double beta;
        protected LowpassFilter xFilt;
        protected LowpassFilter dxFilt;
        protected double dcutoff;

        public double MinCutoff
        {
            get { return minCutoff; }
            set { minCutoff = value; }
        }

        public double Beta
        {
            get { return beta; }
            set { beta = value; }
        }
    	
    	public double CutoffDerivative
        {
            get { return dcutoff; }
            set { dcutoff = value; }
        }

        public double Filter(double x, double rate)
        {
            double dx = firstTime ? 0 : (x - xFilt.Last()) * rate;
            if (firstTime)
                firstTime = false;

            var edx = dxFilt.Filter(dx, Alpha(rate, dcutoff));
            var cutoff = minCutoff + beta * Math.Abs(edx);

            return xFilt.Filter(x, Alpha(rate, cutoff));
        }

        protected double Alpha(double rate, double cutoff)
        {
            var tau = 1.0 / (2 * Math.PI * cutoff);
            var te = 1.0 / rate;
            return 1.0 / (1.0 + tau / te);
        }
    }

    public class LowpassFilter
    {
        public LowpassFilter()
        {
            firstTime = true;
        }

        protected bool firstTime;
        protected double hatXPrev;

        public double Last()
        {
            return hatXPrev;
        }

        public double Filter(double x, double alpha)
        {
            double hatX = 0;
            if (firstTime)
            {
                firstTime = false;
                hatX = x;
            }
            else
                hatX = alpha * x + (1 - alpha) * hatXPrev;

            hatXPrev = hatX;

            return hatX;
        }
    }
}
