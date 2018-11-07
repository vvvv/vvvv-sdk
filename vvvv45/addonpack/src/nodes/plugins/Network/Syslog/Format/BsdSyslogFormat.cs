using VVVV.Nodes.Syslog.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace VVVV.Nodes.Syslog.Format
{
    public class BsdSyslogFormat : ISyslogFormat
    {
        public string GetString(SyslogMessage msg)
        {
            StringBuilder sb = new StringBuilder();

            var pri = SyslogUtil.CreatePri(msg.Facility, msg.Severity);
            var timestamp = msg.TimeStamp.ToString("MMM dd HH:mm:ss");

            return string.Format("<{0}>{1} {2} {3}",
                pri,
                timestamp.Trim(),
                HandleNulls(msg.HostName),
                msg.MessageText);
        }

        private static string HandleNulls(string s)
        {
            if (s == null || s.Trim() == "")
                return SyslogConstants.NilValue;

            return s;
        }
        private static string ReplaceSpaces(string s)
        {
            if (s == null)
                return null;

            return Regex.Replace(s, @"\s+", "_");
        }
    }
}
