#region usings
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Threading;

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
    [PluginInfo(Name = "Encoder",
                Category = "Devices",
                Version = "Phidget",
                Help = "Wrapper for the Phidget Encoders",
                Tags = "Controller, Encoder, HighSpeed",
                Author = "Phlegma",
                AutoEvaluate = true
)]
    #endregion PluginInfo

    public class EncoderNode : IPluginEvaluate, IDisposable
    {
        #region fields & pins


        
        //Input 
        [Input("Position")]
        ISpread<bool> Enable;

        [Input("Position")]
        ISpread<int> FPositionIn;

        [Input("Set")]
        IDiffSpread<bool> FSet;

        [Input("Serial", DefaultValue = 0, IsSingle = true, AsInt = true, MinValue = 0, MaxValue = int.MaxValue)]
        IDiffSpread<int> FSerial;

        [Input("Enable")]
        IDiffSpread<bool> FEnable;

        //Output
        [Output("Attached")]
        ISpread<bool> FAttached;

        [Output("Position")]
        ISpread<int> FPositionOut;

        [Output("IndexPosition")]
        ISpread<int> FIndexPositionOut;

        [Output("Digital")]
        ISpread<bool> FDigitalOut;

        //Logger
        [Import()]
        ILogger FLogger;

        //private Fields
        WrapperEncoder FEncoder;
        private int FEncoderCount = 0;
        private bool disposed;
        private bool FInit = true;
        private bool FEnableFlag = true;
        #endregion fields & piins


        //called when data for any output pin is requested
        public void Evaluate(int SpreadMax)
        {
            try
            {
                if (FSerial.IsChanged || FInit)
                {
                    if (FEncoder != null)
                    {
                        FEncoder.Close();
                        FEncoder = null;
                    }

                    FEncoder = new WrapperEncoder(FSerial[0]);
                    FInit = false;
                }



                if (FEncoder.Attached && FInit == false)
                {
                    FPositionOut.SliceCount = FEncoderCount = FEncoder.GetEncoderCount();
                    FDigitalOut.SliceCount = FIndexPositionOut.SliceCount = FEncoder.GetInputCount();

                    if (FEnable.IsChanged || FEnableFlag)
                    {
                        for (int i = 0; i < FEncoderCount; i++)
                        {
                            FEncoder.SetEnable(i, FEnable[i]);
                        }

                        FEnableFlag = false;
                    }

                    if (FSet.IsChanged || FInit)
                    {
                        for (int i = 0; i < SpreadMax; i++)
                        {

                            if (i < FEncoder.Count && FSet[i] == true)
                                FEncoder.SetPosition(i, FPositionIn[i]);
                        }
                    }

                    for (int i = 0; i < FEncoderCount; i++)
                    {
                        FPositionOut[i] = FEncoder.GetPosition(i);
                        FIndexPositionOut[i] = FEncoder.GetIndexPosition(i);
                    }


                    if (FEncoder.Changed || FInit)
                    {
                        for (int i = 0; i < FEncoder.GetInputCount(); i++)
                            FDigitalOut[i] = FEncoder.GetInputState(i);
                    }


                }
                else
                {
                    FEnableFlag = true;
                }


                FAttached[0] = FEncoder.Attached;
                List<PhidgetException> Exceptions = FEncoder.Errors;
                if (Exceptions != null)
                {
                    foreach (Exception e in Exceptions)
                        FLogger.Log(e);
                }
            }
            catch (PhidgetException ex)
            {
                FAttached[0] = false;
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
                if (FEncoder != null)
                {
                    FEncoder.Close();
                    FEncoder = null;
                }
            }
            disposed = true;
        }

        // Use C# destructor syntax for finalization code.
        // This destructor will run only if the Dispose method 
        // does not get called.
        // It gives your base class the opportunity to finalize.
        // Do not provide destructors in types derived from this class.
        ~EncoderNode()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }

        #endregion
    }
}
