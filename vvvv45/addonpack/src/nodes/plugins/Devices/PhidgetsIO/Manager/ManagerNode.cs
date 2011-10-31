#region usings
using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Collections.Generic;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using VVVV.Core.Logging;
using Phidgets;

#endregion usings

namespace VVVV.Nodes
{
    #region PluginInfo
    [PluginInfo(Name = "Manager",
                Category = "Devices",
                Version = "Phidget",
                Help = "Wrapper for the Phidget Manager",
                Tags = "Manager",
                Author = "Phlegma",
                AutoEvaluate = true
)]
    #endregion PluginInfo

    public class ManagerNode : IPluginEvaluate, IDisposable
    {
        #region fields & pins


        //Input
        [Input("Update", IsBang=true, IsSingle=true)]
        IDiffSpread<bool> FUpdate;

        //Output
        [Output("Device Name")]
        ISpread<string> FName;

        [Output("Label")]
        ISpread<string> FLabel;

        [Output("ID")]
        ISpread<string> FId;

        [Output("Serial")]
        ISpread<string> FSerial;

        [Output("Type")]
        ISpread<string> FType;

        [Output("Version")]
        ISpread<string> FVersion;


        //Logger
        [Import()]
        ILogger FLogger;

        //private Fields
        WrapperManager FManager = new WrapperManager();
        private bool disposed;
        #endregion fields & piins


        //called when data for any output pin is requested
        public void Evaluate(int SpreadMax)
        {
            try
            {
                if (FUpdate.IsChanged)
                {
                    FName.SliceCount = FManager.FPhidget.Devices.Count;
                    FLabel.SliceCount = FManager.FPhidget.Devices.Count;
                    FId.SliceCount = FManager.FPhidget.Devices.Count;
                    FSerial.SliceCount = FManager.FPhidget.Devices.Count;
                    FType.SliceCount = FManager.FPhidget.Devices.Count;
                    FVersion.SliceCount = FManager.FPhidget.Devices.Count;


                    for (int i = 0; i < FManager.FPhidget.Devices.Count; i++)
                    {
                        FName[i] = FManager.FPhidget.Devices[i].Name;
                        FLabel[i] = FManager.FPhidget.Devices[i].Label;
                        FId[i] = FManager.FPhidget.Devices[i].ID.ToString();
                        FSerial[i] = FManager.FPhidget.Devices[i].SerialNumber.ToString();
                        FType[i] = FManager.FPhidget.Devices[i].Type;
                        FVersion[i] = FManager.FPhidget.Devices[i].Version.ToString();
                    }
                }

                List<PhidgetException> Exceptions = FManager.Errors;
                if (Exceptions != null)
                {
                    foreach (Exception e in Exceptions)
                        FLogger.Log(e);
                }
            }
            catch (PhidgetException ex)
            {
                FLogger.Log(ex);
            }
        }


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

                }
                // Release unmanaged resources. If disposing is false, 
                // only the following code is executed.

                // Note that this is not thread safe.
                // Another thread could start disposing the object
                // after the managed resources are disposed,
                // but before the disposed flag is set to true.
                // If thread safety is necessary, it must be
                // implemented by the client.
                if (FManager != null)
                {
                    FManager.Close();
                    FManager = null;
                }
            }
            disposed = true;
        }

        // Use C# destructor syntax for finalization code.
        // This destructor will run only if the Dispose method 
        // does not get called.
        // It gives your base class the opportunity to finalize.
        // Do not provide destructors in types derived from this class.
        ~ManagerNode()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }

        #endregion
    }
}
