#region licence/info

//////project name
//BézierSpread

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

//////initial author
//woei

#endregion licence/info

//use what you need
using System;
using System.Collections.Generic;
using VVVV.PluginInterfaces.V1;

//the vvvv node namespace
namespace VVVV.Nodes
{
	
	//class definition
	public class BezierSpread: IPlugin, IDisposable
    {	          	
    	#region field declaration
    	
    	//the host (mandatory)
    	private IPluginHost FHost; 
    	// Track whether Dispose has been called.
   		private bool FDisposed = false;

    	//input pin declaration
    	private IValueIn FPts;
    	private IValueIn FCtrl;
    	private IValueIn FVecSize;
    	private IValueIn FBinSize;
    	private IValueIn FPhase;
    	private IValueIn FFactor;
    	private IValueIn FClosed;
    	private IValueIn FSpreadC;
    	
    	//output pin declaration
    	private IValueOut FOutput;
    	
    	#endregion field declaration
       
    	#region constructor/destructor
    	
        public BezierSpread()
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
	        	
        		FHost.Log(TLogType.Debug, "BezierSpread is being deleted");
        		
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
        ~BezierSpread()
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
					FPluginInfo.Name = "BézierSpread";
					//the nodes category: try to use an existing one
					FPluginInfo.Category = "Spreads";
					//the nodes version: optional. leave blank if not
					//needed to distinguish two nodes of the same name and category
					FPluginInfo.Version = "Advanced";
					
					//the nodes author: your sign
					FPluginInfo.Author = "woei";
					//describe the nodes function
					FPluginInfo.Help = "";
					//specify a comma separated list of tags that describe the node
					FPluginInfo.Tags = "";
					
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
	    	FHost.CreateValueInput("Control", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FPts);
	    	FPts.SetSubType(double.MinValue, double.MaxValue, 0.01, 0.0, false, false, false);
	    	
	    	FHost.CreateValueInput("Tangent", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FCtrl);
	    	FCtrl.SetSubType(double.MinValue, double.MaxValue, 0.01, 0.0, false, false, false);
	    	
	    	FHost.CreateValueInput("Vector Size", 1, null, TSliceMode.Single, TPinVisibility.Hidden, out FVecSize);
	    	FVecSize.SetSubType(1, double.MaxValue, 1, 1, false, false, true);
	    	
	    	FHost.CreateValueInput("Bin Size", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FBinSize);
	    	FBinSize.SetSubType(double.MinValue, double.MaxValue, 1, -1, false, false, true);
	    	
	    	FHost.CreateValueInput("Phase", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FPhase);
	    	FPhase.SetSubType(double.MinValue, double.MaxValue, 0.01, 0, false, false, false);
	    	
	    	FHost.CreateValueInput("Factor", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FFactor);
	    	FFactor.SetSubType(double.MinValue, double.MaxValue, 0.01, 1, false, false, false);
	    	
	    	FHost.CreateValueInput("Closed", 1, null, TSliceMode.Dynamic, TPinVisibility.Hidden, out FClosed);
	    	FClosed.SetSubType(0, 1, 1, 0, false, true, false);
	    	
	    	FHost.CreateValueInput("Spreadcount", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FSpreadC);
	    	FSpreadC.SetSubType(0, double.MaxValue, 1, 1, false, false, true);
	    	
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
        public unsafe void Evaluate(int SpreadMax)
        {     	
        	//if any of the inputs has changed
        	//recompute the outputs
        	if (FSpreadC.PinIsChanged ||
        	    FPts.PinIsChanged ||
        	    FCtrl.PinIsChanged ||
        	    FVecSize.PinIsChanged ||
        	    FBinSize.PinIsChanged ||
        	    FFactor.PinIsChanged ||
        	    FPhase.PinIsChanged ||
        	    FClosed.PinIsChanged)
        	{	
        		double tmpVecSize;
        		FVecSize.GetValue(0, out tmpVecSize);
        		int vecSize = Math.Max(1,(int)Math.Round(tmpVecSize));
        		
	        	double ptBins;
	        	FBinSize.GetValue(0, out ptBins);
	        	ptBins=Math.Round(ptBins);
	        	int ptsTotal = 0;
	        	List<int> ptBinList = new List<int>();
	        	if (FFactor.SliceCount>1 || ptBins!=0)
	        	{
	        		int ptsMax = Math.Max(FPts.SliceCount,FCtrl.SliceCount);
	        		ptsMax = (int)Math.Ceiling(ptsMax/(double)vecSize);
	        		int binIncr=0;
	        		bool end = false;
	        		while (!end)
	        		{
	        			FBinSize.GetValue(binIncr, out ptBins);
	        			int curBin = (int)Math.Round(ptBins);
	        			if (curBin<0)
	        				curBin = (int)Math.Ceiling(ptsMax/(double)Math.Abs(curBin));
	        			ptBinList.Add(curBin);
	        			binIncr++;
	        			ptsTotal+=curBin;
	        			if (binIncr%FBinSize.SliceCount==0 && ptsTotal>=ptsMax)
	        				end=true;
	        		}
	        	}
	        	
	        	int maxLoop=Math.Max(ptBinList.Count, FPhase.SliceCount);
	        	maxLoop=Math.Max(maxLoop, FFactor.SliceCount);
	        	maxLoop=Math.Max(maxLoop, FClosed.SliceCount);
	        	maxLoop=Math.Max(maxLoop, FSpreadC.SliceCount);
	        	
	        	
	        	
	        	
	        	double* pts, ctrls, spread;
        		int ptC, ctrlC, spreadC;
        		FPts.GetValuePointer(out ptC, out pts);
        		FCtrl.GetValuePointer(out ctrlC, out ctrls);
        		FSpreadC.GetValuePointer(out spreadC, out spread);
        		
	        	int pIncr=0, oIncr=0;
	        	double[] returnArr = new double[0];
	        	for (int c=0; c<maxLoop; c++)
	        	{
	        		double phase, factor;
	        		FPhase.GetValue(c, out phase);
	        		FFactor.GetValue(c, out factor);
	        		
	        		int curSpreadC = (int)Math.Round(spread[c%spreadC]);
	        		Array.Resize(ref returnArr, returnArr.Length+curSpreadC*vecSize);
	        		double[] inArr = new double[curSpreadC];
	        		for (int i=0; i<curSpreadC; i++)
	        			inArr[i]=((i/(double)curSpreadC)*factor)+phase;
	        		
	        		int pCount = ptBinList[c%ptBinList.Count];
	        		for (int v=0; v<vecSize; v++)
	        		{
	        			double[] ptArr = new double[pCount];
	        			double[] ctrlArr = new double[pCount];
	        			for (int p=0; p<pCount; p++)
	        			{
	        				int ptId = pIncr+(p*vecSize)+v;
	        				ptArr[p]=pts[ptId%ptC];
	        				ctrlArr[p]=ctrls[ptId%ctrlC];
	        				
	        			}
	        			
	        			double closed;
	        			FClosed.GetValue(c, out closed);
	        			bool isClosed = closed>0.5;
	        			
	        			double[] outArr = PolyBezier(inArr, ptArr, ctrlArr, isClosed);
	        			
	        			for (int o=0; o<outArr.Length; o++)
	        				returnArr[(oIncr+o)*vecSize+v]=outArr[o];
	        		}
	        		
	        		oIncr+=curSpreadC;
	        		pIncr+=pCount*vecSize;
	        	}
	        	
	        	double* outVal;
	        	FOutput.SliceCount=returnArr.Length;
	        	FOutput.GetValuePointer(out outVal);
	        	for (int i=0; i<returnArr.Length; i++)
	        		outVal[i]=returnArr[i];
        	}      	
        }
             
        #endregion mainloop  
        
        private double[] PolyBezier(double[] input, double[] pts, double[] ctrls, bool IsClosed)
        {
        	double[] output = new double[input.Length];
        	int cCount = pts.Length-1;
        	if (IsClosed)
        		cCount++;
        	for (int i=0; i<input.Length; i++)
        	{
        		double mu = cCount*input[i];
        		while (mu>cCount)
        			mu-=cCount;
        		while (mu<0)
        			mu+=cCount;
        		int cId = (int)Math.Floor(mu);

        		
        		double pt1 = pts[cId%pts.Length];
        		double pt2 = pts[(cId+1)%pts.Length];
        		double c1 = pt1 + ctrls[cId%ctrls.Length];
        		double c2 = pt2 - ctrls[(cId+1)%ctrls.Length];
        		
        		output[i] = BezierCurve(pt1, pt2, c1, c2, mu-cId);
        	}
        	return output;
        }
        
        
        private double BezierCurve(double p1,double p2,double c1,double c2,double mu)
        {
        	double mum1,mum13,mu3;

        	mum1 = 1 - mu;
        	mum13 = mum1 * mum1 * mum1;
        	mu3 = mu * mu * mu;
        	
        	return mum13*p1 + 3*mu*mum1*mum1*c1 + 3*mu*mu*mum1*c2 + mu3*p2;
        }
	}
}
