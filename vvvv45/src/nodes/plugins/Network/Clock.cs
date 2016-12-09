#region usings
using System;
using System.Text;
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
	[PluginInfo(Name = "Clock", 
	Category = "Network", 
	Version = "Boygroup",
	Help = "Basic template with one value in/out", 
	Tags = "",
	AutoEvaluate = true)]
	#endregion PluginInfo
	public class NetworkClockServerNode : IPluginEvaluate
	{
		#region fields & pins
#pragma warning disable 0649
        [Input("Time", IsSingle = true)]
        ISpread<double> FTime;

        [Input("Set", IsBang = true, IsSingle = true)]
        ISpread<bool> FInput;

        [Output("Time")]
        ISpread<double> FOutput;

        [Import]
        IHDEHost FHost;
#pragma warning restore
		#endregion fields & pins
		
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			if(FInput[0])
			{
				FHost.SetRealTime(FTime[0]);
			}

			FOutput[0] = FHost.RealTime;
		}
	}
	
}
