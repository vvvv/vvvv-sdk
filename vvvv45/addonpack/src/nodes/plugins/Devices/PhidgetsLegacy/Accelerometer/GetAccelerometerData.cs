#region licence/info
/////project name
//Phidget Interface Accelerometer

//////description
//VVVV Plug In for the Phidget Interfaces.  http://www.phidgets.com/products.php?category=10
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



using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Text;
using Phidgets;
using Phidgets.Events;

namespace VVVV.Nodes
{
	class GetAccelerometerData
    {
        
    	private Accelerometer m_Accelerometer;
    	private bool FDisposed = false;
    	
    	// Dekleration Properties
    	private double m_xOut;
        private double m_yOut;
        private double m_zOut;
        
        private int m_Status;
        private string[] m_Info;
        
        private double[] m_xFilt;
		private double[] m_yFilt;
		private double[] m_zFilt;
		
		private double m_senseX;
		private double m_senseY;
		private double m_senseZ;
		


        # region Properties
        
        public string[] Info
        {
        	get
        	{
        		return m_Info;
        	}
        	set
        	{
        		m_Info = value;
        	}
        }
        
		public int status
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
        
        public double xOut
        {
            get
            {
                return m_xOut;
            }
            set
            {
                m_xOut = value;
            }
        }
        
        public double yOut
        {
            get
            {
                return m_yOut;
            }
            set
            {
                m_yOut = value;
            }
        }
        
        public double zOut
        {
            get
            {
                return m_zOut;
            }
            set
            {
                m_zOut = value;
            }
        }
        
        public double senseX
        {
            get
            {
                return m_senseX;
            }
            set
            {
                m_senseX = value;
            }
        }
        
        public double senseY
        {
        	get
        	{
        		return m_senseY;
        	}
        	set
        	{
        		m_senseY = value;
        	}
        }
        
        public double senseZ
        {
        	get
        	{
        		return m_senseZ;
        	}
        	set
        	{
        		m_senseZ = value;
        	}
        }
        
        # endregion Properties
    	
        #region constructor/destructor
    	
        public GetAccelerometerData()
        {
            m_xFilt = new double[7];
            m_yFilt = new double[7];
            m_zFilt = new double[7];
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
        ~GetAccelerometerData()
        {
        	// Do not re-create Dispose clean-up code here.
        	// Calling Dispose(false) is optimal in terms of
        	// readability and maintainability.
        	Dispose(false);
        }
        #endregion constructor/destructor

        #region public funcitons

        public void Enable()
        {
            m_Accelerometer.Attach += new AttachEventHandler(Attach);
            m_Accelerometer.Detach += new DetachEventHandler(Detach);
            m_Accelerometer.AccelerationChange += new AccelerationChangeEventHandler(Change);
            m_Accelerometer.Error += new ErrorEventHandler(Error);
        }

        public void Disable()
        {
            m_Accelerometer.Attach -= new AttachEventHandler(Attach);
            m_Accelerometer.Detach -= new DetachEventHandler(Detach);
            m_Accelerometer.AccelerationChange -= new AccelerationChangeEventHandler(Change);
            m_Accelerometer.Error -= new ErrorEventHandler(Error);
        }
        
        
        public void Open(double serialNumber)
        {

            m_Accelerometer = new Accelerometer();
            Enable();

            if (serialNumber > 0)
            {
                m_Accelerometer.open(Convert.ToInt32(serialNumber));
            }
            else if (serialNumber == 0)
            {
                m_Accelerometer.open();
            }else
            {
                Disable();
                m_Accelerometer = null;
            }
        }
        
        public void Close()
        {
            if (m_Accelerometer != null)
            {
                Disable();
                m_Accelerometer.close();
                m_Accelerometer = null;
            }
        }
        
        public void ChangeSensitivitiy()
        {
        	m_Accelerometer.axes[0].Sensitivity = m_senseX;
        	m_Accelerometer.axes[1].Sensitivity = m_senseY;
        	m_Accelerometer.axes[2].Sensitivity = m_senseZ;
        }
        #endregion public functions

        #region Event Handler

        private void Attach(object sender, AttachEventArgs e)
        {
            Accelerometer attached = (Accelerometer)sender;
            m_Status = 0;

            if (attached.Attached)
            {
                string name = sender.ToString();
                m_Status = 1;

                m_Info = new string[] { "Name: " + attached.Name.ToString(), "Serial Number: " + attached.SerialNumber.ToString(), "Version: " + attached.Version.ToString() };

                attached.axes[0].Sensitivity = 0;
                attached.axes[1].Sensitivity = 0;
                if (attached.axes.Count == 3)
                    attached.axes[2].Sensitivity = 0;
            }
        	
        }
        
        private void Change(object sender, AccelerationChangeEventArgs e)
        {
        	int i = 0;
            {
                switch (e.Index)
                {
                    case 0:
                        m_xOut = 0;
                        m_xFilt[6] = e.Acceleration;
                        for (i = 0; i < 6; i++)
                        {
                            m_xFilt[i] = m_xFilt[i + 1];
                            m_xOut = m_xOut + m_xFilt[i];
                        }
                        m_xOut = m_xOut / 6;
                        break;
                    case 1:
                        m_yOut = 0;
                        m_yFilt[6] = e.Acceleration;
                        for (i = 0; i < 6; i++)
                        {
                            m_yFilt[i] = m_yFilt[i + 1];
                            m_yOut = m_yOut + m_yFilt[i];
                        }
                        m_yOut = m_yOut / 6;
                        break;
                    case 2:
                       m_zOut = 0;
                       m_zFilt[6] = e.Acceleration;
                        for (i = 0; i < 6; i++)
                        {
                            m_zFilt[i] = m_zFilt[i + 1];
                            m_zOut = m_zOut + m_zFilt[i];
                        }
                        m_zOut = m_zOut / 6;
                        break;
                }
            }
        }
        
		private void Detach(object sender, DetachEventArgs e)
        {
        	m_Status = 0;
            Close();
            m_Accelerometer = null;
        }
		
		private void Error(object sender, ErrorEventArgs e)
		{

        }

        #endregion Event Handler
    }
}
