#region usings
using System;
using System.IO;
using System.ComponentModel.Composition;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;

using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;
using VVVV.Core.Logging;
using VVVV.Nodes.Syslog.Parser;
#endregion usings

namespace VVVV.Nodes.Syslog
{
    #region PluginInfo
    [PluginInfo(Name = "Syslog", 
                Version = "Join", 
                Category = "Raw", 
                Help = "Creates a (raw) Syslog message that can be sent to a syslog server", 
                Tags = "Debug",
                Author= "sebl",
                Credits = "")]
    #endregion PluginInfo
    public class SyslogJoinNode : Syslog.SyslogMessage, IPluginEvaluate, IPartImportsSatisfiedNotification
    {

        #region fields & pins
        [Input("Message")]
        public IDiffSpread<string> FMessageIn;

        [Input("Facility", DefaultEnumEntry = "local0")]
        public IDiffSpread<SyslogFacility> FFacility;

        [Input("Severity", DefaultEnumEntry = "Debug")]
        public IDiffSpread<SyslogSeverity> FSeverity;

        [Input("Hostname", DefaultString = "")]
        public IDiffSpread<string> FHostname;

        [Input("AppName", DefaultString = "", Visibility = PinVisibility.Hidden)]
        public IDiffSpread<string> FAppName;

        [Input("Process ID", DefaultValue = -1, Visibility = PinVisibility.Hidden)]
        public IDiffSpread<int> FProcId;

        [Input("Message ID", DefaultValue = -1, Visibility = PinVisibility.Hidden)]
        public IDiffSpread<int> FMsgId;

        [Input("Structured Data Name", DefaultString = "")]
        public ISpread<ISpread<string>> FStructuredDataName;

        [Input("Structured Data Value", DefaultString = "")]
        public ISpread<ISpread<string>> FStructuredDataValue;

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

        public static int Max(params int[] values)
        {
            return Enumerable.Max(values);
        }

        //called when data for any output pin is requested
        public void Evaluate(int spreadMax)
        {
            spreadMax = Max(FMessageIn.SliceCount, FFacility.SliceCount, FSeverity.SliceCount, FHostname.SliceCount, FAppName.SliceCount, FProcId.SliceCount, FMsgId.SliceCount, FStructuredDataName.SliceCount, FStructuredDataValue.SliceCount);
            
            //ResizeAndDispose will adjust the spread length and thereby call
            //the given constructor function for new slices and Dispose on old
            //slices.
            FStreamOut.ResizeAndDispose(spreadMax, () => new MemoryStream());

            for (int i = 0; i < spreadMax; i++)
            {
                if (FMessageIn.IsChanged || 
                    FFacility.IsChanged || 
                    FSeverity.IsChanged ||
                    FHostname.IsChanged ||
                    FAppName.IsChanged ||
                    FProcId.IsChanged ||
                    FMsgId.IsChanged /*||
                    FStructuredData.IsChanged*/ )
                {
                    //create msg
                    SyslogMessage msg = Create(FFacility[i], FSeverity[i], FMessageIn[i]);

                    // add properties
                    if (FHostname[i] != string.Empty) msg.HostName = FHostname[i];
                    if (FAppName[i] != string.Empty) msg.AppName = FAppName[i];
                    if (FProcId[i] != -1) msg.ProcessID = FProcId[i].ToString();
                    if (FMsgId[i] != -1) msg.MessageID = FMsgId[i].ToString();

                    // add structured Data
                    List<StructuredDataElement> sd = new List<StructuredDataElement>();

                    for (int s = 0; s < FStructuredDataName[i].SliceCount; s++)
                    {
                        StructuredDataElement sde = new StructuredDataElement();
                        sde.Properties.Add(FStructuredDataName[i][s], FStructuredDataValue[i][s]);

                        sd.Add(sde);
                    }
                    msg.StructuredData = sd;

                    
                    // convert to stream
                    byte[] byteMessage = System.Text.Encoding.ASCII.GetBytes(msg.ToIetfSyslogString());
                    Stream outputStream = FStreamOut[i];

                    // write out
                    outputStream.Position = 0;
                    outputStream.SetLength(byteMessage.Length);
                    outputStream.Write(byteMessage, 0, byteMessage.Length);
                }
            }
            //this will force the changed flag of the output pin to be set
            FStreamOut.Flush(true);
        }

    }

    #region PluginInfo
    [PluginInfo(Name = "Syslog",
                Version = "Split",
                Category = "Raw",
                Help = "Creates a (raw) Syslog message that can be sent to a syslog server",
                Tags = "Debug",
                Author = "sebl")]
    #endregion PluginInfo
    public class SyslogStringSplit : Syslog.SyslogMessage , IPluginEvaluate
    {

        #region fields & pins
        [Input("Raw In")]
        public IDiffSpread<Stream> FStreamIn;

        [Output("Message")]
        public ISpread<String> FMessage;

        [Output("Facility")]
        public ISpread<SyslogFacility> FFacility;

        [Output("Severity")]
        public ISpread<SyslogSeverity> FSeverity;

        [Output("Hostname")]
        public ISpread<string> FHostname;

        [Output("AppName")]
        public ISpread<string> FAppName;

        [Output("Process ID")]
        public ISpread<string> FProcId;

        [Output("Message ID")]
        public ISpread<string> FMsgId;

        [Output("Structured Data Name")]
        public ISpread<ISpread<string>> FStructuredDataName;

        [Output("Structured Data Value")]
        public ISpread<ISpread<string>> FStructuredDataValue;

        #endregion fields & pins

        //called when data for any output pin is requested
        public void Evaluate(int spreadMax)
        {
            spreadMax = FStreamIn.SliceCount;

            FMessage.SliceCount = spreadMax;
            FFacility.SliceCount = spreadMax;
            FSeverity.SliceCount = spreadMax;
            FHostname.SliceCount = spreadMax;
            FAppName.SliceCount = spreadMax;
            FProcId.SliceCount = spreadMax;
            FMsgId.SliceCount = spreadMax;
            FStructuredDataName.SliceCount = spreadMax;
            FStructuredDataValue.SliceCount = spreadMax;

            for (int i = 0; i < spreadMax; i++)
            {
                if (FStreamIn.IsChanged)
                {
                    // Stream > string
                    byte[] buffer = new byte[FStreamIn[i].Length];
                    int test = FStreamIn[i].Read(buffer, 0, (int)FStreamIn[i].Length);
                    string stringMessage = System.Text.Encoding.ASCII.GetString(buffer, 0, buffer.Length);

                    SyslogParser prs = new SyslogParser();
                    SyslogMessage msg = prs.Parse(stringMessage);

                    if (msg != null)
                    {
                        FMessage[i] = msg.MessageText;
                        FFacility[i] = msg.Facility;
                        FSeverity[i] = msg.Severity;
                        FHostname[i] = msg.HostName;
                        FAppName[i] = msg.AppName;
                        FProcId[i] = msg.ProcessID;
                        FMsgId[i] = msg.MessageID;

                        int c = msg.StructuredData.Count;

                        FStructuredDataName[i].SliceCount = c;
                        FStructuredDataValue[i].SliceCount = c;

                        for (int s =0; s < msg.StructuredData.Count; s++)
                        {
                            string key = msg.StructuredData[s].Properties.Keys[0];
                            string value = msg.StructuredData[s].Properties.Get(key);
                            FStructuredDataName[i][s] = key;
                            FStructuredDataValue[i][s] = value;
                        }
                    }
                }
            }
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
    public class LogNode :  IPluginEvaluate
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
