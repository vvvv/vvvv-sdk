using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace NextWindow.Lib
{
    //Declare callback function prototype.
    public delegate void NWReceiveTouchInfoDelegate(Int32 deviceId, Int32 deviceStatus, Int32 packetID, Int32 touches, Int32 ghostTouches);

    //Declare connect event handler prototype
    public delegate void NWConnectEventDelegate(Int32 deviceId);

    //Declare disconnect event handler prototype
    public delegate void NWDisconnectEventDelegate(Int32 deviceId);

    public static class Imports
    {
        public const int MAX_TOUCHES = 32;
        public const int NUM_POLYGON_POINTS = 4;

        [DllImport("NWMultiTouch.dll")]
        public static extern Int32 OpenDevice(Int32 deviceID, NWReceiveTouchInfoDelegate callbackFn);
        [DllImport("NWMultiTouch.dll")]
        public static extern void CloseDevice(Int32 deviceID);
        [DllImport("NWMultiTouch.dll")]
        public static extern Int32 GetConnectedDeviceCount();
        [DllImport("NWMultiTouch.dll")]
        public static extern Int32 GetConnectedDeviceID(Int32 deviceNo);
        [DllImport("NWMultiTouch.dll")]
        public static extern Int32 GetTouch(Int32 deviceID, Int32 packetID, out NWTouchPoint touchPoint, Int32 touch, Int32 ghostTouch);
        [DllImport("NWMultiTouch.dll")]
        public static extern Int32 GetPolygon(Int32 deviceID, Int32 packetID, Int32 touches, Int32 ghostTouches, [In, Out] NWPoint[] pointArray, Int32 size);
        [DllImport("NWMultiTouch.dll")]
        public static extern Int32 GetTouchDeviceInfo(Int32 deviceID, out NWDeviceInfo deviceInfo);
        [DllImport("NWMultiTouch.dll")]
        public static extern Int32 SetReportMode(Int32 deviceID, Int32 reportMode);
        [DllImport("NWMultiTouch.dll")]
        public static extern Int32 GetReportMode(Int32 deviceID);
        [DllImport("NWMultiTouch.dll")]
        public static extern Int32 SetKalmanFilterStatus(Int32 deviceID, bool kalmanOn);
        [DllImport("NWMultiTouch.dll")]
        public static extern Int32 SetTouchScreenDimensions(Int32 deviceID, float xMin, float yMin, float xMax, float yMax);
        [DllImport("NWMultiTouch.dll")]
        public static extern void SetConnectEventHandler(NWConnectEventDelegate connectHandler);
        [DllImport("NWMultiTouch.dll")]
        public static extern void SetDisconnectEventHandler(NWDisconnectEventDelegate disconnectHandler);
    }
}
