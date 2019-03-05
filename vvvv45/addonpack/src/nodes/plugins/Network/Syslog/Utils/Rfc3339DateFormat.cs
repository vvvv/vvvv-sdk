using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace VVVV.Nodes.Syslog.Utils
{
    public class Rfc3339DateFormat
    {
        private static string FormatInUTC = "yyyy-MM-dd'T'HH:mm:ss.fff'Z'";
        private static string Format = "yyyy-MM-dd'T'HH:mm:ss.fffK";
        private static string[] Formats;

        public static string Rfc3339DateTimeFormat
        {
            get { return Format; }
        }
        public static string[] Rfc3339DateTimePatterns
        {
            get
            {
                if (Formats != null && Formats.Length > 0)
                {
                    return Formats;
                }
                else
                {
                    Formats = new string[11];

                    // Rfc3339DateTimePatterns
                    Formats[0] = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffffffK";
                    Formats[1] = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'ffffffK";
                    Formats[2] = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffffK";
                    Formats[3] = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'ffffK";
                    Formats[4] = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffK";
                    Formats[5] = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'ffK";
                    Formats[6] = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fK";
                    Formats[7] = "yyyy'-'MM'-'dd'T'HH':'mm':'ssK";

                    // Fall back patterns
                    Formats[8] = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffffffK"; // RoundtripDateTimePattern
                    Formats[9] = DateTimeFormatInfo.InvariantInfo.UniversalSortableDateTimePattern;
                    Formats[10] = DateTimeFormatInfo.InvariantInfo.SortableDateTimePattern;

                    return Formats;
                }
            }
        }

        public static bool TryParse(string input, out DateTimeOffset result)
        {
            if (input == null || input.Trim() == "")
            {
                result = DateTimeOffset.MinValue;
                return false;
            }

            if (DateTimeOffset.TryParseExact(input, Rfc3339DateTimePatterns, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AssumeLocal, out result))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static DateTimeOffset Parse(string input)
        {
            try
            {
                return DateTimeOffset.ParseExact(input, Rfc3339DateTimePatterns, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AssumeLocal);
            }
            catch (FormatException ex)
            {
                throw new FormatException(
                    string.Format("\"{0}\" is not a valid RFC 3339 date and time format.", input),
                    ex);
            }
        }

        public static string GetString(DateTimeOffset dateTime)
        {
            //check for Z shortcut

            if (dateTime.Offset == TimeSpan.Zero)
                return dateTime.ToString(FormatInUTC, DateTimeFormatInfo.InvariantInfo);
            else
                return dateTime.ToString(Rfc3339DateTimeFormat, DateTimeFormatInfo.InvariantInfo);
        }

        public static bool IsRfc3339Date(string input)
        {
            DateTimeOffset date = new DateTimeOffset();
            return TryParse(input, out date);
        }
    }
}
