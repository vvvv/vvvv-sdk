#region licence/info

//////project name
//Trilerp

//////description
//3 dimensional linear interpolation

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
	public class Trilerp: IPlugin, IDisposable
    {	          	
    	#region field declaration
    	
    	//the host (mandatory)
    	private IPluginHost FHost; 
    	// Track whether Dispose has been called.
   		private bool FDisposed = false;

    	//input pin declaration
    	private IValueIn FPositionInput;
    	private IValueIn FV010Input;
    	private IValueIn FV110Input;
    	private IValueIn FV100Input;
    	private IValueIn FV000Input;
    	private IValueIn FV011Input;
    	private IValueIn FV111Input;
    	private IValueIn FV101Input;
    	private IValueIn FV001Input;
    	private IValueIn FVectorSizeInput;
    	
    	//output pin declaration
    	private IValueOut FPositionOutput;
    	
    	#endregion field declaration
       
    	#region constructor/destructor
    	
        public Trilerp()
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
        ~Trilerp()
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
					FPluginInfo.Name = "Trilerp";
					//the nodes category: try to use an existing one
					FPluginInfo.Category = "3d";
					//the nodes version: optional. leave blank if not
					//needed to distinguish two nodes of the same name and category
					FPluginInfo.Version = "";
					
					//the nodes author: your sign
					FPluginInfo.Author = "tonfilm";
					//describe the nodes function
					FPluginInfo.Help = "3D linear interpolation";
					//specify a comma separated list of tags that describe the node
					FPluginInfo.Tags = "interpolation, InputMorph";
					
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
	    	FHost.CreateValueInput("Input ", 3, null, TSliceMode.Dynamic, TPinVisibility.True, out FPositionInput);
	    	FPositionInput.SetSubType3D(double.MinValue, double.MaxValue, 0.01, 0.5, 0.5, 0.5, false, false, false);
	    	
	    	FHost.CreateValueInput("Front Upper Left ", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FV010Input);
	    	FV010Input.SetSubType(double.MinValue, double.MaxValue, 0.01, 0, false, false, false);
	    	
	    	FHost.CreateValueInput("Front Upper Right ", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FV110Input);
	    	FV110Input.SetSubType(double.MinValue, double.MaxValue, 0.01, 0, false, false, false);
	    	
	    	FHost.CreateValueInput("Front Lower Right ", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FV100Input);
	    	FV100Input.SetSubType(double.MinValue, double.MaxValue, 0.01, 1, false, false, false);
	    	
	    	FHost.CreateValueInput("Front Lower Left ", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FV000Input);
	    	FV000Input.SetSubType(double.MinValue, double.MaxValue, 0.01, 0, false, false, false);
	    	
	    	FHost.CreateValueInput("Back Upper Left ", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FV011Input);
	    	FV011Input.SetSubType(double.MinValue, double.MaxValue, 0.01, 0, false, false, false);
	    	
	    	FHost.CreateValueInput("Back Upper Right ", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FV111Input);
	    	FV111Input.SetSubType(double.MinValue, double.MaxValue, 0.01, 1, false, false, false);
	    	
	    	FHost.CreateValueInput("Back Lower Right ", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FV101Input);
	    	FV101Input.SetSubType(double.MinValue, double.MaxValue, 0.01, 1, false, false, false);
	    	
	    	FHost.CreateValueInput("Back Lower Left ", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FV001Input);
	    	FV001Input.SetSubType(double.MinValue, double.MaxValue, 0.01, 1, false, false, false);
	    	
	    	FHost.CreateValueInput("Vector Size", 1, null, TSliceMode.Single, TPinVisibility.True, out FVectorSizeInput);
	    	FVectorSizeInput.SetSubType(double.MinValue, double.MaxValue, 1, 1, false, false, true);
	    	
	    	//create outputs	    	
	    	FHost.CreateValueOutput("Output", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FPositionOutput);
	    	FPositionOutput.SetSubType(double.MinValue, double.MaxValue, 0.01, 0.5, false, false, false);
	    	
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
        	if (FPositionInput.PinIsChanged || FVectorSizeInput.PinIsChanged ||
        	    FV010Input.PinIsChanged || FV110Input.PinIsChanged || FV100Input.PinIsChanged || FV000Input.PinIsChanged || 
        	    FV011Input.PinIsChanged || FV111Input.PinIsChanged || FV101Input.PinIsChanged || FV001Input.PinIsChanged)
        	{	
	        	
	        	//get vector size
        	    double vs;
        	    FVectorSizeInput.GetValue(0, out vs);
        	    int vectorSize = (int) vs;
	        	
        	    SpreadMax = Math.Max(SpreadMax, FPositionInput.SliceCount * vectorSize);
        		
	        	//first set slicecounts for all outputs
	        	//the incoming int SpreadMax is the maximum slicecount of all input pins, which is a good default
	        	FPositionOutput.SliceCount = SpreadMax;
	        	
	        	
	        	//the variables to fill with the input data
	        	Vector3D vectorSlice;
	        	double V010Slice;
	        	double V110Slice;
	        	double V100Slice;
	        	double V000Slice;
	        	double V011Slice;
	        	double V111Slice;
	        	double V101Slice;
	        	double V001Slice;
	        	
        		//loop for all slices
        		for (int i=0; i < SpreadMax; i++)
        		{		
        			//read data from inputs
        			FPositionInput.GetValue3D(i/vectorSize, out vectorSlice.x, out vectorSlice.y, out vectorSlice.z);
        			FV010Input.GetValue(i, out V010Slice);
        			FV110Input.GetValue(i, out V110Slice);
        			FV100Input.GetValue(i, out V100Slice);
        			FV000Input.GetValue(i, out V000Slice);
        			FV011Input.GetValue(i, out V011Slice);
        			FV111Input.GetValue(i, out V111Slice);
        			FV101Input.GetValue(i, out V101Slice);
        			FV001Input.GetValue(i, out V001Slice);

        			//function per slice
        			V000Slice = VMath.Trilerp(vectorSlice, V010Slice, V110Slice, V100Slice, V000Slice,
        			                                       V011Slice, V111Slice, V101Slice, V001Slice);
        			
        			//write data to outputs
        			FPositionOutput.SetValue(i, V000Slice);
        		}
        	}      	
        }
        private double CalcSpreadMax()
        {
        	
        	//get vector size
        	double vectorSize;
        	FVectorSizeInput.GetValue(0, out vectorSize);
        	
        	int max = Math.Max(FPositionInput.SliceCount, FV000Input.SliceCount);
        	max = Math.Max(max, FV000Input.SliceCount);
        	max = Math.Max(max, FV001Input.SliceCount);
        	max = Math.Max(max, FV010Input.SliceCount);
        	max = Math.Max(max, FV011Input.SliceCount);
        	max = Math.Max(max, FV100Input.SliceCount);
        	max = Math.Max(max, FV101Input.SliceCount);
        	max = Math.Max(max, FV110Input.SliceCount);
        	return Math.Max(max, FV111Input.SliceCount);
        	
        }
             
        #endregion mainloop  
	}
}
