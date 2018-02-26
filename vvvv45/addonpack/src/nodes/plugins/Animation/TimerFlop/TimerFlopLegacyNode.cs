#region licence/info

//////project name
//TimerFlop (Animation)

//////description
//Switches to one if the input has been 1 for a certain time

//////licence
//GNU Lesser General Public License (LGPL)
//english: http://www.gnu.org/licenses/lgpl.html
//german: http://www.gnu.de/lgpl-ger.html

//////language/ide
//C# sharpdevelop 

//////dependencies
//VVVV.PluginInterfaces.V1;

//////initial author
//woei

#endregion licence/info

//use what you need
using System;
using System.Collections.Generic;
using VVVV.PluginInterfaces.V1;

//the vvvv node namespace
namespace VVVV.Nodes
{
	
	//class definition
	public class TimerFlopLegacyNode: IPlugin, IDisposable
    {	          	
    	#region field declaration
    	
    	//the host (mandatory)
    	private IPluginHost FHost; 
    	// Track whether Dispose has been called.
   		private bool FDisposed = false;

    	//input pin declaration
    	private IValueIn FSet;
    	private IValueIn FTime;
    	private IValueIn FReset;
    	
    	//output pin declaration
    	private IValueOut FOutput;
    	private IValueOut FRunning;
    	private IValueOut FActive;
    	
    	//further fields
    	private List<DateTime> FStart;
        
    	#endregion field declaration
       
    	#region constructor/destructor
    	
        public TimerFlopLegacyNode()
        {
			//the nodes constructor
			FStart = new List<DateTime>();
		}
        
        // Implementing IDisposable's Dispose method.
        // Do not make this method virtual.
        // A derived class should not be able to override this method.
        public void Dispose()
        {
        	Dispose(true);
        	// Take yourself off the Finalization queue
        	// to prevent finalization code for this object
        	// from executing a second time.
        	GC.SuppressFinalize(this);
        }
        
        // Dispose(bool disposing) executes in two distinct scenarios.
        // If disposing equals true, the method has been called directly
        // or indirectly by a user's code. Managed and unmanaged resources
        // can be disposed.
        // If disposing equals false, the method has been called by the
        // runtime from inside the finalizer and you should not reference
        // other objects. Only unmanaged resources can be disposed.
        protected virtual void Dispose(bool disposing)
        {
        	// Check to see if Dispose has already been called.
        	if(!FDisposed)
        	{
        		if(disposing)
        		{
        			FStart.Clear();
        		}
        		// Release unmanaged resources. If disposing is false,
        		// only the following code is executed.
	        	
        		FHost.Log(TLogType.Debug, "TimerFlop is being deleted");
        		
        		// Note that this is not thread safe.
        		// Another thread could start disposing the object
        		// after the managed resources are disposed,
        		// but before the disposed flag is set to true.
        		// If thread safety is necessary, it must be
        		// implemented by the client.
        	}
        	FDisposed = true;
        }

        // Use C# destructor syntax for finalization code.
        // This destructor will run only if the Dispose method
        // does not get called.
        // It gives your base class the opportunity to finalize.
        // Do not provide destructors in types derived from this class.
        ~TimerFlopLegacyNode()
        {
        	// Do not re-create Dispose clean-up code here.
        	// Calling Dispose(false) is optimal in terms of
        	// readability and maintainability.
        	Dispose(false);
        }
        #endregion constructor/destructor
        
        #region node name and infos
       
        //provide node infos 
        private static IPluginInfo FPluginInfo;
        public static IPluginInfo PluginInfo
	    {
	        get 
	        {
	        	if (FPluginInfo == null)
				{
					//fill out nodes info
					//see: http://www.vvvv.org/tiki-index.php?page=Conventions.NodeAndPinNaming
					FPluginInfo = new PluginInfo();
					
					//the nodes main name: use CamelCaps and no spaces
					FPluginInfo.Name = "TimerFlop";
					//the nodes category: try to use an existing one
					FPluginInfo.Category = "Animation";
					//the nodes version: optional. leave blank if not
					//needed to distinguish two nodes of the same name and category
					FPluginInfo.Version = "Legacy";
					
					//the nodes author: your sign
					FPluginInfo.Author = "woei";
					//describe the nodes function
					FPluginInfo.Help = "Switches to one if the input has been 1 for a certain time";
					//specify a comma separated list of tags that describe the node
					FPluginInfo.Tags = "";
					
					//give credits to thirdparty code used
					FPluginInfo.Credits = "";
					//any known problems?
					FPluginInfo.Bugs = "";
					//any known usage of the node that may cause troubles?
					FPluginInfo.Warnings = "";
					
					//leave below as is
					System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(true);
					System.Diagnostics.StackFrame sf = st.GetFrame(0);
					System.Reflection.MethodBase method = sf.GetMethod();
					FPluginInfo.Namespace = method.DeclaringType.Namespace;
					FPluginInfo.Class = method.DeclaringType.Name;
					//leave above as is
				}
				return FPluginInfo;
	        }
		}

        public bool AutoEvaluate
        {
        	//return true if this node needs to calculate every frame even if nobody asks for its output
        	get {return true;}
        }
        
        #endregion node name and infos
        
      	#region pin creation
        
        //this method is called by vvvv when the node is created
        public void SetPluginHost(IPluginHost Host)
	    {
        	//assign host
	    	FHost = Host;

	    	//create inputs
			FHost.CreateValueInput("Set", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FSet);
	    	FSet.SetSubType(0, 1, 1, 0, false, true, false);
	    	
	    	FHost.CreateValueInput("Time", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FTime);
	    	FTime.SetSubType(0, double.MaxValue, 0.01, 1.0, false, false, false);
	    	
	    	FHost.CreateValueInput("Reset", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FReset);
	    	FReset.SetSubType(0, 1, 1, 0, true, false, false);

	    	//create outputs	    	
	    	FHost.CreateValueOutput("Output", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FOutput);
	    	FOutput.SetSubType(0, 1, 1, 0, false, true, false);
	    	
	    	FHost.CreateValueOutput("Running", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FRunning);
	    	FRunning.SetSubType(0, 1, 0.01, 0, false, false, false);
	    	
	    	FHost.CreateValueOutput("Active", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FActive);
	    	FActive.SetSubType(0, 1, 1, 0, false, true, false);
        }

        #endregion pin creation
        
        #region mainloop
        
        public void Configurate(IPluginConfig Input)
        {
        	//nothing to configure in this plugin
        	//only used in conjunction with inputs of type cmpdConfigurate
        }
        
        //here we go, thats the method called by vvvv each frame
        //all data handling should be in here
        public void Evaluate(int SpreadMax)
        {          	
        	int diff=FStart.Count-SpreadMax;
        	if (diff>0)
        		FStart.RemoveRange(SpreadMax, diff);
        	
        	for (int i=0; i>diff; i--)
        	{
        		FStart.Add(DateTime.Now);
        	}
        	
        	FOutput.SliceCount = SpreadMax;
        	FRunning.SliceCount = SpreadMax;
        	FActive.SliceCount = SpreadMax;
        	
        	double curSet, curReset, curTime, curActive, curRunning, curOut;
        	for (int i=0; i<SpreadMax; i++)
        	{
        		curActive=0;
        		curRunning=0;
        		curOut = 0;

        		FReset.GetValue(i, out curReset);
        		FSet.GetValue(i, out curSet);
        		if (curSet<0.5 || curReset > 0.5)
        			FStart[i]=DateTime.Now;
        		else
        		{
        			curActive=1;
        			FTime.GetValue(i, out curTime);
        			TimeSpan span = DateTime.Now-FStart[i];
        			double elapsed = ((double)span.TotalMilliseconds/1000.0);
        			curRunning = Math.Min(elapsed/curTime,1.0);
        			if (elapsed>=curTime)
        				curOut=1;
        			
        		}
        		FOutput.SetValue(i, curOut);
        		FRunning.SetValue(i, curRunning);
        		FActive.SetValue(i, curActive);
        	}
        	
        }
             
        #endregion mainloop 
        
	}
	
	
}
