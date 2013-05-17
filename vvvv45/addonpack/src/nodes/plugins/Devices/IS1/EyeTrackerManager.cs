#region usings

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.ComponentModel;

using Tobii.Eyetracking.Sdk;
using Tobii.Eyetracking.Sdk.Exceptions;
using Tobii.Eyetracking.Sdk.Time;


#endregion

//TODO: Clean me up
namespace IS1
{
    public class EyetrackerManager : IDisposable
    {

        #region fields and attributes

        private EyetrackerBrowser FEyetrackerBrowser;
        private Clock FClock;
        private IEyetracker FEyetracker;

        public IEyetracker Eyetracker
        {
            get { return FEyetracker; }
        }
        private SyncManager FSyncManager;
        private string FConnectionName;

        private bool FEyeTrackerStatusOK = false;
        public bool EyetrackerStatusOK
        {
            get { return FEyeTrackerStatusOK; }
        }

        private EyetrackerInfo FTrackerInfo;
        public EyetrackerInfo EyeTrackerInfo
        {
            get { return FTrackerInfo; }
        }
        private string FTrackerStatus = Constants.STATUS_OFF;



        #endregion Fields

        #region events and delegates

        public delegate void OnConnectionErrorDelegate(object sender, ConnectionErrorEventArgs e);
        public delegate void OnCustomErrorDelegate(string errorMessage);
        public delegate void OnCustomWarningDelegate(string warningMessage);
        public delegate void OnStatusChangedDelegate(string status);
        public delegate void OnIsTrackingChangedDelegate(bool isTracking);
        public delegate void OnEyetrackerFoundDelegate(string eyeTrackerStatus);
        public delegate void OnSyncStateChangedDelegate(object sender, SyncStateChangedEventArgs e);


        public event OnConnectionErrorDelegate OnConnectionError;
        public event OnCustomErrorDelegate OnCustomError;
        public event OnStatusChangedDelegate OnStatusChanged;
        public event OnEyetrackerFoundDelegate OnEyetrackerFound;
        public event OnSyncStateChangedDelegate OnSyncStateChanged;

        #endregion

        // constructor
        public EyetrackerManager()
        {

        }

        // browse for eyetracker
        public void FindAndStartEyeTracker()
        {
            FEyetrackerBrowser = new EyetrackerBrowser();
            FClock = new Clock();
            FEyetrackerBrowser.Start();
        }

        // establich connection to eyetracker device
        private void ConnectToTrackerAndStartTracking(EyetrackerInfo info)
        {
            try
            {
                if (FEyetracker == null)
                {
                    // get tracker representation from EyetrackerInfo
                    FEyetracker = EyetrackerFactory.CreateEyetracker(info);
                }

                FEyetracker.ConnectionError -= HandleConnectionError;

                // Trigger device-status-update event for IS1Node
                UpdateDeviceStatus(FTrackerInfo.Status);

                FConnectionName = info.ProductId;
                if (FSyncManager == null)
                {
                    FSyncManager = new SyncManager(FClock, info);
                    FSyncManager.SyncStateChanged += HandleSyncStateChanged;
                }

                SyncState st = FSyncManager.SyncState;
                if (st.StateFlag == SyncStateFlag.Unsynchronized)
                {
                    // TODO: handle the case that eyetracker is unsynchronized to make sure that et-status gets OK
                }

                // register eventhandlers
                FEyetracker.ConnectionError += HandleConnectionError;
                
                // trigger device-status-update event for IS1Node
                UpdateDeviceStatus(FTrackerInfo.Status);

            }
            catch (EyetrackerException ee)
            {
                if (ee.ErrorCode == 0x20000402)
                {
                    // trigger custom-error event for IS1Node
                    OnCustomError("EyetrackerException occured (errorCode " + ee.ErrorCode.ToString() + ") - Failed to upgrade protocol." +
                        "This probably means that the firmware needs to be upgraded to a version that supports the new sdk.");
                }
                else
                {
                    // trigger custom-error event for IS1Node
                    OnCustomError("EyetrackerException occured (errorCode " + ee.ErrorCode.ToString() + ") - Eyetracker responded with error" + ee);
                }
                DisconnectEyetracker(false);
            }
            catch (Exception e)
            {
                // trigger custom-error event for IS1Node
                OnCustomError("Could not connect to eyetracker. " + "Connection failed" + "(" + e.Message + ")");
                DisconnectEyetracker(false);
            }
        }

        // update device state, notify IS1Node by event if state has changed
        private void UpdateDeviceStatus(string status)
        {
            String oldState = FTrackerStatus;
            FTrackerStatus = status;
            if (!FTrackerStatus.Equals(oldState))
            {
                // state has changed
                OnStatusChanged(FTrackerStatus);
            }
        }

        // handle connection errors
        private void HandleConnectionError(object sender, ConnectionErrorEventArgs e)
        {
            // If the connection goes down we dispose the IAsyncEyetracker instance. This will release all resources held by the connection
            OnConnectionError(sender, e);
            DisconnectEyetracker(false);
        }

        // sync state changed
        private void HandleSyncStateChanged(object sender, SyncStateChangedEventArgs e)
        {
            OnSyncStateChanged(sender, e);
        }

        // helper
        private void DisconnectEyetracker(bool DoRescan)
        {
            if (FEyetracker != null)
            {
                FEyetracker.ConnectionError -= HandleConnectionError;
                FEyetracker.Dispose();
                FEyetracker = null;
                FConnectionName = string.Empty;

                FSyncManager.Dispose();

                UpdateDeviceStatus(FTrackerInfo.Status);
                if (!FEyetrackerBrowser.IsStarted)
                {
                    if (DoRescan)
                    {
                        FEyetrackerBrowser.Start();
                    }
                }
                if (FEyetrackerBrowser.IsStarted)
                {
                    FEyetrackerBrowser.Stop();
                }
            }
        }

        // disconnect the eyetracker
        public void DisconnectEyeTracker(bool restartTrackerBrowser)
        {
            DisconnectEyetracker(restartTrackerBrowser);
        }

        // set the XConfiguration to eyetracker, if useCurrentXConfig
        public void UpdateXConfiguration(XConfiguration xconfig, bool useCurrentXConfig)
        {
            if (FEyetracker != null)
            {
                XConfiguration xConfig = null;
                if (useCurrentXConfig)
                {
                    xConfig = FEyetracker.GetXConfiguration();
                    FEyetracker.SetXConfiguration(xConfig);
                }
                else if (!useCurrentXConfig)
                {
                    xConfig = xconfig;
                    FEyetracker.SetXConfiguration(xConfig);
                }
            }
            else
            {
                OnCustomError("Could not change XConfiguration, no eyetracker defined ..");
            }
        }

        public void Dispose()
        {
            // throw new NotImplementedException();
        }
    }
}
