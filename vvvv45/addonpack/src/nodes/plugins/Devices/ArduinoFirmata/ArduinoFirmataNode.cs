#region licence/info

//////project name
//Firmata Plugin

//////description
//Plugin to use with Arduino with Firmata 2.0 OS


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
//wirmachenbunt C.Engler

#endregion licence/info

//use what you need
using System;
using System.Drawing;
using VVVV.PluginInterfaces.V1;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;
using Firmata.NET;

//the vvvv node namespace
namespace VVVV.Nodes
{
	
	//class definition
	public class PluginTemplateNode: IPlugin, IDisposable
    {	          	
    	#region field declaration
    	
    	//the host (mandatory)
    	private IPluginHost FHost; 
    	// Track whether Dispose has been called.
   		private bool FDisposed = false;

    	//input pin declaration
    	private IValueIn SetDigitalOut;
    	private IValueIn ConfigureDigital;
    	private IValueIn ConfigurePWM;
    	private IValueIn SetComPort;
    	private IValueIn EnablePlugin;
    	
    	//output pin declaration
    	private IValueOut AnalogIn;
    	private IValueOut DigitalIn;
    	private Arduino myarduino = null;
    	
    	#endregion field declaration
       
    	#region constructor/destructor
    	
        public PluginTemplateNode()
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
        ~PluginTemplateNode()
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
					FPluginInfo.Name = "ArduinoFirmata";
					//the nodes category: try to use an existing one
					FPluginInfo.Category = "Devices";
					//the nodes version: optional. leave blank if not
					//needed to distinguish two nodes of the same name and category
					FPluginInfo.Version = "Legacy";
					
					//the nodes author: your sign
					FPluginInfo.Author = "wirmachenbunt";
					//describe the nodes function
					FPluginInfo.Help = "use Plugin with Arduino - Firmata 2.0 OS to easily turn the Arduino Board into an Interfaceboard";
					//specify a comma separated list of tags that describe the node
					FPluginInfo.Tags = "";
					
					//give credits to thirdparty code used
					FPluginInfo.Credits = "uses Firmata .NET Lib made by Tim Farley";
					//any known problems?
					FPluginInfo.Bugs = "The Code itself is not optimized for the evaluation strategy of vvvv";
					//any known usage of the node that may cause troubles?
					FPluginInfo.Warnings = "DO NOT USE THIS PLUGIN :) Use the updated Version (which is a module called Arduino) because it implements the FirmataEncode/-Decode nodes freshly coded just for you Arduino lovers!";
					
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
        	get {return true;}
        }
        
        #endregion node name and infos
        
      	#region pin creation
        
        //this method is called by vvvv when the node is created
        public void SetPluginHost(IPluginHost Host)
	    {
        	//assign host
	    	FHost = Host;

	    	//create inputs
	    	
	    	FHost.CreateValueInput("SetDigitalOut", 12, null, TSliceMode.Dynamic, TPinVisibility.True, out SetDigitalOut);
            SetDigitalOut.SetSubType(double.MinValue, double.MaxValue, 1, 0, false, false, true);
	    	
	    	FHost.CreateValueInput("ConfigureDigital", 12, null, TSliceMode.Dynamic, TPinVisibility.True, out ConfigureDigital);
	    	ConfigureDigital.SetSubType(double.MinValue, double.MaxValue, 1, 0, false, false, true);
	    	
	    	FHost.CreateValueInput("ConfigurePWM", 12, null, TSliceMode.Dynamic, TPinVisibility.True, out ConfigurePWM);
	    	ConfigurePWM.SetSubType(double.MinValue, double.MaxValue, 1, 0, false, false, true);

	    	FHost.CreateValueInput("ComPort", 1, null, TSliceMode.Single, TPinVisibility.True, out SetComPort);
	    	SetComPort.SetSubType(0, 100, 1, 0, false, false, true);
	    	
	    	
	    	FHost.CreateValueInput("Enable", 1, null, TSliceMode.Single, TPinVisibility.True, out EnablePlugin);
	    	EnablePlugin.SetSubType(0, 1, 1, 0, false, true, false);
	    	
	    	
	    	//create outputs	    	
	    	FHost.CreateValueOutput("AnalogIn", 6, null, TSliceMode.Dynamic, TPinVisibility.True, out AnalogIn);
	    	AnalogIn.SetSubType(double.MinValue, double.MaxValue, 0.01, 0, false, false, false);
	    	
	    	FHost.CreateValueOutput("DigitalIn", 12, null, TSliceMode.Dynamic, TPinVisibility.True, out DigitalIn);
	    	DigitalIn.SetSubType(double.MinValue, double.MaxValue, 0.01, 0, false, false, true);
	    	
        }

        #endregion pin creation
        
        #region mainloop
        
        public void Configurate(IPluginConfig Input)
        {
        	//nothing to configure in this plugin
        	//only used in conjunction with inputs of type cmpdConfigurate
        }
        
      
        
        
        public void Evaluate(int SpreadMax)
        {     	
        	
        	
        	double enable = 0;
        	double comport = 0;
        	
        	//the variables to fill with the input data
        	double setDigital = 0;
        	double configDigital= 0;
        	double configPWM=0;
	        
	    // setup digital inputs and outputs depending on pin settings
	        	
	        		
	        	
        	
        	// request Pins Enable and ComPort and create Connection to Arduino 
        	EnablePlugin.GetValue(0, out enable);
        	SetComPort.GetValue(0, out comport);	
        	//&& myarduino == null
        	if (enable == 1 && myarduino == null )
        	{
        		myarduino = new Arduino("COM"+comport);
        		// initially setup digital inputs and outputs depending on pin settings 
        		for (int i=0; i<12; i++)
	        		{
	        			ConfigureDigital.GetValue(i, out configDigital);
	        			ConfigurePWM.GetValue(i, out configPWM);
	        			if (configDigital == 0 && configPWM == 0)
	        			{
	        				myarduino.pinMode(i+2,Arduino.INPUT);
	        			}
	        			if (configDigital == 1 || configPWM == 1 )
	        			{
	        				myarduino.pinMode(i+2,Arduino.OUTPUT);
	        			}
	        		}
        		
        	}
        	//destroy connection on disable
        	if(enable == 0 && myarduino != null )
        	{
        		for (int i=0; i<12; i++)
        		{
        			
        			myarduino.digitalWrite(i+2,Arduino.LOW);
        			myarduino.analogWrite(i+2,0);
	      		}
        		myarduino.Close();
        		myarduino = null;
        	}
        	
        	
        	//set slicecounts for all outputs
        	AnalogIn.SliceCount = SpreadMax;
	        DigitalIn.SliceCount = SpreadMax;
	        
	        // this loop executes if plugin is enabled by pin
	        if(enable == 1)
	        {
	        	// setup digital inputs and outputs depending on pin settings
	        	if (ConfigureDigital.PinIsChanged || ConfigurePWM.PinIsChanged )
	        	{
	        		for (int i=0; i<12; i++)
	        		{
	        			ConfigureDigital.GetValue(i, out configDigital);
	        			ConfigurePWM.GetValue(i, out configPWM);
	        			if (configDigital == 0 && configPWM == 0)
	        			{
	        				myarduino.pinMode(i+2,Arduino.INPUT);
	        			}
	        			if (configDigital == 1 || configPWM == 1)
	        			{
	        				myarduino.pinMode(i+2,Arduino.OUTPUT);
	        			}
	        		}
	        	}
	        
	        	
	        	
	        	for (int i=0; i<12; i++)
        		{		
        			//read data from inputs
        			SetDigitalOut.GetValue(i, out setDigital);
        			ConfigureDigital.GetValue(i, out configDigital);
        			ConfigurePWM.GetValue(i, out configPWM);

        			
        			
        			//read data from digital inputs and set output pin
        			if (configDigital == 0 && configPWM == 0)
        			{
        				DigitalIn.SetValue(i, myarduino.digitalRead(i+2));
        			}
        			
        			//just set outputs to something when setdigital input pin changed
        			if(SetDigitalOut.PinIsChanged || ConfigureDigital.PinIsChanged || ConfigurePWM.PinIsChanged)
        			{
        				// DIGITALOUT HIGH set digital output to HIGH if settings apply
        				if (configDigital == 1 && configPWM == 0 && setDigital == 1)
        				{
        					myarduino.digitalWrite(i+2,Arduino.HIGH);
        				}
        				// DIGITALOUT LOW set digital output to LOW if settings apply
        				if (configDigital == 1 && configPWM == 0 && setDigital == 0)
        				{
        					myarduino.digitalWrite(i+2,Arduino.LOW);
        				}
        				// PWM OUT
        				if (configPWM == 1)
        				{
        					myarduino.analogWrite(i+2,Convert.ToInt32(setDigital));
        				}
        				
        			}
        			
        			
        			
        			
        		}
	        
	        
	        // read analog inputs and set output pin of plugin
	          for (int i=0; i<6; i++)
        		{
        			
	          	AnalogIn.SetValue(i, myarduino.analogRead(i));
	      		}
	       
	        }

        }
             
        #endregion mainloop  
	}
}
