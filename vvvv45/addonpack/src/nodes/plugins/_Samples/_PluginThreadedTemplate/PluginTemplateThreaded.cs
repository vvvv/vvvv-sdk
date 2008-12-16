#region licence/info

//////project name
//vvvv plugin template threaded

//////description
//basic vvvv node plugin template with threading.
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
using System.Threading;
using VVVV.PluginInterfaces.V1;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

//the vvvv node namespace
namespace VVVV.Nodes
{
	
	//class definition
	public class PluginTemplateThreaded: IPlugin, IDisposable
    {	          	
    	#region field declaration
    	
    	//the host (mandatory)
    	private IPluginHost FHost; 
    	
    	// Track whether Dispose has been called.
   		private bool FDisposed = false;

    	//input pin declaration
    	private IValueIn FMyVectorInput;
    	private IValueIn FMassInput;
    	private IValueIn FNrOfThreadsInput;
	
    	//output pin declaration
    	private IValueOut FMyVectorOutput;
    	
    	#endregion field declaration
       
    	#region constructor/destructor
    	
        public PluginTemplateThreaded()
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
        ~PluginTemplateThreaded()
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
        			FPluginInfo.Name = "Template";
        			//the nodes category: try to use an existing one
        			FPluginInfo.Category = "Template";
        			//the nodes version: optional. leave blank if not
        			//needed to distinguish two nodes of the same name and category
        			FPluginInfo.Version = "Threaded";
        			
        			//the nodes author: your sign
        			FPluginInfo.Author = "vvvv group";
        			//describe the nodes function
        			FPluginInfo.Help = "Offers a basic code layout to start from when writing a vvvv plugin with multiple threads";
        			//specify a comma separated list of tags that describe the node
        			FPluginInfo.Tags = "template, multithreading, sample";
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
        		}
        		return FPluginInfo;
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
	    	FHost.CreateValueInput("Input", 3, null, TSliceMode.Dynamic, TPinVisibility.True, out FMyVectorInput);
	    	FMyVectorInput.SetSubType(double.MinValue, double.MaxValue, 0.01, 0.5, false, false, false);
	    		
	    	FHost.CreateValueInput("Mass", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FMassInput);
	    	FMassInput.SetSubType(double.MinValue, double.MaxValue, 0.01, 1, false, false, false);	
	    	
	    	FHost.CreateValueInput("Number of Threads", 1, null, TSliceMode.Single, TPinVisibility.True, out FNrOfThreadsInput);
	    	FNrOfThreadsInput.SetSubType(1, double.MaxValue, 1, 4, false, false, true);
	    	
	    	//create outputs	    	
	    	FHost.CreateValueOutput("Output", 3, null, TSliceMode.Dynamic, TPinVisibility.True, out FMyVectorOutput);
	    	FMyVectorOutput.SetSubType(double.MinValue, double.MaxValue, 0.01, 0, false, false, false);
	    	
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
        	if ((FMyVectorInput.PinIsChanged || FNrOfThreadsInput.PinIsChanged) && SpreadMax > 1)
        	{	
	        	//first set slicecounts for all outputs
	        	//the incoming int SpreadMax is the maximum slicecount of all input pins, which is a good default
	        	FMyVectorOutput.SliceCount = SpreadMax;	
	        	
	        	//the variables to fill with the input data
	        	Vector3D currentVectorSlice;
	        	double mass;
	        	Vector3D[] vectorArray = new Vector3D[SpreadMax];
	        	double[] massArray = new double[SpreadMax];
	        	Vector3D[] resultArray = new Vector3D[SpreadMax];
	        	
        		//loop for all slices
        		for (int i=0; i<SpreadMax; i++)
        		{		
        			//read data from inputs
        			FMyVectorInput.GetValue3D(i, out currentVectorSlice.x, out currentVectorSlice.y, out currentVectorSlice.z);
        			FMassInput.GetValue(i, out mass);
        			vectorArray[i] = currentVectorSlice;
        			massArray[i] = mass;
        		}
        		
        		//get number of threads
        		double pinValue;
        		FNrOfThreadsInput.GetValue(0, out pinValue);
        		int threadCount = Math.Max((int) Math.Round(pinValue), 1);
        		
        		//calc how many slices one thread should process
        		int divCount = Math.Max(SpreadMax / threadCount, 1);
        		
        		//how many will be left (if threadCount is not a divisor of SpreadMax)
        		int divRemainder = Math.Max(SpreadMax - divCount * threadCount, 0);
        		
        		//array of the threads
        		Thread[] threadArray = new Thread[threadCount];
        		
        		//create all threads to calc the function
        		//pack up the parameter object and start them
        		for (int i=0; i<threadCount; i++) 
        		{  
        			//pass the function as entry point to the thread
        			threadArray[i] = new Thread(GravitySum);
        			
        			//if not last slice
        			if (i < (threadCount - 1))
        			{
        				//start the thread for the defined range
        				//we pass the following data as an object array (which is an object): 
        				//(input vectors, the array for the result, start index, length of range)
        				threadArray[i].Start(new object[] {vectorArray, massArray, resultArray, i*divCount, divCount});	
        			}
        			else
        			{
        				//start for the last slice range
        				threadArray[i].Start(new object[] {vectorArray, massArray, resultArray, i*divCount, divCount+divRemainder});
        			}
        			
        			
        		}
        		
        		//wait for all threads, important to stay in sync with the mainloop
        		for (int i=0; i<threadCount; i++) 
        		{  
        			threadArray[i].Join();
        		}
        		
        		for (int i=0; i<SpreadMax; i++)
        		{	
        			//write data to outputs
        			currentVectorSlice = resultArray[i];
        			FMyVectorOutput.SetValue3D(i, currentVectorSlice.x, currentVectorSlice.y, currentVectorSlice.z);
        		
        		}
        	}      	
        }
        
        //the function that we pass to the threads
        private void GravitySum(object input)
        {
        	//split up the parameter object we defined as parameter:
        	//new object[] {vectorArray, resultArray, start, length}
        	
        	//first we cast it to an object array
        	object[] inArray = (object[]) input;
        	
        	//then we get the objects in the array and cast them back to our parameters
        	Vector3D[] vectorArray = (Vector3D[]) inArray[0];
        	double[] massArray = (double[]) inArray[1];
        	Vector3D[] resultArray = (Vector3D[]) inArray[2];
        	
        	int size = vectorArray.Length;
        	
        	int start = Math.Min((int) inArray[3], size-1);
        	
        	//calc the end = start + size
        	int end = Math.Min(start + (int) inArray[4], size);
        	
        	for (int i = start; i<end; i++) 
        	{
        		Vector3D sum = new Vector3D(0);
        		
        		//get current slice
        		Vector3D currentSlice = vectorArray[i];
        		
        		//do a large calculation
        		for (int j = 0; j<size; j++) 
        		{
        			//if not itself
        			if (i != j)
        			{
        				//calc gravity
        				Vector3D diff = vectorArray[j] - currentSlice;
        				double length = !diff;

        				sum += (diff / Math.Max(length * length * length, 0.000001)) * massArray[i] * massArray[j];

        			}
        		}
        		
        		//normalize sum vector and write to output array 
        		resultArray[i] = sum / Math.Max(size-1, 1);
        		
        	}
        	
        }
             
        #endregion mainloop  
	}
}
