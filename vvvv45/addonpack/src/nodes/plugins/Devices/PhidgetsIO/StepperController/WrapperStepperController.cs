using System;
using System.Collections.Generic;
using System.Text;

using Phidgets;
using Phidgets.Events;

namespace VVVV.Nodes
{
    class WrapperStepperController:Phidgets<Stepper>,IPhidgetsWrapper
    {
        bool FChanged = false;
        bool[] FChangedIndex;

        public WrapperStepperController(int SerialNumber)
            : base(SerialNumber)
        {
            FChangedIndex  = new bool[FPhidget.steppers.Count];
        }

        public WrapperStepperController()
            : base()
        {
            FChangedIndex = new bool[FPhidget.steppers.Count];
        }


        //Setter
        public void SetCurrent(int Index, double Value)
        {
            FPhidget.steppers[Index].CurrentLimit = Value;
        }
        
        public void SetAcceleration(int Index, double Value)
        {
            FPhidget.steppers[Index].Acceleration = Value;
        }

        public void SetVelocityLimit(int Index, double Value)
        {
            FPhidget.steppers[Index].VelocityLimit = Value;
        }

        public void SetCurrentMotorposition(int Index, Int64 Value)
        {
            FPhidget.steppers[Index].CurrentPosition = Value;
        }

        public void SetTargetMotorposition(int Index, Int64 Value)
        {
            FPhidget.steppers[Index].TargetPosition = Value;
        }

        public void SetEngaged(int Index, bool Value)
        {
            FPhidget.steppers[Index].Engaged = Value;
        }




        //getter
        public double GetCurrent(int Index)
        {
            return FPhidget.steppers[Index].CurrentLimit;
        }

        public double GetCurrentMin(int Index)
        {
            return FPhidget.steppers[Index].CurrentMin;
        }

        public double GetCurrentMax(int Index)
        {
            return FPhidget.steppers[Index].CurrentMax;
        }
        
        public double GetAcceleration(int Index)
        {
            return FPhidget.steppers[Index].Acceleration;
        }

        public double GetAccelerationMin(int Index)
        {
            return FPhidget.steppers[Index].AccelerationMin;
        }

        public double GetAccelerationMax(int Index)
        {
            return FPhidget.steppers[Index].AccelerationMax;
        }

        public double GetVelocity(int Index)
        {
            return FPhidget.steppers[Index].Velocity;
        }

        public double GetVelocityLimit(int Index)
        {
            return FPhidget.steppers[Index].VelocityLimit;
        }

        public double GetVelocityMax(int Index)
        {
            return FPhidget.steppers[Index].VelocityMax;
        }

        public double GetVelocityMin(int Index)
        {
            return FPhidget.steppers[Index].VelocityMin;
        }

        public Int64 GetCurrentMotorPosition(int Index)
        {
            return FPhidget.steppers[Index].CurrentPosition;
        }

        public Int64 GetTargetMotorsPosition(int Index)
        {
            return FPhidget.steppers[Index].TargetPosition;
        }

        public Int64 GetMotorPositionMin(int Index)
        {
            return FPhidget.steppers[Index].PositionMin;
        }

        public Int64 GetMotorPositionMax(int Index)
        {
            return FPhidget.steppers[Index].PositionMax;
        }

        public bool GetEngaged(int Index)
        {
            return FPhidget.steppers[Index].Engaged;
        }

        public bool GetStopped(int Index)
        {
            return FPhidget.steppers[Index].Stopped;
        }


        public override void AddChangedHandler()
        {
            FPhidget.VelocityChange += new VelocityChangeEventHandler(FPhidget_VelocityChange);
            FPhidget.PositionChange += new StepperPositionChangeEventHandler(FPhidget_PositionChange);
        }

        void  FPhidget_PositionChange(object sender, StepperPositionChangeEventArgs e)
        {
            FChanged = true;
        }

        void  FPhidget_VelocityChange(object sender, VelocityChangeEventArgs e)
        {
            FChanged = true;
        }

        public override void RemoveChangedHandler()
        {
            
        }

        public int Count
        {
            get { return FPhidget.steppers.Count; }
        }

        public bool Changed
        {
            get
            {
                bool temp = FChanged;
                FChanged = false;
                return temp;
            }
        }

        public bool[] ChangedIndex
        {
            get
            {
                bool[] temp = FChangedIndex;
                FChangedIndex = null;
                FChangedIndex = new bool[FPhidget.steppers.Count];
                return FChangedIndex;
            }
        }
        
    }
}
