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
	public class QuaternionExp: IPlugin, IDisposable
    {	          	
    	#region field declaration
    	
    	//the host (mandatory)
    	private IPluginHost FHost; 
    	// Track whether Dispose has been called.
   		private bool FDisposed = false;

    	//input pin declaration
    	private IValueIn FMyValueInputX;
        private IValueIn FMyValueInputY;
        private IValueIn FMyValueInputZ;
    	
    	//output pin declaration
    	private IValueOut FMyValueOutputX;
        private IValueOut FMyValueOutputY;
        private IValueOut FMyValueOutputZ;
        private IValueOut FMyValueOutputW;

        private double epsilon = 0.01;

    	#endregion field declaration
       
    	#region constructor/destructor
    	
        public QuaternionExp()
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
	        	
        		if (FHost != null)
	        		FHost.Log(TLogType.Debug, "PluginTemplateNode is being deleted");
        		
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
        ~QuaternionExp()
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
					FPluginInfo.Name = "Exp";
					//the nodes category: try to use an existing one
					FPluginInfo.Category = "Quaternion";
					//the nodes version: optional. leave blank if not
					//needed to distinguish two nodes of the same name and category
					FPluginInfo.Version = "";
					
					//the nodes author: your sign
					FPluginInfo.Author = "fibo";
					//describe the nodes function
					FPluginInfo.Help = "The quaternion exponential function";
					//specify a comma separated list of tags that describe the node
					FPluginInfo.Tags = "4d,exp,map";
					
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
            FHost.CreateValueInput("X ", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FMyValueInputX);
            FMyValueInputX.SetSubType(double.MinValue, double.MaxValue, 0.01, 0, false, false, false);
            FHost.CreateValueInput("Y ", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FMyValueInputY);
            FMyValueInputY.SetSubType(double.MinValue, double.MaxValue, 0.01, 0, false, false, false);
            FHost.CreateValueInput("Z ", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FMyValueInputZ);
            FMyValueInputZ.SetSubType(double.MinValue, double.MaxValue, 0.01, 0, false, false, false);
	    	
	    	//create outputs	    	
            FHost.CreateValueOutput("X ", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FMyValueOutputX);
            FMyValueOutputX.SetSubType(double.MinValue, double.MaxValue, 0.01, 0, false, false, false);
            FHost.CreateValueOutput("Y ", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FMyValueOutputY);
            FMyValueOutputY.SetSubType(double.MinValue, double.MaxValue, 0.01, 0, false, false, false);
            FHost.CreateValueOutput("Z ", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FMyValueOutputZ);
            FMyValueOutputZ.SetSubType(double.MinValue, double.MaxValue, 0.01, 0, false, false, false);
            FHost.CreateValueOutput("W ", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FMyValueOutputW);
            FMyValueOutputW.SetSubType(double.MinValue, double.MaxValue, 0.01, 0, false, false, false);
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
            if (FMyValueInputX.PinIsChanged || FMyValueInputY.PinIsChanged || FMyValueInputZ.PinIsChanged)
        	{	
	        	//the variables to fill with the input data
	        	double x,y,z,w;

                FMyValueOutputX.SliceCount = SpreadMax;
                FMyValueOutputY.SliceCount = SpreadMax;
                FMyValueOutputZ.SliceCount = SpreadMax;
                FMyValueOutputW.SliceCount = SpreadMax;

                //loop for all slices
        		for (int i=0; i<SpreadMax; i++)
        		{		
        			//read data from inputs
                    FMyValueInputX.GetValue(i, out x);
                    FMyValueInputY.GetValue(i, out y);
                    FMyValueInputZ.GetValue(i, out z);
        			
                    // If q = A*(x*i+y*j+z*k) where (x,y,z) is unit length, then
                    // exp(q) = cos(A)+sin(A)*(x*i+y*j+z*k).  If sin(A) is near zero,
                    // use exp(q) = cos(A)+A*(x*i+y*j+z*k) since A/sin(A) has limit 1.

        			w = Math.Sqrt(x * x + y * y + z * z);

                    if (Math.Sin(w) < epsilon || Math.Sin(w) > epsilon)
                    {
                        x *= Math.Sin(w) / w;
                        y *= Math.Sin(w) / w;
                        z *= Math.Sin(w) / w;
                    }
                    
        			//write data to outputs
                    FMyValueOutputX.SetValue(i, x );
                    FMyValueOutputY.SetValue(i, y );
                    FMyValueOutputZ.SetValue(i, z );
                    FMyValueOutputW.SetValue(i, Math.Cos(w));
        		}
        	}      	
        }
             
        #endregion mainloop  
	}
}
