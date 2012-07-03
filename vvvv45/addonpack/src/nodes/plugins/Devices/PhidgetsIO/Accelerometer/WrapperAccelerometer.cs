using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VVVV.Utils.VMath;

using Phidgets;
using Phidgets.Events;


namespace VVVV.Nodes
{
    class WrapperAccelerometer : Phidgets<Accelerometer>, IPhidgetsWrapper
    {

        bool FChanged = false;

        #region constructor

        public WrapperAccelerometer()
            : base()
        {

        }

        public WrapperAccelerometer(int SerialNumber)
            : base(SerialNumber)
        {

        }

        #endregion constructor


        #region Setter fuctions

        public void SetSensitivity(Vector3D Sens)
        {
            FPhidget.axes[0].Sensitivity = Sens.x;
            FPhidget.axes[1].Sensitivity = Sens.y;
            FPhidget.axes[2].Sensitivity = Sens.z;
        }

        #endregion


        #region getter functions



        #endregion getter functions

        public AccelerometerAxisCollection GetAccelerationCollection()
        {
            return FPhidget.axes;
        }



        public override void AddChangedHandler()
        {
            FPhidget.AccelerationChange += new AccelerationChangeEventHandler(AccelerationChange);  
        }

        void AccelerationChange(object sender, AccelerationChangeEventArgs e)
        {
            FChanged = true;
        }

        public override void RemoveChangedHandler()
        {
            FPhidget.AccelerationChange -= new AccelerationChangeEventHandler(AccelerationChange);  
        }


        public int Count
        {
            get { return FPhidget.axes.Count; }
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
