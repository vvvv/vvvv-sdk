#region licence/info

//////project name
//vvvv tweener shaper

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
//Westbam

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
	// TEST FOR MAKING IT A DELEGATE (DO THAT WHEN READY)
    //public delegate double ShapeFunc (Double X);

	//class definition
	public class PluginTemplate: IPlugin, IDisposable
    {	          	
    	#region field declaration
    	
        //
    	// AMOOUNT OFF TRANSITIONS TYPES YOU HAVE FINISHED BY NOW!!
        // (clamps the Pin)
        double FAmountOfTransitionsmade = 11;
        
        //the host (mandatory)
    	private IPluginHost FHost; 
    	// Track whether Dispose has been called.
   		private bool FDisposed = false;

    	//input pin declaration
    	private IValueIn FMyValueInput;
        private IValueIn FTransType;
        private IValueIn FTransMode;
        private IValueIn FInverse;
        
    
      	
    	//output pin declaration
    	private IValueOut FMyValueOutput;
    	private IStringOut FMyStringOutput;
        //private IStringOut FTransModeStringOutput;

    	
    	#endregion field declaration
       
    	#region constructor/destructor
    	
        public PluginTemplate()
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
        ~PluginTemplate()
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
	        	Info.Name = "Tweener";							//use CamelCaps and no spaces
	        	Info.Category = "Value";						//try to use an existing one
	        	Info.Version = "";						//versions are optional. leave blank if not needed
	        	Info.Help = "Applies some classic Tween shaping function to the value (range 0..1)";
	        	Info.Bugs = "";
                Info.Credits = "Zeh Fernando";								//give credits to thirdparty code used
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
        	//
                                   
            //assign host
	    	FHost = Host;

	    	//create inputs

            // The Pin for the Value
	    	FHost.CreateValueInput("Value Input", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FMyValueInput);
	    	FMyValueInput.SetSubType(double.MinValue, double.MaxValue, 0.01, 0.5, false, false, false);

            // The Pin for the Transtype
            // The Input is going to be Integer.
            FHost.CreateValueInput("Transition Type", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FTransType);
            FTransType.SetSubType(0.0, FAmountOfTransitionsmade-1, 1, 0, false, false, true);

            // The Pin for the Trans Mode (in, out, in/out, out/in)
            FHost.CreateValueInput("Transition Mode", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FTransMode);
            FTransMode.SetSubType(0.0, 3.0, 1, 0, false, false, true);

            // The Pin for the Toggle Inverse or Not...
            FHost.CreateValueInput("Inverse", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FInverse);
            FInverse.SetSubType(0.0, 1.0, 1, 0, false, true, true);
	    	
	    	//create outputs	    	
            
            // Values
	    	FHost.CreateValueOutput("Value Output", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FMyValueOutput);
            FMyValueOutput.SetSubType(double.MinValue, double.MaxValue, 0.01, 0, false, false, false);
	    	
            // TransType
	    	FHost.CreateStringOutput("Used Transition TYPE", TSliceMode.Dynamic, TPinVisibility.True, out FMyStringOutput);
	    	FMyStringOutput.SetSubType("", false);

          

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
            if (FMyValueInput.PinIsChanged || FTransType.PinIsChanged || FTransMode.PinIsChanged || FInverse.PinIsChanged)
        	{	
	        	//first set slicecounts for all outputs
	        	//the incoming int SpreadMax is the maximum slicecount of all input pins, which is a good default
	        	FMyValueOutput.SliceCount = SpreadMax;
                FMyStringOutput.SliceCount = SpreadMax;              
	     
	        	
	        	//the variables to fill with the input data
	        	double CurrentValueSlice;
                double CurrentTransType;
                double CurrentTransMode;
                double CurrentInverse;
                double X;
                
                
	        	string CurrentStringSlice;
	        	
        		//loop for all slices
        		for (int i=0; i<SpreadMax; i++)
        		{		
        			//read data from inputs
        			FMyValueInput.GetValue(i, out CurrentValueSlice);
                    FTransType.GetValue(i, out CurrentTransType);
                    FTransMode.GetValue(i, out CurrentTransMode);
                    FInverse.GetValue(i, out CurrentInverse);
                    // Force The Current TransType to an existing TransType!!
                    // Firsty make it Postive (anyone has a better way?) Than take the Leftover.
                    if  (CurrentTransType<0)
                    {
                        CurrentTransType = CurrentTransType * -1;
                    }
                    CurrentTransType = CurrentTransType % FAmountOfTransitionsmade;

                    // And I want the transmode always be 0,1,2 or 3.
                    if (CurrentTransMode < 0)
                    {
                        CurrentTransMode = CurrentTransMode * -1;
                    }
                    CurrentTransMode = CurrentTransMode % 4;

                    // Make the CurrentValue to something I can work with more easy!!
                    X = CurrentValueSlice;
                    // And Map it with a Clamp, so we only have 0.0000 to 1.0000 as an X
                    X = VMath.Map(X, 0.0000, 1.000, 0.000, 1.000, TMapMode.Clamp);
                         
                    //your function per slice
                    
                    // Nr. 0
                    if (CurrentTransType == 0)
                    {
                        CurrentStringSlice = "Linear Easing";
                    }

                    // -= QUADRATIC EASING =-
                    // Nr. 1 In
                    else if (CurrentTransType == 1 && CurrentTransMode == 0)
                    {
                        CurrentStringSlice = "Quadratic Easing In";
                        X = Tweener.QuadEaseIn(X);
                    }

                    // Nr. 1 Out
                    else if (CurrentTransType == 1 && CurrentTransMode == 1)
                    {
                        CurrentStringSlice = "Quadratic Easing Out";
                        X = Tweener.QuadEaseOut(X);
                    }

                    // Nr. 1 In/Out
                    else if (CurrentTransType == 1 && CurrentTransMode == 2)
                    {
                        CurrentStringSlice = "Quadratic Easing in/Out";
                        X = Tweener.QuadEaseInOut(X);
                    }
                    // Nr. 1 Out/in
                    else if (CurrentTransType == 1 && CurrentTransMode == 3)
                    {
                        CurrentStringSlice = "Quadratic Easing Out/In";
                        X = Tweener.QuadEaseOutIn(X);

                    }

                    // -= CUBIC EASING =-
                    // Nr. 2 In
                    else if (CurrentTransType == 2 && CurrentTransMode == 0)
                    {
                        CurrentStringSlice = "Cubic Easing In";
                        X = X * X * X;
                    }

                    // Nr.2 Out
                    else if (CurrentTransType == 2 && CurrentTransMode == 1)
                    {
                        CurrentStringSlice = "Cubic Easing Out";
                        X = X - 1; X = (X * X * X) + 1;
                    }

                    // Nr.2 In/Out
                    else if (CurrentTransType == 2 && CurrentTransMode == 2)
                    {
                        CurrentStringSlice = "Cubic Easing In/Out";
                        if (X <= 0.5)
                        {
                            X = X * 2;
                            X = X * X * X;
                            X = X / 2;
                        }
                        else if (X > 0.5)
                        {
                            X = (X - 0.5) * 2;
                            X = X - 1; X = (X * X * X) + 1;
                            X = (X / 2) + 0.5;
                        }
                    }

                    // Nr.2 Out/In
                    else if (CurrentTransType == 2 && CurrentTransMode == 3)
                    {
                        CurrentStringSlice = "Cubic Easing Out/In";
                        if (X <= 0.5)
                        {
                            X = X * 2;
                            X = X - 1; X = (X * X * X) + 1;
                            X = X / 2;
                        }
                        else if (X > 0.5)
                        {
                            X = (X - 0.5) * 2;
                            X = X * X * X;
                            X = (X / 2) + 0.5;
                        }

                    }

                    // -= QUARTIC EASING =-
                    // Nr.3 In
                    else if (CurrentTransType == 3 && CurrentTransMode == 0)
                    {
                        CurrentStringSlice = "Quartic Easing In";
                        X = X * X * X * X;
                    }

                    // Nr.3 Out
                    else if (CurrentTransType == 3 && CurrentTransMode == 1)
                    {
                        CurrentStringSlice = "Quartic Easing Out";
                        X = X - 1; X = (X * X * X * X) - 1; X = X * -1;
                    }

                    // Nr.3 In/Out
                    else if (CurrentTransType == 3 && CurrentTransMode == 2)
                    {
                        CurrentStringSlice = "Quadric Easing In/Out";
                        if (X <= 0.5)
                        {
                            X = X * 2;
                            X = X * X * X * X;
                            X = X / 2;
                        }
                        else if (X > 0.5)
                        {
                            X = (X - 0.5) * 2;
                            X = X - 1; X = (X * X * X * X) - 1; X = X * -1;
                            X = (X / 2) + 0.5;
                        }
                    }

                    // Nr.3  Out/In
                    else if (CurrentTransType == 3 && CurrentTransMode == 3)
                    {
                        CurrentStringSlice = "Quadric Easing Out/In";
                        if (X <= 0.5)
                        {
                            X = X * 2;
                            X = X - 1; X = (X * X * X * X) - 1; X = X * -1;
                            X = X / 2;
                        }
                        else if (X > 0.5)
                        {
                            X = (X - 0.5) * 2;
                            X = X * X * X * X;
                            X = (X / 2) + 0.5;
                        }
                    }

                    // -= QUINTYIC EASING =-
                    // Nr.4 In 
                    else if (CurrentTransType == 4 && CurrentTransMode == 0)
                    {
                        CurrentStringSlice = "Quintic Easing In";
                        X = X * X * X * X * X;
                    }

                    // Nr.4 Out
                    else if (CurrentTransType == 4 && CurrentTransMode == 1)
                    {
                        CurrentStringSlice = "Quintic Easing Out";
                        X = X - 1; X = X * X * X * X * X; X = X + 1;
                    }

                    // Nr.4 In/Out 
                    else if (CurrentTransType == 4 && CurrentTransMode == 2)
                    {
                        CurrentStringSlice = "Quintic Easing In/Out";
                        if (X <= 0.5)
                        {
                            X = X * 2;
                            X = X * X * X * X * X;
                            X = X / 2;
                        }
                        else if (X > 0.5)
                        {
                            X = (X - 0.5) * 2;
                            X = X - 1; X = X * X * X * X * X; X = X + 1;
                            X = (X / 2) + 0.5;
                        }
                    }

                    // Nr.4 Out/In
                    else if (CurrentTransType == 4 && CurrentTransMode == 3)
                    {
                        CurrentStringSlice = "Quintic Easing Out/In";
                        if (X <= 0.5)
                        {
                            X = X * 2;
                            X = X - 1; X = X * X * X * X * X; X = X + 1;
                            X = X / 2;
                        }
                        else if (X > 0.5)
                        {
                            X = (X - 0.5) * 2;
                            X = X * X * X * X * X;
                            X = (X / 2) + 0.5;
                        }
                    }

                    // -= SINUSOIDAL EASING =-
                    // Nr.5 In 
                    else if (CurrentTransType == 5 && CurrentTransMode == 0)
                    {
                        CurrentStringSlice = "Sinusoidal Easing In";
                        X = -1 * Math.Cos(X * (Math.PI / 2)) + 1;
                    }

                    // Nr.5 In 
                    else if (CurrentTransType == 5 && CurrentTransMode == 1)
                    {
                        CurrentStringSlice = "Sinusoidal Easing Out";
                        X = Math.Sin(X * (Math.PI / 2));
                    }

                    // Nr.5 In/Out
                    else if (CurrentTransType == 5 && CurrentTransMode == 2)
                    {
                        CurrentStringSlice = "Sinusoidal Easing In/Out";
                        if (X <= 0.5)
                        {
                            X = X * 2;
                            X = -1 * Math.Cos(X * (Math.PI / 2)) + 1;
                            X = X / 2;
                        }
                        else if (X > 0.5)
                        {
                            X = (X - 0.5) * 2;
                            X = Math.Sin(X * (Math.PI / 2));
                            X = (X / 2) + 0.5;
                        }
                    }

                    // Nr.5 Out/In
                    else if (CurrentTransType == 5 && CurrentTransMode == 3)
                    {
                        CurrentStringSlice = "Sinusoidal Easing Out/In";
                        if (X <= 0.5)
                        {
                            X = X * 2;
                            X = Math.Sin(X * (Math.PI / 2));
                            X = X / 2;
                        }
                        else if (X > 0.5)
                        {
                            X = (X - 0.5) * 2;
                            X = -1 * Math.Cos(X * (Math.PI / 2)) + 1;
                            X = (X / 2) + 0.5;
                        }
                    }

                    // -= Exponential Easing =-
                    // Nr. 6 In
                    else if (CurrentTransType == 6 && CurrentTransMode == 0)
                    {
                        CurrentStringSlice = "Exponential Easing In";
                        X = Math.Pow(2, 10 * (X - 1)) - 0.001;
                    }
                    // Nr. 6 Out
                    else if (CurrentTransType == 6 && CurrentTransMode == 1)
                    {
                        CurrentStringSlice = "Exponential Easing Out";
                        X = 1.001 * (-Math.Pow(2, -10 * X) + 1);
                    }
                    // Nr. 6 In/Out
                    else if (CurrentTransType == 6 && CurrentTransMode == 2)
                    {
                        CurrentStringSlice = "Exponential Easing In/Out";
                        if (X <= 0.5)
                        {
                            X = X * 2;
                            X = Math.Pow(2, 10 * (X - 1)) - 0.001;
                            X = X / 2;
                        }
                        else if (X > 0.5)
                        {
                            X = (X - 0.5) * 2;
                            X = 1.001 * (-Math.Pow(2, -10 * X) + 1);
                            X = (X / 2) + 0.5;
                        }
                    }

                    // Nr. 6 Out/in
                    else if (CurrentTransType == 6 && CurrentTransMode == 3)
                    {
                        CurrentStringSlice = "Exponential Easing Out/In";
                        if (X <= 0.5)
                        {
                            X = X * 2;
                            X = 1.001 * (-Math.Pow(2, -10 * X) + 1);
                            X = X / 2;
                        }
                        else if (X > 0.5)
                        {
                            X = (X - 0.5) * 2;
                            X = Math.Pow(2, 10 * (X - 1)) - 0.001;
                            X = (X / 2) + 0.5;
                        }
                    }


                    // -= CIRCULAR EASING =-
                    // Nr. 7 In
                    else if (CurrentTransType == 7 && CurrentTransMode == 0)
                    {
                        CurrentStringSlice = "Circulair Easing In";
                        X = -1 * (Math.Sqrt(1 - (X * X)) - 1);
                    }

                    // Nr. 7 Out
                    else if (CurrentTransType == 7 && CurrentTransMode == 1)
                    {
                        CurrentStringSlice = "Circulair Easing Out";
                        X = Math.Sqrt(1 - (X - 1) * (X - 1));
                    }
                                    
                    // Nr. 7 In/Out
                    else if (CurrentTransType == 7 && CurrentTransMode == 2)
                    {
                        CurrentStringSlice = "Circulair Easing In/Out";
                        if (X <= 0.5)
                        {
                            X = X * 2;
                            X = -1 * (Math.Sqrt(1 - (X * X)) - 1);
                            X = X / 2;
                        }
                        else if (X > 0.5)
                        {
                            X = (X - 0.5) * 2;
                            X = Math.Sqrt(1 - (X - 1) * (X - 1));
                            X = (X / 2) + 0.5;
                        }
                    }

                    // Nr. 7 Out/In
                    else if (CurrentTransType == 7 && CurrentTransMode == 3)
                    {
                        CurrentStringSlice = "Circulair Easing Out/In";
                        if (X <= 0.5)
                        {
                            X = X * 2;
                            X = Math.Sqrt(1 - (X - 1) * (X - 1));
                            X = X / 2;
                        }
                        else if (X > 0.5)
                        {
                            X = (X - 0.5) * 2;
                            X = -1 * (Math.Sqrt(1 - (X * X)) - 1);
                            X = (X / 2) + 0.5;
                        }
                    }
                    
                    // -= ELASTIC EASING =-
                    // Nr. 8 In
                    else if (CurrentTransType == 8 && CurrentTransMode == 0)
                    {
                        CurrentStringSlice = "Elastic Easing In";
                        // B = P 0.3, C = A 1.0  X = s 0.075
                        // (-C * power (2, 10 * (A-1)) * sin (( (A-1) - X) * (2 * pi) / B ))
                        X = (-1 * Math.Pow(2, 10 * (X - 1)) * Math.Sin(((X - 1) - 0.075) * (2 * Math.PI) / 0.3));
                    }

                    // Nr. 8 Out
                    else if (CurrentTransType == 8 && CurrentTransMode == 1)
                    {
                        CurrentStringSlice = "Elastic Easing Out";
                        // B = p 0.3    C = a 1.0  X = s 0.075
                        // C * power (2, -10 * A) * sin ( ( A - X) * (2 * pi) / B) + 1
                        X = 1 * Math.Pow(2, -10 * X) * Math.Sin((X - 0.075) * (2 * Math.PI) / 0.3) + 1;
                    }

                    // Nr. 8 In/Out
                    else if (CurrentTransType == 8 && CurrentTransMode == 2)
                    {
                        CurrentStringSlice = "Elastic Easing In/Out";
                        if (X <= 0.5)
                        {
                            X = X * 2;
                            X = (-1 * Math.Pow(2, 10 * (X - 1)) * Math.Sin(((X - 1) - 0.075) * (2 * Math.PI) / 0.3));
                            X = X / 2;
                        }
                        else if (X > 0.5)
                        {
                            X = (X - 0.5) * 2;
                            X = 1 * Math.Pow(2, -10 * X) * Math.Sin((X - 0.075) * (2 * Math.PI) / 0.3) + 1;
                            X = (X / 2) + 0.5;
                        }
                    }
                    // Nr. 8 Out/In
                    else if (CurrentTransType == 8 && CurrentTransMode == 3)
                    {
                        CurrentStringSlice = "Elastic Easing Out/In";
                        if (X <= 0.5)
                        {
                            X = X * 2;
                            X = 1 * Math.Pow(2, -10 * X) * Math.Sin((X - 0.075) * (2 * Math.PI) / 0.3) + 1;
                            X = X / 2;
                        }
                        else if (X > 0.5)
                        {
                            X = (X - 0.5) * 2;
                            X = (-1 * Math.Pow(2, 10 * (X - 1)) * Math.Sin(((X - 1) - 0.075) * (2 * Math.PI) / 0.3));
                            X = (X / 2) + 0.5;
                        }
                    }

                    // -= BACK EASING =-
                    // Nr. 9 In
                    else if (CurrentTransType == 9 && CurrentTransMode == 0)
                    {
                        CurrentStringSlice = "Back Easing In";
                        // X = 1.7016 s
                        // A * A * ((X + 1) * A - X)
                        X = X * X * ((1.7016 + 1) * X - 1.7016);
                    }
                    // Nr. 9 Out
                    else if (CurrentTransType == 9 && CurrentTransMode == 1)
                    {
                        CurrentStringSlice = "Back Easing Out";
                        // X = 1.7016 s
                        // (A-1) * (A-1) * ((X + 1) * (A-1) + X) +1
                        X = (X - 1) * (X - 1) * ((1.7016 + 1) * (X - 1) + 1.7016) + 1;
                    }

                    // Nr. 9 In/Out
                    else if (CurrentTransType == 9 && CurrentTransMode == 2)
                    {
                        CurrentStringSlice = "Back Easing In/Out";
                        if (X <= 0.5)
                        {
                            X = X * 2;
                            X = X = X * X * ((1.7016 + 1) * X - 1.7016);
                            X = X / 2;
                        }
                        else if (X > 0.5)
                        {
                            X = (X - 0.5) * 2;
                            X = X = (X - 1) * (X - 1) * ((1.7016 + 1) * (X - 1) + 1.7016) + 1;
                            X = (X / 2) + 0.5;
                        }
                    }

                    // Nr. 9 Out/In
                    else if (CurrentTransType == 9 && CurrentTransMode == 3)
                    {
                        CurrentStringSlice = "Back Easing Out/In";
                        if (X <= 0.5)
                        {
                            X = X * 2;
                            X = X = (X - 1) * (X - 1) * ((1.7016 + 1) * (X - 1) + 1.7016) + 1;
                            X = X / 2;
                        }
                        else if (X > 0.5)
                        {
                            X = (X - 0.5) * 2;
                            X = X = X * X * ((1.7016 + 1) * X - 1.7016);
                            X = (X / 2) + 0.5;
                        }
                    }
                        
                    // -= BOUNCE EASING =- 
                    // Only bugger that didn't fit in 1 line!!

                    // Nr. 10 In
                    else if (CurrentTransType == 10 && CurrentTransMode == 0)
                    {
                        CurrentStringSlice = "Bounce Easing In";
                        X = 1 - X;
                        if (X < 1 / 2.75)
                            X = 7.5625 * X * X;
                        else if (X < 2 / 2.75)
                        {
                            X = X - (1.5 / 2.75);
                            X = 7.5625 * X * X + 0.75;
                        }
                        else if (X < 2.5 / 2.75)
                        {
                            X = X - (2.25 / 2.75);
                            X = 7.5625 * X * X + 0.9375;
                        }
                        else
                        {
                            X = X - (2.625 / 2.75);
                            X = 7.5625 * X * X + 0.984375;
                        }
                        X = 1 - X;
                    }
                    // Nr. 10 Out 
                    else if (CurrentTransType == 10 && CurrentTransMode == 1)
                    {
                        CurrentStringSlice = "Bounce Easing Out";
                        if (X < 1/2.75)
                            X = 7.5625 * X * X;
                        else if (X < 2/2.75)
                        {
                            X = X - (1.5 / 2.75);
                            X = 7.5625 * X * X + 0.75;
                        }
                        else if (X < 2.5/2.75)
                        {
                            X = X - (2.25 / 2.75);
                            X = 7.5625 * X * X + 0.9375;
                        }   
                        else 
                        { 
                            X = X - (2.625 / 2.75);
                            X = 7.5625 * X * X + 0.984375;
                        }
                    }

                        // Nr. 10 In/Out
                    else if (CurrentTransType == 10 && CurrentTransMode == 2)
                    {
                        CurrentStringSlice = "Bounce Easing In/Out";
                        if (X <= 0.5)
                        {
                            X = X * 2;
                            X = 1 - X;
                            if (X < 1 / 2.75)
                                X = 7.5625 * X * X;
                            else if (X < 2 / 2.75)
                            {
                                X = X - (1.5 / 2.75);
                                X = 7.5625 * X * X + 0.75;
                            }
                            else if (X < 2.5 / 2.75)
                            {
                                X = X - (2.25 / 2.75);
                                X = 7.5625 * X * X + 0.9375;
                            }
                            else
                            {
                                X = X - (2.625 / 2.75);
                                X = 7.5625 * X * X + 0.984375;
                            }
                            X = 1 - X;
                            X = X / 2;
                        }
                        else if (X > 0.5)
                        {
                            X = (X - 0.5) * 2;
                            if (X < 1 / 2.75)
                                X = 7.5625 * X * X;
                            else if (X < 2 / 2.75)
                            {
                                X = X - (1.5 / 2.75);
                                X = 7.5625 * X * X + 0.75;
                            }
                            else if (X < 2.5 / 2.75)
                            {
                                X = X - (2.25 / 2.75);
                                X = 7.5625 * X * X + 0.9375;
                            }
                            else
                            {
                                X = X - (2.625 / 2.75);
                                X = 7.5625 * X * X + 0.984375;
                            }
                            X = (X / 2) + 0.5;
                        }
                    }
                    // Nr. 10 Out/In
                    else if (CurrentTransType == 10 && CurrentTransMode == 3)
                    {
                        CurrentStringSlice = "Bounce Easing Out/In";
                        if (X <= 0.5)
                        {
                            X = X * 2;
                            if (X < 1 / 2.75)
                                X = 7.5625 * X * X;
                            else if (X < 2 / 2.75)
                            {
                                X = X - (1.5 / 2.75);
                                X = 7.5625 * X * X + 0.75;
                            }
                            else if (X < 2.5 / 2.75)
                            {
                                X = X - (2.25 / 2.75);
                                X = 7.5625 * X * X + 0.9375;
                            }
                            else
                            {
                                X = X - (2.625 / 2.75);
                                X = 7.5625 * X * X + 0.984375;
                            }
                            X = X / 2;
                        }
                        else if (X > 0.5)
                        {
                            X = (X - 0.5) * 2;
                            X = 1 - X;
                            if (X < 1 / 2.75)
                                X = 7.5625 * X * X;
                            else if (X < 2 / 2.75)
                            {
                                X = X - (1.5 / 2.75);
                                X = 7.5625 * X * X + 0.75;
                            }
                            else if (X < 2.5 / 2.75)
                            {
                                X = X - (2.25 / 2.75);
                                X = 7.5625 * X * X + 0.9375;
                            }
                            else
                            {
                                X = X - (2.625 / 2.75);
                                X = 7.5625 * X * X + 0.984375;
                            }
                            X = 1 - X;
                            X = (X / 2) + 0.5;
                        }
                    }
                    
                    // I think not needed, but if I by accident set my TransistionTypecount too high....
                    else
                    CurrentStringSlice = "Westbam Made an Error :)";
                    
                    // Inverse The Tweener, as an extra :)
                    if (CurrentInverse == 1)
                    {
                        X = 1 - X;
                        CurrentStringSlice = CurrentStringSlice + " Inversed";
                    }

                    // Set X to the Output...        		    
                    CurrentValueSlice = X;

        			//write data to outputs
        			FMyValueOutput.SetValue(i, CurrentValueSlice);
        			FMyStringOutput.SetString(i, CurrentStringSlice);

                    //
                    // DONT FORGET THE SET THE AMMOUNT OFF FINISHED TWEENERS (TRANSITIONS) ON THE TOP OFF THIS PAGE!!!
                    // 
        		}
        	}      	
        }
             
        #endregion mainloop  
	}
}
