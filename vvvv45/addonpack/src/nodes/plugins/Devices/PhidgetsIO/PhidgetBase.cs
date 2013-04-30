using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using System.Threading;

using Phidgets;
using Phidgets.Events;

namespace VVVV.Nodes
{
    abstract class Phidgets<T> where T : Phidget, new()
    {

        #region Field Declaration

        public T FPhidget;
        public DeviceInfo FInfo;
        public List<PhidgetException> FPhidgetErrors = new List<PhidgetException>();
        private bool FAttached;
        private bool disposed = false;

           
        public struct DeviceInfo
        {
            public string Name;
            public double SerialNumber;
            public double Version;
            public Phidget.PhidgetID PhidgetId;
        }

        #endregion Field Declaration


        public abstract void AddChangedHandler();
        public abstract void RemoveChangedHandler();

        public bool Attached
        {
            get { return FPhidget.Attached; }
        }


        public List<PhidgetException> Errors
        {
            get
            {
                if (FPhidgetErrors.Count > 0)
                {
                    List<PhidgetException> Temp = new List<PhidgetException>(FPhidgetErrors);
                    FPhidgetErrors.Clear();
                    return Temp;
                }
                else
                    return null;
            }
        }

        #region constructor + Close

        public Phidgets()
        {
            FPhidget = new T();
            AddEventHandler();
            AddChangedHandler();
            FPhidget.open();
        }

        public Phidgets(int SerialNumber)
        {
            FPhidget = new T();
            AddEventHandler();
            AddChangedHandler();
           
            try
            {
                if (SerialNumber > 0)
                {
                    FPhidget.open(SerialNumber);
                }
                else
                {
                    FPhidget.open();
                }               
            }
            catch (PhidgetException ex)
            {
                FPhidgetErrors.Add(ex);
                FPhidget.open();
            }
        }
        
        public void Close()
        {
        	FPhidget.close();
        	//give the hardware some time to dissconected
        	Thread.Sleep(30);
        }

        #endregion constructor + Close


        #region Attach Detach Error Handling

        public void AddEventHandler()
        {
            FPhidget.Attach += new AttachEventHandler(AttachHandler);
            FPhidget.Detach += new DetachEventHandler(DetachHandler);
            FPhidget.Error += new ErrorEventHandler(ErrorHandler);
           
        }

        public void RemoveEventHandler()
        {
            FPhidget.Attach -= new AttachEventHandler(AttachHandler);
            FPhidget.Detach -= new DetachEventHandler(DetachHandler);
            FPhidget.Error -= new ErrorEventHandler(ErrorHandler);
        }

        void AttachHandler(object sender, AttachEventArgs e)
        {
            T attached = (T)sender;


            if (attached.Attached)
            {
                FInfo = new DeviceInfo();
                FInfo.Name = attached.Name;
                FInfo.SerialNumber = attached.SerialNumber;
                FInfo.Version = attached.Version;
                FInfo.PhidgetId = attached.ID;
            }  
        }

        void DetachHandler(object sender, DetachEventArgs e)
        {
            T attached = (T)sender;
            FAttached = false;
        }

        void ErrorHandler(object sender, ErrorEventArgs e)
        {
            FPhidgetErrors.Add(e.exception);
        }
        
        #endregion Attach Detach Event Handler


        

    }
}
