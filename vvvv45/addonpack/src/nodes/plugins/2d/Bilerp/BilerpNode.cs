#region licence/info

//////project name
//Bilerp

//////description
//2 dimensional linear interpolation

//////licence
//GNU Lesser General Public License (LGPL)
//english: http://www.gnu.org/licenses/lgpl.html
//german: http://www.gnu.de/lgpl-ger.html

//////language/ide
//C# sharpdevelop 

//////dependencies
//VVVV.PluginInterfaces.V1;
//VVVV.Utils.VMath;

//////initial author
//tonfilm

#endregion licence/info

//use what you need
using System;
using System.Drawing;
using VVVV.PluginInterfaces.V1;
using VVVV.Utils.VMath;

//the vvvv node namespace
namespace VVVV.Nodes
{
	
	//class definition
	public class Bilerp: IPlugin, IDisposable
    {	          	
    	#region field declaration
    	
    	//the host (mandatory)
    	private IPluginHost FHost; 
    	// Track whether Dispose has been called.
   		private bool FDisposed = false;

    	//input pin declaration
    	private IValueIn FPositionInput;
    	private IValueIn FP1Input;
    	private IValueIn FP2Input;
    	private IValueIn FP3Input;
    	private IValueIn FP4Input;
    	private IValueIn FVectorSizeInput;
    	
    	//output pin declaration
    	private IValueOut FValueOutput;
    	
    	#endregion field declaration
       
    	#region constructor/destructor
    	
        public Bilerp()
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
	        	
        		FHost.Log(TLogType.Debug, "PluginTemplate is being deleted");
        		
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
        ~Bilerp()
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
					//see: http://www.vvvv.org/tiki-index.php?page=vvvv+naming+conventions
					FPluginInfo = new PluginInfo();
					
					//the nodes main name: use CamelCaps and no spaces
					FPluginInfo.Name = "Bilerp";
					//the nodes category: try to use an existing one
					FPluginInfo.Category = "2d";
					//the nodes version: optional. leave blank if not
					//needed to distinguish two nodes of the same name and category
					FPluginInfo.Version = "";
					
					//the nodes author: your sign
					FPluginInfo.Author = "tonfilm";
					//describe the nodes function
					FPluginInfo.Help = "2D linear interpolation";
					//specify a comma separated list of tags that describe the node
					FPluginInfo.Tags = "interpolation, inputmorph";
					
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
	    	FHost.CreateValueInput("Input ", 2, null, TSliceMode.Dynamic, TPinVisibility.True, out FPositionInput);
	    	FPositionInput.SetSubType2D(double.MinValue, double.MaxValue, 0.01, 0.5, 0.5, false, false, false);
	    	
	    	FHost.CreateValueInput("Upper Left Value ", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FP1Input);
	    	FP1Input.SetSubType(double.MinValue, double.MaxValue, 0.01, 0, false, false, false);
	    	
	    	FHost.CreateValueInput("Upper Right Value ", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FP2Input);
	    	FP2Input.SetSubType(double.MinValue, double.MaxValue, 0.01, 0, false, false, false);
	    	
	    	FHost.CreateValueInput("Lower Right Value ", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FP3Input);
	    	FP3Input.SetSubType(double.MinValue, double.MaxValue, 0.01, 1, false, false, false);
	    	
	    	FHost.CreateValueInput("Lower Left Value ", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FP4Input);
	    	FP4Input.SetSubType(double.MinValue, double.MaxValue, 0.01, 1, false, false, false);
	    	
	    	FHost.CreateValueInput("Vector Size", 1, null, TSliceMode.Single, TPinVisibility.True, out FVectorSizeInput);
	    	FVectorSizeInput.SetSubType(double.MinValue, double.MaxValue, 1, 1, false, false, true);
	    		
	    	//create outputs	    	
	    	FHost.CreateValueOutput("Output", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FValueOutput);
	    	FValueOutput.SetSubType(double.MinValue, double.MaxValue, 0.01, 0.5, false, false, false);
	    	
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
        	//if any of the inputs has changed
        	//recompute the outputs
        	if (FPositionInput.PinIsChanged || FVectorSizeInput.PinIsChanged || FP1Input.PinIsChanged || 
        	    FP2Input.PinIsChanged || FP3Input.PinIsChanged || FP4Input.PinIsChanged)
        	{	
	        	//get vector size
        	    double vs;
        	    FVectorSizeInput.GetValue(0, out vs);
        	    int vectorSize = (int) vs;
	        	
        	    SpreadMax = Math.Max(SpreadMax, FPositionInput.SliceCount * vectorSize);
        	    
        	    //first set slicecounts for all outputs
	        	//the incoming int SpreadMax is the maximum slicecount of all input pins, which is a good default
	        	FValueOutput.SliceCount = SpreadMax;
	        	
	        	//the variables to fill with the input data
	        	Vector2D vectorSlice;
	        	double p1Slice;
	        	double p2Slice;
	        	double p3Slice;
	        	double p4Slice;
	        	
        		//loop for all slices
        		for (int i=0; i<SpreadMax; i++)
        		{		
        			//read data from inputs
        			FPositionInput.GetValue2D(i/vectorSize, out vectorSlice.x, out vectorSlice.y);
        			FP1Input.GetValue(i, out p1Slice);
        			FP2Input.GetValue(i, out p2Slice);
        			FP3Input.GetValue(i, out p3Slice);
        			FP4Input.GetValue(i, out p4Slice);

        			//function per slice
        			p1Slice = VMath.Bilerp(vectorSlice, p1Slice, p2Slice, p3Slice, p4Slice);
        			
        			//write data to outputs
        			FValueOutput.SetValue(i, p1Slice);
        		}
        	}      	
        }
             
        #endregion mainloop  
	}
}


