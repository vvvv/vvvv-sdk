using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace vvvv.Utils
{
    [StructLayout(LayoutKind.Explicit)]
    public struct TScalarRawData
    {
        [FieldOffset(0)]
        public int i;

        [FieldOffset(0)]
        public double d;
    }
}
