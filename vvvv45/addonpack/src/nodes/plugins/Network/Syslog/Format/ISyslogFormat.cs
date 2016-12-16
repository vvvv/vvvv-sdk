using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VVVV.Nodes.Syslog.Format
{
    public interface ISyslogFormat
    {
        string GetString(SyslogMessage msg);
    }
}
