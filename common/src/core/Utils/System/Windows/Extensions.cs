using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Windows
{
    public static class Extensions
    {
        public static System.Drawing.Point ToDrawingPoint(this Point point)
        {
            return new System.Drawing.Point((int)point.X, (int)point.Y);
        }
    }
}
