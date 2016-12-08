#region licence/info
/////project name
//Phidget Interface Circular Touch

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
using System.Collections.Generic;
using System.Text;
using Phidgets;
using Phidgets.Events;

namespace VVVV.Nodes
{
    class GetTouchData
    {
        private InterfaceKit m_CTouch;
        private int m_Status;
        private string[] m_Info;
        private double m_Position;
        //private double m_Sense;
        private double[] m_Press;

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

        public double Position
        {
            get
            {
                return m_Position;
            }
            set
            {
                m_Position = value;
            }
        }

        public double[] Press
        {
            get
            {
                return m_Press;
            }
            set
            {
                m_Press = value;
            }
        }

        # endregion Properties

        #region constructor

        public GetTouchData()
        {
            m_CTouch = new InterfaceKit();
            m_Press = new Double[2];
        }

        #endregion constructor

        #region public Functions

        // this Funtkion are called by the VVVV plugin template

        public void Enable(double Serial)
        {

            m_CTouch.InputChange += new InputChangeEventHandler(m_CTouch_InputChange);
            m_CTouch.SensorChange += new SensorChangeEventHandler(m_CTouch_SensorChange);
            m_CTouch.Attach += new AttachEventHandler(m_CTouch_Attach);
            m_CTouch.Detach += new DetachEventHandler(m_CTouch_Detach);
            if (Serial > 0)
            {
                m_CTouch.open(Convert.ToInt32(Serial));
            }
            else
            {
                m_CTouch.open();
            }

            if (m_CTouch.Name == "Phidget Touch Rotation")
            {
                if (Serial != 0)
                {
                    Close();
                    m_CTouch.InputChange += new InputChangeEventHandler(m_CTouch_InputChange);
                    m_CTouch.SensorChange += new SensorChangeEventHandler(m_CTouch_SensorChange);
                    m_CTouch.Attach += new AttachEventHandler(m_CTouch_Attach);
                    m_CTouch.Detach += new DetachEventHandler(m_CTouch_Detach);
                    m_CTouch.open(Convert.ToInt32(Serial));
                }
                else if (Serial != 0 && m_CTouch.SerialNumber != Serial)
                {
                    m_CTouch.close();
                }
                else
                {
                }
            }
            else
            {
                m_CTouch.close();
            }
        }

        public void Close()
        {
            m_CTouch.InputChange -= new InputChangeEventHandler(m_CTouch_InputChange);
            m_CTouch.SensorChange -= new SensorChangeEventHandler(m_CTouch_SensorChange);
            m_CTouch.Attach -= new AttachEventHandler(m_CTouch_Attach);
            m_CTouch.Detach -= new DetachEventHandler(m_CTouch_Detach);
            m_CTouch.close();
        }

        public void SetSense(double sense)
        {
            sense = sense * 100;
            m_CTouch.sensors[0].Sensitivity = Convert.ToInt32(sense);
        }


        #endregion public Functions

        # region Handler Funktions

        void m_CTouch_Detach(object sender, DetachEventArgs e)
        {
            m_Status = 0;
        }

        void m_CTouch_Attach(object sender, AttachEventArgs e)
        {
            InterfaceKit attached = (InterfaceKit)sender;
            m_Info = new string[] { "Type: " + attached.Type.ToString(), "Name: " + attached.Name.ToString(), "Serial Number: " + attached.SerialNumber.ToString(), "Version: " + attached.Version.ToString() };
            m_Status = 1;
        }

        void m_CTouch_InputChange(object sender, InputChangeEventArgs e)
        {
            int index = e.Index;
            if (index == 0)
            {
                m_Press[0] = Convert.ToDouble(e.Value);
            }
            else if (index == 1)
            {
                m_Press[1] = Convert.ToDouble(e.Value);
            }
        }

        void m_CTouch_SensorChange(object sender, SensorChangeEventArgs e)
        {
            double value;
            value = e.Value;
            m_Position = value / 1000 ;
        }

        #endregion Handler Funktions

    } 
}
