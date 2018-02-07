using System;
using System.ComponentModel.Composition;
using VVVV.PluginInterfaces.V2;

//the vvvv node namespace
namespace VVVV.Nodes
{
	[PluginInfo(Name = "TimerFlop", Category = "Animation", Author = "woei", Help = "Switches to one if the input has been 1 for a certain time", AutoEvaluate = true)]
	public class TimerFlopNode: IPluginEvaluate
    {	          	
    	[Input("Set")]
    	public ISpread<bool> FSet;

    	[Input("Time", DefaultValue = 1, MinValue = 0)]
    	public ISpread<double> FTime;

    	[Input("Reset", IsBang = true)]
    	public ISpread<bool> FReset;

    	[Output("Output")]
    	public ISpread<bool> FOutput;

    	[Output("Running")]
    	public ISpread<double> FRunning;

    	[Output("Active")]
    	public ISpread<bool> FActive;

    	[Import]
    	IPluginHost2 FHost;

        private Spread<double> FStart = new Spread<double>(0);
       
        public void Evaluate(int SpreadMax)
        {
            double now;
            FHost.GetCurrentTime(out now);
        	
        	FOutput.SliceCount = SpreadMax;
        	FRunning.SliceCount = SpreadMax;
        	FActive.SliceCount = SpreadMax;

        	FStart.SliceCount = SpreadMax;
        	
        	for (int i=0; i<SpreadMax; i++)
        	{
        		var curRunning = 0.0;
        		var curOut = false;

                if (FReset[i] || (FSet[i] && !FActive[i]))
                    FStart[i] = now;
        		
                FActive[i] = FSet[i];
        		
        		if (FActive[i])
        		{
                    double elapsed = now - FStart[i];
        			curRunning = Math.Min(elapsed/FTime[i],1.0);
        			if (elapsed>=FTime[i])
        				curOut=true;
        		}
        		FOutput[i] = curOut;
        		FRunning[i] = curRunning;
        	}
        }
	}
}