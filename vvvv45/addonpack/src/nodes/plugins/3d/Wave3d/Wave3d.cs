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
	public class Wave3d: IPlugin, IDisposable
    {	          	
    	#region field declaration
    	
    	//the host (mandatory)
    	private IPluginHost FHost; 
    	
    	// Track whether Dispose has been called.
   		private bool FDisposed = false;

    	//input pin declaration
    	private IValueIn FExternalPressureInput;
    	private IValueIn FAttackInput;
    	private IValueIn FDecayInput;
    	private IValueIn FResetInput;
    	private IValueIn FSizeInput;
    	private IValueIn FRenderInput;
    	private IValueIn FNrOfThreadsInput;
	
    	//output pin declaration
    	private IValueOut FMyVectorOutput;
    	
    	private float[] FCurrentState;
    	private float[] FLastState;
    	private float[] FBeforeLastState;
    	private double FAttack;
    	private double FDecay;
    	private int FSize;
    	private int FSizeX;
    	private int FSizeY;
    	private int FSizeZ;
    	private int FThreadCount;
    	
    	#endregion field declaration
       
    	#region constructor/destructor
    	
        public Wave3d()
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
        ~Wave3d()
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
	        	Info.Name = "Wave";							//use CamelCaps and no spaces
	        	Info.Category = "3d";			//try to use an existing one
	        	Info.Version = "";						//versions are optional. leave blank if not needed
	        	Info.Help = "3d wave simulation";
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
	    	FHost.CreateValueInput("External Pressure", 4, null, TSliceMode.Dynamic, TPinVisibility.True, out FExternalPressureInput);
	    	FExternalPressureInput.SetSubType(double.MinValue, double.MaxValue, 0.01, 0, false, false, false);
	    	
	    	FHost.CreateValueInput("Attack", 1, null, TSliceMode.Single, TPinVisibility.True, out FAttackInput);
	    	FAttackInput.SetSubType(double.MinValue, double.MaxValue, 0.01, 0.5, false, false, false);
	    	
	    	FHost.CreateValueInput("Decay", 1, null, TSliceMode.Single, TPinVisibility.True, out FDecayInput);
	    	FDecayInput.SetSubType(double.MinValue, double.MaxValue, 0.001, 0.01, false, false, false);
	    	
	    	FHost.CreateValueInput("Reset", 1, null, TSliceMode.Single, TPinVisibility.True, out FResetInput);
	    	FResetInput.SetSubType(0, 1, 1, 0, true, false, false);
	    	
	    	FHost.CreateValueInput("Size", 3, null, TSliceMode.Dynamic, TPinVisibility.True, out FSizeInput);
	    	FSizeInput.SetSubType(double.MinValue, double.MaxValue, 1, 10, false, false, true);	
	    	
	    	FHost.CreateValueInput("Render", 1, null, TSliceMode.Single, TPinVisibility.True, out FRenderInput);
	    	FRenderInput.SetSubType(0, 1, 1, 1, false, true, true);	
	    	
	    	FHost.CreateValueInput("Number of Threads", 1, null, TSliceMode.Single, TPinVisibility.True, out FNrOfThreadsInput);
	    	FNrOfThreadsInput.SetSubType(1, double.MaxValue, 1, 4, false, false, true);
	    	
	    	//create outputs	    	
	    	FHost.CreateValueOutput("Output", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FMyVectorOutput);
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
        	
        	//if size changed
        	if (FSizeInput.PinIsChanged || FResetInput.PinIsChanged)
        	{	
	        	
	        	//get the size
	        	double pinValue;
	        	
	        	FSizeInput.GetValue(0, out pinValue);
	        	FSizeX = Math.Max((int) Math.Round(pinValue), 1);
	        	
	        	FSizeInput.GetValue(1, out pinValue);
	        	FSizeY = Math.Max((int) Math.Round(pinValue), 1);
	        	
	        	FSizeInput.GetValue(2, out pinValue);
	        	FSizeZ = Math.Max((int) Math.Round(pinValue), 1);
	        	
	        	FSize = FSizeX * FSizeY * FSizeZ;
	        	
	        	SpreadMax = FSize;
	        	FMyVectorOutput.SliceCount = SpreadMax;
	        	
	        	//get mem for the arrays
	        	FCurrentState = new float[SpreadMax];
	        	FLastState = new float[SpreadMax];
	        	FBeforeLastState = new float[SpreadMax];
	        	
	        	//init the arrays
	        	for (int i=0; i<SpreadMax; i++) 
	        	{
	        		FCurrentState[i] = 0;
	        		FLastState[i] = 0;
	        		FBeforeLastState[i] = 0;
	        		
	        	}
        	}

        	//shall we render?
        	double val;
        	FRenderInput.GetValue(0, out val);
        	if (val < 0.5) return;
        	        	
        	//get other parameters
        	if (FNrOfThreadsInput.PinIsChanged || FAttackInput.PinIsChanged || FDecayInput.PinIsChanged)
        	{
        		//get number of threads
        		double pinValue;
        		FNrOfThreadsInput.GetValue(0, out pinValue);
        		FThreadCount = Math.Max((int) Math.Round(pinValue), 1);
        		
        		FAttackInput.GetValue(0, out pinValue);
        		FAttack = pinValue;
        		
        		FDecayInput.GetValue(0, out pinValue);
        		FDecay = 1-pinValue;
        		
        	}
        	
        	//the variables to fill with the input data
        	Vector4D externalPressure;
        	
        	//read data from inputs
        	FExternalPressureInput.GetValue4D(0, out externalPressure.x, out externalPressure.y, out externalPressure.z, out externalPressure.w);
        	
        	//calc how many slices one thread should process
        	int divCount = Math.Max(FSize / FThreadCount, 1);
        	
        	//how many will be left (if threadCount is not a divisor of SpreadMax)
        	int divRemainder = Math.Max(FSize - divCount * FThreadCount, 0);
        	
        	//array of the threads
        	Thread[] threadArray = new Thread[FThreadCount];
        	
        	//create all threads to calc the function
        	//pack up the parameter object and start them
        	for (int i=0; i<FThreadCount; i++)
        	{
        		//pass the function as entry point to the thread
        		threadArray[i] = new Thread(CalcPressure);
        		
        		//if not last slice
        		if (i < (FThreadCount - 1))
        		{
        			//start the thread for the defined range
        			//we pass the following data as an object array (which is an object):
        			//(input vectors, the array for the result, start index, length of range)
        			threadArray[i].Start(new object[] {externalPressure, i*divCount, divCount});
        		}
        		else
        		{
        			//start for the last slice range
        			threadArray[i].Start(new object[] {externalPressure, i*divCount, divCount+divRemainder});
        		}
        		
        	}
        	
        	//wait for all threads, important to stay in sync with the mainloop
        	for (int i=0; i<FThreadCount; i++)
        	{
        		threadArray[i].Join();
        	}
        	
        	for (int i=0; i<FSize; i++)
        	{
        		//write data to outputs
        		FMyVectorOutput.SetValue(i, FCurrentState[i]);
        		
        	}
        	
        	//swap data
        	float[] temp1, temp2;
        	temp1 = FLastState;
        	temp2 = FBeforeLastState;
        	FLastState = FCurrentState;
        	FBeforeLastState = temp1;
        	FCurrentState = temp2;
  
        }
        
        //the function that we pass to the threads
        private void CalcPressure(object input)
        {
        	//split up the parameter object we defined as parameter:
        	//new object[] {pressure, start, length}
        	
        	//first we cast it to an object array
        	object[] inArray = (object[]) input;
        	
        	//then we get the objects in the array and cast them back to our parameters
        	Vector4D extPressure = (Vector4D) inArray[0];
        	
        	int start = Math.Min((int) inArray[1], FSize-1);
        	
        	//calc the end = start + length
        	int end = Math.Min(start + (int) inArray[2], FSize);
        	
        	for (int slice = start; slice<end; slice++) 
        	{

        		//calc the 3d indices
        		int i = slice % FSizeX;
        		int j = (slice / FSizeX) % FSizeY;
        		int k = slice / (FSizeX*FSizeY);
        		
        		//get earlier states
        		float p1 = FLastState[slice];
        		float p2 = FBeforeLastState[slice];
        		
        		//get neighbours
        		float n1 = Get3D(FLastState, i+1, j, k);
        		float n2 = Get3D(FLastState, i-1, j, k);
        		float n3 = Get3D(FLastState, i, j+1, k);
        		float n4 = Get3D(FLastState, i, j-1, k);
        		float n5 = Get3D(FLastState, i, j, k+1);
        		float n6 = Get3D(FLastState, i, j, k-1);
        		
        		//sum up the neighbours
        		float p1n = (n1 + n2 + n3 + n4 + n5 + n6) * 0.1666f;
        		
        		//wave distribution formula
        		FCurrentState[slice] = (p1 + (float)FDecay * (p1 - p2)) + (float)FAttack * (p1n - p1);
        		
        		//add external pressure
        		Vector3D pos = (new Vector3D(i, j, k) / new Vector3D(FSizeX*0.5, FSizeY*0.5, FSizeZ*0.5)) - 1;
        		if(VMath.Dist(extPressure.xyz, pos) < 0.2)
        		{
        			FCurrentState[slice] += (float)extPressure.w;
        		}
        		
        	}
        	
        }
        
        //get in 3d array
        private float Get3D(float[] state, int i, int j, int k)
        {
			i = mirror(i, FSizeX);
        	j = mirror(j, FSizeY);
        	k = mirror(k, FSizeZ);
        	return state[ i + FSizeX * j + FSizeX * FSizeY * k ];
        }
        
        //mirror the index inside a range
        private int mirror(int n, int d)
        {
        	int dm = d-1;
        	if (n > dm) n = dm + dm - n;
        	if (n < 0) n = -n;
        	return VMath.Clamp(n, 0, dm);
        }
             
        #endregion mainloop  
	}
}
