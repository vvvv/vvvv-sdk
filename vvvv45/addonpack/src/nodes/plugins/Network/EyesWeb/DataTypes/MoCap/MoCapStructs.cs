using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace vvvv.Utils
{
    public enum EYW_MOCAP_ITEM_TYPE
    {
        EYW_MOCAP_SCALAR = 0,
        EYW_MOCAP_POINT_2D = 1,
        EYW_MOCAP_POINT_3D = 2,
        EYW_MOCAP_VECTOR_2D = 3,
        EYW_MOCAP_VECTOR_3D = 4,
        EYW_MOCAP_BOUND_2D = 5,
        EYW_MOCAP_BOUND_3D = 6,
        EYW_MOCAP_INVALID = -1
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MOCAP_ITEM
    {
        public EYW_MOCAP_ITEM_TYPE dwType;
        public double dFactor;
        public double dReliability;
        public double dValue1;
        public double dValue2;
        public double dValue3;
        public double dValue4;
        public double dValue5;
        public double dValue6;
    }
}
