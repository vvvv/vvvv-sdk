using VVVV.Nodes.Syslog.Utils;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace VVVV.Nodes.Syslog.Parser
{
    public class SyslogParser
    {
        #region Expressions

        //#Priority
        //(\<(?<PRI>\d+)\>(?<VERSION>\d+)?)?
        //\ *
        //
        //#Time Stamp
        //(?<TIMESTAMP>
        //
        //#BSD Format
        //(
        //(?<MONTH>Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec)
        //\ +
        //(?<DAY>\d+)
        //(\ +(?<YEAR>\d+))?
        //)
        //
        //\ +
        //(?<HOUR>\d+):
        //(?<MINUTE>\d+):
        //(?<SECOND>\d+):?
        //)
        //
        //
        //
        //#Host Name
        //\ 
        //(?<HOSTNAME>[\w!-~]+)
        //
        //#Message
        //\ 
        //(?<MESSAGE>.*)
        public const string RFC3164Format = @"(\<(?<PRI>\d+)\>(?<VERSION>\d+)?)? \ * (?<TIMESTAMP> ( (?<MONTH>Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec) \ + (?<DAY>\d+) (\ +(?<YEAR>\d+))? ) \ + (?<HOUR>\d+): (?<MINUTE>\d+): (?<SECOND>\d+):? ) \ (?<HOSTNAME>[\w!-~]+) \ (?<MESSAGE>.*)";

        //#Priority
        //(\<(?<PRI>\d+)\>(?<VERSION>\d+)?)?
        //\ *
        //
        //#Time Stamp
        //(?<TIMESTAMP>
        //
        //#ISO 3339 Format
        //(
        //(?<YEAR>\d+)
        //-
        //(?<MONTH>\d+)
        //-
        //(?<DAY>\d+)
        //)
        //T+
        //(?<HOUR>\d+):
        //(?<MINUTE>\d+):
        //(?<SECOND>\d+)
        //(\.(?<MILLISECONDS>\d+))?
        //(?<OFFSET>Z|(\+|\-)\d+:\d+)?
        //)
        //
        //
        //
        //\ 
        //(?<HOSTNAME>[\w!-~]+)
        //
        //\ 
        //(?<APPNAME>[\w!-~]+)
        //
        //\ 
        //(?<PROCID>[\w!-~]+)
        //
        //\ 
        //(?<MSGID>[\w!-~]+)
        //
        //\ 
        //
        //#Structure Data
        //(?<SD>-|
        //([(?<SID>[\w!-~]+)         
        //\s*
        //(?<SDPARAM>(?<SDKEY>[\w!-~]+)\=\"(?<SDVALUE>.*)\"\s*)*
        //]
        //)
        //\ 
        //(?<MESSAGE>.*)
        //public const string RFC5424Format = @"(\<(?<PRI>\d+)\>(?<VERSION>\d+)?)? \ * (?<TIMESTAMP> ( (?<YEAR>\d+) - (?<MONTH>\d+) - (?<DAY>\d+) ) T+ (?<HOUR>\d+): (?<MINUTE>\d+): (?<SECOND>\d+) (\.(?<MILLISECONDS>\d+))? (?<OFFSET>Z|(\+|\-)\d+:\d+)? ) \ (?<HOSTNAME>[\w!-~]+) \ (?<APPNAME>[\w!-~]+) \ (?<PROCID>[\w!-~]+) \ (?<MSGID>[\w!-~]+) \ (?<SD>-| ([(?<SID>[\w!-~]+) \s* (?<SDPARAM>(?<SDKEY>[\w!-~]+)\=\""(?<SDVALUE>.*)\""\s*)* ] ) \ (?<MESSAGE>.*)";
        public const string RFC5424Format = @"(\<(?<PRI>\d+)\>(?<VERSION>\d+)?)? \ * (?<TIMESTAMP> ( (?<YEAR>\d+) - (?<MONTH>\d+) - (?<DAY>\d+) ) T+ (?<HOUR>\d+): (?<MINUTE>\d+): (?<SECOND>\d+) (\.(?<MILLISECONDS>\d+))? (?<OFFSET>Z|(\+|\-)\d+:\d+)? ) \ (?<HOSTNAME>[\w!-~]+) \ (?<APPNAME>[\w!-~]+) \ (?<PROCID>[\w!-~]+) \ (?<MSGID>[\w!-~]+) \  (?<SD>-|(\[.*\])) \ ?(?<MESSAGE>.*)?";

        #endregion

        public SyslogMessage Parse(string text)
        {
            if (text == null)
                return null;

            SyslogMessage result = new SyslogMessage();

            Regex rfc3164 = new Regex("^" + RFC3164Format + "$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.IgnorePatternWhitespace);
            Regex rfc5424 = new Regex("^" + RFC5424Format + "$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.IgnorePatternWhitespace);


            text = text.Trim();

            Match m = rfc3164.Match(text);

            if (m.Success)
            {
                GetRFC3164Results(result, m);
                return result;
            }
            else
            {
                m = rfc5424.Match(text);

                if (m.Success)
                {
                    GetRFC5424Results(result, m);
                    return result;
                }
            }

            return null;
        }

        public IList<StructuredDataElement> ParseStructuredData(string sd)
        {
            List<StructuredDataElement> results = new List<StructuredDataElement>();

            sd = sd.Trim();

            StructuredDataElement current = null;

            //normalize structured data
            using (StringReader sr = new StringReader(sd))
            {
                char c = (char)sr.Read();

                //first char should be start of sd element
                if (c == '[')
                    current = new StructuredDataElement();
                else
                    throw new FormatException(); //not in format

                current = new StructuredDataElement();

                //we have the start of thing, read the name
                current.ID = sr.ReadUntilSpace();


                while (sr.Peek() != -1)
                {
                    //now read property names
                    string key = sr.ReadUntilCharAndThenConsume('=');

                    //next char should be quote
                    if (sr.Read() != '"')
                        throw new FormatException(); //not in format

                    //now read the value
                    string value = sr.ReadUntilCharAndThenConsume('"');

                    //if value ends in escpae char, then 
                    while (value.EndsWith("\\"))
                    {
                        string addition = "\"" + sr.ReadUntilCharAndThenConsume('"');
                        value += addition;
                    }


                    value = SyslogUtil.UnescapeStructuredDataValue(value);
                    current.Properties.Add(key, value);

                    //end of key/value pair, read another one
                    sr.MaybeConsumeSpaces();

                    if (sr.Peek() == ']') //check to see if we are end of sd
                    {
                        sr.Read();
                        results.Add(current);
                        current = null;
                    }
                    if (sr.Peek() == '[') //check to see if we are new start
                    {
                        //consume start char
                        sr.Read();

                        current = new StructuredDataElement();

                        //we have the start of thing, read the name
                        string name = sr.ReadUntilSpace();
                        current.ID = name;
                    }
                }

            }

            return results;
        }

        private void GetRFC3164Results(SyslogMessage result, Match m)
        {
            if (m.Groups["PRI"].Success)
            {
                int pri = int.Parse(m.Groups["PRI"].Value);
                SetPriorityValues(result, pri);
            }
            else //pri value not found, use unknown values
            {
                result.Facility = SyslogFacility.Unknown;
                result.Severity = SyslogSeverity.Unknown;
            }

            string dateString = string.Format("{0} {1}{2}",
                m.Groups["MONTH"].Value,
                m.Groups["DAY"].Value,
                m.Groups["YEAR"].Success ? m.Groups["YEAR"].Value : "");

            string timeString = string.Format("{0}:{1}:{2}",
                m.Groups["HOUR"].Value,
                m.Groups["MINUTE"].Value,
                m.Groups["SECOND"].Value);

            string dateTimeString = dateString + " " + timeString;

            string[] dateFormats = new string[] {
                "MMM d yy HH:mm:ss",
                "MMM d yyyy HH:mm:ss",
                "MMM d HH:mm:ss"
            };

            DateTimeOffset date = DateTimeOffset.ParseExact(dateTimeString, dateFormats, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AssumeLocal);
            result.TimeStamp = date;

            if (m.Groups["HOSTNAME"].Success)
                if (m.Groups["HOSTNAME"].Value != SyslogConstants.NilValue)
                    result.HostName = m.Groups["HOSTNAME"].Value;

            if (m.Groups["MESSAGE"].Success)
                result.MessageText = m.Groups["MESSAGE"].Value;
        }

        private void GetRFC5424Results(SyslogMessage result, Match m)
        {
            if (m.Groups["PRI"].Success)
            {
                int pri = int.Parse(m.Groups["PRI"].Value);
                SetPriorityValues(result, pri);
            }
            else //pri value not found, use unknown values
            {
                result.Facility = SyslogFacility.Unknown;
                result.Severity = SyslogSeverity.Unknown;
            }

            if (m.Groups["VERSION"].Success)
            {
                result.Version = m.Groups["VERSION"].Value;
            }

            DateTimeOffset timeStamp = Utils.Rfc3339DateFormat.Parse(m.Groups["TIMESTAMP"].Value);
            result.TimeStamp = timeStamp;

            if (m.Groups["HOSTNAME"].Value != SyslogConstants.NilValue)
                result.HostName = m.Groups["HOSTNAME"].Value;

            if (m.Groups["APPNAME"].Value != SyslogConstants.NilValue)
                result.AppName = m.Groups["APPNAME"].Value;

            if (m.Groups["PROCID"].Value != SyslogConstants.NilValue)
                result.ProcessID = m.Groups["PROCID"].Value;

            if (m.Groups["MSGID"].Value != SyslogConstants.NilValue)
                result.MessageID = m.Groups["MSGID"].Value;

            //Structure Data
            if (m.Groups["SD"].Success && m.Groups["SD"].Value != SyslogConstants.NilValue)
            {
                foreach (Capture sdcap in m.Groups["SD"].Captures)
                {
                    GetStructuredData(result, sdcap.Value);
                }
            }

            if (m.Groups["MESSAGE"].Success)
                result.MessageText = m.Groups["MESSAGE"].Value;
        }

        private void GetStructuredData(SyslogMessage result, string sdString)
        {
            if (sdString == SyslogConstants.NilValue)
                return;

            var list = this.ParseStructuredData(sdString);

            foreach (var item in list)
            {
                result.StructuredData.Add(item);
            }
        }

        private void SetPriorityValues(SyslogMessage msg, int priorityValue)
        {
            SyslogFacility facility;
            SyslogSeverity severity;

            SyslogUtil.GetPriValues(priorityValue, out facility, out severity);

            msg.Facility = facility;
            msg.Severity = severity;
        }
    }


   

   

    

}
