#region licence/info

//////project name
//Undistort (like in OpenCV)

//////description
//Undistort (like in OpenCV), but this time for positions within the source pic

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
	        	Info.Help = "Undistort using OpenCV cam calib parameters. use when you want to map destinationtexcoords to source textcoords.";
	        	Info.Bugs = "";
	        	Info.Credits = "";								//give credits to thirdparty code used
	        	Info.Warnings = "";
	        	Info.Tags = "opencv, purely functional, deterministic, map, distort";
	        	Info.Author = "vvvv group";
	        	
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
	//class definition
	public class Inv_UndistortNode: IPlugin, IDisposable
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
    	private IValueOut FTries;
    	
    	//distortion parameters
    	Vector4D FDist;
    	Vector2D FFocal;
    	Vector2D FPrincipal;
    	Vector2D FReso;
    	
    	
    	#endregion field declaration
    	
    	#region constructor/destructor
    	
        public Inv_UndistortNode()
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
        ~Inv_UndistortNode()
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
	        	Info.Version = "OpenCV Inverse";						//versions are optional. leave blank if not needed
	        	Info.Help = "Undistort using OpenCV cam calib parameters. use when you want to map source texcoords to destination textcoords.";
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

	    	FHost.CreateValueOutput("Tries", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FTries);
	    	FOutput.SetSubType(1, double.MaxValue, 1, 0, false, false, false);
	    	
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
        	FTries.SliceCount = SpreadMax;
        	
        	//the variable to fill with the input data
        	Vector2D currentPosition;
        	
        	//loop for all slices
        	for (int i=0; i<SpreadMax; i++)
        	{
        		//read data from inputs
        		FInput.GetValue2D(i, out currentPosition.x, out currentPosition.y);
        		
        		int tries;
        		
           		//function per slice
        		currentPosition = Undistort(currentPosition, FFocal, FPrincipal, FDist, FReso, out tries);
        		
        		//write data to outputs
        		FOutput.SetValue2D(i, currentPosition.x, currentPosition.y);
        		FTries.SetValue(i, tries);
        	}
        	
        }
        
        #endregion mainloop

        //distortion function and partial derivatives of distortion function (for p.x, p.y)
        Vector2D distortd(Vector2D p, double k1, double k2, double p1, double p2, out Vector2D d1, out Vector2D d2)
        {
        	double sq_x = p.x*p.x;
        	double sq_y = p.y*p.y;
        	double sq_r = sq_x + sq_y;

        	double a_= (k1 + k2 * sq_r);
        	double a = 1 + sq_r * a_;
        	double b = 2*p.x*p.y;
        	double c = a_ + sq_r * k2;

        	d1.x = 2*sq_x*c + a + 2*p1*p.y + 6*p2*p.x;
        	d1.y = b*c + 2*p2*p.y + 2*p1*p.x;
        	d2.x = d1.y;
        	d2.y = 2*sq_y*c + a + 2*p2*p.x + 6*p1*p.y;
				
        	Vector2D q;
        	q.x = a*p.x + b*p1 + p2*(sq_r+2*p.x*p.x);
        	q.y = a*p.y + p1*(sq_r+2*p.y*p.y) + b*p2;

        	return q;
        	
        	//maxima helped to get the derivative:  http://en.wikipedia.org/wiki/Maxima_(software)
        	
        	// the partial derivative distort'() for px
        	//dx=px*(2*px*(k2*(py^2+px^2)+k1)+2*k2*px*(py^2+px^2))+(py^2+px^2)*(k2*(py^2+px^2)+k1)+2*p1*py+6*p2*px+1
        	//dy=py*(2*px*(k2*(py^2+px^2)+k1)+2*k2*px*(py^2+px^2))+2*p2*py+2*p1*px
        	        	
        	//command was:
        	//diff([(1 + (px*px + py*py) * (k1 + k2 * (px*px + py*py)))*px + (2*px*py)*p1 
 			//+ p2*((px*px + py*py)+2*px*px), (1 + (px*px + py*py) * (k1 + k2 * (px*px + py*py)))*py
  			//+ p1*((px*px + py*py)+2*py*py) + (2*px*py)*p2], px);
  			//so in fact it is like saying
  			//diff([q.x, q.y], px) 
  			//and that is give me the vector what's happening in that distort function when moving px a little
        	
        	// the partial derivative distort'() for py
        	//dx=px*(2*py*(k2*(py^2+px^2)+k1)+2*k2*py*(py^2+px^2))+2*p2*py+2*p1*px
        	//dy=py*(2*py*(k2*(py^2+px^2)+k1)+2*k2*py*(py^2+px^2))+(py^2+px^2)*(k2*(py^2+px^2)+k1)+6*p1*py+2*p2*px+1
        	
  			//command was the same but for py: diff([q.x, q.y], py) 
  			
  			//the lines above are handmade simplifications through substitutions, handmade...
        	
        }
        
        //coordinate undistortion
        Vector2D Undistort(Vector2D Pos, Vector2D FocalLength, Vector2D PrincipalPoint, Vector4D Distortion, Vector2D Resolution, out int n)
        {
        	Pos = (Pos * Resolution - PrincipalPoint) / FocalLength;
        	
        	double eps = 1;
        	Vector2D goal = Pos;
        	Vector2D guess = goal;
        	Vector2D dx;
        	Vector2D dy;
        	n = 0;
        	while ((eps > 0.001) && (n <100))
        	{       	   	
        	  Pos = distortd(guess, Distortion.x, Distortion.y, Distortion.z, Distortion.w, out dx, out dy);
        	  
        	  // we wanted to put a guess into distort which ouputs the goal.
        	  // let's see how big the difference to the goal really is:
        	  
        	  Vector2D gap = goal-Pos;
        	  eps = !gap; 
        	  	
        	  //now that we know how big the gap is, we try to go there linearly by moving in the guess system
        	  //dx and dy vectors tell us how the outcome will change when moving guess in x or y direction

        	  //maxima command:
        	  //
        	  //solve([   gapx = a*dxx + b*dyx, 
        	  //		  gapy = a*dxy + b*dyy],       [a, b]);
        	  //
        	  //answer: [[a=-(dyy*gapx-dyx*gapy)/(dxy*dyx-dxx*dyy),b=(dxy*gapx-dxx*gapy)/(dxy*dyx-dxx*dyy)]]
        	    
        	  //so lets get the gap relative to the local coordinate system (around last guess)
        	  //by that we transform the gap from the goal=distort(guess) room into the guess room
        	  
        	  double a = -(dy.y*gap.x-dy.x*gap.y)/(dx.y*dy.x-dx.x*dy.y);
        	  double b =  (dx.y*gap.x-dx.x*gap.y)/(dx.y*dy.x-dx.x*dy.y);
        	         	  
        	  guess.x += a;
        	  guess.y += b;
       	  
        	  n++;
        	}
        	
        	// we want to know the optimal guess, not the lastly calculated Pos (which should anyway be near the goal)
        	Pos = guess;

        	Pos = (Pos * FocalLength + PrincipalPoint) / Resolution;
        	
        	return Pos;
        }

	}
}
