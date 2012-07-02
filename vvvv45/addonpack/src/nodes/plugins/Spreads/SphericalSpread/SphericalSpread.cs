#region licence/info

//////project name
//SphericalSpread

//////description
//creates a spread of points distributed evenyl on a sphere

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

//////original code snippet
//  http://cgafaq.info/wiki/Evenly_distributed_points_on_sphere
//	dlong := pi*(3-sqrt(5))
//	long := 0
//	dz := 2.0/N
//	z := 1 - dz/2
//	for k := 0 .. N-1
//	r := sqrt(1-z*z)
//	pt[k] := (cos(long)*r, sin(long)*r, z)
//	z := z - dz
//	long := long + dlong

#endregion licence/info

//use what you need
using System;
using System.Drawing;
using System.Collections.Generic;
using VVVV.Utils.VMath;
using VVVV.PluginInterfaces.V1;

//the vvvv node namespace
namespace VVVV.Nodes
{
	
	//class definition
	public class SphericalSpread: IPlugin, IDisposable
    {	          	
    	#region field declaration
    	
		//the host (mandatory)
    	private IPluginHost FHost;
    	// Track whether Dispose has been called.
   		private bool FDisposed = false;
    	
    	//input pin declaration
    	private IValueIn FInput;
    	private IValueIn FRadius;
    	private IValueIn FFactor;
    	private IValueIn FSpreadCount;
    	   	
    	//output pin declaration
    	private IValueOut FOutput;
    	
    	#endregion field declaration
       
    	#region constructor/destructor
    	
        public SphericalSpread()
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
	        	
        		FHost.Log(TLogType.Debug, "SphericalSpread is being deleted");
        		
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
        ~SphericalSpread()
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
					FPluginInfo.Name = "SphericalSpread";
					//the nodes category: try to use an existing one
					FPluginInfo.Category = "Spreads";
					//the nodes version: optional. leave blank if not
					//needed to distinguish two nodes of the same name and category
					FPluginInfo.Version = "";
					
					//the nodes author: your sign
					FPluginInfo.Author = "woei";
					//describe the nodes function
					FPluginInfo.Help = "Evenly distributes points on a sphere";
					//specify a comma separated list of tags that describe the node
					FPluginInfo.Tags = "";
					
					//give credits to thirdparty code used
					FPluginInfo.Credits = "http://cgafaq.info/wiki/Evenly_distributed_points_on_sphere";
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
	    	FHost.CreateValueInput("Input", 3, null, TSliceMode.Dynamic, TPinVisibility.True, out FInput);
	    	FInput.SetSubType3D(double.MinValue, double.MaxValue, 0.01, 0, 0, 0, false, false, false);
	    		    	
  			FHost.CreateValueInput("Radius", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FRadius);
	    	FRadius.SetSubType(double.MinValue, double.MaxValue, 0.01, 1, false, false, false);
	    	
	    	FHost.CreateValueInput("Factor", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FFactor);
	    	FFactor.SetSubType(0, 1, 0.01, 1, false, false, false);
	    		    	
	    	FHost.CreateValueInput("Spread Count", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FSpreadCount);
	    	FSpreadCount.SetSubType(0, int.MaxValue, 1, 1, false, false, true);
	    		    	
	    	//create outputs	    	
	    	FHost.CreateValueOutput("Output", 3, null, TSliceMode.Dynamic, TPinVisibility.True, out FOutput);
	    	FOutput.SetSubType3D(double.MinValue, double.MaxValue, 0.01, 0, 0, 0, false, false, false);
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
        	if (
        		FInput.PinIsChanged ||
        		FRadius.PinIsChanged ||
        		FFactor.PinIsChanged ||
        		FSpreadCount.PinIsChanged
        		)
        	{	
        		List<Vector3D> outVec = new List<Vector3D>();
        		double curXSlice, curYSlice, curZSlice, xPos, yPos, zPos;
        		double tmpSize, currentRadius, currentFactor;
        		int currentBinSize = 0, myIncrement = 0;
        		
        		double dlong, l, dz, z, r;			//part of the formula

        		dlong = Math.PI*(3-Math.Sqrt(5.0)); //part of the formula
        		
        		
        		FOutput.SliceCount=0;

        		//loop for maximal spread count
        		for (int i=0; i<SpreadMax; i++)
        		{
        			FInput.GetValue3D(i, out curXSlice, out curYSlice, out curZSlice);
        			FRadius.GetValue(i, out currentRadius);
        			FFactor.GetValue(i, out currentFactor);
        			FSpreadCount.GetValue(i, out tmpSize);
        			
        			currentBinSize = (int)Math.Round(tmpSize);	//use spreadcount as an integer
        			
        			dz = (currentFactor*2.0)/currentBinSize;	//part of the formula
        			z  = 1.0 - dz/2.0;							//part of the formula
        			l = 0.0;									//part of the formula
        			
        			//loop for each bin size
        			for (int j=0; j<currentBinSize; j++)
        			{
        				r    = Math.Sqrt(1.0-z*z);
        				xPos = Math.Cos(l)*r*currentRadius;
        				yPos = Math.Sin(l)*r*currentRadius;
        				zPos = z*currentRadius;
        				
        				z = z - dz;								
        				l = l + dlong;							
						
        				//set output
        				outVec.Add(new Vector3D(xPos+curXSlice, yPos+curYSlice, zPos+curZSlice));
        			}
        			myIncrement += currentBinSize;
        		}
        		FOutput.SliceCount  = outVec.Count;
        		for (int i = 0; i<outVec.Count; i++)
        		{
        			FOutput.SetValue3D(i, outVec[i].x, outVec[i].y, outVec[i].z);
        		}
        	}
        }
             
        #endregion mainloop  
	}
}
