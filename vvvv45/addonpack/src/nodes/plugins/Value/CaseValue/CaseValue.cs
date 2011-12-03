#region licence/info

//////project name
//CaseValue

//////description
//Routes different inputs depending on a string

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
using System.Collections;
using VVVV.PluginInterfaces.V1;

//the vvvv node namespace
namespace VVVV.Nodes
{
	
	//class definition
	public class CaseValue: IPlugin, IDisposable
    {	          	
    	#region field declaration
    	
    	//the host (mandatory)
    	private IPluginHost FHost; 
    	// Track whether Dispose has been called.
   		private bool FDisposed = false;

   		//config pin declaration
   		private IStringConfig FCases;
   		
    	//input pin declaration
    	private IStringIn FSwitch;
    	private IValueIn FIn0;
    	private IValueIn FIn1;
    	
    	//output pin declaration
    	private IValueOut FOutput;
    	
    	//pin list
    	private IValueIn[] FPinArr;
    	private string[] FCaseArr;
    	private int[] FHit;
    	private bool FEvaluate;
    	
    	#endregion field declaration
       
    	#region constructor/destructor
    	
        public CaseValue()
        {
        	FEvaluate = false;
        	FHit = new int[1];
        	FPinArr = new IValueIn[2];
        	FCaseArr = new string[2];
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
        			//
        		}
        		// Release unmanaged resources. If disposing is false,
        		// only the following code is executed.
	        	
        		FHost.Log(TLogType.Debug, "Case (Value) is being deleted");
        		
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
        ~CaseValue()
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
					FPluginInfo.Name = "Case";
					//the nodes category: try to use an existing one
					FPluginInfo.Category = "Value";
					//the nodes version: optional. leave blank if not
					//needed to distinguish two nodes of the same name and category
					FPluginInfo.Version = "";
					
					//the nodes author: your sign
					FPluginInfo.Author = "woei";
					//describe the nodes function
					FPluginInfo.Help = "Routes different inputs depending on a string";
					//specify a comma separated list of tags that describe the node
					FPluginInfo.Tags = "switch";
					
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

	    	//create config
	    	FHost.CreateStringConfig("Cases", TSliceMode.Single, TPinVisibility.OnlyInspector, out FCases);
	    	FCases.SetSubType("one, two", false);
	    	
	    	//create inputs
	    	FHost.CreateStringInput("Switch", TSliceMode.Dynamic, TPinVisibility.True, out FSwitch);
	    	FSwitch.SetSubType("one", false);
	    		
	    	FHost.CreateValueInput("one", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FIn0);
	    	FIn0.SetSubType(double.MinValue, double.MaxValue, 0.01, 0.0, false, false, false);
	    	
	    	FHost.CreateValueInput("two", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FIn1);
	    	FIn1.SetSubType(double.MinValue, double.MaxValue, 0.01, 0.0, false, false, false);
	    	
	    	//create outputs	    	
	    	FHost.CreateValueOutput("Value Output", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FOutput);
	    	FOutput.SetSubType(double.MinValue, double.MaxValue, 0.01, 0, false, false, false);
	    	
	    	FHit[0] = 0;
	    	FPinArr[0] = FIn0;
	    	FCaseArr[0]= "one";
	    	FPinArr[1] = FIn1;
	    	FCaseArr[1]= "two";
        }

        #endregion pin creation
        
        #region mainloop
        
        public void Configurate(IPluginConfig Input)
        {
        	try
        	{
        	if (Input == FCases)
        	{
        		string caseString;
        		FCases.GetString(0, out caseString);
        		string[] inArr = caseString.Split(new char[]{','}, StringSplitOptions.RemoveEmptyEntries);
        		
        		int formerPinCount = FPinArr.Length;
        		if (inArr.Length!=formerPinCount)
        			FEvaluate = true;
        			
        		if (inArr.Length<formerPinCount)
        			for (int d=formerPinCount-1; d>=inArr.Length; d--)
        				FHost.DeletePin(FPinArr[d]);
        		
        		Array.Resize(ref FPinArr, inArr.Length);
        		Array.Resize(ref FCaseArr, inArr.Length);
        		
        		for(int i=0; i<inArr.Length; i++)
        		{
        			string curName = inArr[i].Trim();
        			if (FCaseArr[i]!=curName)
        			{
        				if (i<formerPinCount)
        					FHost.DeletePin(FPinArr[i]);
        				IValueIn newPin;
        				FHost.CreateValueInput(curName, 1, null, TSliceMode.Dynamic, TPinVisibility.True, out newPin);
        				newPin.SetSubType(double.MinValue, double.MaxValue, 0.01, 0.0, false, false, false);
        				FPinArr[i]=newPin;
        				FCaseArr[i]=curName;
        				FEvaluate = true;
        			}
        		}
        	}
        	}
        	catch (Exception e)
        	{
        		FHost.Log(TLogType.Debug, e.Message);
        		FHost.Log(TLogType.Debug, e.StackTrace);
        	}
        }
        
        //here we go, thats the method called by vvvv each frame
        //all data handling should be in here
        public void Evaluate(int SpreadMax)
        {     	
        	if (FSwitch.PinIsChanged || FEvaluate)
        	{
        		Array.Resize(ref FHit, SpreadMax);
	        	FOutput.SliceCount = SpreadMax;
	        	
	        	string curCase;
        		for (int i=0; i<SpreadMax; i++)
        		{		
        			FSwitch.GetString(i, out curCase);

        			int def = -1;
        			int hit = -1;
        			for(int s = 0; s<FCaseArr.Length; s++)
        			{
        				if (curCase == FCaseArr[s])
        					hit = s;
        				if (FCaseArr[s] == "default")
        					def = s;
        			}
        			
        			if (hit<0)
        				hit=def;
        			if (hit>=0 && hit!=FHit[i])
        			{
        				FHit[i] = hit;
        				FEvaluate = true;
        			}
        		}
        	}
        	
        	if (!FEvaluate)
        	{
        		foreach (int id in FHit)
        		{
        			if (FPinArr[id].PinIsChanged)
        			{
        				FEvaluate=true;
        				break;
        			}
        		}
        	}
        	
        	if (FEvaluate)
        	{
        		unsafe
        		{
        			double* outVals;
        			FOutput.GetValuePointer(out outVals);
        			for (int i=0; i<SpreadMax; i++)
        			{
        				double* inVals;
        				int inCount;
        				FPinArr[FHit[i]].GetValuePointer(out inCount, out inVals);
        				
        				outVals[i]=inVals[i%inCount];
        			}
        		}
        		FEvaluate=false;
        	}
        }
             
        #endregion mainloop  
	}
}
