using System;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V1;
using NextWindow.Lib;

namespace VVVV.Nodes
{  
    public class NWTouchDataNode : IPlugin, IDisposable
    {
        #region Plugin Info
        public static IPluginInfo PluginInfo
        {
            get
            {
                IPluginInfo Info = new PluginInfo();
                Info.Name = "NWTouchData";							//use CamelCaps and no spaces
                Info.Category = "Devices";						//try to use an existing one
                Info.Version = "NextWindow";						//versions are optional. leave blank if not needed
                Info.Help = "Retrieves touch data from a next window device";
                Info.Bugs = "";
                Info.Credits = "";								//give credits to thirdparty code used
                Info.Warnings = "";
                Info.Author = "vux";
                Info.Tags = "Tracking,Multi Touch";

                //leave below as is
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(true);
                System.Diagnostics.StackFrame sf = st.GetFrame(0);
                System.Reflection.MethodBase method = sf.GetMethod();
                Info.Namespace = method.DeclaringType.Namespace;
                Info.Class = method.DeclaringType.Name;
                return Info;
                //leave above as is
            }
        }
        #endregion

        #region Fields
        private IPluginHost FHost;

        //private IValueIn FPinInEnabled;

        private IValueOut FPinOutDevicesCount;
        private IValueOut FPinOutId;
        private IValueOut FPinOutPosition;
        private IValueOut FPinOutSize;
        private IValueOut FPinOutVelocity;
        private IStringOut FPinOutType;
        private IStringOut FpinOutEventType;
        private IValueOut FpinOutLevel;
        private IValueOut FPinOutOnData;

        private bool FInvalidateDeviceCount = true;
        private bool FInvalidateTouches = true;
        private int FDeviceCount = 0;
        private int myDeviceID = -1;
        private int FCurrentTouches = 0;
        private int FpacketID = -1;

        private Dictionary<int, NWTouchPoint> FTouches = new Dictionary<int, NWTouchPoint>();
        private object m_lock = new object(); 

        NWReceiveTouchInfoDelegate eventHandler;
        NWConnectEventDelegate connectEventHandler;
        NWDisconnectEventDelegate disconnectEventHandler;
        #endregion

        #region Auto Evaluate
        public bool AutoEvaluate
        {
            get { return true; }
        }
        #endregion

        #region Set Plugin Host
        public void SetPluginHost(IPluginHost Host)
        {
            //assign host
            this.FHost = Host;
            
            eventHandler = new NWReceiveTouchInfoDelegate(ReceiveTouchInformation);
            connectEventHandler = new NWConnectEventDelegate(ConnectEventHandler);
            disconnectEventHandler = new NWDisconnectEventDelegate(ConnectEventHandler);

            //Set the connect and disconnect event handlers.
            Imports.SetConnectEventHandler(connectEventHandler);
            Imports.SetDisconnectEventHandler(disconnectEventHandler);
     
            //this.FHost.CreateValueInput("Enabled", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInEnabled);
            //this.FPinInEnabled.SetSubType(0, 1, 1, 0, false, true, false);
        
            this.FHost.CreateValueOutput("Devices Count", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinOutDevicesCount);
            this.FPinOutDevicesCount.SetSubType(0, double.MaxValue, 1, 0, false, false, true);


            this.FHost.CreateValueOutput("Id", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutId);
            this.FPinOutId.SetSubType(double.MinValue, double.MaxValue, 1, 0, false, false, true);
        
            this.FHost.CreateValueOutput("Position", 2, null , TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutPosition);
            this.FPinOutPosition.SetSubType2D(double.MinValue, double.MaxValue, 0.01, 0, 0, false, false, false);

            this.FHost.CreateValueOutput("Size", 2, null , TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutSize);
            this.FPinOutSize.SetSubType2D(double.MinValue, double.MaxValue, 0.01, 0, 0, false, false, false);
            
            this.FHost.CreateValueOutput("Velocity", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutVelocity);
            this.FPinOutVelocity.SetSubType(double.MinValue, double.MaxValue, 0.01, 0, false, false, false);

            this.FHost.CreateStringOutput("Event Type", TSliceMode.Dynamic, TPinVisibility.True, out this.FpinOutEventType);
            this.FpinOutEventType.SetSubType("", false);
   
            this.FHost.CreateStringOutput("Type", TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutType);
            this.FPinOutType.SetSubType("", false);

            this.FHost.CreateValueOutput("Confidence Level", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FpinOutLevel);
            this.FpinOutLevel.SetSubType(double.MinValue, double.MaxValue, 1, 0, false, false, true);
    
            this.FHost.CreateValueOutput("On Data", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinOutOnData);
            this.FPinOutOnData.SetSubType(0, 1, 1, 0, false, true, false);
      
        }
        #endregion

        #region Configurate
        public void Configurate(IPluginConfig Input)
        {
        }
        #endregion

        #region Evaluate
        public void Evaluate(int SpreadMax)
        {
            if (this.FInvalidateDeviceCount)
            {
                this.FDeviceCount = Imports.GetConnectedDeviceCount();
                this.FPinOutDevicesCount.SetValue(0, this.FDeviceCount);
                this.FInvalidateDeviceCount = false;

                this.myDeviceID = -1;

                if (this.FDeviceCount > 0)
                {
                    myDeviceID = Imports.GetConnectedDeviceID(0);
                    Imports.OpenDevice(myDeviceID, eventHandler);
                    Imports.SetReportMode(myDeviceID, (int)ReportModes.RM_SLOPESMODE);
                    Imports.SetTouchScreenDimensions(myDeviceID, -1, -1, 1, 1);
                }
            }

            this.FPinOutOnData.SetValue(0, Convert.ToDouble(this.FInvalidateTouches));

            if (this.FInvalidateTouches)
            {
                lock (m_lock)
                {
                    //Loop through all the touches.

                    this.FPinOutPosition.SliceCount = this.FTouches.Count;
                    this.FPinOutId.SliceCount = this.FTouches.Count;
                    this.FPinOutType.SliceCount = this.FTouches.Count;
                    this.FPinOutSize.SliceCount = this.FTouches.Count;
                    this.FPinOutVelocity.SliceCount = this.FTouches.Count;
                    this.FpinOutLevel.SliceCount = this.FTouches.Count;
                    this.FpinOutEventType.SliceCount = this.FTouches.Count;

                    int cnt = 0;
                    foreach (int key in this.FTouches.Keys)
                    {
                        NWTouchPoint touch = this.FTouches[key];
                        this.FPinOutId.SetValue(cnt, touch.TouchID);
                        this.FPinOutPosition.SetValue2D(cnt, touch.TouchPos.x, -touch.TouchPos.y);
                        this.FPinOutSize.SetValue2D(cnt, touch.Width, touch.Height);
                        this.FPinOutVelocity.SetValue(cnt, touch.Velocity);
                        this.FpinOutLevel.SetValue(cnt, touch.ConfidenceLevel);
                        if (touch.TouchType == (int)TouchType.TT_CENTROID)
                        {
                            this.FPinOutType.SetString(cnt, "Centroid");
                        }
                        else
                        {
                            if (touch.TouchType == (int)TouchType.TT_GHOSTTOUCH)
                            {
                                this.FPinOutType.SetString(cnt, "Ghost");
                            }
                            else
                            {
                                this.FPinOutType.SetString(cnt, "Touch");
                            }
                        }

                        if (touch.TouchEventType == (int)TouchEventType.TE_TOUCH_DOWN)
                        {
                            this.FpinOutEventType.SetString(cnt, "Down");
                        }
                        else
                        {
                            if (touch.TouchEventType == (int)TouchEventType.TE_TOUCH_UP)
                            {
                                this.FpinOutEventType.SetString(cnt, "Up");
                            }
                            else
                            {
                                this.FpinOutEventType.SetString(cnt, "Move");
                            }
                        }
                        cnt++;
                    }

                }
                this.FInvalidateTouches = false;
            }
        }
        #endregion

        #region Dispose
        public void Dispose()
        {
            if (myDeviceID > -1)
            {
                Imports.SetConnectEventHandler(null);
                Imports.SetDisconnectEventHandler(null);
                Imports.CloseDevice(myDeviceID);
            }
        }
        #endregion

        #region Connect/Disconnect
        private void ConnectEventHandler(Int32 deviceId)
        {
            this.FInvalidateDeviceCount = true;
        }
        #endregion

        #region Touch Handler
        private void ReceiveTouchInformation(Int32 deviceId, Int32 deviceStatus, Int32 packetID, Int32 touches, Int32 ghostTouches)
        {
            //Check if we have received touch information.
            if (deviceStatus == (int)DeviceStatus.DS_TOUCH_INFO)
            {
                lock (m_lock)
                {
                    this.FTouches.Clear();
                    this.FpacketID = packetID;
                    this.FCurrentTouches = touches;
                    
                    for (int tch = 0; tch < Imports.MAX_TOUCHES; tch++)
                    {
                        if ((this.FCurrentTouches & (1 << tch)) > 0)
                        {
                            //Get the touch information.
                            NWTouchPoint touchPt = new NWTouchPoint();
                            SuccessCode retCode = (SuccessCode)Imports.GetTouch(myDeviceID, FpacketID, out touchPt, (1 << tch), 0);
                            if (retCode == SuccessCode.SUCCESS)
                            {
                                if (touchPt.TouchEventType == (int)TouchEventType.TE_TOUCH_DOWN
                                || touchPt.TouchEventType == (int)TouchEventType.TE_TOUCHING)
                                {
                                    this.FTouches[touchPt.TouchID] = touchPt;
                                }
                            }
                        }
                    }
                    this.FInvalidateTouches = true;
                }
            }
        }
        #endregion

    }
        
        
}
