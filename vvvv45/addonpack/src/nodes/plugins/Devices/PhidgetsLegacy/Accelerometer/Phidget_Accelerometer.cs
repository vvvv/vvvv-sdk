#region licence/info

//////project name
//Phidget Interface Accelerometer

//////description
//VVVV Plug In for the Phidget Interfaces. http://www.phidgets.com/products.php?category=10
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
//the phidgets drivers which you can find on http://www.phidgets.com/downloads_sections.php

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
    public class Phidget_Accelerometer : IPlugin, IDisposable
    {
        #region field declaration

        //the host (mandatory)
        private IPluginHost FHost;
        private bool FDisposed = false;

        //input pin declaration
        private IValueIn FEnable;
        private IValueIn FSensitivity;
        private IValueIn FSerial;

        //output pin declaration
        private IValueOut FConnected;
        private IStringOut FInfo;
        private IValueOut FAxis;


        //Accelerometer
        private GetAccelerometerData Accelerometer;
        #endregion field declaration

        #region constructor/destructor

        public Phidget_Accelerometer()
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
            if (!FDisposed)
            {
                if (disposing)
                {
                    // Dispose managed resources.
                }
                // Release unmanaged resources. If disposing is false,
                // only the following code is executed.

                if (FHost != null)
	                FHost.Log(TLogType.Debug, "IO Phidget Accelerometer is being deleted");
                
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
        ~Phidget_Accelerometer()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            //Dispose(false);
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
                    FPluginInfo.Version = "Phidget Accelerometer Legacy";

                    //the nodes author: your sign
                    FPluginInfo.Author = "phlegma";
                    //describe the nodes function
                    FPluginInfo.Help = "Offers a connection to the Phidget Interface Accelerometer";
                    //specify a comma separated list of tags that describe the node
                    FPluginInfo.Tags = "Phidget, USB-Interface, Hardware, Sensors, A/D";

                    //give credits to thirdparty code used
                    FPluginInfo.Credits = "http://www.phidget.com";
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
            get { return true; }
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
            FEnable.SetSubType(0, 1, 1, 0, false, false, true);

            FHost.CreateValueInput("Sensitivity XYZ", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FSensitivity);
            FSensitivity.SetSubType(double.MinValue, double.MaxValue, 0.01, 0, false, false, false);

            FHost.CreateValueInput("Serial Number", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FSerial);
            FSerial.SetSubType(0, double.MaxValue, 1, 0, false, false, true);

            //create outputs
            FHost.CreateValueOutput("Axis XYZ", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FAxis);
            FAxis.SetSubType(0, 1, 0.0001, 0, false, false, false);

            FHost.CreateValueOutput("Connected", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FConnected);
            FConnected.SetSubType(0, 1, 1, 0, false, true, true);

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

            int status = 0;
            double sense = 0;
            double sense1 = 0;
            double sense2 = 0;
            double enable = 0;
            double Serial;

            FEnable.GetValue(0, out enable);
            FSerial.GetValue(0, out Serial);



            if (FEnable.PinIsChanged || FSerial.PinIsChanged)
            {
                if (FSerial.PinIsChanged)
                {
                    if (Accelerometer != null)
                    {
                        Accelerometer.Close();
                        Accelerometer = null;
                    }
                }

                if (enable == 1)
                {
                    Accelerometer = new GetAccelerometerData();
                    Accelerometer.Open(Serial);
                }
                else
                {
                    if (Accelerometer != null)
                    {
                        FInfo.SliceCount = 1;
                        FInfo.SetString(0, "Disabled");
                        Accelerometer.Close();
                        Accelerometer = null;
                    }
                }
            }

            int imax = 0;
            try
            {
                imax = Accelerometer.Info.Length;
                status = Accelerometer.status;

            }
            catch (NullReferenceException)
            {
                status = 0;
            }

            FConnected.SliceCount = 1;
            FConnected.SetValue(0, status);

            if (enable == 1)
            {

                for (int i = 0; i < 4; i++)
                {
                    FAxis.SliceCount = 3;
                    if (i == 0)
                        FAxis.SetValue(0, Accelerometer.xOut);
                    else if (i == 1)
                        FAxis.SetValue(1, Accelerometer.yOut);
                    else
                        FAxis.SetValue(2, Accelerometer.zOut);
                }


                if (FSensitivity.PinIsChanged)
                {
                    FSensitivity.GetValue(0, out sense);
                    FSensitivity.GetValue(1, out sense1);
                    FSensitivity.GetValue(2, out sense2);


                    Accelerometer.senseX = sense;
                    Accelerometer.senseY = sense1;
                    Accelerometer.senseZ = sense2;
                    Accelerometer.ChangeSensitivitiy();
                }


                for (int i = 0; i <= imax; i++)
                {
                    FInfo.SliceCount = imax;
                    //write data to outputs
                    FInfo.SetString(i, Accelerometer.Info[i]);
                }



            }
         }


        #endregion mainloop
        }
}
