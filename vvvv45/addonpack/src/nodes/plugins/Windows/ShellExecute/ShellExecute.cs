#region usings
using System;
using System.ComponentModel.Composition;
using System.Diagnostics;

using VVVV.PluginInterfaces.V2;
#endregion usings

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "ShellExecute", Category = "Windows", Version = "Advanced", AutoEvaluate = true, Help = "Executes a program (w/o parameters) and returns the output, errors and status", Tags = "process, command line, cmd", Author="zeos", Credits = "alg - addonpack integration")]
	#endregion PluginInfo
	public class ShellExecute : IPluginEvaluate, IDisposable
	{
		#region fields & pins & contructor & destructor & private members 
		[Input("File", DefaultString = "CMD.EXE", StringType = StringType.Filename, IsSingle = true, Order = 0)]
		ISpread<string> FFileIn;
		
		[Input("Commandline Arguments", DefaultString = "/c dir", IsSingle = true, Order = 1)]
		ISpread<string>FArgsIn;
		
		[Input("Show Window", DefaultValue = 0, IsSingle = true, Order = 2)]
		ISpread<bool>FWindowIn;
		
		[Input("Block until finished", DefaultValue = 0, IsSingle = true, Order = 3)]
		ISpread<bool>FWaitIn;
		
		[Input("Kill", DefaultValue = 0, IsBang=true, IsSingle = true, Order =4)]
		IDiffSpread<bool>FKillIn;
		
		[Input("Execute", IsBang=true, IsSingle = true, Order = 5)]
		IDiffSpread<bool> FExecuteIn;

		[Output("Result")]
		ISpread<string> FResultOut;
		
		[Output("Error")]
		ISpread<string> FErrorOut;
		
		[Output("PID", IsSingle = true, Visibility = PinVisibility.Hidden)]
		ISpread<string> FProcIDOut;
		
		[Output("ExitCode", IsSingle = true)]
		ISpread<int> FExitCodeOut;
		
		[Output("IsRunning", IsBang = true, IsSingle = true)]
		ISpread<bool> FRunningOut;
		
		[Output("Completed", IsBang = true, IsSingle = true)]
		ISpread<bool> FCompletedOut;
		
		private Process FProcess;
	    private bool FRunning;
		private bool FCompleted;
		private readonly object FLock = new object();
		private bool FDisposed;
		private int FDoBang;
		
		[ImportingConstructor]
		public ShellExecute()
		{
			FProcess = new Process {EnableRaisingEvents = true};
		    FProcess.OutputDataReceived += OnDataReceived;
	        FProcess.ErrorDataReceived 	+= OnErrorReceived;
			FProcess.Exited += OnExited;	
			
			FProcess.StartInfo.RedirectStandardOutput = true;
	        FProcess.StartInfo.RedirectStandardError = true;
			FProcess.StartInfo.UseShellExecute = false;
		}
		
		~ShellExecute()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);
		}

		protected void Dispose(bool disposing)
		{
			// Check to see if Dispose has already been called.
			if(!FDisposed)
			{
				if(disposing)
				{
					// Dispose managed resources.
					FProcess.OutputDataReceived -= OnDataReceived;
		       	 	FProcess.ErrorDataReceived -= OnErrorReceived;
					FProcess.Exited -= OnExited;
				    FProcess = null;
				}
				// Release unmanaged resources. If disposing is false,
				// only the following code is executed.
			}
			FDisposed = true;
		}
		#endregion fields & pins

		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			if((FExecuteIn.IsChanged && FExecuteIn[0]) && (!string.IsNullOrEmpty(FFileIn[0])) && !FRunning)
			{
				FResultOut.SliceCount = 0;
				FErrorOut.SliceCount = 0;
				FProcIDOut.SliceCount = 1;
				FExitCodeOut.SliceCount = 1;
				FExitCodeOut[0] = -1;
				FProcIDOut[0] = string.Empty;
				FCompletedOut[0] = false;
				FCompleted = false;
				FRunning = false;
				
				try 
				{
					FProcess.CancelOutputRead();
					FProcess.CancelErrorRead();
				}
				catch 
				{
					
				}
				
				//set up process info
		        FProcess.StartInfo.CreateNoWindow = !FWindowIn[0];
				FProcess.StartInfo.FileName = FFileIn[0];
				FProcess.StartInfo.Arguments = FArgsIn[0];
				
				try
				{
					FRunning = FProcess.Start();
					
					FProcIDOut[0] = FProcess.Id.ToString();
					
					FProcess.BeginOutputReadLine();
					FProcess.BeginErrorReadLine();
					
					if(FWaitIn[0]) 
					{
						FProcess.WaitForExit();
						FDoBang = 2;
					}
				
				}
				catch (Exception e)
				{
					FErrorOut.Add("Error on start: " +e.Message); 
				}
			}
			
			
			if(FDoBang>0) 
			{
				FDoBang--;
				if(FDoBang == 0)
				FRunningOut[0] = true;
			}
			else lock(FLock) { FCompletedOut[0] = FCompleted; }
			
			lock(FLock) { FRunningOut[0] = FRunning; }
			
			//kill the process?
			if(FKillIn.IsChanged && FKillIn[0] && FRunning && !FCompleted)
			{
				try
				{
					FProcess.Kill();	
				}
				catch (Exception e)
				{
					FErrorOut.Add("Error on kill: " + e.Message);
				}
			}
		}
		 
		internal void OnDataReceived(object sender, DataReceivedEventArgs e)
		{
			//FLogger.Log(LogType.Debug, "DataReceived.");
			if (e.Data != null)
		    {
		        string ln = (e.Data) + Environment.NewLine;
		    	//FOutputQueue.Enqueue(ln);
		    	FResultOut.Add(ln);
		    
			}
		}
		
		internal void OnErrorReceived(object sender, DataReceivedEventArgs e)
		{
			//FLogger.Log(LogType.Debug, "ErrorReceived.{0}", e.Data);
		    if( e.Data != null ) 
			{
				string ln = (e.Data) + Environment.NewLine;
				FErrorOut.Add(ln);
			}
		}
		
		internal void OnExited(object sender, EventArgs e)
		{
			//FLogger.Log(LogType.Debug, "Done.");
			var p = sender as Process;
			if(p != null) lock(FLock) 
			{
				FExitCodeOut[0] = FProcess.ExitCode;
				FProcIDOut[0] = string.Empty;
				
				FRunning = false;
				FCompleted = true;
			}
		}
	}
}
