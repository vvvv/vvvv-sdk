using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VVVV.Nodes.Syslog
{
    public abstract class AbstractSyslog
    {
        public enum Facility : int
        {
            Kernel = 0,
            User = 1,
            Mail = 2,
            System = 3,
            Security = 4,
            Internally = 5,
            Printer = 6,
            News = 7,
            UUCP = 8,
            cron = 9,
            Security2 = 10,
            Ftp = 11,
            Ntp = 12,
            Audit = 13,
            Alert = 14,
            Clock2 = 15,
            local0 = 16,
            local1 = 17,
            local2 = 18,
            local3 = 19,
            local4 = 20,
            local5 = 21,
            local6 = 22,
            local7 = 23,
        }
        public enum Level : int     // that's called Severity sometimes... i think Level explains it better
        {
            Emergency = 0,
            Alert = 1,
            Critical = 2,
            Error = 3,
            Warning = 4,
            Notice = 5,
            Info = 6,
            Debug = 7,
        }
    }
}
