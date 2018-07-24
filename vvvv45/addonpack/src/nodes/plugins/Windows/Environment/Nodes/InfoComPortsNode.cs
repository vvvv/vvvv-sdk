#region usings
using System;
using System.ComponentModel.Composition;
using System.IO.Ports;


using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using VVVV.Core.Logging;
#endregion usings

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "Info",
	            Category = "System",
	            Version = "ComPorts",
	            Help = "get active ComPortsNames from Windows",
	            Tags = "")]
	#endregion PluginInfo
	
	//u7angel
	
	public class InfoComPorts : IPluginEvaluate
	{
		[Input("Update", IsBang=true)]
		IDiffSpread<bool> FInput;

		[Output("Output")]
		ISpread<string> FOutput;

		[Import()]
		ILogger FLogger;
		
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			if (FInput.IsChanged)
			{
				string[] ports = SerialPort.GetPortNames();
				SpreadMax = ports.GetLength(0);
				FOutput.SliceCount = SpreadMax;
				for (int i = 0; i < SpreadMax; i++)
					FOutput[i] = ports[i];
			}
		}
	}
}
