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
using System.Collections.Generic;
using VVVV.PluginInterfaces.V1;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

//the vvvv node namespace
namespace VVVV.Nodes
{
	
	//class definition
	public class MultiCons: IPlugin, IDisposable
    {	          	
    	#region field declaration

        //the host (mandatory)
        private IPluginHost FHost;
        //Track whether Dispose has been called.
        private bool FDisposed = false;

        //input pin declaration
        private IValueConfig FPinCountPin;
        private IValueConfig FPinCountCons;
        private IValueIn FInputPin1_1;
        private IValueIn FInputPin1_2;
        private IValueIn FSelectPin1;
        private IValueIn FInputPin2_1;
        private IValueIn FInputPin2_2;
        private IValueIn FSelectPin2;

        //output pin declaration
        private IValueOut FOutputPin1;
        private IValueOut FOutputPin2;

        //input pins
        private List<IValueIn> FInputPinList;
        private List<IValueIn> FSelectPinList;
        private List<IValueOut> FOutputPinList;

        #endregion field declaration
       
    	#region constructor/destructor
    	
        public MultiCons()
        {
			//the nodes constructor
            FInputPinList = new List<IValueIn>();
            FSelectPinList = new List<IValueIn>();
            FOutputPinList = new List<IValueOut>();
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
                    FInputPinList.Clear();
                    FSelectPinList.Clear();
                    FOutputPinList.Clear();
        		}
        		// Release unmanaged resources. If disposing is false,
        		// only the following code is executed.
	        	
        		if (FHost != null)
	        		FHost.Log(TLogType.Debug, "Multicons is being deleted");
        		
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
        ~MultiCons()
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
                    FPluginInfo.Name = "MultiCons";
					//the nodes category: try to use an existing one
					FPluginInfo.Category = "Spreads";
					//the nodes version: optional. leave blank if not
					//needed to distinguish two nodes of the same name and category
					FPluginInfo.Version = "Advanced";
					
					//the nodes author: your sign
					FPluginInfo.Author = "fibo";
					//describe the nodes function
					FPluginInfo.Help = "More than Cons";
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
            FHost.CreateValueConfig("Pin Count", 1, null, TSliceMode.Single, TPinVisibility.OnlyInspector, out FPinCountPin);
            FPinCountPin.SetSubType(1, double.MaxValue, 1, 2, false, false, true);

            FHost.CreateValueConfig("Cons Count", 1, null, TSliceMode.Single, TPinVisibility.OnlyInspector, out FPinCountCons);
            FPinCountCons.SetSubType(1, double.MaxValue, 1, 2, false, false, true);

            FHost.CreateValueInput("Input 1,1", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FInputPin1_1);
            FInputPin1_1.SetSubType(double.MinValue, double.MaxValue, 0.01, 0.0, false, false, false);

            FHost.CreateValueInput("Input 1,2", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FInputPin1_2);
            FInputPin1_2.SetSubType(double.MinValue, double.MaxValue, 0.01, 0.0, false, false, false);

            FHost.CreateValueInput("Select 1", 1, null, TSliceMode.Single, TPinVisibility.True, out FSelectPin1);
            FSelectPin1.SetSubType(0, double.MaxValue, 1, 1, false, false, true);

            FHost.CreateValueInput("Input 2,1", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FInputPin2_1);
            FInputPin2_1.SetSubType(double.MinValue, double.MaxValue, 0.01, 0.0, false, false, false);

            FHost.CreateValueInput("Input 2,2", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FInputPin2_2);
            FInputPin2_2.SetSubType(double.MinValue, double.MaxValue, 0.01, 0.0, false, false, false);

            FHost.CreateValueInput("Select 2", 1, null, TSliceMode.Single, TPinVisibility.True, out FSelectPin2);
            FSelectPin2.SetSubType(0, double.MaxValue, 1, 1, false, false, true);

            FInputPinList.Add(FInputPin1_1);
            FInputPinList.Add(FInputPin1_2);
            FInputPinList.Add(FInputPin2_1);
            FInputPinList.Add(FInputPin2_2);
            FSelectPinList.Add(FSelectPin1);
            FSelectPinList.Add(FSelectPin2);

            //create outputs	    	
            FHost.CreateValueOutput("Output 1", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FOutputPin1);
            FOutputPin1.SetSubType(double.MinValue, double.MaxValue, 0.01, 0, false, false, false);

            FHost.CreateValueOutput("Output 2", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FOutputPin2);
            FOutputPin2.SetSubType(double.MinValue, double.MaxValue, 0.01, 0, false, false, false);

            FOutputPinList.Add(FOutputPin1);
            FOutputPinList.Add(FOutputPin2);
        }

        #endregion pin creation
        
        #region mainloop

        public void Configurate(IPluginConfig Input)
        {
            if (Input == FPinCountPin || Input == FPinCountCons)
            {
                double countPin;
                double countCons;
                FPinCountPin.GetValue(0, out countPin);
                FPinCountCons.GetValue(0, out countCons);

                // delete all pins.
                for (int i = 0; i < FInputPinList.Count; i++)
                {
                    IValueIn pinToDelete = FInputPinList[i];
                    FHost.DeletePin(pinToDelete);
                }
                for (int i = 0; i < FSelectPinList.Count; i++)
                {
                    IValueIn pinToDelete = FSelectPinList[i];
                    FHost.DeletePin(pinToDelete);
                }
                for (int i = 0; i < FOutputPinList.Count; i++)
                {
                    IValueOut pinToDelete = FOutputPinList[i];
                    FHost.DeletePin(pinToDelete);
                }
                FInputPinList.Clear();
                FSelectPinList.Clear();
                FOutputPinList.Clear();

                // create new pins.
                for (int i = 0; i < countCons; i++)
                {
                    for (int j = 0; j < countPin; j++)
                    {
                        IValueIn inputPin;

                        FHost.CreateValueInput("Input " + (i + 1) + "," + (j + 1), 1, null, TSliceMode.Dynamic, TPinVisibility.True, out inputPin);
                        inputPin.SetSubType(double.MinValue, double.MaxValue, 0.01, 0.0, false, false, false);

                        FInputPinList.Add(inputPin);
                    }

                    IValueIn selectPin;
                    FHost.CreateValueInput("Select " + (i + 1), 1, null, TSliceMode.Single, TPinVisibility.True, out selectPin);
                    selectPin.SetSubType(0, double.MaxValue, 1, 1, false, false, true);
                    FSelectPinList.Add(selectPin);
                }
                for (int i = 0; i < countPin; i++) 
                {
                    IValueOut outputPin;
                    FHost.CreateValueOutput("Output " + (i + 1), 1, null, TSliceMode.Dynamic, TPinVisibility.True, out outputPin);
                    outputPin.SetSubType(double.MinValue, double.MaxValue, 0.01, 0, false, false, false);
                    FOutputPinList.Add(outputPin);
                }
            }
        }
    
        //here we go, thats the method called by vvvv each frame
        //all data handling should be in here
        public void Evaluate(int SpreadMax)
        {
            double countPin;
            double countCons;
            FPinCountPin.GetValue(0, out countPin);
            FPinCountCons.GetValue(0, out countCons);

            int[] select = new int[(int)(countCons)];

            for (int cc = 0; cc < countCons;cc++ ) 
            {
                double currentValue;
                FSelectPinList[cc].GetValue(0, out currentValue);
                select[cc] = (int)currentValue;
            }

            for (int cp = 0; cp < countPin; cp++)
            {
                //get spread counts
                int[] countArray = new int[(int)(countCons)];
                int totalSpreadCount = 0;

                // indexOfInputPin = i * countPin + cp
                for (int i = 0; i < countCons; i++)
                {
                    // * select[i] so if it is 0 no value is copied
                    countArray[i] = FInputPinList[(int)(i * countPin + cp)].SliceCount * select[i];
                    totalSpreadCount += countArray[i];
                }

                FOutputPinList[cp].SliceCount = totalSpreadCount;

                //copy values
                int slice = 0;
                for (int i = 0; i < countCons; i++)
                {
                    for (int j = 0; j < countArray[i]; j++)
                    {
                        double currentValueSlice;
                        FInputPinList[(int)(i * countPin + cp)].GetValue(j, out currentValueSlice);
                        FOutputPinList[cp].SetValue(slice++, currentValueSlice);
                    }
                }
            }
        }
        #endregion mainloop
    }
}
