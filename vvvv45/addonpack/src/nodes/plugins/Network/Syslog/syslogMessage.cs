using System;
using System.Collections.Generic;
using System.Text;

namespace VVVV.Nodes.Syslog
{
    public class SyslogMessage
    {
        //SYSLOG HEADER

        //Priority Value = ( FACILITY * 8 ) + SEVERITY
        public SyslogFacility Facility { get; set; }
        public SyslogSeverity Severity { get; set; }

        public string Version { get; set; }
        public DateTimeOffset TimeStamp { get; set; }
        public string HostName { get; set; }
        public string AppName { get; set; }
        public string ProcessID { get; set; }
        public string MessageID { get; set; }

        //Structured Data
        //public IList<StructuredDataElement> StructuredData { get; private set; }
        public IList<StructuredDataElement> StructuredData { get;  set; }    // why should it be private???

        //The Message
        public string MessageText { get; set; }

        public SyslogMessage()
        {
            StructuredData = new List<StructuredDataElement>();
            Facility = SyslogFacility.Unknown;
            Severity = SyslogSeverity.Unknown;
            Version = SyslogConstants.Version;
        }

        public static SyslogMessage Create(string message)
        {
            SyslogMessage msg = new SyslogMessage();
            msg.Facility = SyslogConstants.DefaultFacility;
            msg.Severity = SyslogConstants.DefaultSeverity;
            msg.TimeStamp = DateTimeOffset.Now;
            msg.HostName = Environment.MachineName;
            msg.MessageText = message;
            return msg;
        }
        public static SyslogMessage Create(SyslogSeverity severity, string message)
        {
            SyslogMessage msg = new SyslogMessage();
            msg.Facility = SyslogConstants.DefaultFacility;
            msg.Severity = severity;
            msg.TimeStamp = DateTimeOffset.Now;
            msg.HostName = Environment.MachineName;
            msg.MessageText = message;
            return msg;
        }
        public static SyslogMessage Create(SyslogFacility facility, SyslogSeverity severity, string message)
        {
            SyslogMessage msg = new SyslogMessage();
            msg.Facility = facility;
            msg.Severity = severity;
            msg.TimeStamp = DateTimeOffset.Now;
            msg.HostName = Environment.MachineName;
            msg.MessageText = message;
            return msg;
        }

        public string ToBsdSyslogString()
        {
            return new Format.BsdSyslogFormat().GetString(this);
        }
        public string ToIetfSyslogString()
        {
            return new Format.IetfSyslogFormat().GetString(this);
        }
    }
}
