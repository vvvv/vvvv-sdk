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
	public class EisensteinPrimes: IPlugin, IDisposable
    {	          	
    	#region field declaration
    	
    	//the host (mandatory)
    	private IPluginHost FHost; 
    	// Track whether Dispose has been called.
   		private bool FDisposed = false;

    	//input pin declaration
        private IValueIn FMyValueInputM;
        private IValueIn FMyValueInputN;
        private IValueIn FMyValuePhaseM;
        private IValueIn FMyValuePhaseN;

    	//output pin declaration
        private IValueOut FMyValueOutputX;
        private IValueOut FMyValueOutputY;
        private IValueOut FMyValueOutputIsPrime;
    	
    	#endregion field declaration
       
    	#region constructor/destructor
    	
        public EisensteinPrimes()
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
        ~EisensteinPrimes()
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
					FPluginInfo.Name = "EisensteinPrimes";
					//the nodes category: try to use an existing one
					FPluginInfo.Category = "Spreads";
					//the nodes version: optional. leave blank if not
					//needed to distinguish two nodes of the same name and category
					FPluginInfo.Version = "";
					
					//the nodes author: your sign
					FPluginInfo.Author = "fibo";
					//describe the nodes function
					FPluginInfo.Help = "Eisenstein's integers lattice";
					//specify a comma separated list of tags that describe the node
					FPluginInfo.Tags = "2d, prime numbers,lattice,spread";
					
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
            FHost.CreateValueInput("Input m ", 1, null, TSliceMode.Single, TPinVisibility.True, out FMyValueInputM);
            FMyValueInputM.SetSubType(1,double.MaxValue,1,1,false,false,true);

            FHost.CreateValueInput("Input n ", 1, null, TSliceMode.Single, TPinVisibility.True, out FMyValueInputN);
            FMyValueInputN.SetSubType(1, double.MaxValue, 1, 1, false, false, true);

            FHost.CreateValueInput("Phase m ", 1, null, TSliceMode.Single, TPinVisibility.True, out FMyValuePhaseM);
            FMyValuePhaseM.SetSubType(double.MinValue, double.MaxValue, 1, 0, false, false, true);

            FHost.CreateValueInput("Phase n ", 1, null, TSliceMode.Single, TPinVisibility.True, out FMyValuePhaseN);
            FMyValuePhaseN.SetSubType(double.MinValue, double.MaxValue, 1, 0, false, false, true);
	    	
	    	//create outputs	    	
            FHost.CreateValueOutput("Output X ", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FMyValueOutputX);
            FMyValueOutputX.SetSubType(1, double.MaxValue, 1, 0, false, false, false);

            FHost.CreateValueOutput("Output Y ", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FMyValueOutputY);
            FMyValueOutputY.SetSubType(1, double.MaxValue, 1, 0, false, false, false);
            
            FHost.CreateValueOutput("IsPrime ", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FMyValueOutputIsPrime);
            FMyValueOutputIsPrime.SetSubType(0, double.MaxValue, 0, 0, false, false, true);
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
            if (FMyValueInputM.PinIsChanged || FMyValueInputN.PinIsChanged || FMyValuePhaseM.PinIsChanged || FMyValuePhaseN.PinIsChanged )
        	{
                double m, n;
                FMyValueInputM.GetValue(0, out m);
                FMyValueInputN.GetValue(0, out n);

                double phaseM, phaseN;
                FMyValuePhaseM.GetValue(0, out phaseM);
                FMyValuePhaseN.GetValue(0, out phaseN);

                int index;                
                int sliceCount = (int)(m * n);
	        	
                FMyValueOutputX.SliceCount = sliceCount;
                FMyValueOutputY.SliceCount = sliceCount;
                FMyValueOutputIsPrime.SliceCount = sliceCount;
           
                for (int j=0; j<n; j++)
        		{
                    for (int i = 0; i < m; i++)
                    {
                        index = (int)(m * j + i);
                        //write data to outputs
                        double x = Convert.ToDouble(i) - Convert.ToDouble(j) / 2;
                        double y = Convert.ToDouble(j) * Math.Sqrt(3) / 2;
                        FMyValueOutputX.SetValue(index, x);
                        FMyValueOutputY.SetValue(index, y);
                        FMyValueOutputIsPrime.SetValue(index, isprime(i + Convert.ToInt32(phaseM), j + Convert.ToInt32(phaseN)));
                    }
                }
        	}      	
        }
             
        #endregion mainloop  

        private int isprime(int a,int b) 
        {
            //Eisenstein's integers norm
            int n = a * a - a * b + b * b;
            
            //FHost.Log(TLogType.Debug, "a=" + a + " b=" + b + " norm=" + n);
            for (int i = 2; i < Math.Sqrt(n); i++) 
            {
                if ((n % i) == 0) 
                {
                    return 0;
                }
            }
            //FHost.Log(TLogType.Debug, "is prime");
            return 1; 
        }
        
	}
    
}