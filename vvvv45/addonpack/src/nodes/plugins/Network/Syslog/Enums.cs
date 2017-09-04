using System;
using System.Collections.Generic;
using System.Text;

namespace VVVV.Nodes.Syslog
{
    /// <summary>
    /// syslog severities
    /// </summary>
    /// <remarks>
    /// <para>
    /// The syslog severities.
    /// </para>
    /// </remarks>
    public enum SyslogSeverity
    {
        /// <summary>
        /// Unknown Value
        /// </summary>
        Unknown = -1,

        /// <summary>
        /// 0 - System Is Unusable
        /// </summary>
        Emergency = 0,

        /// <summary>
        /// 1 - Action Must Be Taken Immediately
        /// </summary>
        Alert = 1,

        /// <summary>
        /// 2 - Critical Conditions
        /// </summary>
        Critical = 2,

        /// <summary>
        /// 3 - Error Conditions
        /// </summary>
        Error = 3,

        /// <summary>
        /// 4 - Warning Conditions
        /// </summary>
        Warning = 4,

        /// <summary>
        /// 5 - Normal but Significant Condition
        /// </summary>
        Notice = 5,

        /// <summary>
        /// 6 - Informational
        /// </summary>
        Informational = 6,

        /// <summary>
        /// 7 - Debug-Level Messages
        /// </summary>
        Debug = 7
    };

    /// <summary>
    /// syslog facilities
    /// </summary>
    /// <remarks>
    /// <para>
    /// The syslog facilities
    /// </para>
    /// </remarks>
    public enum SyslogFacility
    {
        /// <summary>
        /// Unknown Value
        /// </summary>
        Unknown = -1,

        /// <summary>
        /// 0 - Kernel Messages
        /// </summary>
        Kernel = 0,

        /// <summary>
        /// 1 - Random User-Level Messages
        /// </summary>
        User = 1,

        /// <summary>
        /// 2 - Mail System
        /// </summary>
        Mail = 2,

        /// <summary>
        /// 3 - System Daemons
        /// </summary>
        Daemons = 3,

        /// <summary>
        /// 4 - Security/Authorization Messages
        /// </summary>
        Authorization = 4,

        /// <summary>
        /// 5 - Messages Generated Internally by Syslogd
        /// </summary>
        Syslog = 5,

        /// <summary>
        /// 6 - Line Printer Subsystem
        /// </summary>
        Printer = 6,

        /// <summary>
        /// 7 - Network News Subsystem
        /// </summary>
        News = 7,

        /// <summary>
        /// 8 - UUCP subsystem
        /// </summary>
        Uucp = 8,

        /// <summary>
        /// 9 - Clock (Cron/At) Daemon
        /// </summary>
        Clock = 9,

        /// <summary>
        /// 10 - Security/Authorization Messages (private)
        /// </summary>
        AuthorizationPrivate = 10,

        /// <summary>
        /// 11 - FTP Daemon
        /// </summary>
        Ftp = 11,

        /// <summary>
        /// 12 - NTP Subsystem
        /// </summary>
        Ntp = 12,

        /// <summary>
        /// 13 - Log Audit
        /// </summary>
        Audit = 13,

        /// <summary>
        /// 14 - Log Alert
        /// </summary>
        Alert = 14,

        /// <summary>
        /// clock daemon
        /// </summary>
        Clock2 = 15,

        /// <summary>
        /// 16 - Reserved For Local Use
        /// </summary>
        Local0 = 16,

        /// <summary>
        /// 17 - Reserved For Local Use
        /// </summary>
        Local1 = 17,

        /// <summary>
        /// 18 - Reserved For Local Use
        /// </summary>
        Local2 = 18,

        /// <summary>
        /// 19 - Reserved For Local Use
        /// </summary>
        Local3 = 19,

        /// <summary>
        /// 20 - Reserved For Local Use
        /// </summary>
        Local4 = 20,

        /// <summary>
        /// 21 - Reserved For Local Use
        /// </summary>
        Local5 = 21,

        /// <summary>
        /// 22 - Reserved For Local Use
        /// </summary>
        Local6 = 22,

        /// <summary>
        /// 23 - Reserved For Local Use
        /// </summary>
        Local7 = 23
    }
}

