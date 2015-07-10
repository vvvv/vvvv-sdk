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
	[PluginInfo(Name = "TestMatrix4x4", Category = "Transform", Help = "Basic template with one transform in/out", Tags = "matrix")]
	#endregion PluginInfo
	public class TransformTestMatrix4x4Node : IPluginEvaluate
	{
		#region fields & pins
		[Input("Input")]
		public IDiffSpread<Matrix4x4> FInput;

		[Output("Output")]
		public ISpread<Boolean> FOutput;

		[Import()]
		public ILogger FLogger;
		#endregion fields & pins

		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			FOutput[0] = FInput.IsChanged;
			//FLogger.Log(LogType.Debug, "Logging to Renderer (TTY)");
		}
	}
}
