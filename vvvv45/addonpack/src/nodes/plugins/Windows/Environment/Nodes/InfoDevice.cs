#region usings
using System;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using System.Management;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

using VVVV.Core.Logging;
#endregion usings

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "Info", Category = "System", Version = "Device", Help = "Retrieves hardware information", Author = "woei")]
	#endregion PluginInfo
	public class InfoDeviceNode : IPluginEvaluate, IDisposable
	{
		#region fields & pins
		#region pins
		[Input("Refresh", IsBang = true)]
		public IDiffSpread<bool> FRefresh;
		
		[Output("Mainboard")]
		public ISpread<string> FMB;
		
		[Output("Processor")]
    	public ISpread<string> FCPU;
		
		[Output("RAM")]
    	public ISpread<string> FRAM;
		
		[Output("HDD")]
    	public ISpread<string> FHDD;
		
		[Output("USB Controller")]
    	public ISpread<string> FUSB;
		
		[Output("1394 Controller")]
    	public ISpread<string> FFirewire;
		
		[Output("Sound")]
    	public ISpread<string> FAudio;
		
		[Output("Graphics")]
    	public ISpread<string> FVideo;
		
		[Output("All Devices")]
    	public ISpread<string> FDevice;

		[OutputAttribute("Working")]
		public ISpread<bool> FWorking;
		
		[Import()]
		public ILogger FLogger;
		#endregion pins
		
		#region fields
		private string[] allNames = new string[9] {
        			"DeviceID","Name",
        			"Description","Manufacturer",
        			"Service","ConfigManagerErrorCode","Availability","Status","StatusInfo"};
		private string[] bbNames = new string[5]{
        			"Manufacturer","Product","Model",
        			"Version","SerialNumber"};
		private string[] mbNames = new string[3]{"PrimaryBusType","SecondaryBusType","Status"};
		private string[] cpuNames = new string[8]{
        			"DeviceID","Name",
        			"Description","AddressWidth",
        			"L2CacheSize","L3CacheSize",
        			"Availability","CpuStatus"};
		private string[] ramNames = new string[7] {
        			"DeviceLocator","Name",
        			"DataWidth","Capacity",
        			"FormFactor","MemoryType",
        			"Status"};
		private string[] hddNames = new string[9] {
        			"DeviceID","Model",
        			"InterfaceType","Size","Partitions",
        			"MediaLoaded","MediaType",
        			"Availability","Status"};
		
		private string[] usbNames = new string[4] {
        			"DeviceID","Name",
        			"ProtocolSupported","Status"};
		
		private string[] audioNames = new string[6] {
        			"DeviceID","Name",
        			"Manufacturer","ProductName",
        			"Status","ConfigManagerErrorCode"};
		
        private string[] fwNames = new string[4] {
        			"DeviceID","Name",
        			"ProtocolSupported","Status"};
		
		private string[] videoNames = new string[9] {
        			"DeviceID","Name",
        			"AdapterCompatibility","VideoProcessor","AdapterRAM",
        			"InfFilename","InstalledDisplayDrivers","Availability","Status"};
		
		private CancellationTokenSource FCts;
		private Task<string[]>[] FTasks = new Task<string[]>[9];
		private bool FFirstframe = true;
		#endregion fields
		#endregion fields & pins

		public void Dispose()
		{
			CancelRunningTasks();
		}
		
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			if (FRefresh[0] || FFirstframe)
        	{	
        		FFirstframe = false;
        		CancelRunningTasks();
        		
        		CreateTasks();
        		FWorking[0] = true;
        		foreach (var t in FTasks)
        			t.Start();
        	}
			
			if (FWorking[0])
			{
				FWorking[0] = false;
				foreach (var tt in FTasks)
					 if (!tt.IsCompleted)
					 		FWorking[0] = true;
			}
		}
		
		
		
		private string[] GetMainboardProperties()
		{
			List<string> lmbOut = new List<string>(GetManagementClassProperties("Win32_BaseBoard",bbNames));
    		List<string> mb = new List<string>(GetManagementClassProperties("Win32_MotherboardDevice",mbNames));
    		for (int i=0; i<mb.Count; i++)
    			lmbOut[i%lmbOut.Count]+=Environment.NewLine+mb[i];
			
			return lmbOut.ToArray();
		}
		
		private IEnumerable<string> GetManagementClassProperties(string key, string[] properties)
        {
        	ManagementClass mc = new ManagementClass(key);
        	mc.Options.UseAmendedQualifiers = true;
        	foreach (ManagementObject mo in mc.GetInstances())
        	{
        		string pageOut = string.Empty;
        		foreach (string pdn in properties)
        		{
        			string line = string.Empty;
        			try
        			{
        				string curProp = mo.Properties[pdn].Value.GetType().ToString();
        				if (pageOut!=string.Empty)
        					pageOut+=Environment.NewLine;
        				switch (curProp)
        				{
        					case "System.String[]":
        						string[] str = (string[])mo.Properties[pdn].Value;
        						foreach (string st in str)
        							line += st + " ";
        						break;
        					case "System.UInt16[]":
        						ushort[] shortData = (ushort[])mo.Properties[pdn].Value;
        						foreach (ushort st in shortData)
        							line += st.ToString() + " ";
        						break;
        					default:
        						line = mo.Properties[pdn].Value.ToString();
        						break;
        				}
        				pageOut+=pdn+": "+line;
        			}
        			catch
        			{
        			}
        		}
        		yield return pageOut;
        	}
        }
		
		private void CreateTasks()
		{
			FCts = new CancellationTokenSource();
			FTasks = new Task<string[]>[9];
			
			FTasks[0] = new Task<string[]>(() => { 
				FCts.Token.ThrowIfCancellationRequested(); 
				return GetManagementClassProperties("Win32_Processor",cpuNames).ToArray();
			} );
        	FTasks[0].ContinueWith(t => { FCts.Token.ThrowIfCancellationRequested(); FCPU.AssignFrom(t.Result); }, 
					FCts.Token, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Default);
			
			FTasks[1] = new Task<string[]>(() => { 
				FCts.Token.ThrowIfCancellationRequested(); 
				return GetManagementClassProperties("Win32_PhysicalMemory",ramNames).ToArray(); 
			} );
       		FTasks[1].ContinueWith(t => { FCts.Token.ThrowIfCancellationRequested(); FRAM.AssignFrom(t.Result); }, 
					FCts.Token, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Default);
			
			FTasks[2] = new Task<string[]>(() => { FCts.Token.ThrowIfCancellationRequested(); return GetManagementClassProperties("Win32_DiskDrive",hddNames).ToArray(); } );
       		FTasks[2].ContinueWith(t => { FCts.Token.ThrowIfCancellationRequested(); FHDD.AssignFrom(t.Result); }, 
					FCts.Token, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Default);
			
			FTasks[3] = new Task<string[]>(() => { FCts.Token.ThrowIfCancellationRequested(); return GetManagementClassProperties("Win32_USBController",usbNames).ToArray(); } );
       		FTasks[3].ContinueWith(t => { FCts.Token.ThrowIfCancellationRequested(); FUSB.AssignFrom(t.Result); }, 
					FCts.Token, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Default);
			
			FTasks[4] = new Task<string[]>(() => { FCts.Token.ThrowIfCancellationRequested(); return GetManagementClassProperties("Win32_1394Controller",fwNames).ToArray(); } );
       		FTasks[4].ContinueWith(t => { FCts.Token.ThrowIfCancellationRequested(); FFirewire.AssignFrom(t.Result); }, 
					FCts.Token, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Default);
			
			FTasks[5] = new Task<string[]>(() => { FCts.Token.ThrowIfCancellationRequested(); return GetManagementClassProperties("Win32_SoundDevice",audioNames).ToArray(); } );
       		FTasks[5].ContinueWith(t => { FCts.Token.ThrowIfCancellationRequested(); FAudio.AssignFrom(t.Result); }, 
					FCts.Token, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Default);
			
			FTasks[6] = new Task<string[]>(() => { FCts.Token.ThrowIfCancellationRequested(); return GetManagementClassProperties("Win32_VideoController",videoNames).ToArray(); } );
       		FTasks[6].ContinueWith(t => { FCts.Token.ThrowIfCancellationRequested(); FVideo.AssignFrom(t.Result); }, 
					FCts.Token, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Default);
			
			FTasks[7] = new Task<string[]>(() => { FCts.Token.ThrowIfCancellationRequested(); return GetManagementClassProperties("Win32_PnPEntity",allNames).ToArray(); } );
       		FTasks[7].ContinueWith(t => { FCts.Token.ThrowIfCancellationRequested(); FDevice.AssignFrom(t.Result); }, 
					FCts.Token, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Default);
			
			FTasks[8] = new Task<string[]>(() => { FCts.Token.ThrowIfCancellationRequested(); return GetMainboardProperties(); } );
       		FTasks[8].ContinueWith(t => { FCts.Token.ThrowIfCancellationRequested(); FMB.AssignFrom(t.Result); }, 
					FCts.Token, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Default);
		}
		
		private void CancelRunningTasks()
        {
            if (FCts != null)
            {
                FCts.Cancel();
                try
                {
                	foreach (var t in FTasks)
                		t.Wait();
                }
                catch (AggregateException e)
                {
                    foreach (var exception in e.InnerExceptions)
                        FLogger.Log(exception);
                }
                FCts.Dispose();
                FCts = null;

               	foreach (var tt in FTasks)
            		tt.Dispose();
            }
        }

	}
}
