#region licence/info

//////project name
//Tangent (Spreads)

//////description
//calculates the weighted tangents of the incoming points

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
	public class TangentSpreads: IPlugin, IDisposable
    {	          	
    	#region field declaration
    	
    	//the host (mandatory)
    	private IPluginHost FHost; 
    	// Track whether Dispose has been called.
   		private bool FDisposed = false;

    	//input pin declaration
    	private IValueIn FIn;
    	private IValueIn FVectorSize;
    	private IValueIn FBinSize;
    	
    	//output pin declaration
    	private IValueOut FOut;
    	
    	#endregion field declaration
       
    	#region constructor/destructor
    	
        public TangentSpreads()
        {
			//the nodes constructor
			//nothing to declare for this node
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
        			// Dispose managed resources.
        		}
        		// Release unmanaged resources. If disposing is false,
        		// only the following code is executed.
	        	
        		FHost.Log(TLogType.Debug, "Tangent (Spreads) is being deleted");
        		
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
        ~TangentSpreads()
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
					FPluginInfo.Name = "Tangent";
					//the nodes category: try to use an existing one
					FPluginInfo.Category = "Spreads";
					//the nodes version: optional. leave blank if not
					//needed to distinguish two nodes of the same name and category
					FPluginInfo.Version = "";
					
					//the nodes author: your sign
					FPluginInfo.Author = "woei";
					//describe the nodes function
					FPluginInfo.Help = "Calculates the mean direction (to previous and next)";
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
        	get {return false;}
        }
        
        #endregion node name and infos
        
      	#region pin creation
        
        //this method is called by vvvv when the node is created
        public void SetPluginHost(IPluginHost Host)
	    {
        	//assign host
	    	FHost = Host;

	    	//create inputs
	    	FHost.CreateValueInput("Input", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FIn);
	    	FIn.SetSubType(double.MinValue, double.MaxValue, 0.01, 0.5, false, false, false);
	    	
	    	FHost.CreateValueInput("Vector Size", 1, null, TSliceMode.Single, TPinVisibility.Hidden, out FVectorSize);
	    	FVectorSize.SetSubType(1, double.MaxValue, 1, 1, false, false, true);
	    	
	    	FHost.CreateValueInput("Bin Size", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FBinSize);
	    	FBinSize.SetSubType(double.MinValue, double.MaxValue, 1, -1, false, false, true);
	    	
	    	
	    	//create outputs	    	
	    	FHost.CreateValueOutput("Output", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FOut);
	    	FOut.SetSubType(double.MinValue, double.MaxValue, 0.01, 0, false, false, false);
	    	
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
        public unsafe void Evaluate(int SpreadMax)
        {     	
        	//if any of the inputs has changed
        	//recompute the outputs
        	if (FIn.PinIsChanged || FVectorSize.PinIsChanged || FBinSize.PinIsChanged)
        	{	
	        	double* inVals;
	        	int inC;
	        	FIn.GetValuePointer(out inC, out inVals);
	        	
	        	double tmpVec;
	        	FVectorSize.GetValue(0, out tmpVec);
	        	int vecSize = Math.Max((int)Math.Round(tmpVec),1);
	        	int vecMax = (int)Math.Ceiling(inC/(decimal)vecSize);
	        	
	        	double firstBin;
	        	FBinSize.GetValue(0, out firstBin);
	        	int total = 0;
	        	List<int> binList = new List<int>();
	        	if (!(FBinSize.SliceCount==1 && firstBin== 0))
	        	{
	        		int binIncr=0;
	        		bool end = false;
	        		while (!end)
	        		{
	        			double tmpBin;
	        			FBinSize.GetValue(binIncr, out tmpBin);
	        			int curBin = (int)Math.Round(tmpBin);
	        			if (curBin<0)
	        				curBin = vecMax/Math.Abs(curBin);
	        			binList.Add(curBin);
	        			binIncr++;
	        			total+=curBin;
	        			if (binIncr%FBinSize.SliceCount==0 && total>=vecMax)
	        				end=true;
	        			
	        		}
	        	}
	        	
	        	FOut.SliceCount=vecMax*vecSize;
	        	double* outVals;
	        	FOut.GetValuePointer(out outVals);
	        	int outIncr=0;
	        	foreach (int b in binList)
	        	{
	        		for (int i=0; i<b*vecSize; i++)
	        		{
	        			int prev = i-vecSize;
	        			int next = i+vecSize;
	        			if (prev<0)
	        				outVals[i+outIncr]=(inVals[(outIncr+next)%inC]-inVals[(i+outIncr)%inC]);
	        			else if (next>=b*vecSize)
	        				outVals[i+outIncr]=(inVals[(i+outIncr)%inC]-inVals[(outIncr+prev)%inC]);
	        			else
	        			{
	        				outVals[i+outIncr]=(inVals[(outIncr+next)%inC]-inVals[(i+outIncr)%inC])/2;
	        				outVals[i+outIncr]+=(inVals[(i+outIncr)%inC]-inVals[(outIncr+prev)%inC])/2;
	        			}
	        		}
	        		outIncr+=b*vecSize;
	        	}
        	}      	
        }
             
        #endregion mainloop  
	}
}
