using VVVV.Nodes.Syslog.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace VVVV.Nodes.Syslog.Format
{
    public class IetfSyslogFormat : ISyslogFormat
    {
        public string GetString(SyslogMessage msg)
        {
            int pri = SyslogUtil.CreatePri(msg.Facility, msg.Severity);
            string version = msg.Version;
            string structuredDataString = null;

            if (version == null || version.Trim() == "")
                version = SyslogConstants.Version;

            if (msg.StructuredData == null || msg.StructuredData.Count == 0)
                structuredDataString = SyslogConstants.NilValue;
            else
            {
                StringBuilder sb = new StringBuilder();
                foreach (var sd in msg.StructuredData)
                {
                    GetStructuredDataString(sd, sb);
                }
                structuredDataString = sb.ToString();
            }


            return string.Format("<{0}>{1} {2} {3} {4} {5} {6} {7} {8}",
                pri,
                version,
                Rfc3339DateFormat.GetString(msg.TimeStamp),
                ReplaceSpaces(HandleNulls(msg.HostName)),
                ReplaceSpaces(HandleNulls(msg.AppName)),
                ReplaceSpaces(HandleNulls(msg.ProcessID)),
                ReplaceSpaces(HandleNulls(msg.MessageID)),
                structuredDataString,
                msg.MessageText);
        }

        internal void GetStructuredDataString(StructuredDataElement sd, StringBuilder sb)
        {
            string idName = null;

            if (sd == null)
                return;

            if (sd.ID == null)
                idName = "DATA";
            else
                idName = sd.ID;

            sb.Append("[");
            sb.Append(GetStructuredDataID(idName));

            foreach (string key in sd.Properties.AllKeys)
            {
                foreach (var value in sd.Properties.GetValues(key))
                {
                    sb.Append(" ");
                    sb.AppendFormat("{0}=\"{1}\"",
                        GetStructuredDataID(key),
                        GetStructuredDataValue(value));
                }
            }

            sb.Append("]");
        }


        #region Helper Methods

        private static string GetHeaderValue(string header)
        {
            return ReplaceNewLines(
                   ReplaceSpaces(
                   HandleNulls(header)));
        }

        private static string GetStructuredDataID(string s)
        {
            s = ReplaceNewLines(s);
            s = ReplaceSpaces(s);

            //remove special characters
            s = s.Replace("\\", "")
                .Replace("\"", "")
                .Replace("]", "")
                .Replace("=", "");

            return s;
        }
        private static string GetStructuredDataValue(string s)
        {
            s = SyslogUtil.EscpaeStructuredDataValue(s);
            s = ReplaceNewLines(s);

            return s;
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
        private static string ReplaceNewLines(string s)
        {
            if (s == null)
                return null;

            return s.Replace("\r", "").Replace("\n", "");
        }

        #endregion
    }
}
