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
    [PluginInfo(Name = "LED",
                Category = "Devices",
                Version = "Phidget",
                Help = "Wrapper for the Phidget LED 64 Advanced board",
                Tags = "Controller, LED",
                Author = "Phlegma",
                AutoEvaluate = true
)]
    #endregion PluginInfo

    public class LED64AdvancedNode : IPluginEvaluate, IDisposable
    {
        #region fields & pins


        
        //Input 
        [Input("Brightnes", DefaultValue = 0, MinValue = 0, MaxValue = 1)]
        IDiffSpread<double> FBrightness;

        [Input("Voltage", IsSingle = true)]
        IDiffSpread<LED.LEDVoltage> FVoltageIn;

        [Input("Current Limit", IsSingle = true)]
        IDiffSpread<LED.LEDCurrentLimit> FCurrentLimitIn;

        [Input("Serial", DefaultValue = 0, IsSingle = true, AsInt = true, MinValue = 0, MaxValue = int.MaxValue)]
        IDiffSpread<int> FSerial;


        //Output
        [Output("Attached")]
        ISpread<bool> FAttached;

        [Output("Count", DefaultValue = 0)]
        ISpread<int> FCountOut;

        [Output("Voltage")]
        ISpread<string> FVoltageOut;

        [Output("Currnet Limit")]
        ISpread<string> FCurrentLimitOut;


        //Logger
        [Import()]
        ILogger FLogger;

        //private Fields
        WrapperLED64Advanced FLed;
        private bool disposed;
        private bool FInit = true;
        #endregion fields & piins


        //called when data for any output pin is requested
        public void Evaluate(int SpreadMax)
        {
            try
            {
                if (FSerial.IsChanged)
                {
                    if (FLed != null)
                    {
                        FLed.Close();
                        FLed = null;
                    }
                    FLed = new WrapperLED64Advanced(FSerial[0]);
                    FInit = true;
                }

                if (FLed.Attached)
                {
                    if (FVoltageIn.IsChanged || FInit)
                        FLed.SetVoltage(FVoltageIn[0]);

                    if (FCurrentLimitIn.IsChanged || FInit)
                        FLed.SetCurrentLimit(FCurrentLimitIn[0]);

                    if (FBrightness.IsChanged || FInit)
                    {
                        for (int i = 0; i < SpreadMax; i++)
                        {
                            if (i < FLed.Count)
                                FLed.SetDiscreteLED(i, Convert.ToInt16(FBrightness[i] * 100));
                            else if (i >= FLed.Count)
                                FLogger.Log(LogType.Warning, "The {0} with Serial: {1} provides only {2} LEDs.", FLed.FInfo.Name, FLed.FInfo.SerialNumber, FLed.Count);
                        }
                    }

                    FVoltageOut[0] = FLed.GetVoltage().ToString();
                    FCurrentLimitOut[0] = FLed.GetCurrentLimit().ToString();
                    FCountOut[0] = FLed.Count;

                    FInit = false;
                }

                FAttached[0] = FLed.Attached;

                List<PhidgetException> Exceptions = FLed.Errors;
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
                if (FLed != null)
                {
                    FLed.Close();
                    FLed = null;
                }
            }
            disposed = true;
        }

        // Use C# destructor syntax for finalization code.
        // This destructor will run only if the Dispose method 
        // does not get called.
        // It gives your base class the opportunity to finalize.
        // Do not provide destructors in types derived from this class.
        ~LED64AdvancedNode()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }

        #endregion
    }
}
