using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using VVVV.Utils.Streams;

namespace VVVV.Nodes.Syslog
{

    public class SyslogMessage : AbstractSyslog
    {
        public DateTime TimeStamp { get; set; }
        public string SourceIP { get; set; }
        public string SourceSystem { get; set; }
        public string Tag { get; set; }
        public Facility Facility { get; set; }
        public Level Level { get; set; }
        public string Message { get; set; }

        /// <summary>
        /// Parses the syslog rawMessage.
        /// </summary>
        /// <param name="rawMessage">The raw rawMessage.</param>
        /// <returns>A SyslogMessage object containing the rawMessage fields</returns>
        //public SyslogMessage ParseSyslogMessage(string senderIP, string rawMessage)
        public SyslogMessage ParseSyslogMessage( Stream StreamIn)
        {
            // RFC format: <133>Jul 19 19:05:32 GRAFFEN-PC NLog: This is a sample trace message
            // GS Format:  <14>GS_LOG: [00:0B:82:06:46:BF][000][FFFF][01000821] Send SIP message: 8 To 87.54.25.114:5060
            
            byte[] buffer = new byte[1024];
            int test = StreamIn.Read(buffer, 0, (int)StreamIn.Length);
            string rawMessage = System.Text.Encoding.ASCII.GetString(buffer, 0, buffer.Length);

            //RFC3164Format = @"(\<(?<PRI>\d+)\>(?<VERSION>\d+)?)? \ * (?<TIMESTAMP> ( (?<MONTH>Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec) \ + (?<DAY>\d+) (\ +(?<YEAR>\d+))? ) \ + (?<HOUR>\d+): (?<MINUTE>\d+): (?<SECOND>\d+):? ) \ (?<HOSTNAME>[\w!-~]+) \ (?<MESSAGE>.*)";
            //RFC5424Format = @"(\<(?<PRI>\d+)\>(?<VERSION>\d+)?)? \ * (?<TIMESTAMP> ( (?<YEAR>\d+) - (?<MONTH>\d+) - (?<DAY>\d+) ) T+ (?<HOUR>\d+): (?<MINUTE>\d+): (?<SECOND>\d+) (\.(?<MILLISECONDS>\d+))? (?<OFFSET>Z|(\+|\-)\d+:\d+)? ) \ (?<HOSTNAME>[\w!-~]+) \ (?<APPNAME>[\w!-~]+) \ (?<PROCID>[\w!-~]+) \ (?<MSGID>[\w!-~]+) \  (?<SD>-|(\[.*\])) \ ?(?<MESSAGE>.*)?";
            //string header = string.Format("{0}{1} {2} {3} {4}", pri, VERSION, timestamp, hostname, tag);

          //Regex r = new Regex(@"<(?<Priority>[0-9]{1,3})>(?<Date>[A-z]{3}\s[\d]{2}\s[\d]{2}:[\d]{2}:[\d]{2})\s(?<SourceSystem>[A-z0-9\-\.]*)\s((?<Tag>[A-z0-9]*):\s)?(?<Message>.*)", RegexOptions.Compiled | RegexOptions.ExplicitCapture);
            Regex r = new Regex(@"(\<(?<Priority>\d+)\>(?<VERSION>\d+)?)? \ * (?<Date> ( (?<YEAR>\d+) - (?<MONTH>\d+) - (?<DAY>\d+) ) T+ (?<HOUR>\d+): (?<MINUTE>\d+): (?<SECOND>\d+) (\.(?<MILLISECONDS>\d+))? (?<OFFSET>Z|(\+|\-)\d+:\d+)? ) \ (?<HOSTNAME>[\w!-~]+) \ (?<APPNAME>[\w!-~]+) \ (?<PROCID>[\w!-~]+) \ (?<MSGID>[\w!-~]+) \  (?<Tag>-|(\[.*\])) \ ?(?<Message>.*)?", RegexOptions.Compiled | RegexOptions.ExplicitCapture);
            Match m;
            Pri p;
            SyslogMessage msg = new SyslogMessage();

            m = r.Match(rawMessage);
            // Check for valid DATE stamp
            if (m.Groups["Date"].Value == String.Empty) 
            {
                var msgParts = rawMessage.Split('>');
                StringBuilder newMsg = new StringBuilder();
                newMsg.Append(msgParts[0]);
                newMsg.Append(">");
                newMsg.Append(DateTime.Now.ToString("MMM dd HH:mm:ss"));
                newMsg.Append(" ");
                //newMsg.Append(senderIP);
                newMsg.Append("192.168.100.255");
                newMsg.Append(" ");
                newMsg.Append(msgParts[1]);
                // Re-parse this new message
                m = r.Match(newMsg.ToString());
            }
            p = new Pri(m.Groups["Priority"].Value);
            msg = new SyslogMessage();
            msg.TimeStamp = DateTime.Now;
            //msg.SourceIP = senderIP;
            msg.SourceIP = "192.168.100.255";
            msg.SourceSystem = m.Groups["SourceSystem"].Value;
            msg.Level = p.Severity;
            msg.Facility = p.Facility;
            msg.Tag = m.Groups["Tag"].Value;
            msg.Message = m.Groups["Message"].Value;
            return msg;
            
        }

        public struct Pri
        {
            public Facility Facility;
            public Level Severity;
            public Pri(string strPri)
            {
                int intPri = Convert.ToInt32(strPri);
                int intFacility = intPri >> 3;
                int intSeverity = intPri & 0x7;
                this.Facility = (Facility)Enum.Parse(typeof(Facility),
                   intFacility.ToString());
                this.Severity = (Level)Enum.Parse(typeof(Level),
                   intSeverity.ToString());
            }
            public override string ToString()
            {
                return string.Format("{0}.{1}", this.Facility, this.Severity);
            }
        }
    }

    
}
