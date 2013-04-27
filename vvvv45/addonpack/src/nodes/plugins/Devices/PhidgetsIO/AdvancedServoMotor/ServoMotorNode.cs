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
	[PluginInfo(Name = "ServoMotor",
	            Category = "Devices",
                Version = "Phidget",
	            Help = "Wrapper for the Phidget Servo Motor Controllers",
	            Tags = "Controller",
                Author = "Phlegma",
                AutoEvaluate=true
)]
	#endregion PluginInfo

    public class ServoMotorNode : IPluginEvaluate
	{
		#region fields & pins
        
        

        //Input 
        [Input("Engaged", DefaultValue = 0)]
        IDiffSpread<bool> FEngagedIn;

        [Input("Positon", DefaultValue = 0, MinValue=0, MaxValue=1)]
        IDiffSpread<double> FPositonIn;

        [Input("SpeedRamping", DefaultValue = 0)]
        IDiffSpread<bool> FSpeedRampingIn;

        [Input("Acceleration", DefaultValue = 0, MinValue=0,MaxValue=1)]
        IDiffSpread<double> FAcclerationIn;

        [Input("Velocity Limit", DefaultValue = 1000, MinValue=0, MaxValue=1)]
        IDiffSpread<double> FVelocityLimitIn;

        [Input("Serial", DefaultValue = 0, IsSingle = true, AsInt=true, MinValue=0,MaxValue=int.MaxValue)]
        IDiffSpread<int> FSerial;

        [Input("ServoType")]
        IDiffSpread<Phidgets.ServoServo.ServoType> FServoTypeIn;

        [Input("Enable User Values")]
        IDiffSpread<bool> FEnableCustomValues;

        [Input("MinUs", Visibility = PinVisibility.OnlyInspector, DefaultValue = 500, MinValue = 0)]
        IDiffSpread<double> FMinUs;

        [Input("MaxUs", Visibility = PinVisibility.OnlyInspector, DefaultValue = 2500, MinValue = 0)]
        IDiffSpread<double> FMaxUs;

        [Input("Degrees", Visibility = PinVisibility.OnlyInspector, DefaultValue = 180, MinValue = 0)]
        IDiffSpread<double> FDegrees;

        [Input("VelocityMax", Visibility = PinVisibility.OnlyInspector, DefaultValue = 5000, MinValue=0)]
        IDiffSpread<double> FVelocityMax;

        //Output
        [Output("Attached")]
        ISpread<bool> FAttached;

        [Output("Count", DefaultValue = 0)]
        ISpread<int> FCountOut;

        [Output("Power Consumption", DefaultValue = 0)]
        ISpread<double> FPowerConsumptionOut;

        [Output("Stopped", DefaultValue = 0)]
        ISpread<bool> FStoppedOut;

        [Output("Velocity", DefaultValue = 0)]
        ISpread<double> FVelocityOut;

        [Output("Position", DefaultValue = 0)]
        ISpread<double> FPositionOut;

        [Output("ServoType")]
        ISpread<string> FServoTypeOut;


        //Logger
		[Import()]
		ILogger FLogger;

        //priavte fields
        WrapperServoMotor FServo;
        private bool FInit = true;
		#endregion fields & piins


        //called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
            try
            {
                if (FSerial.IsChanged)
                {
                    if (FServo != null)
                    {
						FServo.Close();
                        FServo = null;
                    }
                    FServo = new WrapperServoMotor(FSerial[0]);
                    FInit = true;
                }

                if (FServo.Attached)
                {

                    bool Changed = FServo.Changed;

                    for (int i = 0; i < SpreadMax; i++)
                    {

                        if (i < FServo.Count && FEngagedIn[i])
                        {
                            //input
                            if (!FEnableCustomValues[i])
                            {
                                if (FInit || FEnableCustomValues.IsChanged || FServoTypeIn.IsChanged || FEngagedIn.IsChanged)
                                    FServo.SetType(i, FServoTypeIn[i]);              
                            }
                            else
                            {
                                if (FInit || FEnableCustomValues.IsChanged || FMinUs.IsChanged || FMaxUs.IsChanged || FDegrees.IsChanged || FVelocityMax.IsChanged)
                                    FServo.SetServoparameter(i, FMinUs[i], FMaxUs[i], FDegrees[i], FVelocityMax[i]);
                            }

                            if (FInit || FEngagedIn.IsChanged)   
                                FServo.SetEngaged(i, FEngagedIn[i]);
                

                            if (FInit || FPositonIn.IsChanged)
                            {
                                //TODO check the rotation of the motor.
                                double Position = VMath.Map(FPositonIn[i], 0, 1, FServo.GetPositionMin(i), Math.Round(FServo.GetPositionMax(i), 4, MidpointRounding.ToEven) - 0.0001, TMapMode.Float);
                                FServo.SetPosition(i, Position);
                            }

                            if (FInit || FSpeedRampingIn.IsChanged || FEngagedIn.IsChanged)
                                FServo.SetSpeedRamping(i, FSpeedRampingIn[i]);
                            

                            if (FInit || FAcclerationIn.IsChanged || FEngagedIn.IsChanged)
                            {
                                double Acceleration = VMath.Map(FAcclerationIn[i], 0, 1, FServo.GetAccelerationMin(i), Math.Round(FServo.GetAccelerationMax(i), 4, MidpointRounding.ToEven), TMapMode.Float);
                                FServo.SetAcceleration(i, Acceleration);
                            }

                            if (FInit || FVelocityLimitIn.IsChanged || FEngagedIn.IsChanged)
                            {
                                double Velocity = VMath.Map(FVelocityLimitIn[i], 0, 1, FServo.GetVelocityMin(i), Math.Round(FServo.GetVelocityMax(i), 4, MidpointRounding.ToEven), TMapMode.Float);
                                FServo.SetVelocityLimit(i, Velocity);
                            }

							
							FPowerConsumptionOut.SliceCount = FServo.Count;
							FStoppedOut.SliceCount = FServo.Count;
							FServoTypeOut.SliceCount = FServo.Count;
							FCountOut[0] = FServo.Count;

                            //output
                            FPowerConsumptionOut[i] = FServo.GetCurrent(i);
                            FStoppedOut[i] = FServo.GetStopped(i);
                            FServoTypeOut[i] = FServo.GetServoType(i).ToString();
                        }

                            //general
                        if (Changed)
                        {
							FPositionOut.SliceCount = FServo.Count;
							FVelocityOut.SliceCount = FServo.Count;
							
                            //Position
                            FPositionOut[i] = VMath.Map(FServo.GetPosition(i), FServo.GetPositionMin(i), FServo.GetPositionMax(i), 0, 1, TMapMode.Float);

                            //Velocity
                            FVelocityOut[i] = VMath.Map(FServo.GetVelocity(i), FServo.GetVelocityMin(i), FServo.GetVelocityMax(i), 0, 1, TMapMode.Float);
                        }
                        
                        else if (i >= FServo.Count)
                            FLogger.Log(LogType.Warning, "The {0} with Serial: {1} provides only {2} motors.", FServo.FInfo.Name, FServo.FInfo.SerialNumber, FServo.Count);
                    }

                    FInit = false;
                }

                FAttached[0] = FServo.Attached;

                List<PhidgetException> Exceptions = FServo.Errors;
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
