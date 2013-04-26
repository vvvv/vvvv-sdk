// standard values of eyetracker device
// 172.68.195.1
// Portnumber 4455
// syncport 5547

#region usings

using System;
using System.ComponentModel.Composition;
using System.ComponentModel;
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
    [PluginInfo(Name = "Calibration", Category = "Devices", Version = "IS1", Help = "Calibration for Eyetracker IS1", Tags = "Eytracker, Tobii")]
    #endregion PluginInfo


    public class CalibrationNode : IPluginEvaluate
    {
        #region fields & pins

        #region Input
        [Input("Device", IsSingle = true)]
        IDiffSpread<IEyetracker> FEyetrackerIn;

        [Input("Calibrate", IsBang = true, DefaultValue = 0, IsSingle = true)]
        IDiffSpread<bool> FInputDoCalibration;

        [Input("Cancel", IsBang = true, DefaultValue = 0, IsSingle = true)]
        IDiffSpread<bool> FInputCancelCalibration;

        [Input("Calibration Point XY")]
        ISpread<Vector2D> FCalibrationPointsXY;

        [Input("Interval", IsSingle = true, DefaultValue = 2500)]
        ISpread<float> FInterval;

        [Input("Calibration Threshold", IsSingle = true, DefaultValue = 0.05)]
        IDiffSpread<double> FCalibThreshold;

        [Input("Save", IsSingle = true, IsBang = true)]
        IDiffSpread<bool> FSave;

        [Input("Load", IsSingle = true, IsBang = true)]
        IDiffSpread<bool> FLoad;

        [Input("File", IsSingle = true, StringType = StringType.Filename)]
        IDiffSpread<string> FFile;

        #endregion Input

        #region Output
        [Output("State")]
        ISpread<bool> FOutputIsCalibrationState;

        [Output("Collecting gaze samples")]
        ISpread<bool> FOutputIsCollectingSamples;

        [Output("Last Calibration succesful")]
        ISpread<bool> FOutputLastCalibSuccessful;

        [Output("OnCalibrationPointChange")]
        ISpread<bool> FOnCalibrationPointChange;

        [Output("Point")]
        ISpread<Vector2D> FOutputCalibPointRel;

        [Output("Map Left")]
        ISpread<Vector2D> FMapLeft;

        [Output("Map Right")]
        ISpread<Vector2D> FMapRight;

        [Output("True")]
        ISpread<Vector2D> FTrue;

        [Output("Validity")]
        ISpread<Vector2D> FValidity;

        [Output("True Calibration Points")]
        ISpread<Vector2D> FLoadedCalibrationPoints;


        #endregion Output

        [Import()]
        ILogger FLogger;

        private CalibrationRunner FCalibrationRunner;
        private IEyetracker FEyetracker;
        private bool FInit = true;
        private bool FLogNotConnected = true;
        private BackgroundWorker saveCalibFileWorker;
        private BackgroundWorker loadCalibFileWorker;
        private bool onCalibPointChanged = false;

        private List<Vector2D> FCalibrationPointsLoaded;


        #endregion fields & pins


        // called when data for any output pin is requested
        public void Evaluate(int SpreadMax)
        {
            if (FInit)
            {
                FEyetrackerIn.Changed += new SpreadChangedEventHander<IEyetracker>(FEyetrackerIn_Changed);
                InitBackgroundWorkers();
            }
            if (FEyetracker != null)
            {
                // check if Calibration was triggered by Input Pin and a tracker is connected
                if (FInputDoCalibration.IsChanged && FInputDoCalibration[0] == true)
                {
                    DoCalibrationProcedure();
                }

                if (FInputCancelCalibration.IsChanged && FInputCancelCalibration[0] == true)
                {
                    if (FCalibrationRunner != null)
                    {
                        //TODO:Check if this is working 
                        FCalibrationRunner.AbortCalibration();
                    }
                }

                if (FSave.IsChanged && FSave[0] == true)
                {
                    saveCalibFileWorker.RunWorkerAsync();
                }

                if (FLoad.IsChanged && FLoad[0] == true)
                {
                    loadCalibFileWorker.RunWorkerAsync();
                }
                FLogNotConnected = true;
            }
            else
            {
                if (FLogNotConnected)
                {
                    FLogger.Log(LogType.Error, "No Eyetracker Connected");
                    FLogNotConnected = false;
                }
            }
            if (onCalibPointChanged)
            {
                FOnCalibrationPointChange[0] = true;
                onCalibPointChanged = false;
            }
            else
            {
                FOnCalibrationPointChange[0] = false;
            }

            // Think FInit has to be reset here, ha?
            FInit = false;
        }



        private void InitBackgroundWorkers()
        {
            saveCalibFileWorker = new BackgroundWorker();
            saveCalibFileWorker.DoWork += new DoWorkEventHandler(saveCalibFileWorker_DoWork);
            saveCalibFileWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(saveCalibFileWorker_RunWorkerCompleted);
            loadCalibFileWorker = new BackgroundWorker();
            loadCalibFileWorker.DoWork += new DoWorkEventHandler(loadCalibFileWorker_DoWork);
            loadCalibFileWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(loadCalibFileWorker_RunWorkerCompleted);

        }

        void loadCalibFileWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                //TODO: Make the Reading Async because of Framedrops in vvvv
                FileStream Stream = new FileStream(FFile[0], FileMode.Open, FileAccess.Read);
                byte[] RawData = new byte[Stream.Length];
                Stream.Read(RawData, 0, (int)Stream.Length);
                Stream.Close();
                Calibration calib = new Calibration(RawData);
                FCalibrationRunner.Calibration = calib;
            }
            catch (Exception ex)
            {
                FLogger.Log(LogType.Error, ex.Message);
            }
        }

        void saveCalibFileWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                //TODO: Make the writing Async because of Framedrops in vvvv
                FCalibrationRunner.GetCalibration();
                Calibration calib = FCalibrationRunner.Calibration;
                FileStream Stream = new FileStream(FFile[0], FileMode.Create, FileAccess.Write);
                Stream.Write(calib.RawData, 0, calib.RawData.Length);
                Stream.Flush(true);
                Stream.Close();
            }
            catch (Exception ex)
            {
                FLogger.Log(LogType.Error, ex.Message);
            }
        }


        void loadCalibFileWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            FLogger.Log(LogType.Debug, "Loading Calibration File done ...");
        }

        void saveCalibFileWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            FLogger.Log(LogType.Debug, "Saving Calibration File done ...");
        }


        void FEyetrackerIn_Changed(IDiffSpread<IEyetracker> spread)
        {
            if (spread.SliceCount == 0)
            {
                if (FEyetracker != null)
                {
                    FEyetracker = null;
                    FCalibrationRunner.OnTriggerCalibrationPoint -= FCalibrationRunner_OnTriggerCalibrationPoint;
                    FCalibrationRunner.OnCalibrationDone -= FCalibrationRunner_OnCalibrationDone;
                    FCalibrationRunner.OnCalibrationAborted -= FCalibrationRunner_OnCalibrationAborted;
                    FCalibrationRunner.OnLastCalibPointTriggered -= FCalibrationRunner_OnLastCalibPointTriggered;
                    FCalibrationRunner = null;
                }
            }
            else
            {
                FEyetracker = spread[0];
                FCalibrationRunner = new CalibrationRunner(spread[0], FInterval[0]);
                // register event listeners
                FCalibrationRunner.OnTriggerCalibrationPoint += FCalibrationRunner_OnTriggerCalibrationPoint;
                FCalibrationRunner.OnCalibrationDone += FCalibrationRunner_OnCalibrationDone;
                FCalibrationRunner.OnCalibrationAborted += FCalibrationRunner_OnCalibrationAborted;
                FCalibrationRunner.OnLastCalibPointTriggered += FCalibrationRunner_OnLastCalibPointTriggered;
            }
        }

        #region Calibration routine

        // Start calibration procedure
        private void DoCalibrationProcedure()
        {

            FCalibrationRunner.SetCalibrationPoints(MapValues_VVVVToET(FCalibrationPointsXY.ToList()));

            try
            {
                // trigger calibration process
                FOutputIsCalibrationState[0] = true;
                FOutputIsCollectingSamples[0] = true;
                FLogger.Log(LogType.Debug, "Calibration state entered");

                FCalibrationRunner.RunCalibration(FEyetracker, FInterval[0], FCalibThreshold[0]);
            }
            catch (EyetrackerException ee)
            {
                FOutputIsCalibrationState[0] = false;
                FOutputIsCollectingSamples[0] = false;
                FLogger.Log(LogType.Error, "Calibration failed. Got exception " + ee);
            }
            catch (Exception e)
            {
                FOutputIsCalibrationState[0] = false;
                FOutputIsCollectingSamples[0] = false;
                FLogger.Log(LogType.Error, "Calibration not performed. Exception: " + e);
            }
        }

        // OnCalibrationDone
        void FCalibrationRunner_OnCalibrationDone(Calibration cal, bool IsSuccessful)
        {
            if (IsSuccessful)
            {
                FLogger.Log(LogType.Debug, "Calibration state exited, calibration successful");
                FOutputLastCalibSuccessful[0] = IsSuccessful;
            }
            else
            {
                FLogger.Log(LogType.Error, "Calibration state exited, calibration not successful");
                FOutputLastCalibSuccessful[0] = IsSuccessful;
            }

            if (cal != null)
            {
                WriteCalibDataToOutput(cal);
            }
            FOutputIsCalibrationState[0] = false;
            FOutputIsCollectingSamples[0] = false;
        }

        // OnCalibrationbAborted
        void FCalibrationRunner_OnCalibrationAborted()
        {
            FLogger.Log(LogType.Debug, "Calibration aborted, calibration state exited");
            FOutputIsCalibrationState[0] = false;
            FOutputIsCollectingSamples[0] = false;
        }

        private void WriteCalibDataToOutput(Calibration calibration)
        {
            List<CalibrationPlotItem> calibPlot = calibration.Plot;
            List<Vector2D> TrueXYPoints = new List<Vector2D>();


            FMapLeft.SliceCount = FMapRight.SliceCount = FTrue.SliceCount = FValidity.SliceCount = calibPlot.Count;

            for (int i = 0; i < calibPlot.Count; i++)
            {
                FMapLeft[i] = MapValue_ETToVVVV(new Vector2D(calibPlot[i].MapLeftX, calibPlot[i].MapLeftY));
                FMapRight[i] = MapValue_ETToVVVV(new Vector2D(calibPlot[i].MapRightX, calibPlot[i].MapRightY));
                FTrue[i] = MapValue_ETToVVVV(new Vector2D(calibPlot[i].TrueX, calibPlot[i].TrueY));
                FValidity[i] = new Vector2D(calibPlot[i].ValidityLeft, calibPlot[i].ValidityRight);


                Vector2D v = MapValue_ETToVVVV(new Vector2D(calibPlot[i].TrueX, calibPlot[i].TrueY));
                bool exists = false;
                int c = TrueXYPoints.Count;
                for (int j = 0; j < TrueXYPoints.Count; j++)
                {
                    if (v == TrueXYPoints[j])
                    {
                        exists = true;
                    }
                }
                if (c == 0 || (c > 0 && !exists))
                {
                    TrueXYPoints.Add(v);
                }
            }
            FLoadedCalibrationPoints.AssignFrom(TrueXYPoints);

        }


        // OnCalibrationPointTriggered
        internal void FCalibrationRunner_OnTriggerCalibrationPoint(Point2D point, int pointCount, int index)
        {
            FOutputCalibPointRel[0] = MapValue_ETToVVVV(new Vector2D(point.X, point.Y));
            onCalibPointChanged = true;
        }

        internal void FCalibrationRunner_OnLastCalibPointTriggered()
        {
            FOutputIsCollectingSamples[0] = false;
        }


        // vvvv to eyetracker mapping
        private List<Vector2D> MapValues_VVVVToET(List<Vector2D> pointList)
        {
            List<Vector2D> pList = new List<Vector2D>();
            for (int i = 0; i < pointList.Count; i++)
            {
                pList.Add(new Vector2D(
                    VMath.Map(pointList[i].x, -1, 1, 0, 1, TMapMode.Float),
                    VMath.Map(pointList[i].y, -1, 1, 1, 0, TMapMode.Float)
                    ));
            }
            return pList;
        }

        // Eyetracker to vvvv mapping
        private Vector2D MapValue_ETToVVVV(Vector2D v)
        {
            return new Vector2D(
                VMath.Map(v.x, 0, 1, -1, 1, TMapMode.Float),
                VMath.Map(v.y, 0, 1, 1, -1, TMapMode.Float)
                );
        }

        #endregion Calibration routine
    }
}


