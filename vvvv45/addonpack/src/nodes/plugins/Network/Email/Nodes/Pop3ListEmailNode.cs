using System;
using System.Collections.Generic;
using System.Text;

using VVVV.PluginInterfaces.V1;
using System.Collections;

namespace VVVV.Nodes
{
    
    public class Pop3ListEmailNode : IPlugin, IDisposable
    {
        #region Plugin Info
        public static IPluginInfo PluginInfo
        {
            get
            {
                IPluginInfo Info = new PluginInfo();
                Info.Name = "ListEmails";							//use CamelCaps and no spaces
                Info.Category = "Network";						//try to use an existing one
                Info.Version = "POP3";						//versions are optional. leave blank if not needed
                Info.Help = "Retrieves Emails from a POP3 mailbox";
                Info.Bugs = "";
                Info.Credits = "";								//give credits to thirdparty code used
                Info.Warnings = "";
                Info.Author = "vux";
                Info.Tags = "pop3,email";

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
        private IStringIn FPinInHost;
        private IValueIn FPinInPort;
        private IStringIn FPinInUsername;
        private IStringIn FPinInPassword;
        private IValueIn FPinInCount;
        private IValueIn FPinInRefresh;

        private IStringOut FPinOutFrom;
        private IStringOut FPinOutSubject;
        private IStringOut FPinOutBody;
        //private 
        #endregion

        #region Auto Evaluate
        public bool AutoEvaluate
        {
            get { return false; }
        }
        #endregion

        #region Set Plugin Host
        public void SetPluginHost(IPluginHost Host)
        {
            //assign host
            this.FHost = Host;

            this.FHost.CreateStringInput("Host", TSliceMode.Single, TPinVisibility.True, out this.FPinInHost);
            this.FPinInHost.SetSubType("", false);

            this.FHost.CreateValueInput("Port", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInPort);
            this.FPinInPort.SetSubType(0, 65000, 1, 110, false, false, true);

            this.FHost.CreateStringInput("Username", TSliceMode.Single, TPinVisibility.True, out this.FPinInUsername);
            this.FPinInUsername.SetSubType("", false);

            this.FHost.CreateStringInput("Password", TSliceMode.Single, TPinVisibility.True, out this.FPinInPassword);
            this.FPinInPassword.SetSubType("", false);

            this.FHost.CreateValueInput("Count", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInCount);
            this.FPinInCount.SetSubType(0, Double.MaxValue, 1, 10, false, false, true);

            
            this.FHost.CreateValueInput("Update", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInRefresh);
            this.FPinInRefresh.SetSubType(0, 1, 1, 0, true, false, false);

            
            this.FHost.CreateStringOutput("From", TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutFrom);
            this.FPinOutFrom.SetSubType("", false);
        
            this.FHost.CreateStringOutput("Subject", TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutSubject);
            this.FPinOutSubject.SetSubType("", false);

            this.FHost.CreateStringOutput("Body", TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutBody);
            this.FPinOutBody.SetSubType("", false);
        
        
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
            double dblrefresh;
            this.FPinInRefresh.GetValue(0, out dblrefresh);

            if (dblrefresh >= 0.5)
            {
                string host, username, pwd;
                double dblport,dblcount;
                this.FPinInHost.GetString(0, out host);
                this.FPinInPort.GetValue(0, out dblport);
                this.FPinInUsername.GetString(0, out username);
                this.FPinInPassword.GetString(0, out pwd);
                this.FPinInCount.GetValue(0, out dblcount);

                Higuchi.Net.Pop3.Pop3Client client = new Higuchi.Net.Pop3.Pop3Client();
                client.ServerName = host;
                client.Port = Convert.ToInt32(dblport);
                client.UserName = username;
                client.Password = pwd;
                if (client.Authenticate())
                {

                    long Count = client.GetTotalMessageCount();



                    
                    int max = Math.Min(Convert.ToInt32(dblcount), Convert.ToInt32(Count));

                    this.FPinOutSubject.SliceCount = max;
                    this.FPinOutBody.SliceCount = max;
                    this.FPinOutFrom.SliceCount = max;
                    
                    long idx = Count;
                    for (int i = 0; i < max; i++)
                    {
                        Higuchi.Net.Pop3.Pop3Message msg = client.GetMessage(idx);
                        this.FPinOutFrom.SetString(i, msg.From);
                        this.FPinOutSubject.SetString(i, msg.Subject);
                        this.FPinOutBody.SetString(i, msg.BodyText);
                        idx--;
                    }
                }
                else
                {
                    this.FHost.Log(TLogType.Error, "Authentication failed");
                    this.FPinOutBody.SliceCount = 0;
                    this.FPinOutSubject.SliceCount = 0;
                    this.FPinOutFrom.SliceCount = 0;
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
