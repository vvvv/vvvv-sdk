#region usings
using System;
using System.Management;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using VVVV.Core.Logging;
using System.Threading;
using System.Threading.Tasks;
#endregion usings

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "Processors", 
				Category = "System", 
				Help = "Reports number of physical processors, processor cores and logical processors", 
				Tags = "core")]
	#endregion PluginInfo
	public class SystemProcessorsNode : IPluginEvaluate, IDisposable
	{
        #region fields & pins
        [Output("Physical Processors", IsSingle = true)]
        ISpread<int> FPhysical;
        [Output("Cores", IsSingle = true)]
        ISpread<int> FCores;
        [Output("Logical Processors", IsSingle = true)]
        ISpread<int> FLogical;

		[Import()]
        public ILogger FLogger;

        bool FFirstframe = true;
        CancellationTokenSource FCts;
        Task FPhysicalCountTask;
        Task FCoreCountTask;
        #endregion fields & pins

        public void Dispose()
        {
            CancelRunningTasks();
        }

        //called when data for any output pin is requested
        public void Evaluate(int SpreadMax)
		{
            if (FFirstframe)
            {
                FCts = new CancellationTokenSource();
                FPhysicalCountTask = new Task(() => 
                {
                    FCts.Token.ThrowIfCancellationRequested();
                    FPhysical[0] = GetPhysicalProcessorCount();
                    return;
                });
                FPhysicalCountTask.Start();

                FCoreCountTask = new Task(() =>
                {
                    FCts.Token.ThrowIfCancellationRequested();
                    FCores[0] = GetCoresCount();
                    return;
                });
                FCoreCountTask.Start();

                FLogical[0] = Environment.ProcessorCount;

                FFirstframe = false;
            }
        }

        int GetPhysicalProcessorCount()
        {
            //via http://stackoverflow.com/questions/1542213/how-to-find-the-number-of-cpu-cores-via-net-c
            foreach (var item in new System.Management.ManagementObjectSearcher("Select * from Win32_ComputerSystem").Get())
            {
                return int.Parse(item["NumberOfProcessors"].ToString());
            }

            return 0;
        }

        int GetCoresCount()
        {
            int coreCount = 0;
            foreach (var item in new System.Management.ManagementObjectSearcher("Select * from Win32_Processor").Get())
            {
                coreCount += int.Parse(item["NumberOfCores"].ToString());
            }
            return coreCount;
        }

        private void CancelRunningTasks()
        {
            if (FCts != null)
            {
                FCts.Cancel();
                try
                {
                    FPhysicalCountTask.Wait();
                    FCoreCountTask.Wait();
                }
                catch (AggregateException e)
                {
                    foreach (var exception in e.InnerExceptions)
                        FLogger.Log(exception);
                }
                FCts.Dispose();
                FCts = null;

                FPhysicalCountTask.Dispose();
                FCoreCountTask.Dispose();
            }
        }
    }
}
