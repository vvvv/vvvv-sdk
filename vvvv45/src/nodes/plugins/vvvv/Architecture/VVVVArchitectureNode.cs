#region usings
using System;
using System.ComponentModel.Composition;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using VVVV.Core.Logging;
#endregion usings

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "Architecture", 
				Category = "VVVV", 
				Help = "Returns the CPU architecture of this vvvv instance.", 
				Tags = "processor, x64, x86, cpu")]
	#endregion PluginInfo
	public class ArchitectureNode : IPluginEvaluate
	{
		[ImportingConstructor]
		public ArchitectureNode([Output("Output")] ISpread<string> output, [Output("Is x64")] ISpread<bool> x64)
		{
		    if (IntPtr.Size == 4)
		        output[0] = "x86";
		    else if (IntPtr.Size == 8)
		    {
		        output[0] = "x64";
		        x64[0] = true;
		    }
		    else
		        output[0] = "Welcome to the Future!";
		}

		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			//nothing to do here
		}
	}
}
