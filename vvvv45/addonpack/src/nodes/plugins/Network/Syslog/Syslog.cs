#region usings
using System;
using System.IO;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;

using VVVV.Core.Logging;

using VVVV.Nodes.Syslog;

#endregion usings

namespace VVVV.Nodes
{
    #region PluginInfo
    [PluginInfo(Name = "Syslog", 
                Version = "", 
                Category = "Raw", 
                Help = "Creates a (raw) Syslog message that can be sent to a syslog server", 
                Tags = "Debug",
                Author= "sebl")]
    #endregion PluginInfo
    public class SyslogStringNode : Syslog.AbstractSyslog, IPluginEvaluate, IPartImportsSatisfiedNotification
    {

        #region fields & pins
        [Input("Message")]
        public IDiffSpread<string> FMessageIn;

        [Input("Tag")]
        public IDiffSpread<string> FTag;

        [Input("Facility", DefaultEnumEntry = "local0")]
        public IDiffSpread<Facility> FFacility;

        [Input("Level", DefaultEnumEntry = "Debug")]
        public IDiffSpread<Level> FLevel;

        [Output("Message")]
        public ISpread<Stream> FStreamOut;

        private const int VERSION = 1;

        #endregion fields & pins

        //called when all inputs and outputs defined above are assigned from the host
        public void OnImportsSatisfied()
        {
            //start with an empty stream output
            FStreamOut.SliceCount = 0;
        }

        //called when data for any output pin is requested
        public void Evaluate(int spreadMax)
        {
            //ResizeAndDispose will adjust the spread length and thereby call
            //the given constructor function for new slices and Dispose on old
            //slices.
            FStreamOut.ResizeAndDispose(spreadMax, () => new MemoryStream());
            for (int i = 0; i < spreadMax; i++)
            {
                if (FMessageIn.IsChanged || FFacility.IsChanged || FLevel.IsChanged)
                {
                    byte[] message = ConstructMessage(FLevel[i], FFacility[i], FMessageIn[i]);

                    Stream outputStream = FStreamOut[i];

                    outputStream.Position = 0;
                    outputStream.SetLength(message.Length);
                    outputStream.Write(message, 0, message.Length);
                }
            }
            //this will force the changed flag of the output pin to be set
            FStreamOut.Flush(true);
        }


        private byte[] ConstructMessage(Level level, Facility facility, string tag, string message = "")
        {
            int prival = (( int )facility) * 8 + (( int )level);
            string pri = string.Format("<{0}>", prival);
            string timestamp =
            new DateTimeOffset(DateTime.Now, TimeZoneInfo.Local.GetUtcOffset(DateTime.Now)).ToString("yyyy-MM-ddTHH:mm:ss.fffzzz");
            string hostname = Dns.GetHostEntry(Environment.UserDomainName).HostName;

            string header = string.Format("{0}{1} {2} {3} {4}", pri, VERSION, timestamp, hostname, tag);

            List<byte> syslogMsg = new List<byte>();
            syslogMsg.AddRange(System.Text.Encoding.ASCII.GetBytes(header));
            syslogMsg.AddRange(System.Text.Encoding.ASCII.GetBytes(" "));
            syslogMsg.AddRange(System.Text.Encoding.UTF8.GetBytes(message));

            return syslogMsg.ToArray();
        }



    }



    #region PluginInfo
    [PluginInfo(Name = "Logger", 
                Version = "", 
                Category = "VVVV", 
                Help = "Logs a given String to the TTY", 
                Tags = "debug",
                Author= "sebl",
                AutoEvaluate = true)]
    #endregion PluginInfo
    public class LogNode : Syslog.AbstractSyslog, IPluginEvaluate
    {
        
        #region fields & pins
        [Input("Message")]
        public IDiffSpread<string> FLogMessage;

        [Input("Log Type", DefaultEnumEntry = "Debug")]
        public IDiffSpread<LogType> FLogtype;

        [Import()]
        ILogger FLogger;
        #endregion fields & pins


        public void Evaluate(int spreadMax)
        {
            for (int i = 0; i < spreadMax; i++)
            {
                if (FLogMessage.IsChanged || FLogtype.IsChanged)
                {
                    if (FLogMessage[i].Length > 0)
                        FLogger.Log(FLogtype[i], FLogMessage[i]);
                }
            }
        }


    }



}
