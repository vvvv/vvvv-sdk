using System;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V1;
using System.Diagnostics;

namespace VVVV.Nodes
{
    
    public class EventLogWriterNode : IPlugin, IDisposable
    {
        #region Plugin Info
        public static IPluginInfo PluginInfo
        {
            get
            {
                IPluginInfo Info = new PluginInfo();
                Info.Name = "Writer";							//use CamelCaps and no spaces
                Info.Category = "Windows";						//try to use an existing one
                Info.Version = "EventLog";						//versions are optional. leave blank if not needed
                Info.Help = "Writes into the windows event log";
                Info.Bugs = "";
                Info.Credits = "";								//give credits to thirdparty code used
                Info.Warnings = "";
                Info.Author = "vux";
                Info.Tags = "windows,event,log";

                //leave below as is
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(true);
                System.Diagnostics.StackFrame sf = st.GetFrame(0);
                System.Reflection.MethodBase method = sf.GetMethod();
                Info.Namespace = method.DeclaringType.Namespace;
                Info.Class = method.DeclaringType.Name;
                return Info;
                //leave above as is
            }
        }
        #endregion

        #region Fields
        private IPluginHost FHost;

        private IStringIn FPinInName;
        private IStringIn FPinInSource;
        private IStringIn FPinInMessage;
        private IEnumIn FPinInType;
        private IValueIn FPinInEventID;
        private IValueIn FPinInDoWrite;
        #endregion

        #region Auto Evaluate
        public bool AutoEvaluate
        {
            get { return true; }
        }
        #endregion

        #region Set Plugin Host
        public void SetPluginHost(IPluginHost Host)
        {
            //assign host
            this.FHost = Host;

            this.FHost.UpdateEnum("Windows Event Log Types", "Information", Enum.GetNames(typeof(EventLogEntryType)));

            //this.FHost.CreateStringInput("Log Name", TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInName);
            //this.FPinInName.SetSubType("Application", false);

            this.FHost.CreateStringInput("Source", TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInSource);
            this.FPinInSource.SetSubType("", false);
    
            this.FHost.CreateStringInput("Message", TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInMessage);
            this.FPinInMessage.SetSubType("", false);

            this.FHost.CreateEnumInput("Event Type",TSliceMode.Dynamic,TPinVisibility.True, out this.FPinInType);
            this.FPinInType.SetSubType("Windows Event Log Types");

            this.FHost.CreateValueInput("Event ID", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInEventID);
            this.FPinInEventID.SetSubType(0, double.MaxValue, 1, 0, false,false, true);

            this.FHost.CreateValueInput("Write", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInDoWrite);
            this.FPinInDoWrite.SetSubType(0, 1, 1, 0, true,false, false);
        
        
        }
        #endregion

        #region Configurate
        public void Configurate(IPluginConfig Input)
        {
        }
        #endregion

        #region Evaluate
        public void Evaluate(int SpreadMax)
        {
            for (int i = 0; i < SpreadMax; i++)
            {
                string src,msg,type,name;
                double dblid,dbldowrite;

                this.FPinInDoWrite.GetValue(i, out dbldowrite);

                if (dbldowrite >= 0.5)
                {
                    //this.FPinInName.GetString(i, out name);
                    this.FPinInSource.GetString(i, out src);
                    this.FPinInMessage.GetString(i, out msg);
                    this.FPinInType.GetString(i, out type);
                    this.FPinInEventID.GetValue(i, out dblid);

                    EventLogEntryType et = (EventLogEntryType)Enum.Parse(typeof(EventLogEntryType), type);

                    if (src == null) { src = String.Empty; }
                    if (msg == null) { msg = String.Empty; }
                    //if (name == null) { name = "Application"; }

                    try
                    {
                        //EventLog log = new EventLog(name);
                        EventLog.WriteEntry(src, msg, et, Convert.ToInt32(dblid));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }
        #endregion

        #region Dispose
        public void Dispose()
        {
        }
        #endregion
    }
        
        
}
