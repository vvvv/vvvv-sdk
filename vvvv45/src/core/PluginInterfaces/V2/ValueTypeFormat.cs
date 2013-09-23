using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VVVV.PluginInterfaces.V2
{
    // Compatible with System.TypeCode
    public enum ValueTypeFormat
    {
        Double = 14,
        Boolean = 3,
        SByte = 5,
        Byte = 6,
        Int16 = 7,
        UInt16 = 8,
        Int32 = 9,
        UInt32 = 10,
        Int64 = 11,
        UInt64 = 12,
        Single = 13
    }

    public static class ValueTypeFormatUtils
    {
        public static int SizeOf(ValueTypeFormat format)
        {
            switch (format)
            {
                case ValueTypeFormat.Boolean:
                    return sizeof(Boolean);
                case ValueTypeFormat.SByte:
                    return sizeof(SByte);
                case ValueTypeFormat.Byte:
                    return sizeof(Byte);
                case ValueTypeFormat.Int16:
                    return sizeof(Int16);
                case ValueTypeFormat.UInt16:
                    return sizeof(UInt16);
                case ValueTypeFormat.Int32:
                    return sizeof(Int32);
                case ValueTypeFormat.UInt32:
                    return sizeof(UInt32);
                case ValueTypeFormat.Int64:
                    return sizeof(Int64);
                case ValueTypeFormat.UInt64:
                    return sizeof(UInt64);
                case ValueTypeFormat.Single:
                    return sizeof(Single);
                case ValueTypeFormat.Double:
                    return sizeof(Double);
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
