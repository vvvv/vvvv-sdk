using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using VVVV.Nodes.Syslog;

namespace VVVV.Nodes.Syslog.Utils
{
    public class SyslogUtil
    {
        public static string EscpaeStructuredDataValue(string sdValue)
        {
            //Inside PARAM-VALUE, the characters '"' (ABNF %d34), '\' (ABNF %d92),
            //and ']' (ABNF %d93) MUST be escaped.  This is necessary to avoid
            //parsing errors.  Escaping ']' would not strictly be necessary but is
            //REQUIRED by this specification to avoid syslog application
            //implementation errors.  Each of these three characters MUST be
            //escaped as '\"', '\\', and '\]' respectively.  

            //fancy escaping regex: (?<!\\)(?<CHAR>\"|\])|((?<!\\)(?<CHAR>\\)(?!\\))
            //replace :             \${CHAR}

            return Regex.Replace(sdValue, @"(\\|\""\])", "\\$1");
        }
        public static string UnescapeStructuredDataValue(string sdValue)
        {
            return Regex.Replace(sdValue, @"\\(\\|\""|\])", "$1");
        }

        public static void GetPriValues(int priorityValue, out SyslogFacility facility, out SyslogSeverity severity)
        {
            int facilityValue = priorityValue / 8;
            int severityValue = priorityValue % 8;

            facility = (SyslogFacility)facilityValue;
            severity = (SyslogSeverity)severityValue;
        }
        public static int CreatePri(SyslogFacility facility, SyslogSeverity severity)
        {
            if (facility == SyslogFacility.Unknown)
                facility = SyslogConstants.DefaultFacility;
            if (severity == SyslogSeverity.Unknown)
                severity = SyslogConstants.DefaultSeverity;

            return ((int)facility * 8) + (int)severity;
        }

    }
}