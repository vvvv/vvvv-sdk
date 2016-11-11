using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace System
{
    public static class TimeSpanExtensions
    {
        public static string ToSecondsString(this TimeSpan interval)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0:f1} s", interval.TotalSeconds);
        }

        public static string ToMilliSecondsString(this TimeSpan interval)
        {

            return string.Format(CultureInfo.InvariantCulture, "{0:f1} ms", interval.TotalMilliseconds);
        }

        public static string ToMicroSecondsString(this TimeSpan interval)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0:f1} µs", interval.TotalMilliseconds * 1000);
        }
    }
}
