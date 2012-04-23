using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.XPath;

using Phidgets;
using Phidgets.Events;

namespace VVVV.Nodes
{
    public class WrapperManager
    {

        #region Field Declaration

        public Manager FPhidget;
        public DeviceInfo FInfo;
        public List<PhidgetException> FPhidgetErrors = new List<PhidgetException>();
        private bool FChanged;
        private bool disposed = false;


        public struct DeviceInfo
        {
            public string Name;
            public double SerialNumber;
            public double Version;
            public Phidget.PhidgetID PhidgetId;
        }

        #endregion Field Declaration


        public bool Changed
        {
            get 
            {
                bool temp = FChanged;
                FChanged = false;
                return FChanged; 
            }
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

        public WrapperManager()
        {
            FPhidget = new Manager();
            AddEventHandler();
            FPhidget.open();
        }



        public void Close()
        {
            Dispose();
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
            Manager attached = (Manager)sender;
            FChanged = true;

        }

        void DetachHandler(object sender, DetachEventArgs e)
        {
            Manager attached = (Manager)sender;
            FChanged = true;
        }

        void ErrorHandler(object sender, ErrorEventArgs e)
        {
            FPhidgetErrors.Add(e.exception);
        }

        #endregion Attach Detach Event Handler


        #region IDisposable Members

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
            if (!this.disposed)
            {
                // If disposing equals true, dispose all managed 
                // and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.
                    FPhidget.close();
                }
                // Release unmanaged resources. If disposing is false, 
                // only the following code is executed.

                // Note that this is not thread safe.
                // Another thread could start disposing the object
                // after the managed resources are disposed,
                // but before the disposed flag is set to true.
                // If thread safety is necessary, it must be
                // implemented by the client.
                
            }
            disposed = true;
        }

        // Use C# destructor syntax for finalization code.
        // This destructor will run only if the Dispose method 
        // does not get called.
        // It gives your base class the opportunity to finalize.
        // Do not provide destructors in types derived from this class.
        ~WrapperManager()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }


        #endregion
    }
}
