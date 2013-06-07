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
	[PluginInfo(Name = "PID", 
				Category = "VVVV", 
				Help = "Returns the Process ID of this vvvv instance.", 
				Tags = "process")]
	#endregion PluginInfo
	public class VVVVPIDNode : IPluginEvaluate
	{
		#region fields & pins
		[Output("ID", IsSingle=true)]
		ISpread<int> FOutput;
		#endregion fields & pins

		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			FOutput[0] = System.Diagnostics.Process.GetCurrentProcess().Id;
		}
	}
}
