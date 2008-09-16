#region licence/info

//////project name
//Reverse (Spreads Advanced)

//////description
//Reverse slice order with advanced options

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
using System.Drawing;
using System.Collections.Generic;
using VVVV.PluginInterfaces.V1;

//the vvvv node namespace
namespace VVVV.Nodes
{
	
	//class definition
	public class ReverseSpreadsAdv: IPlugin, IDisposable
    {	          	
    	#region field declaration
    	
    	//the host (mandatory)
    	private IPluginHost FHost; 
    	// Track whether Dispose has been called.
   		private bool FDisposed = false;

    	//input pin declaration
    	private IValueIn FInput;
    	private IValueIn FVecSize;
    	private IValueIn FBinSize;
		private IValueIn FDo;
    	
    	//output pin declaration
    	private IValueOut FOutput;
    	
    	#endregion field declaration
       
    	#region constructor/destructor
    	
        public ReverseSpreadsAdv()
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
	        	
        		FHost.Log(TLogType.Debug, "Reverse (Spreads Advanced) is being deleted");
        		
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
        ~ReverseSpreadsAdv()
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
	        	Info.Name = "Reverse";							//use CamelCaps and no spaces
	        	Info.Category = "Spreads";						//try to use an existing one
	        	Info.Version = "Advanced";						//versions are optional. leave blank if not needed
	        	Info.Help = "Reverse slice order with advanced options";
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
	    	FHost.CreateValueInput("Input", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FInput);
	    	FInput.SetSubType(double.MinValue, double.MaxValue, 0.01, 0.5, false, false, false);
	    	
	    	FHost.CreateValueInput("Vector Size", 1, null, TSliceMode.Dynamic, TPinVisibility.Hidden, out FVecSize);
	    	FVecSize.SetSubType(1, double.MaxValue, 1, 1, false, false, true);
	    	
	    	FHost.CreateValueInput("Bin Size", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FBinSize);
	    	FBinSize.SetSubType(-1, double.MaxValue, 1, -1, false, false, true);
	    	
	    	FHost.CreateValueInput("Do reverse", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FDo);
	    	FDo.SetSubType(0, 1, 1, 1, false, true, false);
	    	
	    	//create outputs	    	
	    	FHost.CreateValueOutput("Output", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FOutput);
	    	FOutput.SetSubType(double.MinValue, double.MaxValue, 0.01, 0, false, false, false);
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
        	if (FInput.PinIsChanged ||
        	    FVecSize.PinIsChanged ||
        	    FBinSize.PinIsChanged ||
        	    FDo.PinIsChanged)
        	{			        	
	        	//the variables to fill with the input data	        	
	        	List<int> vecSize = new List<int>();
	        	List<int> binSize = new List<int>();
	        	int idInc = 0;
	        	
				//getting loop count
        		int maxLoop = Math.Max(FVecSize.SliceCount, FBinSize.SliceCount);       		
        		int slicecounter = 0;
        		do
        		{
        			double tmpVec;
	        		FVecSize.GetValue(idInc, out tmpVec);
	        		vecSize.Add((int)Math.Round(tmpVec));
	        		
	        		double tmpBin;
	        		FBinSize.GetValue(idInc, out tmpBin);
	        		binSize.Add((int)Math.Round(tmpBin));
        			
	        		int tmpAdd = vecSize[idInc]*binSize[idInc];
        			if (tmpAdd<0)
        			{
        				tmpAdd=FInput.SliceCount;
        			}
        			slicecounter+=tmpAdd;
        			idInc++;
        			FHost.Log(TLogType.Debug, slicecounter.ToString());
        		} while (!((slicecounter>=FInput.SliceCount)&&(idInc>=maxLoop)));
        		
        		//looping action
        		List<double> outList = new List<double>();
        		int increment = 0;
        		for (int i=0; i<idInc; i++)
        		{	
        			int vec = vecSize[i%vecSize.Count];
        			int bin = binSize[i%binSize.Count];
        			
        			if (bin<0)
	        		{
        				bin=(int)Math.Round((double)FInput.SliceCount/vec);
	        		}
        			
        			List<double> copyBin = new List<double>();
        			//loop for bin count
        			for (int j = 0; j<bin; j++)
        			{
        				double curDo;
        				FDo.GetValue(i, out curDo);
        				
        				//get vector
        				List<double> copyVec = new List<double>();
        				for (int jj = 0; jj<vec; jj++)
        				{
        					double tmpVal;
        					FInput.GetValue(increment, out tmpVal);
        					copyVec.Add(tmpVal);
        					
        					increment++;
        				}
        				
        				//insert reversed (or not) into bin
        				if (curDo>0.5)
        				{
        					copyBin.InsertRange(0, copyVec);
        				}
        				else
        				{
        					copyBin.AddRange(copyVec);
        				}
        			}
        			//aggregate to final list
        			outList.AddRange(copyBin);
        		}
        		
        		//write data to outputs
        		FOutput.SliceCount=outList.Count;
        		for (int i = 0; i<outList.Count; i++)
        		{
        			FOutput.SetValue(i, outList[i]);
        		}
        	}      	
        }
             
        #endregion mainloop  
	}
}
