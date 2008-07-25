#region licence/info

//////project name
//vvvv plugin template

//////description
//basic vvvv node plugin template.
//Copy this an rename it, to write your own plugin node.

//////licence
//GNU Lesser General Public License (LGPL)
//english: http://www.gnu.org/licenses/lgpl.html
//german: http://www.gnu.de/lgpl-ger.html

//////language/ide
//C# sharpdevelop 

//////dependencies
//VVVV.PluginInterfaces.V1;
//VVVV.Utils.VColor;
//VVVV.Utils.VMath;

//////initial author
//vvvv group

#endregion licence/info

//use what you need
using System;
using System.Drawing;
using VVVV.PluginInterfaces.V1;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

//the vvvv node namespace
namespace VVVV.Nodes
{
	
	//class definition
	public class PluginTemplate: IPlugin, IDisposable
    {	          	
    	#region field declaration
    	
    	//the host (mandatory)
    	private IPluginHost FHost; 
    	// Track whether Dispose has been called.
   		private bool FDisposed = false;

    	//input pin declaration
    	private IValueIn FMyValueInput;
    	private IStringIn FMyStringInput;
    	private IColorIn FMyColorInput;
    	private ITransformIn FMyTransformInput;
    	
    	//output pin declaration
    	private IValueOut FMyValueOutput;
    	private IStringOut FMyStringOutput;
    	private IColorOut FMyColorOutput;
    	private ITransformOut FMyTransformOutput;
    	
    	#endregion field declaration
       
    	#region constructor/destructor
    	
        public PluginTemplate()
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
        ~PluginTemplate()
        {
        	// Do not re-create Dispose clean-up code here.
        	// Calling Dispose(false) is optimal in terms of
        	// readability and maintainability.
        	Dispose(false);
        }
        #endregion constructor/destructor
        
        #region node name and infos
       
        //provide node infos 
        public static IPluginInfo PluginInfo
	    {
	        get 
	        {
	        	//fill out nodes info
	        	//see: http://www.vvvv.org/tiki-index.php?page=vvvv+naming+conventions
	        	IPluginInfo Info = new PluginInfo();
	        	Info.Name = "Template";							//use CamelCaps and no spaces
	        	Info.Category = "Template";						//try to use an existing one
	        	Info.Version = "Simple";						//versions are optional. leave blank if not needed
	        	Info.Help = "Offers a basic code layout to start from when writing a vvvv plugin";
	        	Info.Bugs = "";
	        	Info.Credits = "";								//give credits to thirdparty code used
	        	Info.Warnings = "";
	        	
	        	//leave below as is
	        	System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(true);
   				System.Diagnostics.StackFrame sf = st.GetFrame(0);
   				System.Reflection.MethodBase method = sf.GetMethod();
   				Info.Namespace = method.DeclaringType.Namespace;
   				Info.Class = method.DeclaringType.Name;
   				return Info;
   				//leave above as is
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
	    	FHost.CreateValueInput("Value Input", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FMyValueInput);
	    	FMyValueInput.SetSubType(double.MinValue, double.MaxValue, 0.01, 0.5, false, false, false);
	    	
	    	FHost.CreateStringInput("String Input", TSliceMode.Dynamic, TPinVisibility.True, out FMyStringInput);
	    	FMyStringInput.SetSubType("hello c#", false);	

	    	FHost.CreateColorInput("Color Input", TSliceMode.Dynamic, TPinVisibility.True, out FMyColorInput);
	    	FMyColorInput.SetSubType(VColor.Green, false);
	    	
	    	FHost.CreateTransformInput("Transform Input", TSliceMode.Dynamic, TPinVisibility.True, out FMyTransformInput);
	    	
	    	//create outputs	    	
	    	FHost.CreateValueOutput("Value Output", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FMyValueOutput);
	    	FMyValueOutput.SetSubType(double.MinValue, double.MaxValue, 0.01, 0, false, false, false);
	    	
	    	FHost.CreateStringOutput("String Output", TSliceMode.Dynamic, TPinVisibility.True, out FMyStringOutput);
	    	FMyStringOutput.SetSubType("", false);

	    	FHost.CreateColorOutput("Color Output", TSliceMode.Dynamic, TPinVisibility.True, out FMyColorOutput);
	    	FMyColorOutput.SetSubType(VColor.Green, false);
	    	
	    	FHost.CreateTransformOutput("Transform Output", TSliceMode.Dynamic, TPinVisibility.True, out FMyTransformOutput);
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
        	if (FMyValueInput.PinIsChanged || FMyStringInput.PinIsChanged || FMyColorInput.PinIsChanged)
        	{	
	        	//first set slicecounts for all outputs
	        	//the incoming int SpreadMax is the maximum slicecount of all input pins, which is a good default
	        	FMyValueOutput.SliceCount = SpreadMax;
	        	FMyStringOutput.SliceCount = SpreadMax;
	        	FMyColorOutput.SliceCount = SpreadMax;
	        	FMyTransformOutput.SliceCount = SpreadMax; 	
	        	
	        	//the variables to fill with the input data
	        	double currentValueSlice;
	        	string currentStringSlice;
	        	RGBAColor currentColorSlice;
	        	Matrix4x4 currentTransformSlice;
	        	
        		//loop for all slices
        		for (int i=0; i<SpreadMax; i++)
        		{		
        			//read data from inputs
        			FMyValueInput.GetValue(i, out currentValueSlice);
        			FMyStringInput.GetString(i, out currentStringSlice);
        			FMyColorInput.GetColor(i, out currentColorSlice);
        			FMyTransformInput.GetMatrix(i, out currentTransformSlice);

        			//your function per slice
        			currentValueSlice = currentValueSlice*2;
        			currentStringSlice = currentStringSlice.Replace("c#", "vvvv");
        			currentColorSlice = VColor.Complement(currentColorSlice);
        			currentTransformSlice = currentTransformSlice * VMath.Scale(2, 2, 2);
        			
        			//write data to outputs
        			FMyValueOutput.SetValue(i, currentValueSlice);
        			FMyStringOutput.SetString(i, currentStringSlice);
        			FMyColorOutput.SetColor(i, currentColorSlice);
        			FMyTransformOutput.SetMatrix(i, currentTransformSlice);
        		}
        	}      	
        }
             
        #endregion mainloop  
	}
}
