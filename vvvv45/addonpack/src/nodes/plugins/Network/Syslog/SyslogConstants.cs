using System;
using System.Collections.Generic;
using System.Text;

namespace VVVV.Nodes.Syslog
{
    public static class SyslogConstants
    {
        public const string NilValue = "-";
        public const string Version = "1";

        public const SyslogFacility DefaultFacility = SyslogFacility.User;
        public const SyslogSeverity DefaultSeverity = SyslogSeverity.Informational;
    }
}