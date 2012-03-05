using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace NextWindow.Lib
{
            #region Structures

        //Define the required structures - from NWMultiTouch.h
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct NWTouchPoint
        {
            public Int32 TouchID;
            public Int32 TouchType;
            public Int64 TouchStart;
            public NWPoint TouchPos;
            public float Velocity;
            public float Acceleration;
            public float TouchArea;
            public Int32 TouchEventType;
            public Int32 ConfidenceLevel;
            public float Height;
            public float Width;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct NWDeviceInfo
        {
            public Int32 SerialNumber;
            public Int32 ModelNumber;
            public Int32 VersionMajor;
            public Int32 VersionMinor;
            public Int32 ProductID;
            public Int32 VendorID;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct NWDebugTouchInfo
        {
            public NWPoint[][] actualSizes;
            public float touchSymmetry;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct NWPoint
        {
            public float x;
            public float y;
        }

        #endregion


    #region Enums

    //Define the enums that are used - from NWMultiTouch.h
    public enum SuccessCode
    {
        SUCCESS = 1,
        ERR_DEVICE_NOT_OPEN = -1,
        ERR_INVALID_PACKET_ID = -2,
        ERR_INVALID_TOUCH_ID = -3,
        ERR_TOO_MANY_TOUCHES = -4,
        ERR_DEVICE_DOES_NOT_EXIST = -5,
        ERR_DISPLAY_DOES_NOT_EXIST = -6,
        ERR_FUNCTION_NOT_SUPPORTED = -7,
        ERR_INVALID_SENSOR_NUMBER = -8,
        ERR_SLOPES_MODE_NOT_SUPPORTED = -9
    }

    public enum TouchEventType
    {
        TE_TOUCH_DOWN = 1,
        TE_TOUCHING = 2,
        TE_TOUCH_UP = 3
    }

    public enum TouchType
    {
        TT_TOUCH = 1,
        TT_GHOSTTOUCH = 2,
        TT_CENTROID = 3
    }

    public enum DeviceStatus
    {
        DS_CONNECTED = 1,
        DS_TOUCH_INFO = 2,
        DS_DISCONNECTED = 3
    }

    public enum ReportModes
    {
        RM_NONE = 0,
        RM_MOUSE = 1,
        RM_MULTITOUCH = 2,
        RM_DIGITISER = 4,
        RM_SLOPESMODE = 8
    }

    #endregion
}
