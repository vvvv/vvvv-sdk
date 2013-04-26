// standard values of eyetracker device
// 172.68.195.1
// Portnumber 4455
// syncport 5547

#region usings

using System;
using System.ComponentModel.Composition;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Timers;
using System.IO;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;
using VVVV.Core.Logging;

using Tobii.Eyetracking.Sdk;
using Tobii.Eyetracking.Sdk.Exceptions;

#endregion usings


namespace IS1
{
    #region PluginInfo
    [PluginInfo(Name = "Tracking", Category = "Devices", Version = "IS1", Help = "Eyetracker IS1 Node", Tags = "")]
    #endregion PluginInfo


    public class TrackingNode : IPluginEvaluate
    {
        #region fields & pins

        #region Input
        [Input("Device", IsSingle = true)]
        IDiffSpread<IEyetracker> FEyetrackerIn;

        [Input("Enable", IsToggle = true, DefaultValue = 0, IsSingle = true)]
        IDiffSpread<bool> FEnable;

        #endregion Input

        #region Output

        [Output("TimeStamp")]
        ISpread<double> FOutputTimeStamp;

        [Output("Left Eye Pos")]
        ISpread<Vector3D> FOutputLeftEyePos;

        [Output("Right Eye Pos")]
        ISpread<Vector3D> FOutputRightEyePos;

        [Output("Left Eye Pos Rel")]
        ISpread<Vector3D> FOutputLeftEyePosRel;

        [Output("Right Eye Pos Rel")]
        ISpread<Vector3D> FOutputRightEyePosRel;

        [Output("Left Eye Gaze Point")]
        ISpread<Vector3D> FOutputLeftEyeGazePoint;

        [Output("Right Eye Gaze Point")]
        ISpread<Vector3D> FOutputRightEyeGazePoint;

        [Output("Gaze Point 2D")]
        ISpread<Vector2D> FOutputGazePoint2D;

        [Output("Left Eye Validity")]
        ISpread<int> FOutputLeftEyeValidity;

        [Output("Right Eye Validity")]
        ISpread<int> FOutputRightEyeValidity;

        [Output("Left Eye Pupil Diameter")]
        ISpread<float> FOutputLeftEyePupilDiameter;

        [Output("Right Eye Pupil Diameter")]
        ISpread<float> FOutputRightEyePupilDiameter;

        #endregion Output

        [Import()]
        ILogger FLogger;

        private IEyetracker FEyetracker;
        private bool FConnectionChanged = false;
        private bool FInit = true;
        private int FSliceCount = 0;

        #endregion fields & pins


        // called when data for any output pin is requested
        public void Evaluate(int SpreadMax)
        {
            if (FInit)
                FEyetrackerIn.Changed += new SpreadChangedEventHander<IEyetracker>(FEyetrackerIn_Changed);

            if (FConnectionChanged)
            {
                FOutputTimeStamp.SliceCount
                    = FOutputGazePoint2D.SliceCount
                    = FOutputLeftEyeGazePoint.SliceCount
                    = FOutputLeftEyePos.SliceCount
                    = FOutputLeftEyePosRel.SliceCount
                    = FOutputLeftEyePupilDiameter.SliceCount
                    = FOutputLeftEyeValidity.SliceCount
                    = FOutputRightEyeGazePoint.SliceCount
                    = FOutputRightEyePos.SliceCount
                    = FOutputRightEyePosRel.SliceCount
                    = FOutputRightEyePupilDiameter.SliceCount
                    = FOutputRightEyeValidity.SliceCount
                    = SpreadMax;
            }

            // if (FEyetracker != null)
            if (FEyetracker != null)
            {
                try
                {
                    if ((FConnectionChanged || FEnable.IsChanged) && FEnable[0] == true)
                    {
                        FEyetracker.GazeDataReceived += GazeDataReceive;
                        // tell device to start tracking
                        FEyetracker.StartTracking();
                    }
                    else if ((FConnectionChanged || FEnable.IsChanged) && FEnable[0] == false)
                    {
                        FEyetracker.GazeDataReceived -= GazeDataReceive;
                        FEyetracker.StopTracking();
                    }
                }
                catch (Exception eee)
                {
                    FLogger.Log(LogType.Debug, "Error produced by TrackingNode, " + eee.Message);
                }
            }

            FConnectionChanged = false;
            FInit = false;
        }

        void FEyetrackerIn_Changed(IDiffSpread<IEyetracker> spread)
        {

            if (spread.SliceCount == 0)
            {
                if (FEyetracker != null)
                {

                    FEyetracker.StopTracking();
                    FEyetracker.Dispose();
                    FEyetracker = null;
                }
                FSliceCount = 0;
            }
            else
            {
                FEyetracker = spread[0];
                // maybe leave this line away??
                // FEyetracker.StartTracking();
                // FEyetracker.GazeDataReceived += GazeDataReceive;
                FSliceCount = spread.SliceCount;
            }
            FConnectionChanged = true;

        }

        #region data handler


        // OnGazeDataReceive handler
        void GazeDataReceive(object sender, GazeDataEventArgs e)
        {
            if (FSliceCount > 0)
            {
                CustomGazeData cgd = new CustomGazeData(e.GazeDataItem);
                FOutputTimeStamp[0] = e.GazeDataItem.TimeStamp;
                FOutputLeftEyePos[0] = cgd.LEPos3D;
                FOutputLeftEyePosRel[0] = cgd.LEPos3DRel;
                FOutputLeftEyeGazePoint[0] = cgd.LEGazePoint;
                FOutputLeftEyePupilDiameter[0] = cgd.LEPupilDiameter;
                FOutputLeftEyeValidity[0] = cgd.LEValidity;

                FOutputRightEyePos[0] = cgd.REPos3D;
                FOutputRightEyePosRel[0] = cgd.REPos3DRel;
                FOutputRightEyeGazePoint[0] = cgd.REGazePoint;
                FOutputRightEyePupilDiameter[0] = cgd.REPupilDiameter;
                FOutputRightEyeValidity[0] = cgd.REValidity;


                Vector2D? gazeVec = GetAverageGazePoint(e.GazeDataItem);
                if (gazeVec.HasValue)
                {
                    FOutputGazePoint2D[0] = MapValue_ETToVVVV(gazeVec.Value);
                }
                else
                {
                    // 
                }
            }
        }

        // filter gaze data by EyeValidity
        private Vector2D? GetAverageGazePoint(GazeDataItem data)
        {
            if (data.LeftValidity == 0 && data.RightValidity == 0)
            {
                return new Vector2D((data.LeftGazePoint2D.X + data.RightGazePoint2D.X) * 0.5, (data.LeftGazePoint2D.Y + data.RightGazePoint2D.Y) * 0.5);
            }
            else if (data.LeftValidity < 2 && data.LeftValidity <= data.RightValidity)
            {
                return new Vector2D(data.LeftGazePoint2D.X, data.LeftGazePoint2D.Y);
            }
            else if (data.RightValidity < 2 && data.RightValidity <= data.LeftValidity)
            {
                return new Vector2D(data.RightGazePoint2D.X, data.RightGazePoint2D.Y);
            }
            else if (data.LeftValidity == 2 && data.RightValidity == 2)
            {
                return new Vector2D(data.LeftGazePoint2D.X, data.LeftGazePoint2D.Y);
            }
            else
            {
                return null;
            }
        }


        // Eyetracker to vvvv mapping
        private Vector2D MapValue_ETToVVVV(Vector2D v)
        {
            double inMin = 0;
            double inMax = 1;

            double outMinX = -1;
            double outMaxX = 1;
            double outMinY = 1;
            double outMaxY = -1;

            double x = VMath.Map(v.x, inMin, inMax, outMinX, outMaxX, TMapMode.Float);
            double y = VMath.Map(v.y, inMin, inMax, outMinY, outMaxY, TMapMode.Float);

            return new Vector2D(x, y);
        }

        #endregion data handler


    }
}


