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
	public class BilerpTransform: IPlugin, IDisposable
    {	          	
    	#region field declaration
    	
    	//the host (mandatory)
    	private IPluginHost FHost; 
    	// Track whether Dispose has been called.
   		private bool FDisposed = false;

    	//input pin declaration
    	private ITransformIn FTransformInput;
    	private IValueIn FP1Input;
    	private IValueIn FP2Input;
    	private IValueIn FP3Input;
    	private IValueIn FP4Input;
    	
    	//output pin declaration
    	private ITransformOut FTransformOutput;
    	
    	#endregion field declaration
       
    	#region constructor/destructor
    	
        public BilerpTransform()
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
        ~BilerpTransform()
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
					FPluginInfo.Category = "Transform";
					//the nodes version: optional. leave blank if not
					//needed to distinguish two nodes of the same name and category
					FPluginInfo.Version = "";
					
					//the nodes author: your sign
					FPluginInfo.Author = "tonfilm";
					//describe the nodes function
					FPluginInfo.Help = "builds a 4x4 matrix to do a 2D linear interpolattion between 4d-vectors with interpolation values in the 4d-form (x, y, x*y, 1)";
					//specify a comma separated list of tags that describe the node
					FPluginInfo.Tags = "interpolation, matrix, InputMorph";
					
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
	    	FHost.CreateTransformInput("Transform In", TSliceMode.Dynamic, TPinVisibility.True, out FTransformInput);
	    	
	    	FHost.CreateValueInput("Upper Left Point ", 4, null, TSliceMode.Dynamic, TPinVisibility.True, out FP1Input);
	    	FP1Input.SetSubType4D(double.MinValue, double.MaxValue, 0.01, 0, 1, 0, 1, false, false, false);
	    	
	    	FHost.CreateValueInput("Upper Right Point ", 4, null, TSliceMode.Dynamic, TPinVisibility.True, out FP2Input);
	    	FP2Input.SetSubType4D(double.MinValue, double.MaxValue, 0.01, 1, 1, 0, 1, false, false, false);
	    	
	    	FHost.CreateValueInput("Lower Right Point ", 4, null, TSliceMode.Dynamic, TPinVisibility.True, out FP3Input);
	    	FP3Input.SetSubType4D(double.MinValue, double.MaxValue, 0.01, 1, 0, 0, 1, false, false, false);
	    	
	    	FHost.CreateValueInput("Lower Left Point ", 4, null, TSliceMode.Dynamic, TPinVisibility.True, out FP4Input);
	    	FP4Input.SetSubType4D(double.MinValue, double.MaxValue, 0.01, 0, 1, 0, 1, false, false, false);
	    	
	    	
	    	//create outputs	    	
	    	FHost.CreateTransformOutput("Transform Out", TSliceMode.Dynamic, TPinVisibility.True, out FTransformOutput);
	    	
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
        	if (FTransformInput.PinIsChanged || FP1Input.PinIsChanged || 
        	    FP2Input.PinIsChanged || FP3Input.PinIsChanged || FP4Input.PinIsChanged)
        	{	
	        	//first set slicecounts for all outputs
	        	//the incoming int SpreadMax is the maximum slicecount of all input pins, which is a good default
	        	FTransformOutput.SliceCount = SpreadMax;	
	        	
	        	//the variables to fill with the input data
	        	Matrix4x4 matrixSlice;
	        	Vector4D p1Slice;
	        	Vector4D p2Slice;
	        	Vector4D p3Slice;
	        	Vector4D p4Slice;
	        	
        		//loop for all slices
        		for (int i=0; i<SpreadMax; i++)
        		{		
        			//read data from inputs
        			FTransformInput.GetMatrix(i, out matrixSlice);
        			FP1Input.GetValue4D(i, out p1Slice.x, out p1Slice.y, out p1Slice.z, out p1Slice.w);
        			FP2Input.GetValue4D(i, out p2Slice.x, out p2Slice.y, out p2Slice.z, out p2Slice.w);
        			FP3Input.GetValue4D(i, out p3Slice.x, out p3Slice.y, out p3Slice.z, out p3Slice.w);
        			FP4Input.GetValue4D(i, out p4Slice.x, out p4Slice.y, out p4Slice.z, out p4Slice.w);

        			//function per slice
        			matrixSlice = VMath.BilerpMatrix(p1Slice, p2Slice, p4Slice, p3Slice) * matrixSlice;
        			
        			//write data to outputs
        			FTransformOutput.SetMatrix(i, matrixSlice);
        		}
        	}      	
        }
             
        #endregion mainloop  
	}
}
