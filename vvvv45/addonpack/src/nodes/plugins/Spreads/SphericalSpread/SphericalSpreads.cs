#region licence/info

//////project name
//vvvv plugin SphericalSpreads

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

using VVVV.PluginInterfaces.V1;

//the vvvv node namespace
namespace VVVV.Nodes
{
	
	//class definition
	public class SphericalSpreads: IPlugin
    {	          	
    	#region field declaration
    	
		//the host (mandatory)
    	private IPluginHost FHost; 
    	
    	//input pin declaration
    	private IValueIn FMyInput;
    	private IValueIn FMyRadius;
    	private IValueIn FMyFactor;
    	private IValueIn FMySpreadCount;
    	   	
    	//output pin declaration
    	private IValueOut FMyOutput;
    	
    	#endregion field declaration
       
    	#region constructor/destructor
    	
        public SphericalSpreads()
        {
			//the nodes constructor
			//nothing to declare for this node
		}
        
        ~SphericalSpreads()
	    {
	    	//the nodes destructor
        	//nothing to destruct
	    }

        #endregion constructor/destructor
        
        #region node name and infos
       
        //provide node infos 
        public static IPluginInfo PluginInfo
	    {
	        get 
	        {
	        	//fill out nodes info
	        	IPluginInfo Info = new PluginInfo();
	        	Info.Name = "SphericalSpreads";
	        	Info.Category = "Spreads";
	        	Info.Version = "";
	        	Info.Help = "Evenly distributes Points on a Sphere";
	        	Info.Bugs = "";
	        	Info.Credits = "";
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
	    	FHost.CreateValueInput("Input", 3, null, TSliceMode.Dynamic, TPinVisibility.True, out FMyInput);
	    	FMyInput.SetSubType3D(double.MinValue, double.MaxValue, 0.01, 0, 0, 0, false, false, false);
	    		    	
  			FHost.CreateValueInput("Radius", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FMyRadius);
	    	FMyRadius.SetSubType(double.MinValue, double.MaxValue, 0.01, 1, false, false, false);
	    	
	    	FHost.CreateValueInput("Factor", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FMyFactor);
	    	FMyFactor.SetSubType(0, 1, 0.01, 1, false, false, false);
	    		    	
	    	FHost.CreateValueInput("Spread Count", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FMySpreadCount);
	    	FMySpreadCount.SetSubType(0, int.MaxValue, 1, 1, false, false, true);
	    		    	
	    	//create outputs	    	
	    	FHost.CreateValueOutput("Output", 3, null, TSliceMode.Dynamic, TPinVisibility.True, out FMyOutput);
	    	FMyOutput.SetSubType3D(double.MinValue, double.MaxValue, 0.01, 0, 0, 0, false, false, false);
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
        		FMyInput.PinIsChanged ||
        		FMyRadius.PinIsChanged ||
        		FMyFactor.PinIsChanged ||
        		FMySpreadCount.PinIsChanged
        		)
        	{		
        		double currentXSlice, currentYSlice, currentZSlice, xPos, yPos, zPos;
        		double tmpSize, currentRadius, currentFactor;
        		int currentBinSize = 0, myIncrement = 0;
        		
        		double dlong, l, dz, z, r;			//part of the formula

        		dlong = Math.PI*(3-Math.Sqrt(5.0)); //part of the formula
        		l = 0.0;							//part of the formula
        		
        		FMyOutput.SliceCount=0;

        		//loop for maximal spread count
        		for (int i=0; i<SpreadMax; i++)
        		{
        			FMyInput.GetValue3D(i, out currentXSlice, out currentYSlice, out currentZSlice);
        			FMyRadius.GetValue(i, out currentRadius);
        			FMyFactor.GetValue(i, out currentFactor);
        			FMySpreadCount.GetValue(i, out tmpSize);
        			
        			currentBinSize = (int)Math.Round(tmpSize);	//use spreadcount as an integer
        			
        			dz = (currentFactor*2.0)/currentBinSize;	//part of the formula
        			z  = 1.0 - dz/2.0;							//part of the formula
        			
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
        				FMyOutput.SliceCount= j + myIncrement + 1;
        				FMyOutput.SetValue3D(j+myIncrement, (xPos+currentXSlice), (yPos+currentYSlice), (zPos+currentZSlice));
        			}
        			myIncrement += currentBinSize;
        		}
        	}
        }
             
        #endregion mainloop  
	}
}
