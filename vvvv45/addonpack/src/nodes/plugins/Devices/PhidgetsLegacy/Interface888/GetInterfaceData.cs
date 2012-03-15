//////project name
//Phidget Interface 888

//////description
//VVVV Plug In for the Phidget Interfaces.  http://www.phidgets.com/products.php?category=1
//reads the data from the hardware and passes it to the vvv plugIn

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



using System;
using System.Collections.Generic;
using System.Text;
using Phidgets;
using Phidgets.Events;

namespace VVVV.Nodes
{
    class GetInterfaceData
    {

        private bool FDisposed = false;

        private InterfaceKit m_IKit;
        private int m_Status = 0;

        public struct DeviceInfo
        {
            public string Name;
            public double SerialNumber;
            public double Version;
            public object Device;
            public int DigitalInputs;
            public int DigitalOutputs;
            public int AnalogOutputs;
        }

        List<DeviceInfo> m_Devices = new List<DeviceInfo>();

        private double[] m_AnalogInputs;
        private double[] m_DigitalInputs;
        private double[] m_DigitalOutput;
        private bool mAttached;

        # region Properties

        public List<DeviceInfo> InfoDevice
        {
            get
            {
                return m_Devices;
            }
            set
            {
                m_Devices = value;
            }
        }

        public int Status
        {
            get
            {
                return m_Status;
            }
            set
            {
                m_Status = value;
            }
        }

        public double[] AnalogInputs
        {
            get
            {
                return m_AnalogInputs;
            }
            set
            {
                m_AnalogInputs = value;
            }
        }

        public double[] DigitalInputs
        {
            get
            {
                return m_DigitalInputs;
            }
            set
            {
                m_DigitalInputs = value;
            }
        }

        public double[] DigitalOutput
        {
            get
            {
                return m_DigitalOutput;
            }
            set
            {
                m_DigitalOutput = value;
            }
        }

        public bool Attached
        {
            get
            {
                return mAttached;
            }
        }


        # endregion Properties

        #region constructor

        public GetInterfaceData()
        {
            m_Devices = new List<DeviceInfo>();
            
           
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
                Close();
        		// Note that this is not thread safe.
        		// Another thread could start disposing the object
        		// after the managed resources are disposed,mm
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
        ~GetInterfaceData()
        {
        	// Do not re-create Dispose clean-up code here.
        	// Calling Dispose(false) is optimal in terms of
        	// readability and maintainability.
        	Dispose(false);
        }




        #endregion constructor

        #region public Functions

        // this Funtkion are called by the VVVV plugin template

        public void Enable()
        {
            m_IKit.Attach += new AttachEventHandler(m_IKit_Attach);
            m_IKit.Detach += new DetachEventHandler(m_IKit_Detach);
            m_IKit.Error += new ErrorEventHandler(m_IKit_Error);
            m_IKit.InputChange += new InputChangeEventHandler(m_IKit_InputChange);
            m_IKit.OutputChange += new OutputChangeEventHandler(m_IKit_OutputChange);
            m_IKit.SensorChange += new SensorChangeEventHandler(m_IKit_SensorChange);
        }

        public void Disable()
        {
            m_IKit.Attach -= new AttachEventHandler(m_IKit_Attach);
            m_IKit.Detach -= new DetachEventHandler(m_IKit_Detach);
            m_IKit.Error -= new ErrorEventHandler(m_IKit_Error);
            m_IKit.InputChange -= new InputChangeEventHandler(m_IKit_InputChange);
            m_IKit.OutputChange -= new OutputChangeEventHandler(m_IKit_OutputChange);
            m_IKit.SensorChange -= new SensorChangeEventHandler(m_IKit_SensorChange);
        }

        public void Open(double Serial)
        {
            m_IKit = new InterfaceKit();
            Enable();
            if (Serial > 0)
            {
                m_IKit.open(Convert.ToInt32(Serial));
            }
            else if (Serial == 0)
            {
                m_IKit.open();
            }
            else
            {
                Disable();
                m_IKit = null;
            }
        }

        public void Close()
        {
            if (m_IKit != null)
            {
                Disable();
                m_IKit.close();
                m_IKit = null;
            }
        }

        public void SetSense(double[] sense)
        {
            for (int i = 0; i < AnalogInputs.Length;i++ )
            {
                sense[i] = sense[i] * 100;
                m_IKit.sensors[i].Sensitivity = Convert.ToInt32(sense[i]);
            }
        }

        public void SetDigitalOutput(double[] digiOut)
        {
            for (int i = 0; i < m_DigitalOutput.Length; i++)
            {
                m_IKit.outputs[i] = Convert.ToBoolean(digiOut[i]);
            }

        }

        public void SetRatiometric(double Ratiomatric)
        {
            m_IKit.ratiometric = Convert.ToBoolean(Ratiomatric);
        }


        #endregion public Functions

        # region Handler Funktions

        void m_IKit_SensorChange(object sender, SensorChangeEventArgs e)
        {
            double value = e.Value;
            value = value / 1000;
            m_AnalogInputs[e.Index] = value;

        }

        void m_IKit_OutputChange(object sender, OutputChangeEventArgs e)
        {
            m_DigitalOutput[e.Index] = Convert.ToDouble(e.Value);

        }

        void m_IKit_InputChange(object sender, InputChangeEventArgs e)
        {
            m_DigitalInputs[e.Index] = Convert.ToDouble(e.Value);
       
        }

        void m_IKit_Attach(object sender, AttachEventArgs e)
        {
           
            InterfaceKit attached = (InterfaceKit)sender;
            m_Status = 0;
            mAttached = true;
            if (attached.Attached)
            {
                m_Devices.Clear();
                m_Status = 1;
                DeviceInfo Infos = new DeviceInfo();
                Infos.Name = e.Device.Name;
                Infos.SerialNumber = e.Device.SerialNumber;
                Infos.Version = e.Device.Version;
                Infos.Device = sender;
                Infos.DigitalInputs = attached.inputs.Count;
                Infos.DigitalOutputs = attached.outputs.Count;
                Infos.AnalogOutputs = attached.sensors.Count;
                m_Devices.Add(Infos);

                m_AnalogInputs = new double[attached.sensors.Count];
                m_DigitalInputs = new double[attached.inputs.Count];
                m_DigitalOutput = new double[attached.outputs.Count];
            }
        }

        void m_IKit_Detach(object sender, DetachEventArgs e)
        {
            mAttached = false;
            m_Status = 0;
            Close();
            m_IKit = null;
        }

        void m_IKit_Error(object sender, ErrorEventArgs e)
        {

        }

        #endregion Handler Funktions

    }
}
