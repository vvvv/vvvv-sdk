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
	public class UndistortNode: IPlugin, IDisposable
    {	          	
    	#region field declaration
    	
    	//the host (mandatory)
    	private IPluginHost FHost; 
    	// Track whether Dispose has been called.
   		private bool FDisposed = false;

    	//input pin declaration
    	private IValueIn FInput;
    	private IValueIn FDistortion;
    	private IValueIn FFocalLength;
    	private IValueIn FPrincipalPoint;
    	private IValueIn FResolution;
    	
    	
    	//output pin declaration
    	private IValueOut FOutput;
    	
    	//distortion parameters
    	Vector4D FDist;
    	Vector2D FFocal;
    	Vector2D FPrincipal;
    	Vector2D FReso;
    	
    	
    	#endregion field declaration
    	
    	#region constructor/destructor
    	
        public UndistortNode()
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
        ~UndistortNode()
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
	        	Info.Name = "Undistort";							//use CamelCaps and no spaces
	        	Info.Category = "2d";						//try to use an existing one
	        	Info.Version = "OpenCV";						//versions are optional. leave blank if not needed
	        	Info.Help = "Undistort using OpenCV cam calib parameters";
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
	    	FHost.CreateValueInput("Input", 2, null, TSliceMode.Dynamic, TPinVisibility.True, out FInput);
	    	FInput.SetSubType(double.MinValue, double.MaxValue, 0.01, 0.5, false, false, false);
	    	
	    	FHost.CreateValueInput("Distortion", 4, null, TSliceMode.Dynamic, TPinVisibility.True, out FDistortion);
	    	FDistortion.SetSubType(double.MinValue, double.MaxValue, 0.01, 0.5, false, false, false);
	    	
	    	FHost.CreateValueInput("Focal Length", 2, null, TSliceMode.Dynamic, TPinVisibility.True, out FFocalLength);
	    	FFocalLength.SetSubType(double.MinValue, double.MaxValue, 0.01, 0.5, false, false, false);
	    	
	    	FHost.CreateValueInput("Principal Point", 2, null, TSliceMode.Dynamic, TPinVisibility.True, out FPrincipalPoint);
	    	FPrincipalPoint.SetSubType(double.MinValue, double.MaxValue, 0.01, 0.5, false, false, false);
	    	
	    	FHost.CreateValueInput("Resolution", 2, null, TSliceMode.Dynamic, TPinVisibility.True, out FResolution);
	    	FResolution.SetSubType(double.MinValue, double.MaxValue, 0.01, 0.5, false, false, false);

	    	//create outputs	    	
	    	FHost.CreateValueOutput("Output", 2, null, TSliceMode.Dynamic, TPinVisibility.True, out FOutput);
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
        	if (FDistortion.PinIsChanged || 
        	    FFocalLength.PinIsChanged ||
        	    FPrincipalPoint.PinIsChanged ||
        	    FResolution.PinIsChanged)
        	{
        		//get the distortion values
        		FDistortion.GetValue4D(0, out FDist.x, out FDist.y, out FDist.z, out FDist.w);
        		FFocalLength.GetValue2D(0, out FFocal.x, out FFocal.y);
        		FPrincipalPoint.GetValue2D(0, out FPrincipal.x, out FPrincipal.y);
        		FResolution.GetValue2D(0, out FReso.x, out FReso.y);
        	}
        	
        	//set slicecounts for output
        	//here its the same as the input
        	SpreadMax = FInput.SliceCount;
        	FOutput.SliceCount = SpreadMax;
        	
        	//the variable to fill with the input data
        	Vector2D currentPosition;
        	
        	//loop for all slices
        	for (int i=0; i<SpreadMax; i++)
        	{
        		//read data from inputs
        		FInput.GetValue2D(i, out currentPosition.x, out currentPosition.y);
        		
           		//function per slice
        		currentPosition = Undistort(currentPosition, FFocal, FPrincipal, FDist, FReso);
        		
        		//write data to outputs
        		FOutput.SetValue2D(i, currentPosition.x, currentPosition.y);
        	}
        	
        }
        
        #endregion mainloop

        //distortion function
        Vector2D distort(Vector2D p, double k1, double k2, double p1, double p2)
        {

        	double sq_r = p.x*p.x + p.y*p.y;

        	Vector2D q;
        	double a = 1 + sq_r * (k1 + k2 * sq_r);
        	double b = 2*p.x*p.y;

        	q.x = a*p.x + b*p1 + p2*(sq_r+2*p.x*p.x);
        	q.y = a*p.y + p1*(sq_r+2*p.y*p.y) + b*p2;

        	return q;
        }

        //coordinate undistortion
        Vector2D Undistort(Vector2D Pos, Vector2D FocalLength, Vector2D PrincipalPoint, Vector4D Distortion, Vector2D Resolution)
        {
        	Pos = (Pos * Resolution - PrincipalPoint) / FocalLength;
        	Pos = distort(Pos, Distortion.x, Distortion.y, Distortion.z, Distortion.w);
        	Pos = (Pos * FocalLength + PrincipalPoint) / Resolution;

        	return Pos;
        }

	}
}
