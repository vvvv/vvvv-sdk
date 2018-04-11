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
                Help = "Controls the Phidget LED 64 Advanced board",
                Tags = "controller",
                Author = "Phlegma",
                AutoEvaluate = true
)]
    #endregion PluginInfo

    public class LED64AdvancedNode : IPluginEvaluate
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

    }
}
