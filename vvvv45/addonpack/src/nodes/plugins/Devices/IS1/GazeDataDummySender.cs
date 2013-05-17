using System;
using System.Timers;

using VVVV.Utils.VMath;

namespace IS1
{
    public class GazeDataDummySender
    {
        #region fields

        MyGazeDataItem _myGdItem;
        private float _testValue;
        Object myLock = new object();
        Random rand = new Random();

        #endregion

        // constructor
        public GazeDataDummySender()
        {
            System.Timers.Timer aTimer = new System.Timers.Timer();
            aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            aTimer.Interval = 10;
            aTimer.Enabled = true;
        }

        // definition of a dummy GazeDataType for testing Values to Output-Pins
        public struct MyGazeDataItem
        {
            sbyte _timeStamp;
            Vector3D _lePos;
            Vector3D _rePos;
            Vector3D _lePosRel;
            Vector3D _rePosRel;
            Vector3D _leGazePoint;
            Vector3D _reGazePoint;
            Vector2D _leGazePointRel;
            Vector2D _reGazePointRel;
            int _leValidityCode;
            int _reValidityCode;
            float _lePupilDiameter;
            float _rePupilDiameter;

            public MyGazeDataItem(sbyte timeStamp, Vector3D lePos, Vector3D rePos, Vector3D lePosRel, Vector3D rePosRel,
                Vector3D leGazePoint, Vector3D reGazePoint, Vector2D leGazePointRel, Vector2D reGazePointRel,
                int leValCode, int reValCode, float lePupilDiameter, float rePupilDiameter)
            {
                _timeStamp = timeStamp;
                _lePos = lePos;
                _rePos = rePos;
                _lePosRel = lePosRel;
                _rePosRel = rePosRel;
                _leGazePoint = leGazePoint;
                _reGazePoint = reGazePoint;
                _leGazePointRel = leGazePointRel;
                _reGazePointRel = reGazePointRel;
                _leValidityCode = leValCode;
                _reValidityCode = reValCode;
                _lePupilDiameter = lePupilDiameter;
                _rePupilDiameter = rePupilDiameter;
            }

            public Vector3D LePos { get { return _lePos; } set { _lePos = value; } }
            public Vector3D RePos { get { return _rePos; } set { _rePos = value; } }
            public Vector3D LePosRel { get { return _lePosRel; } set { _lePosRel = value; } }
            public Vector3D RePosRel { get { return _rePosRel; } set { _rePosRel = value; } }
            public Vector3D LeGazePoint { get { return _leGazePoint; } set { _leGazePoint = value; } }
            public Vector3D ReGazePoint { get { return _reGazePoint; } set { _reGazePoint = value; } }
            public Vector2D LeGazePointRel { get { return _leGazePointRel; } set { _leGazePointRel = value; } }
            public Vector2D ReGazePointRel { get { return _reGazePointRel; } set { _reGazePointRel = value; } }
            public int LeValidityCode { get { return _leValidityCode; } set { _leValidityCode = value; } }
            public int ReValidityCode { get { return _reValidityCode; } set { _reValidityCode = value; } }
            public float LePupilDiameter { get { return _lePupilDiameter; } set { _lePupilDiameter = value; } }
            public float RePupilDiameter { get { return _rePupilDiameter; } set { _rePupilDiameter = value; } }
        }

        // react on Timer Elapsed
        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            lock (myLock)
            {
                _testValue = rand.Next();
                _myGdItem = CreateDummyGazeData();
            }
        }

        private MyGazeDataItem CreateDummyGazeData()
        {
            MyGazeDataItem gdi = new MyGazeDataItem();
            return gdi;
        }

        public float getTestValue()
        {
            return this._testValue;
        }
    }
}
