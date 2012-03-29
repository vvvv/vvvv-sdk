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
using System.Diagnostics;

namespace VVVV.Nodes
{
    class GetEncoderHSData
    {

        private bool FDisposed = false;

        private Phidgets.Encoder m_Encoder;
        private int m_Status = 0;

        public struct DeviceInfo
        {
            public string Name;
            public double SerialNumber;
            public double Version;
            public object Device;
            public int EncoderInputs;

        }

        List<DeviceInfo> m_Devices = new List<DeviceInfo>();

        private double[] m_EncoderInputs;
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

        public double[] EncoderInputs
        {
            get
            {
                return m_EncoderInputs;
            }
            set
            {
                m_EncoderInputs = value;
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

        public GetEncoderHSData()
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
        ~GetEncoderHSData()
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
            m_Encoder.Attach += new AttachEventHandler(m_IKit_Attach);
            m_Encoder.Detach += new DetachEventHandler(m_IKit_Detach);
            m_Encoder.Error += new ErrorEventHandler(m_IKit_Error);

            m_Encoder.InputChange += new InputChangeEventHandler(m_IKit_InputChange);
            m_Encoder.PositionChange += new EncoderPositionChangeEventHandler(EncoderPositionChange);
        }

        public void Disable()
        {
            m_Encoder.Attach -= new AttachEventHandler(m_IKit_Attach);
            m_Encoder.Detach -= new DetachEventHandler(m_IKit_Detach);
            m_Encoder.Error -= new ErrorEventHandler(m_IKit_Error);

            m_Encoder.InputChange -= new InputChangeEventHandler(m_IKit_InputChange);
            m_Encoder.PositionChange -= new EncoderPositionChangeEventHandler(EncoderPositionChange);
        }

        public void Open(double Serial)
        {
            m_Encoder = new Phidgets.Encoder();
            Enable();
            if (Serial > 0)
            {
                m_Encoder.open(Convert.ToInt32(Serial));
            }
            else if (Serial == 0)
            {
                m_Encoder.open();
            }
            else
            {
                Disable();
                m_Encoder = null;
            }
        }

        public void Close()
        //FConnected.SetValue(0, m_IKitData.Status);
{
            if (m_Encoder != null)
            {
                Disable();
                m_Encoder.close();
                m_Encoder = null;
            }
        }

        public void SetPosition(double[] pPosition)
        {
            for (int i = 0; i < pPosition.Length; i++)
            {
                m_Encoder.encoders[i] = (int)pPosition[i]; 
            }
        }



        #endregion public Functions





        # region Handler Funktions

        void EncoderPositionChange(object sender, EncoderPositionChangeEventArgs e)
        {
            double tRelativChange = e.PositionChange;
            double tPosition = m_Encoder.encoders[e.Index];
            m_EncoderInputs[e.Index] = tPosition;
           
        }

        void m_IKit_InputChange(object sender, InputChangeEventArgs e)
        {
           
        }

        void m_IKit_Attach(object sender, AttachEventArgs e)
        {
           
            Phidgets.Encoder attached = (Phidgets.Encoder)sender;
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
                Infos.EncoderInputs = attached.encoders.Count;

                m_Devices.Add(Infos);
                m_EncoderInputs = new double[attached.encoders.Count];

            }
        }

        void m_IKit_Detach(object sender, DetachEventArgs e)
        {
            mAttached = false;
            m_Status = 0;
            Close();
            m_Encoder = null;
        }



        void m_IKit_Error(object sender, ErrorEventArgs e)
        {

        }

        #endregion Handler Funktions

    }
}
