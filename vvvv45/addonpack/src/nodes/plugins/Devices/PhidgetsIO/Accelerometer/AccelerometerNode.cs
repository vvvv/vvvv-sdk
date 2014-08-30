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
                Help = "Manages the 1059 - PhidgetAccelerometer 3-Axis",
                Tags = "controller",
                Author = "Phlegma",
                AutoEvaluate = true
)]
    #endregion PluginInfo

    public class AccelerometerNode : IPluginEvaluate
    {
        #region fields & pins


        
        //Input 
        [Input("Serial", DefaultValue = 0, IsSingle = true, AsInt = true, MinValue = 0, MaxValue = int.MaxValue)]
        IDiffSpread<int> FSerial;

        //Output
        [Output("Attached")]
        ISpread<bool> FAttached;

        [Output("Acceleration")]
        ISpread<Vector2D> FAcceleration;

        [Output("Acceleration Minimum")]
        ISpread<Vector2D> FAccelerationMin;

        [Output("Acceleration Maximum")]
        ISpread<Vector2D> FAccelerationMax;


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
                        FAcceleration[0] = new Vector2D(FAccelerometer.GetAccelerationCollection()[0].Acceleration, FAccelerometer.GetAccelerationCollection()[1].Acceleration);
                        FAccelerationMin[0] = new Vector2D(FAccelerometer.GetAccelerationCollection()[0].AccelerationMin, FAccelerometer.GetAccelerationCollection()[1].AccelerationMin);
                        FAccelerationMax[0] = new Vector2D(FAccelerometer.GetAccelerationCollection()[0].AccelerationMax, FAccelerometer.GetAccelerationCollection()[1].AccelerationMax);
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

    }
}
