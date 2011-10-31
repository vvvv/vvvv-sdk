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
    [PluginInfo(Name = "Accelerometer",
                Category = "Devices",
                Version = "Phidget",
                Help = "Wrapper for the Phidget Acceleromter",
                Tags = "Controller, LED",
                Author = "Phlegma",
                AutoEvaluate = true
)]
    #endregion PluginInfo

    public class AccelerometerNode : IPluginEvaluate, IDisposable
    {
        #region fields & pins


        
        //Input 
        [Input("Serial", DefaultValue = 0, IsSingle = true, AsInt = true, MinValue = 0, MaxValue = int.MaxValue)]
        IDiffSpread<int> FSerial;

        //Output
        [Output("Attached")]
        ISpread<bool> FAttached;

        [Output("Acceleration")]
        ISpread<Vector3D> FAcceleration;

        [Output("Acceleration Minimum")]
        ISpread<Vector3D> FAccelerationMin;

        [Output("Acceleration Maximum")]
        ISpread<Vector3D> FAccelerationMax;


        //Logger
        [Import()]
        ILogger FLogger;

        //private Fields
        WrapperAccelerometer FAccelerometer;
        private bool disposed;
        #endregion fields & piins


        //called when data for any output pin is requested
        public void Evaluate(int SpreadMax)
        {
            try
            {
                if (FSerial.IsChanged)
                {
                    if (FAccelerometer != null)
                    {
                        FAccelerometer.Close();
                        FAccelerometer = null;
                    }
                    FAccelerometer = new WrapperAccelerometer(FSerial[0]);
                }

                if (FAccelerometer.Attached)
                {
                    if (FAccelerometer.Changed)
                    {
                        FAcceleration[0] = new Vector3D(FAccelerometer.GetAccelerationCollection()[0].Acceleration, FAccelerometer.GetAccelerationCollection()[1].Acceleration, FAccelerometer.GetAccelerationCollection()[2].Acceleration);
                        FAccelerationMin[0] = new Vector3D(FAccelerometer.GetAccelerationCollection()[0].AccelerationMin, FAccelerometer.GetAccelerationCollection()[1].AccelerationMin, FAccelerometer.GetAccelerationCollection()[2].AccelerationMin);
                        FAccelerationMax[0] = new Vector3D(FAccelerometer.GetAccelerationCollection()[0].AccelerationMax, FAccelerometer.GetAccelerationCollection()[1].AccelerationMax, FAccelerometer.GetAccelerationCollection()[2].AccelerationMax);
                    }
                }

                FAttached[0] = FAccelerometer.Attached;

                List<PhidgetException> Exceptions = FAccelerometer.Errors;
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
                if (FAccelerometer != null)
                {
                    FAccelerometer.Close();
                    FAccelerometer = null;
                }
            }
            disposed = true;
        }

        // Use C# destructor syntax for finalization code.
        // This destructor will run only if the Dispose method 
        // does not get called.
        // It gives your base class the opportunity to finalize.
        // Do not provide destructors in types derived from this class.
        ~AccelerometerNode()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }

        #endregion
    }
}
