#region usings
using System;
using System.ComponentModel.Composition;
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
	[PluginInfo(Name = "Stepper Unipolar",
	            Category = "Devices",
                Version = "Phidget",
                Help = "Wrapper for the Phidget Stepper Unipolar Motors",
	            Tags = "Phidget , Device, Controller, Stepper, Motor",
                Author = "Phlegma")]
	#endregion PluginInfo

    public class StepperUnipolar4MotorsNode : IPluginEvaluate
	{
		#region fields & pins
        [Input("Engaged", DefaultValue = 0)]
        IDiffSpread<bool> FEngagedIn;

        [Input("Position", DefaultValue = 0)]
        IDiffSpread<double> FPositionIn;

        [Input("Steps/Cycle", DefaultValue = 400)]
        IDiffSpread<int> FStepsCycle;

        [Input("Velocity Limit", DefaultValue=0.5)]
        IDiffSpread<double> FVelocityIn;

        [Input("Acceleration", DefaultValue = 0.5, MinValue=0, MaxValue=1)]
        IDiffSpread<double> FAcceleration;

        [Input("Serial", DefaultValue = 0, IsSingle = true, AsInt = true, MinValue = 0, MaxValue = int.MaxValue)]
        IDiffSpread<int> FSerial;

        //Output
        [Output("Attached")]
        ISpread<bool> FAttached;

        [Output("Count", DefaultValue = 0)]
        ISpread<int> FCountOut;

        [Output("Stopped", DefaultValue = 0)]
        ISpread<bool> FStoppedOut;

        [Output("Velocity", DefaultValue = 0)]
        ISpread<double> FVelocityOut;

        [Output("Position", DefaultValue = 0)]
        ISpread<double> FPositionOut;

        [Output("Current Position", DefaultValue = 0, Visibility = PinVisibility.OnlyInspector, AsInt = true)]
        ISpread<double> FCurrentPosition;

        [Output("Target Position", DefaultValue = 0, Visibility = PinVisibility.OnlyInspector, AsInt=true)]
        ISpread<double> FTargetPosition;

        [Output("Position Maximum", Visibility = PinVisibility.OnlyInspector, AsInt=true)]
        ISpread<double> FPositionMax;

        [Output("Position Minimum",Visibility=PinVisibility.OnlyInspector, AsInt=true)]
        ISpread<double> FPositionMin;

		[Import()]
		ILogger FLogger;

        WrapperStepperUnipolar4Motors FStepper;
        bool FInit = true;
		#endregion fields & pins


 
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
            try
            {
                if (FSerial.IsChanged)
                {
                    if (FStepper != null)
                    {
                        FStepper.Close();
                        FStepper = null;
                    }
                    FStepper = new WrapperStepperUnipolar4Motors(FSerial[0]);
                    FInit = true;
                }

                if (FStepper.Attached)
                {
                    bool[] ChangedIndex = FStepper.ChangedIndex;

                    if (FInit)
                    {
                        FPositionOut.SliceCount = FStepper.Count;
                        FStoppedOut.SliceCount = FStepper.Count;
                        FVelocityOut.SliceCount = FStepper.Count;
                        FPositionMin.SliceCount = FStepper.Count;
                        FPositionMax.SliceCount = FStepper.Count;
                        FTargetPosition.SliceCount = FStepper.Count;
                        FCurrentPosition.SliceCount = FStepper.Count;
                        FCountOut[0] = FStepper.Count;

                        for (int i = 0; i < FStepper.Count; i++)
                        {
                            FPositionMax[i] = Convert.ToDouble(FStepper.GetMotorPositionMax(i));
                            FPositionMin[i] = Convert.ToDouble(FStepper.GetMotorPositionMin(i));
                        }
                    }


                    bool EngagedChanged = false;

                    for (int i = 0; i < FStepper.Count; i++)
                    {
                        if (FEngagedIn.IsChanged || FInit)
                        {
                            FStepper.SetEngaged(i,FEngagedIn[i]);
                            EngagedChanged = true;
                        }

                        if (FStepper.GetEngaged(i))
                        {
                            if (FPositionIn.IsChanged || FInit || EngagedChanged)
                            {
                                double Position = VMath.Map(FPositionIn[i], 0, 1, 0, (double)FStepsCycle[i], TMapMode.Float);
                                FStepper.SetTargetMotorposition(i, Convert.ToInt64(Position));
                            }

                            if (FVelocityIn.IsChanged || FAcceleration.IsChanged || FInit || EngagedChanged)
                            {
                                double Velocity = VMath.Map(FVelocityIn[i], 0, 1, FStepper.GetVelocityMin(i), FStepper.GetVelocityMax(i), TMapMode.Float);
                                FStepper.SetVelocityLimit(i, Velocity);

                                double Acceleration = VMath.Map(FAcceleration[i], 0, 1, FStepper.GetAccelerationMin(i), FStepper.GetAccelerationMax(i), TMapMode.Float);
                                FStepper.SetAcceleration(i, Acceleration);
                            }

                            //if (ChangedIndex[i])
                            //{
                                FPositionOut[i] = VMath.Map(Convert.ToDouble(FStepper.GetCurrentMotorPosition(i)), 0, (double)FStepsCycle[i],0,1, TMapMode.Float);
                                FVelocityOut[i] = VMath.Map(Convert.ToDouble(FStepper.GetVelocity(i)),FStepper.GetVelocityMin(i),FStepper.GetVelocityMax(i),0,1, TMapMode.Float);
                                FCurrentPosition[i] = Convert.ToDouble(FStepper.GetCurrentMotorPosition(i));
                                FTargetPosition[i] = Convert.ToDouble(FStepper.GetTargetMotorsPosition(i));

                            //}
                        }

                        FStoppedOut[i] = FStepper.GetStopped(i);
                        EngagedChanged = false;
                    }

                    FInit = false;
                }

                FAttached[0] = FStepper.Attached;

                List<PhidgetException> Exceptions = FStepper.Errors;
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
