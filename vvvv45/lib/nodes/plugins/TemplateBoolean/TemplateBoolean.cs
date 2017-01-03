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
	[PluginInfo(Name = "Template",
	            Category = "Boolean",
	            Help = "Basic template with one boolean in/out",
	            Tags = "c#")]
	#endregion PluginInfo
	public class Template : IPluginEvaluate
	{
		#region fields & pins
		[Input("Input")]
        public ISpread<bool> FInput;

		[Output("bool")]
        public ISpread<bool> FOutput;

		[Import()]
        public ILogger FLogger;
		#endregion fields & pins

		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			FOutput.SliceCount = SpreadMax;

			for (int i = 0; i < SpreadMax; i++)
				FOutput[i] = !FInput[i];

			//FLogger.Log(LogType.Debug, "hi TTY");
		}
	}
}
