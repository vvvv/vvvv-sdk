#region usings
using System;
using System.ComponentModel.Composition;
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
	[PluginInfo(Name = "Kill", 
				Category = "Windows", 
				Help = "Kills running processes. Handle with care!", 
				Tags = "process",
				AutoEvaluate = true)]
	#endregion PluginInfo
	public class C2WindowsKillNode : IPluginEvaluate
	{
		#region fields & pins
		[Input("Process ID", DefaultValue = -1)]
		public ISpread<int> FID;
		
		[Input("Kill", IsBang = true)]
		public ISpread<bool> FKill;

		[Import()]
		public ILogger FLogger;
		#endregion fields & pins

		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			for (int i = 0; i < SpreadMax; i++)
			{
				try
				{
					if (FKill[i])
						Process.GetProcessById(FID[i]).Kill();
				}
				catch (Exception e)
				{
					FLogger.Log(LogType.Error, "Killing PID " + FID[i] + ": " + e.Message);
				}
			}	
		}
	}
}
