using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Phidgets;
using Phidgets.Events;

namespace VVVV.Nodes
{
    class WrapperIOBoards: Phidgets<InterfaceKit>, IPhidgetsWrapper
    {

        bool FChanged = false;
		bool FSensorChanged = false;
		bool FDigitalInputChanged = false;

        #region constructor

        public WrapperIOBoards()
            : base()
        {

        }

        public WrapperIOBoards(int SerialNumber)
            : base(SerialNumber)
        {

        }

        #endregion constructor


        #region setter fuctions


        public void SetDigitalOutput(int Index, bool value)
        {
            FPhidget.outputs[Index] = value;
        }

        public void SetRadiometric(bool value)
        {
            FPhidget.ratiometric = value;
        }

        public void SetDataRate(int Index, int value)
        {
            FPhidget.sensors[Index].DataRate = value;
        }


        #endregion setter function


        #region getter functions

        public int GetInputCount()
        {
            return FPhidget.inputs.Count;
        }

        public bool GetInputState(int Index)
        {
            return FPhidget.inputs[Index];
        }

        public int GetOutputCount()
        {
            return FPhidget.outputs.Count;
        }

        public bool GetOutputState(int Index)
        {
            return FPhidget.outputs[Index];
        }

        public int GetSensorCount()
        {
            return FPhidget.sensors.Count;
        }

        public int GetSensorValue(int Index)
        {
            return FPhidget.sensors[Index].Value;
        }

        public int GetSensorRawValue(int Index)
        {
            return FPhidget.sensors[Index].RawValue;
        }

        public int GetDataRate(int Index)
        {
            return FPhidget.sensors[Index].DataRate;
        }

        public int GetDataRateMin(int Index)
        {
            return FPhidget.sensors[Index].DataRateMin;
        }


        public int GetDataRateMax(int Index)
        {
            return FPhidget.sensors[Index].DataRateMax;
        }

        public bool GetRadiometric()
        {
            return FPhidget.ratiometric;
        }



        #endregion getter functions




        public override void AddChangedHandler()
        {
            FPhidget.SensorChange += new SensorChangeEventHandler(SensorChange);
            FPhidget.InputChange += new InputChangeEventHandler(InputChange);
            FPhidget.OutputChange += new OutputChangeEventHandler(OutputChange);
        }

        void OutputChange(object sender, OutputChangeEventArgs e)
        {
            
        }

        void InputChange(object sender, InputChangeEventArgs e)
        {
            FChanged = true;
			FDigitalInputChanged = true;
        }

        void SensorChange(object sender, SensorChangeEventArgs e)
        {
            FChanged = true;
			FSensorChanged = true;
        }

        public override void RemoveChangedHandler()
        {
            FPhidget.SensorChange -= new SensorChangeEventHandler(SensorChange);
            FPhidget.InputChange -= new InputChangeEventHandler(InputChange);
            FPhidget.OutputChange -= new OutputChangeEventHandler(OutputChange);
        }


        public int Count
        {
            get { return FPhidget.outputs.Count; }
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

		public bool SensorChanged
		{
			get
			{
				bool temp = FSensorChanged;
				FSensorChanged = false;
				return temp;
			}
		}
		
		public bool DigitalInputChanged
		{
			get
			{
				bool temp = FDigitalInputChanged;
				FDigitalInputChanged = false;
				return temp;
			}
		}


    }
}
