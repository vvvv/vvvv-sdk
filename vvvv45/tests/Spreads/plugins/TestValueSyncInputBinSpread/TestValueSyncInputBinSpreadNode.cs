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
	[PluginInfo(Name = "SyncInputBinSpread", Category = "Value", Version = "Test", Help = "Basic template with one value in/out", Tags = "")]
	#endregion PluginInfo
	public class TestValueSyncInputBinSpreadNode : IPluginEvaluate
	{
		[Input("Input", AutoValidate = false)]
		ISpread<ISpread<double>> FInput;

		[Output("Exception")]
		ISpread<string> FExceptionMsg;

		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			FExceptionMsg.SliceCount = 0;
			
			try
			{
				FInput.Sync();
			}
			catch (Exception e)
			{
				FExceptionMsg.SliceCount = 1;
				FExceptionMsg[0] = e.Message;
			}
		}
	}
}
