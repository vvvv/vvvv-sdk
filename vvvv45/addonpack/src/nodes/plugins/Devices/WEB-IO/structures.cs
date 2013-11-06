using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace VVVV.Nodes
{
    namespace structs
    {
                

            [StructLayout(LayoutKind.Sequential)] 
            public class WriteRegister
            {

                public UInt16 Start_1;
                public UInt16 Start_2;
                public UInt16 StructType;
                public UInt16 StructLength;
                public UInt16 Amount;
                public UInt16 Value;

            }

            [StructLayout(LayoutKind.Sequential)]
            public class RegisterRequest
            {

                public UInt16 Start_1;
                public UInt16 Start_2;
                public UInt16 StructType;
                public UInt16 StructLength;

            }

            [StructLayout(LayoutKind.Sequential)]
            public class setBit
            {

                public UInt16 Start_1;
                public UInt16 Start_2;
                public UInt16 StructType;
                public UInt16 StructLength;
                public UInt16 Mask;
                public UInt16 Value;

            }

            [StructLayout(LayoutKind.Sequential)]
            public class registerState
            {

                public UInt16 Start_1;
                public UInt16 Start_2;
                public UInt16 StructType;
                public UInt16 StructLength;
                public UInt16 DriverID;
                public UInt16 InputValue;
                public UInt16 OutputValue;

            }

            [StructLayout(LayoutKind.Sequential)]
            public class readCounter
            {

                public UInt16 Start_1;
                public UInt16 Start_2;
                public UInt16 StructType;
                public UInt16 StructLength;
                public UInt16 CounterIndex;

            }

            [StructLayout(LayoutKind.Sequential)]
            public class readClearCounter
            {

                public UInt16 Start_1;
                public UInt16 Start_2;
                public UInt16 StructType;
                public UInt16 StructLength;
                public UInt16 CounterIndex;

            }

            [StructLayout(LayoutKind.Explicit)]
            public class Counter
            {

                [FieldOffset(0)] public UInt16 Start_1;
                [FieldOffset(2)] public UInt16 Start_2;
                [FieldOffset(4)] public UInt16 StructType;
                [FieldOffset(6)] public UInt16 StructLength;
                [FieldOffset(8)] public UInt16 CounterIndex;
                [FieldOffset(10)] public UInt32 CounterValue;

            }

            [StructLayout(LayoutKind.Sequential)]
            public class ReadAllCounter
            {

                public UInt16 Start_1;
                public UInt16 Start_2;
                public UInt16 StructType;
                public UInt16 StructLength;

            }

            [StructLayout(LayoutKind.Explicit)]
            public class AllCounter
            {

                [FieldOffset(0)] public UInt16 Start_1;
                [FieldOffset(2)] public UInt16 Start_2;
                [FieldOffset(4)] public UInt16 StructType;
                [FieldOffset(6)] public UInt16 StructLength;
                [FieldOffset(8)] public UInt16 CounterNoOf;

                [FieldOffset(12)]
                [MarshalAs(UnmanagedType.ByValArray, SizeConst=12)] public UInt32[] CounterValues;
            }    
    }
}
