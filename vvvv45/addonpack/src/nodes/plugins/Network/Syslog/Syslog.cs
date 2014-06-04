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
                Version = "join", 
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

        [Input("AppName", DefaultString = "")]
        public IDiffSpread<string> FAppName;

        [Input("Process ID", DefaultString = "")]
        public IDiffSpread<string> FProcId;

        [Input("Message ID", DefaultString = "")]
        public IDiffSpread<string> FMsgId;

        [Input("Structured Data Name", DefaultString = "")]
        public ISpread<ISpread<string>> FStructuredDataName;

        [Input("Structured Data Value", DefaultString = "")]
        public ISpread<ISpread<string>> FStructuredDataValue;

        [Input("Structured Data ID", DefaultString = "vvvv", Visibility = PinVisibility.OnlyInspector, IsSingle = true)]
        public ISpread<string> FSDID;

        [Input("Enterprise ID", DefaultString = "44444", Visibility = PinVisibility.OnlyInspector, IsSingle = true)]
        public ISpread<string> FEID;

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
                    if (FProcId[i] != string.Empty) msg.ProcessID = FProcId[i];
                    if (FMsgId[i] != string.Empty) msg.MessageID = FMsgId[i];

                    // add structured Data
                    List<StructuredDataElement> sd = new List<StructuredDataElement>();
                    StructuredDataElement sde = new StructuredDataElement();
                    sde.ID = FSDID[0] + "@" + FEID[0];  // set id according to rfc5424 ... the enterprise ID is a wildcard anyway

                    for (int s = 0; s < FStructuredDataName[i].SliceCount; s++)
                    {
                        sde.Properties.Add(FStructuredDataName[i][s], FStructuredDataValue[i][s]);
                    }
                    sd.Add(sde);
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
                Version = "split",
                Category = "Raw",
                Help = "splits a (raw) Syslog message into its atomics",
                Tags = "Debug",
                Author = "sebl")]
    #endregion PluginInfo
    public class SyslogStringSplit : Syslog.SyslogMessage , IPluginEvaluate
    {
        #region fields & pins
        [Input("Raw In")]
        public ISpread<Stream> FStreamIn;

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

        //ISpread<Stream> oldStream;

        #endregion fields & pins

        //called when data for any output pin is requested
        public void Evaluate(int spreadMax)
        {
            FMessage.SliceCount = spreadMax;
            FFacility.SliceCount = spreadMax;
            FSeverity.SliceCount = spreadMax;
            FHostname.SliceCount = spreadMax;
            FAppName.SliceCount = spreadMax;
            FProcId.SliceCount = spreadMax;
            FMsgId.SliceCount = spreadMax;
            FStructuredDataName.SliceCount = spreadMax;
            FStructuredDataValue.SliceCount = spreadMax;

            //bool testor = oldStream != FStreamIn;

            if (FStreamIn.IsChanged)  //IsChanged doesn't work with Streams
            {
                //if (firstrun == true) firstrun = false;
                //oldStream = FStreamIn;


                for (int i = 0; i < spreadMax; i++)
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

                        int sdCount = msg.StructuredData.Count; // is normally 1 (when created with Syslog (Raw join), because it can't pack several sd packages )

                        FStructuredDataName[i].SliceCount = 0;
                        FStructuredDataValue[i].SliceCount = 0;
                        
                        for (int s = 0; s < sdCount; s++)
                        {
                            // each sd contains n sd elements
                            int keycount = msg.StructuredData[s].Properties.Count;

                            for (int k =0; k < keycount; k++)
                            {
                                FStructuredDataName[i].Add<string>(msg.StructuredData[s].Properties.AllKeys[k]);
                                FStructuredDataValue[i].Add<string>(msg.StructuredData[s].Properties.GetValues(k)[0]);
                            }
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
