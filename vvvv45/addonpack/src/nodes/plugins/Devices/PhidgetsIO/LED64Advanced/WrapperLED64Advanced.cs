using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Phidgets;
using Phidgets.Events;


namespace VVVV.Nodes
{
    class WrapperLED64Advanced : Phidgets<LED>, IPhidgetsWrapper
    {

        bool FChanged = false;

        #region constructor

        public WrapperLED64Advanced()
            : base()
        {

        }

        public WrapperLED64Advanced(int SerialNumber)
            : base(SerialNumber)
        {

        }

        #endregion constructor


        #region Setter fuctions

        public void SetCurrentLimit(LED.LEDCurrentLimit CurrentLimit)
        {
            if (CurrentLimit != LED.LEDCurrentLimit.INVALID)
            {
                FPhidget.CurrentLimit = CurrentLimit;
            }
        }

        public void SetVoltage(LED.LEDVoltage Voltage)
        {
            if (Voltage != LED.LEDVoltage.INVALID)
            {
                FPhidget.Voltage = Voltage;
            }
        }

        public void SetDiscreteLED(int Index, int value)
        {
            FPhidget.leds[Index] = value;
        }


        #endregion


        #region getter functions



        #endregion getter functions

        public int GetDiscreteLED(int Index)
        {
            return FPhidget.leds[Index];
        }

        public LED.LEDVoltage GetVoltage()
        {
            return FPhidget.Voltage;
        }

        public LED.LEDCurrentLimit GetCurrentLimit()
        {
            return FPhidget.CurrentLimit;
        }



        public override void AddChangedHandler()
        {

        }

        public override void RemoveChangedHandler()
        {

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
            get { return FPhidget.leds.Count; }
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


    }
}
