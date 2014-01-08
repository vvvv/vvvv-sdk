using SharpDX.RawInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VVVV.Nodes.Input
{
    class DeviceComparer : IComparer<DeviceInfo>
    {
        static List<string> FClassCodeOrdering = new List<string>() { "HID", "ACPI", "USB" };
        public int Compare(DeviceInfo x, DeviceInfo y)
        {
            var xIndex = FClassCodeOrdering.IndexOf(x.GetClassCode());
            var yIndex = FClassCodeOrdering.IndexOf(y.GetClassCode());
            if (xIndex == -1) xIndex = int.MaxValue;
            if (yIndex == -1) yIndex = int.MaxValue;
            return xIndex.CompareTo(yIndex);
        }
    }
}
