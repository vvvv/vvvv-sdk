using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VVVV.Utils.VMath;

using Phidgets;
using Phidgets.Events;


namespace VVVV.Nodes
{
    class WrapperEncoder : Phidgets<Phidgets.Encoder>, IPhidgetsWrapper
    {

        bool FChanged = false;

        #region constructor

        public WrapperEncoder()
            : base()
        {
                
        }

        public WrapperEncoder(int SerialNumber)
            : base(SerialNumber)
        {
      
        }

        #endregion constructor


        #region Setter fuctions

        public void SetPosition(int Index, int value)
        {
            switch (FPhidget.ID)
            {
                case Phidget.PhidgetID.ENCODER_HS_4ENCODER:
                    FPhidget.encodersWithEnable[Index].Position= value;
                    break;
                default:
                    FPhidget.encoders[Index] = value;
                    break;
            }
        }

        #endregion Setter fucntions


        #region getter functions

        public int GetInputCount()
        {
            return FPhidget.encoders.Count;
        }

        public bool GetInputState(int Index)
        {
            return FPhidget.inputs[Index];
        }

        public int GetEncoderCount()
        {
            switch (FPhidget.ID)
            {
                case Phidget.PhidgetID.ENCODER_HS_4ENCODER:
                    return FPhidget.encodersWithEnable.Count;
                default:
                    return FPhidget.encoders.Count;
            }
        }

        public int GetPosition(int Index)
        {
            switch (FPhidget.ID)
            {
                case Phidget.PhidgetID.ENCODER_HS_4ENCODER:
                    return FPhidget.encodersWithEnable[Index].Position;
                default:
                    return FPhidget.encoders[Index];
            }
        }

        public int GetIndexPosition(int Index)
        {
            switch (FPhidget.ID)
            {
                case Phidget.PhidgetID.ENCODER_HS_4ENCODER:
                    try
                    {
                        return FPhidget.encodersWithEnable[Index].IndexPosition;
                    }
                    catch (PhidgetException ex)
                    {
                        return 0;
                    }
                default:
                    return 0;
            }
        }

        public void SetEnable(int Index, bool value)
        {
            if (FPhidget.ID == Phidget.PhidgetID.ENCODER_HS_4ENCODER)
                FPhidget.encodersWithEnable[Index].Enabled = value;
        }


        #endregion getter functions


        public override void AddChangedHandler()
        {
            FPhidget.InputChange += new InputChangeEventHandler(InputChange);
        }


        void InputChange(object sender, InputChangeEventArgs e)
        {
            FChanged = true;
        }



        public override void RemoveChangedHandler()
        {
            FPhidget.InputChange -= new InputChangeEventHandler(InputChange);
        }


        public int Count
        {
            get { return FPhidget.encoders.Count; }
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
