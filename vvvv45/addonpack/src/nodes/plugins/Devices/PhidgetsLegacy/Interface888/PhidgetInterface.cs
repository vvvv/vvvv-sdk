#region licence/info

//////project name
//Phidget Interface 888

//////description
//VVVV Plug In for the Phidget Interfaces.  http://www.phidgets.com/products.php?category=1
//you can connect an Phidget Interface to vvv an controll the digital In and Out's.

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
//the phidgets drivers which you can find on  http://www.phidgets.com/downloads_sections.php

//////initial author
//phlegma 

#endregion licence/info

//use what you need
using System;
using System.Drawing;
using VVVV.PluginInterfaces.V1;
using Phidgets;
using Phidgets.Events;

//the vvvv node namespace
namespace VVVV.Nodes
{
	//class definition
	public class PhidgetInterface: IPlugin, IDisposable
    {	          	
    	#region field declaration
    	
    	//the host (mandatory)
    	private IPluginHost FHost; 
    	// Track whether Dispose has been called.
   		private bool FDisposed = false;

    	//input pin declaration
    	private IValueIn FEnable;
        private IValueIn FSensitivity;
        private IValueIn FSerial;
        private IValueIn FDigitalOut;
        private IValueIn FRatiomatric;

    	
    	//output pin declaration
    	private IValueOut FAnalogIn;
        private IValueOut FDigitalIn;
        //private IValueOut FConnected;
        private IStringOut FInfo;
        private IValueOut FDigiOutCount;

        //GetInterfaceData
        private GetInterfaceData m_IKitData;
        private Manager Anzahl = new Manager();
    	
    	#endregion field declaration
       


    	#region constructor/destructor
    	
        public PhidgetInterface()
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
	        		FHost.Log(TLogType.Debug, "IO Phidget InterfaceKit 888 is being deleted");
        		
        		if (m_IKitData != null)
        		{
	                m_IKitData.Close();
    	            m_IKitData = null;
        		}
    	            
         		
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
        ~PhidgetInterface()
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
                    FPluginInfo.Name = "IO";
                    //the nodes category: try to use an existing one
                    FPluginInfo.Category = "Devices";
                    //the nodes version: optional. leave blank if not
                    //needed to distinguish two nodes of the same name and category
                    FPluginInfo.Version = "Phidget InterfaceKit 888 Legacy";

                    //the nodes author: your sign
                    FPluginInfo.Author = "phlegma";
                    //describe the nodes function
                    FPluginInfo.Help = "Offers a connection to the Phidget InterfaceKit 8/8/8";
                    //specify a comma separated list of tags that describe the node
                    FPluginInfo.Tags = "Phidget, USB-Interface, Hardware, Sensors, A/D";

                    //give credits to thirdparty code used
                    FPluginInfo.Credits = "http://www.phidget.com";
                    //any known problems?
                    FPluginInfo.Bugs = "somthing werong with the connected Pin";
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
	    	FHost.CreateValueInput("Enable", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FEnable);
            FEnable.SetSubType(0, 1, 1, 0, false, true, true);

            FHost.CreateValueInput("Digital Out", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FDigitalOut);
            FDigitalOut.SetSubType(0, 1, 1, 0, false, true, true);

            FHost.CreateValueInput("Sensitivity", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FSensitivity);
            FSensitivity.SetSubType(0, 1, 0.01, 0.1, false, false, false);

            FHost.CreateValueInput("Ratiometric", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FRatiomatric);
            FRatiomatric.SetSubType(0, 1, 1, 1, false, true, true);

            FHost.CreateValueInput("Serial", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FSerial);
            FSerial.SetSubType(0, double.MaxValue, 1, 0, false, false, true);

            //create outputs	    	
	    	FHost.CreateValueOutput("Analog In", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FAnalogIn);
            FAnalogIn.SetSubType(double.MinValue, double.MaxValue, 0.0001, 0, false, false, false);

            FHost.CreateValueOutput("Digital In", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FDigitalIn);
            FDigitalIn.SetSubType(0, 1, 1, 0, false, true, true);

            FHost.CreateValueOutput("Number of Digital Outputs", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FDigiOutCount);
            FDigiOutCount.SetSubType(0, 1, 1, 0, false, true, true);
            
            //FHost.CreateValueOutput("Connected", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FConnected);
            //FConnected.SetSubType(0, 1, 1, 0, false, true, true);
	    	
            FHost.CreateStringOutput("Info", TSliceMode.Dynamic, TPinVisibility.True, out FInfo);
            FInfo.SetSubType("Disabled", false);

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
            
            double Enable;
            double Serial;
            double Ratiomatric;

            //FConnected.SliceCount = 1;
            
            FSerial.GetValue(0, out Serial);
            FEnable.GetValue(0, out Enable);
            FRatiomatric.GetValue(0, out Ratiomatric);




            try
            {

                if (FSerial.PinIsChanged || FEnable.PinIsChanged)
                {

                    if (FSerial.PinIsChanged)
                    {
                        if (m_IKitData != null)
                        {
                            m_IKitData.Close();
                            m_IKitData = null;
                        }
                    }

                    if (Enable > 0.5)
                    {
                        if (m_IKitData == null)
                        {
                            m_IKitData = new GetInterfaceData();
                            m_IKitData.Open(Serial);
                        }
                    }
                    else
                    {
                        if (m_IKitData != null)
                        {
                            FInfo.SliceCount = 1;
                            FInfo.SetString(0, "Disabled");
                            m_IKitData.Close();
                            m_IKitData = null;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                FHost.Log(TLogType.Error, "Error by initialising Phidget");
                FHost.Log(TLogType.Error, ex.Message.ToString());
            }





            if (Enable == 1 && m_IKitData.Attached)
            {
                //

                int SliceCountAnalogIn = m_IKitData.InfoDevice.ToArray()[0].AnalogOutputs;
                try
                {


                    //Setting Values of analog inputs

                    if (m_IKitData.InfoDevice.ToArray()[0].AnalogOutputs != 0)
                    {

                        FAnalogIn.SliceCount = SliceCountAnalogIn;
                        for (int i = 0; i < SliceCountAnalogIn; i++)
                        {
                            FAnalogIn.SetValue(i, m_IKitData.AnalogInputs[i]);
                        }
                    }




                    //setting Sensitivity of Analog Inputs
                    try
                    {
                        if (FSensitivity.PinIsChanged)
                        {
                            double SliceCountSense = FSensitivity.SliceCount;
                            double[] SenseValue = new double[SliceCountAnalogIn];
                            for (int i = 0; i < SliceCountAnalogIn; i++)
                            {
                                double sense;
                                FSensitivity.GetValue(i, out sense);
                                SenseValue[i] = sense;

                            }
                            m_IKitData.SetSense(SenseValue);
                        }
                    }
                    catch (Exception ex)
                    {
                        FHost.Log(TLogType.Error, "Error in Phidget " + m_IKitData.InfoDevice.ToArray()[0].Name + " setting Analog  Output");
                        FHost.Log(TLogType.Error, ex.Message.ToString());
                    }





                    //Setting Values Digital Inputs
                    try
                    {
                        if (m_IKitData.InfoDevice.ToArray()[0].DigitalInputs != 0)
                        {
                            int SliceCountDigitalIn = m_IKitData.InfoDevice.ToArray()[0].DigitalInputs;
                            FDigitalIn.SliceCount = SliceCountDigitalIn;
                            for (int i = 0; i < SliceCountDigitalIn; i++)
                            {
                                FDigitalIn.SetValue(i, m_IKitData.DigitalInputs[i]);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        FHost.Log(TLogType.Error, "Error in Phidget " + m_IKitData.InfoDevice.ToArray()[0].Name + " setting Digitial Output");
                        FHost.Log(TLogType.Error, ex.Message.ToString());
                    }





                    try
                    {
                        if (m_IKitData.InfoDevice.ToArray()[0].DigitalOutputs != 0)
                        {
                            int SliceCountDigitalOut = m_IKitData.InfoDevice.ToArray()[0].DigitalOutputs;

                            FDigiOutCount.SetValue(1, SliceCountDigitalOut);
                            double[] digiOut = new double[m_IKitData.InfoDevice.ToArray()[0].DigitalInputs];

                            for (int i = 0; i < SliceCountDigitalOut; i++)
                            {
                                FDigitalOut.GetValue(i, out digiOut[i]);
                            }
                            m_IKitData.SetDigitalOutput(digiOut);
                        }
                    }
                    catch (Exception ex)
                    {
                        FHost.Log(TLogType.Error, "Error in Phidget " + m_IKitData.InfoDevice.ToArray()[0].Name + " getting Digitial Output");
                        FHost.Log(TLogType.Error, ex.Message.ToString());
                    }





                    //setting Phidget Infos
                    try
                    {
                        int SpreadSizeInfo = 3;
                        for (int i = 0; i < SpreadSizeInfo; i++)
                        {
                            FInfo.SliceCount = 3;
                            switch (i)
                            {
                                case 0:
                                    FInfo.SetString(i, "Name: " + m_IKitData.InfoDevice.ToArray()[0].Name);
                                    break;
                                case 1:
                                    FInfo.SetString(i, "Serial: " + m_IKitData.InfoDevice.ToArray()[0].SerialNumber.ToString());
                                    break;
                                case 2:
                                    FInfo.SetString(i, "Version: " + m_IKitData.InfoDevice.ToArray()[0].Version.ToString());
                                    break;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        FHost.Log(TLogType.Error, "Error in Phidget " + m_IKitData.InfoDevice.ToArray()[0].Name + " setting Phidget Infos");
                        FHost.Log(TLogType.Error, ex.Message.ToString());
                    }





                    // setting Radiometric
                    try
                    {
                        if (m_IKitData != null)
                        {
                            //FConnected.SetValue(0, m_IKitData.Status);
                            if (m_IKitData.InfoDevice.ToArray()[0].Name.Contains("8/8/8"))
                            {
                                m_IKitData.SetRatiometric(Ratiomatric);
                            }

                        }
                        else
                        {
                            //FConnected.SetValue(0, 0);
                        }
                    }
                    catch (Exception ex)
                    {
                        FHost.Log(TLogType.Error, "Error in Phidget " + m_IKitData.InfoDevice.ToArray()[0].Name + " setting Radiometric");
                        FHost.Log(TLogType.Error, ex.Message.ToString());
                    }
                }
                catch (Exception ex)
                {
                    FHost.Log(TLogType.Error, "Error in Phidget " + m_IKitData.InfoDevice.ToArray()[0].Name );
                    FHost.Log(TLogType.Error, ex.Message.ToString());
                }
            }
            


            
        }
             
        #endregion mainloop  
	}
}
