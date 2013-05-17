#region usings

using System;
using System.ComponentModel.Composition;
using System.Collections.Generic;

using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;
using VVVV.Core.Logging;

using Tobii.Eyetracking.Sdk;
using Tobii.Eyetracking.Sdk.Exceptions;
using Tobii.Eyetracking.Sdk.Time;

#endregion usings


namespace TobiiEyetracker
{
    #region PluginInfo
    [PluginInfo(Name = "Eyetracker", Category = "Devices", Version = "TobiiEyetracker", Help = "Tobii Eyetracker Node", Tags = "", Author = "niggos, phlegma")]
    #endregion PluginInfo

    public class TobiiEyetracker : IPluginEvaluate
    {

        #region fields & pins

        [Input("EytrackerInfo", IsSingle = true)]
        IDiffSpread<EyetrackerInfo> FEyetrackerInfoIn;

        [Input("Framerate", DefaultValue = 40, IsSingle = true)]
        IDiffSpread<float> FFramerateIn;

        [Input("Update", IsBang = true, DefaultValue = 0, IsSingle = true)]
        IDiffSpread<bool> FUpdateIn;

        [Input("Enable", IsToggle = true, DefaultValue = 0, IsSingle = true)]
        IDiffSpread<bool> FEnable;

        [Input("XConfiguration", IsSingle = true)]
        IDiffSpread<XConfiguration> FXConfigurationIn;

        [Output("Device", IsSingle = true)]
        ISpread<IEyetracker> FEyetrackerOut;

        [Output("Sync State")]
        ISpread<String> FSyncStateOut;

        [Output("Serial")]
        ISpread<string> FSerial;

        [Output("Framerates")]
        ISpread<float> FFramerates;

        [Output("Current Framerate")]
        ISpread<float> FCurrentFramerate;

        [Output("Illumination Mode")]
        ISpread<string> FIlluminationMode;

        [Output("Calibration Lower Left")]
        ISpread<Vector3D> FLowerLeft;

        [Output("Calibration Upper Left")]
        ISpread<Vector3D> FUpperLeft;

        [Output("Calibration Upper Right")]
        ISpread<Vector3D> FUpperRight;

        [Import()]
        ILogger FLogger;
        private IEyetracker FEyetracker;
        private EyetrackerInfo FEyetrackerInfo;
        private bool FInit = true;
        private bool FConnectionChanged = true;
        private SyncManager FSyncManager;
        private Clock FClock;


        private bool FEyetrackerStatusOK = false;
        private bool FCalibrationState = false;
        private bool FCalibrationPointChanged = false;
        private bool FFilterInvalidData = true;

        private bool FXConfigSetToOutput = false;

        private int frameCount = 0;
        private int noXConfCount = 0;


        private List<Vector2D> FCalibrationPointList;
        private CalibrationRunner FCalibrationRunner;
        #endregion fields & pins

        // called when data for any output pin is requested
        public void Evaluate(int SpreadMax)
        {

            if (FInit)
                FEyetrackerInfoIn.Changed += new SpreadChangedEventHander<EyetrackerInfo>(FEyetrackerInfo_Changed);

            if (!FXConfigSetToOutput && FUpdateIn.IsChanged && FUpdateIn[0] == true)
            {
                ReadAndOutputXConfig();
            }
            
            // Enabled true
            if ((FConnectionChanged || FEnable.IsChanged) && FEnable[0] == true)
            {
                try
                {
                    if (FEyetrackerInfo != null)
                    {
                        FEyetrackerOut.SliceCount = FEyetrackerInfoIn.SliceCount;
                        if (FSyncManager == null)
                        {
                            FClock = new Clock();
                            FSyncManager = new SyncManager(FClock, FEyetrackerInfo);
                            FSyncManager.SyncManagerError += new EventHandler(FSyncManager_SyncManagerError);
                            FSyncManager.SyncStateChanged += new EventHandler<SyncStateChangedEventArgs>(FSyncManager_SyncStateChanged);
                        }

                        if (FEyetracker == null)
                        {
                            // get tracker representation from EyetrackerInfo
                            FEyetracker = EyetrackerFactory.CreateEyetracker(FEyetrackerInfo);
                            FEyetracker.ConnectionError += new EventHandler<ConnectionErrorEventArgs>(FEyetracker_ConnectionError);
                            FEyetracker.XConfigurationChanged += new EventHandler<XConfigurationChangedEventArgs>(FEyetracker_XConfigurationChanged);
                            FEyetracker.FramerateChanged += new EventHandler<FramerateChangedEventArgs>(FEyetracker_FramerateChanged);
                            FEyetrackerOut[0] = FEyetracker;
                            ReadAndOutputXConfig();
                        }
                    }
                }
                catch (EyetrackerException ex)
                {
                    if (ex.ErrorCode == 0x20000402)
                    {
                        // trigger custom-error event for IS1Node
                        FLogger.Log(LogType.Error, "EyetrackerException occured (errorCode " + ex.ErrorCode.ToString() + ") - Failed to upgrade protocol." +
                                    "This probably means that the firmware needs to be upgraded to a version that supports the new sdk.");
                    }
                    else
                    {
                        FLogger.Log(LogType.Error, "Error Code: " + ex.ErrorCode + " Message: " + ex.Message);
                    }

                }
                catch (Exception e)
                {
                    FLogger.Log(LogType.Error, "Could not connect to eyetracker. " + "Connection failed" + "(" + e.Message + ")");
                }
            }
            
            // Enabled false
            else if ((FConnectionChanged || FEnable.IsChanged) && FEnable[0] == false)
            {
                // disconnect and stop eye tracker correctly
                if (FEyetracker != null)
                {
                    FEyetracker.ConnectionError -= new EventHandler<ConnectionErrorEventArgs>(FEyetracker_ConnectionError);
                    FEyetracker.Dispose();
                    FEyetracker = null;
                    FEyetrackerOut.SliceCount = 1;
                }
                if (FSyncManager != null)
                {
                    FSyncManager.SyncManagerError -= new EventHandler(FSyncManager_SyncManagerError);
                    FSyncManager.SyncStateChanged -= new EventHandler<SyncStateChangedEventArgs>(FSyncManager_SyncStateChanged);
                    FSyncManager.Dispose();
                    FSyncManager = null;
                    FClock = null;
                }
                frameCount = 0;
                noXConfCount = 0;
            }

            // input params changed
            if (FSyncManager != null && FEyetracker != null)
            {
                if (FFramerateIn.IsChanged)
                {
                    FEyetracker.SetFramerate(FFramerateIn[0]);
                }

                if (FUpdateIn.IsChanged && FUpdateIn[0] == true)
                {
                    FSerial[0] = FEyetracker.GetUnitInfo().SerialNumber;
                    IList<float> Framerate = FEyetracker.EnumerateFramerates();
                    FFramerates.SliceCount = Framerate.Count;
                    if (Framerate.Count == 1)
                    {
                        FCurrentFramerate[0] = Framerate[0];
                    }
                    for (int i = 0; i < Framerate.Count; i++)
                    {
                        FFramerates[i] = Framerate[i];
                    }
                    try
                    {
                        IList<string> IlluminationModes = FEyetracker.EnumerateIlluminationModes();
                        FIlluminationMode.SliceCount = IlluminationModes.Count;
                        for (int i = 0; i < IlluminationModes.Count; i++)
                        {
                            FIlluminationMode[i] = IlluminationModes[i];
                        }
                    }
                    catch (EyetrackerException ex)
                    {
                        FLogger.Log(LogType.Error, "Could not set illumination mode, " + ex.Message);
                    }

                    XConfiguration x = FEyetracker.GetXConfiguration();
                    FLowerLeft[0] = new Vector3D(x.LowerLeft.X, x.LowerLeft.Y, x.LowerLeft.Z) * 0.1;
                    FUpperLeft[0] = new Vector3D(x.UpperLeft.X, x.UpperLeft.Y, x.UpperLeft.Z) * 0.1;
                    FUpperRight[0] = new Vector3D(x.UpperRight.X, x.UpperRight.Y, x.UpperRight.Z) * 0.1;
                }
                if (FXConfigurationIn.IsChanged)
                {
                    try
                    {
                        if (FXConfigurationIn[0] != null)
                            FEyetracker.SetXConfiguration(FXConfigurationIn[0]);
                    }
                    catch (Exception ee)
                    {
                        FLogger.Log(LogType.Error, "Could not set XConfiguration, " + ee.Message);
                    }
                }
            }
            
            if (FConnectionChanged && FEyetrackerInfo == null)
            {
                // Eyetracker has been disconnected, (recognized by error)
            }

            if (FEyetrackerInfoIn.IsChanged)
            {
                
            }
            FConnectionChanged = false;
            FInit = false;
        }


        void FEyetracker_FramerateChanged(object sender, FramerateChangedEventArgs e)
        {
            FCurrentFramerate[0] = e.Framerate;
        }

        void FEyetracker_XConfigurationChanged(object sender, XConfigurationChangedEventArgs e)
        {
            FLowerLeft[0] = new Vector3D(e.Configuration.LowerLeft.X, e.Configuration.LowerLeft.Y, e.Configuration.LowerLeft.Z) * 0.1;
            FUpperLeft[0] = new Vector3D(e.Configuration.UpperLeft.X, e.Configuration.UpperLeft.Y, e.Configuration.UpperLeft.Z) * 0.1;
            FUpperRight[0] = new Vector3D(e.Configuration.UpperRight.X, e.Configuration.UpperRight.Y, e.Configuration.UpperRight.Z) * 0.1;
        }

        void ReadAndOutputXConfig()
        {
            if (FEyetracker != null)
            {
                try
                {
                    XConfiguration x = FEyetracker.GetXConfiguration();
                    FLowerLeft[0] = new Vector3D(x.LowerLeft.X, x.LowerLeft.Y, x.LowerLeft.Z) * 0.1;
                    FUpperLeft[0] = new Vector3D(x.UpperLeft.X, x.UpperLeft.Y, x.UpperLeft.Z) * 0.1;
                    FUpperRight[0] = new Vector3D(x.UpperRight.X, x.UpperRight.Y, x.UpperRight.Z) * 0.1;
                    frameCount = 0;
                    FXConfigSetToOutput = true;
                    noXConfCount = 0;
                }
                catch (EyetrackerException ee)
                {
                    FLogger.Log(LogType.Error, "Could not read XConfiguration from Eyetracker:  " + ee.Message);
                    FXConfigSetToOutput = false;
                    noXConfCount += 1;
                }
                catch (Exception e)
                {
                    FLogger.Log(LogType.Error, "Could not read XConfiguration from Eyetracker:  " + e.Message);
                    FXConfigSetToOutput = false;
                    noXConfCount += 1;
                }
            }
            else
            {
                FLogger.Log(LogType.Error, "Could not read XConfiguration from Eyetracker (FEyetracker is null)");
                FXConfigSetToOutput = false;
                frameCount = 0;
                noXConfCount += 1;
            }
        }

        void FSyncManager_SyncManagerError(object sender, EventArgs e)
        {
            FLogger.Log(LogType.Error, "SyncManagerError");
        }

        void FSyncManager_SyncStateChanged(object sender, SyncStateChangedEventArgs e)
        {
            SyncStateFlag Sync = e.SyncState.StateFlag;
            switch (Sync)
            {
                case SyncStateFlag.Unsynchronized:
                    FSyncStateOut[0] = SyncStateFlag.Unsynchronized.ToString();
                    break;
                case SyncStateFlag.Stabilizing:
                    FSyncStateOut[0] = SyncStateFlag.Stabilizing.ToString();
                    break;
                case SyncStateFlag.Synchronized:
                    FSyncStateOut[0] = SyncStateFlag.Synchronized.ToString();
                    break;
                default:
                    throw new Exception("Invalid value for SyncStateFlag");
            }
        }

        void FEyetracker_ConnectionError(object sender, ConnectionErrorEventArgs e)
        {
            FLogger.Log(LogType.Error, "Eyetracker Connection Error:" + e.ErrorCode);
            FConnectionChanged = true;
        }

        void FEyetrackerInfo_Changed(IDiffSpread<EyetrackerInfo> spread)
        {
            int c = spread.SliceCount;

            if (spread.SliceCount == 0)
            {
                if (FEyetrackerInfo != null)
                {
                    FEyetrackerInfo = null;
                }
            }
            else
            {
                FEyetrackerInfo = spread[0];
            }
            FConnectionChanged = true;
        }
    }
}


