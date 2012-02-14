#region licence/info

//////project name
//Phidget Manager

//////description
//VVVV Plug In for the Phidget Interfaces. http://www.phidgets.com/
//shows you all phidgets which are connected to the computer with name,serial number and so on 

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

using System;
using System.Collections.Generic;
using System.Text;
using Phidgets;
using Phidgets.Events;

namespace VVVV.Nodes
{
    class ManagePhidgets
    {

        private Manager m_PhidgetManager;
        private string m_LibaryVersion;
        public struct DeviceInfo
        {
            public string Name;
            public double SerialNumber;
            public double Version;
            public object Device;
        }
        List<DeviceInfo> m_Devices = new List<DeviceInfo>();

        #region field declaration

        public string LibaryVersion
        {
            get
            {
                return m_LibaryVersion;
            }
            set
            {
                m_LibaryVersion = value;
            }
        }

        public List<DeviceInfo> Devices
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

        #endregion field declaration

        #region constructor

        public ManagePhidgets()
        {
            m_PhidgetManager = new Manager();
            
            m_LibaryVersion = Phidget.LibraryVersion;
        }



        #endregion constructor

        #region EventHandler
        // This Funktions are called from the Phidget Devices when they pluged in/out 

         void m_PhidgetManager_Detach(object sender, DetachEventArgs e)
        {
            DeviceInfo Infos = new DeviceInfo();
            Infos.Name = e.Device.Name;
            Infos.SerialNumber = e.Device.SerialNumber;
            Infos.Version = e.Device.Version;

            m_Devices.Remove(Infos);
             //m_Devices.Remove(e.Device.Name);
            //m_Devices.Remove(e.Device.SerialNumber);
            //m_Devices.Remove = (e.Device.Version);
        }

        void m_PhidgetManager_Attach(object sender, AttachEventArgs e)
        {
             

             DeviceInfo Infos = new DeviceInfo();
             Infos.Name = e.Device.Name;
             Infos.SerialNumber= e.Device.SerialNumber;
             Infos.Version = e.Device.Version;

             InterfaceKit m_kit = new InterfaceKit();
             Infos.Device = m_kit;

             m_Devices.Add(Infos);
        }

        #endregion EventHandler

        #region public Funktion

        // This Funktions are called by the Plugin Template

        public void Enable()
        {
            m_Devices.Clear();
            m_PhidgetManager.Attach += new AttachEventHandler(m_PhidgetManager_Attach);
            m_PhidgetManager.Detach += new DetachEventHandler(m_PhidgetManager_Detach);
            m_PhidgetManager.open();    
        }

        public void Disable()
        {
            m_PhidgetManager.Attach -= new AttachEventHandler(m_PhidgetManager_Attach);
            m_PhidgetManager.Detach -= new DetachEventHandler(m_PhidgetManager_Detach);
            m_PhidgetManager.close();
            m_Devices.Clear();
        }

        public static void Hallo(){
        }

        #endregion public Funktion
    }
}
