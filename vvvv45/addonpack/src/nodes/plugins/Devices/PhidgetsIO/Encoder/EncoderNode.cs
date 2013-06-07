#region usings
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;


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
                Tags = "Controller, HighSpeed",
                Author = "Phlegma",
                AutoEvaluate = true
)]
    #endregion PluginInfo

    public class EncoderNode : IPluginEvaluate
    {
        #region fields & pins


        
        //Input 
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

                    if (FSet.IsChanged)
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


                    if (FEncoder.Changed)
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


       
    }
}
