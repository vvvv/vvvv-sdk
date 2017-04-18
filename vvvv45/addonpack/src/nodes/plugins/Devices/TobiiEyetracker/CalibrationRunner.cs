#region usings

using System;
using System.Collections.Generic;
using System.Timers;

using VVVV.Utils.VMath;

using Tobii.Eyetracking.Sdk;

#endregion

namespace TobiiEyetracker
{
    public class CalibrationRunner
    {
        #region attributes

        private Queue<Point2D> FCalibrationPoints;
        private Calibration FCalibration;
        private IEyetracker FEyetracker;
        private readonly System.Timers.Timer FSleepTimer;
        private float FIntervalTime = 2500;
        private int FCalibPointsCount;
        private Point2D FCurrentCalibrationPoint;
        private bool FCalibrationState = false;
        private double FCalibThreshold = 1;


        public Calibration Calibration
        {
            get
            {
                return FCalibration;
            }
            set
            {
                SetCalibration(value, true);
            }
        }

        #endregion

        #region events and delegates

        public delegate void OnTriggerCalibrationPointDelegate(Point2D point, int pointCount, int index);
        public event OnTriggerCalibrationPointDelegate OnTriggerCalibrationPoint;

        public delegate void OnCalibrationDoneDelegate(Calibration Calibration, bool calibSuccessful);
        public event OnCalibrationDoneDelegate OnCalibrationDone;

        public delegate void OnCalibrationAbortedDelegate();
        public event OnCalibrationAbortedDelegate OnCalibrationAborted;

        public delegate void OnLastCalibPointTriggeredDelegate();
        public event OnLastCalibPointTriggeredDelegate OnLastCalibPointTriggered;

        #endregion

        // constructor
        public CalibrationRunner(IEyetracker Eyetracker, float Interval)
        {
            FEyetracker = Eyetracker;
            FEyetracker.ConnectionError += HandleConnectionError;

            FIntervalTime = Interval;
            FCalibrationPoints = new Queue<Point2D>();
            FSleepTimer = new System.Timers.Timer();
        }

        // start the calibration process on the given Eyetracker
        public void RunCalibration(IEyetracker tracker, float interval, double threshold)
        {
            FCalibrationState = true;
            FCalibThreshold = threshold;
            try
            {
                FSleepTimer.Interval = interval;
                FSleepTimer.Elapsed += HandleTimerElapsed;
                FEyetracker.StartCalibration();
            }
            catch (Exception ee)
            {
                FCalibrationState = false;
            }

            // begin collecting data for the calibration points
            StartNextOrFinish();
        }

        // trigger next calib point or finish calibration
        private void StartNextOrFinish()
        {
            if (FCalibrationPoints.Count > 0)
            {
                // get (next) calibration point to be computed
                FCurrentCalibrationPoint = FCalibrationPoints.Dequeue();

                // inform ISNode send current calibration point to ISNode by triggering "OnTriggerCalibrationPoint" event
                OnTriggerCalibrationPoint(FCurrentCalibrationPoint, FCalibPointsCount, FCalibPointsCount - FCalibrationPoints.Count);

                // start Timer for calibration:
                FSleepTimer.Start();
            }
            else
            {
                // process collected calibration data
                OnLastCalibPointTriggered();
                // TODO: HandleConnectionError object disposed Exception here when eytracker is disabled during calibration
                FEyetracker.ComputeCalibrationAsync(ComputeCompleted);
                FSleepTimer.Elapsed -= HandleTimerElapsed;
            }
        }

        // called everytime a time interval has passed
        private void HandleTimerElapsed(object sender, EventArgs e)
        {
            FSleepTimer.Stop();

            // Add data to the temporary calibration buffer:
            // (catch ObjectDiposedException here when cancelling calibration)
            try
            {
                FEyetracker.AddCalibrationPointAsync(FCurrentCalibrationPoint, PointCompleted);
            }
            catch (Exception ex)
            {

            }
        }

        // handle calibration Point processing completed
        private void PointCompleted(object sender, AsyncCompletedEventArgs<Empty> e)
        {
            StartNextOrFinish();
        }

        // called when computing the calibration has finished
        private void ComputeCompleted(object sender, AsyncCompletedEventArgs<Empty> e)
        {
            if (FCalibrationState)
            {
                Calibration Calib = FEyetracker.GetCalibration();
                if (e.Error != null)
                {
                    // inform ISNode on calibration finished with errors(s)
                    OnCalibrationDone(Calib, false);
                }
                else
                {
                    // inform tracker that calibration is finished
                    // Save collected calibration data to eyetracker
                    SetCalibration(Calib, true);
                }

                FEyetracker.StopCalibration();
                FCalibrationState = false;
            }
        }

        // write calibration to eyetracker device if succssful
        private void SetCalibration(Calibration calibration, bool Success)
        {
            if (FEyetracker != null)
            {
                try
                {
                    if (Success)
                    {
                        // try to remove bad Plot Samples here
                        calibration = RemoveBadSamplesFromCalibrationPlot(calibration);
                        FEyetracker.SetCalibration(calibration);
                    }
                }
                catch (Exception ex)
                {

                }
                finally
                {
                    OnCalibrationDone(calibration, Success);
                }
            }
        }

        // remove bad samples from calib plot
        private Tobii.Eyetracking.Sdk.Calibration RemoveBadSamplesFromCalibrationPlot(Tobii.Eyetracking.Sdk.Calibration calibration)
        {
            int n = calibration.Plot.Count;
            List<CalibrationPlotItem> calibPlotList = new List<CalibrationPlotItem>();

            try
            {
                for (int i = n; i > 1; i--)
                {
                    CalibrationPlotItem cpi = calibration.Plot[i - 1];
                    if (IsBadCalibrationPlot(cpi, FCalibThreshold))
                    {
                        calibration.Plot.Remove(cpi);
                    }
                }
            }
            catch (Exception e)
            {

            }
            return calibration;
        }

        // helper
        private bool IsBadCalibrationPlot(CalibrationPlotItem cpi, double threshold)
        {
            Vector2D vLeft = new Vector2D(cpi.MapLeftX, cpi.MapLeftY);
            Vector2D vRight = new Vector2D(cpi.MapRightX, cpi.MapRightY);
            Vector2D vTrue = new Vector2D(cpi.TrueX, cpi.TrueY);
            double lLeft = Math.Abs((vLeft - vTrue).Length);
            double lRight = Math.Abs((vRight - vTrue).Length);

            if (lLeft > threshold || lRight > threshold)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        public void GetCalibration()
        {
            FCalibration = FEyetracker.GetCalibration();
        }

        // handle connection error (abort calibration)
        private void HandleConnectionError(object sender, ConnectionErrorEventArgs e)
        {
            // Abort calibration if the connection fails
            AbortCalibration();
        }

        // cancel calibration
        public void AbortCalibration()
        {
            if (FCalibrationState)
            {
                FSleepTimer.Stop();
                FEyetracker.StopCalibration();
                FCalibration = null;
                FCalibrationState = false;
                FSleepTimer.Elapsed -= HandleTimerElapsed;
                OnCalibrationAborted();
            }
        }

        internal void SetCalibrationPoints(List<Vector2D> FCalibrationPointList)
        {
            // delete all calibrationPoints
            FCalibrationPoints.Clear();
            FCalibPointsCount = FCalibrationPointList.Count;
            // add the Points defined by InputPins as Vector2D
            for (int i = 0; i < FCalibrationPointList.Count; i++)
            {
                Point2D p = new Point2D(FCalibrationPointList[i].x, FCalibrationPointList[i].y);
                FCalibrationPoints.Enqueue(p);
            }
        }
    }
}
