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
	[PluginInfo(Name = "Bug", Category = "ISpread<ISpread<>>", Help = "", Tags = "")]
	#endregion PluginInfo
	public class MultiDimBugger : IPluginEvaluate
	{
		#region fields & pins
		[Input("Input")]
        public ISpread<ISpread<double>> FInput;
		
		[Output("Output")]
        public ISpread<ISpread<double>> FOutput;
		
		[Import]
		public ILogger FLogger;
		#endregion fields & pins

		//called when data for any output pin is requested
		public void Evaluate(int spreadMax)
		{
			FOutput.AssignFrom(FInput);
		}
	}
}
