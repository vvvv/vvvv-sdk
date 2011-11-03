using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Phidgets;
using Phidgets.Events;


namespace VVVV.Nodes
{
    class WrapperServoMotor:Phidgets<AdvancedServo>,IPhidgetsWrapper
    {

        bool FChanged = false;

        #region constructor

        public WrapperServoMotor():base()
        {
            
        }

        public WrapperServoMotor(int SerialNumber) : base(SerialNumber) 
        {
            
        }

        #endregion constructor


        #region Setter fuctions

        public void SetAcceleration(int Index, double Value)
        {
            if (Index < FPhidget.servos.Count)
                FPhidget.servos[Index].Acceleration = Value;
        }

        public void SetVelocityLimit(int Index, double Value)
        {
            if (Index < FPhidget.servos.Count)
                FPhidget.servos[Index].VelocityLimit = Value;
        }

        public void SetPosition(int Index, double Value)
        {
            if (Index < FPhidget.servos.Count)
                FPhidget.servos[Index].Position = Value;
        }

        public void SetPositionMax(int Index, double Value)
        {
            if (Index < FPhidget.servos.Count)
                FPhidget.servos[Index].PositionMax = Value;
        }
        public void SetPositionMin(int Index, double Value)
        {
            if (Index < FPhidget.servos.Count)
                FPhidget.servos[Index].PositionMin = Value;
        }

        public void SetSpeedRamping(int Index, bool Value)
        {
            if (Index < FPhidget.servos.Count)
                FPhidget.servos[Index].SpeedRamping = Value;
        }

        public void SetEngaged(int Index, bool Value)
        {
            if (Index < FPhidget.servos.Count)
                FPhidget.servos[Index].Engaged = Value;
        }

        public void SetType(int Index,Phidgets.ServoServo.ServoType EnumEntry)
        {
            FPhidget.servos[Index].Type = EnumEntry;
        }

        public void SetServoparameter(int ServoIndex, double MinUS, double MaxUs, double Degress, double VelocityMax)
        {
            FPhidget.servos[ServoIndex].setServoParameters(MinUS, MaxUs, Degress, VelocityMax);
        }
        

        #endregion


        #region getter functions

        public double GetAcceleration(int Index)
        {
            return FPhidget.servos[Index].Acceleration;
        }

        public double GetAccelerationMax(int Index)
        {
            return FPhidget.servos[Index].AccelerationMax;
        }

        public double GetAccelerationMin(int Index)
        {
            return FPhidget.servos[Index].AccelerationMin;
        }

        public double GetVelocity(int Index)
        {
            return FPhidget.servos[Index].Velocity;
        }

        public double GetVelocityMax(int Index)
        {
            return FPhidget.servos[Index].VelocityMax;
        }

        public double GetVelocityMin(int Index)
        {
            return FPhidget.servos[Index].VelocityMin;
        }

        public double GetVelocityLimit(int Index)
        {
            return FPhidget.servos[Index].VelocityLimit;
        }

        public double GetPosition(int Index)
        {
            return FPhidget.servos[Index].Position;
        }

        public double GetPositionMax(int Index)
        {
            return FPhidget.servos[Index].PositionMax;
        }

        public double GetPositionMin(int Index)
        {
            return FPhidget.servos[Index].PositionMin;
        }

        public double GetCurrent(int Index)
        {
            return FPhidget.servos[Index].Current;
        }

        public bool GetSpeedRamping(int Index)
        {
            return FPhidget.servos[Index].SpeedRamping;
        }

        public bool GetEngaged(int Index)
        {
            return FPhidget.servos[Index].Engaged;
        }

        public bool GetStopped(int Index)
        {
            return FPhidget.servos[Index].Stopped;
        }

        public Enum GetServoType(int Index)
        {

            return FPhidget.servos[Index].Type;
        }

        #endregion getter functions


        #region IPhidgetsWrapper Members

        public override void AddChangedHandler()
        {
            FPhidget.PositionChange += new PositionChangeEventHandler(PositionChange);
            FPhidget.VelocityChange += new VelocityChangeEventHandler(VelocityChange);
        }

        public override void RemoveChangedHandler()
        {
            FPhidget.PositionChange -= new PositionChangeEventHandler(PositionChange);
            FPhidget.VelocityChange -= new VelocityChangeEventHandler(VelocityChange);
        }


        void VelocityChange(object sender, VelocityChangeEventArgs e)
        {
            FChanged = true;
        }

        void PositionChange(object sender, PositionChangeEventArgs e)
        {
            FChanged = true;
        }

        public int Count
        {
            get { return FPhidget.servos.Count; }
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

        #endregion
    }
}
