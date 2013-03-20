#region usings
using System;
using System.Management;
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
	[PluginInfo(Name = "Processors", 
				Category = "System", 
				Help = "Reports number of physical processors, processor cores and logical processors", 
				Tags = "core")]
	#endregion PluginInfo
	public class SystemProcessorsNode : IPluginEvaluate
	{
		#region fields & pins
		ISpread<int> FPhysical;
		ISpread<int> FCores;
		ISpread<int> FLogical;

		[Import()]
		ILogger FLogger;
		#endregion fields & pins
		
		[ImportingConstructor]
		public SystemProcessorsNode([Output("Physical Processors", IsSingle = true)] ISpread<int> physical, [Output("Cores", IsSingle = true)] ISpread<int> cores, [Output("Logical Processors", IsSingle = true)] ISpread<int> logical)
		{
			FPhysical = physical;
			FCores = cores;
			FLogical = logical;
			
			//via http://stackoverflow.com/questions/1542213/how-to-find-the-number-of-cpu-cores-via-net-c
			foreach (var item in new System.Management.ManagementObjectSearcher("Select * from Win32_ComputerSystem").Get())
			{
			    FPhysical[0] = int.Parse(item["NumberOfProcessors"].ToString());
			}
			
			int coreCount = 0;
			foreach (var item in new System.Management.ManagementObjectSearcher("Select * from Win32_Processor").Get())
			{
			    coreCount += int.Parse(item["NumberOfCores"].ToString());
			}
			FCores[0] = coreCount;
			
			FLogical[0] = Environment.ProcessorCount;
		}

		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			
		}
	}
}
