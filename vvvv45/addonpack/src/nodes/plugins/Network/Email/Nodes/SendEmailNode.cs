using System;
using System.Collections.Generic;
using System.Text;

using VVVV.PluginInterfaces.V1;
using System.Net.Mail;
using System.Net;

namespace VVVV.Nodes
{
    
    public class SendEmailNode : IPlugin, IDisposable
    {
        #region Plugin Info
        public static IPluginInfo PluginInfo
        {
            get
            {
                IPluginInfo Info = new PluginInfo();
                Info.Name = "SendEmail";							//use CamelCaps and no spaces
                Info.Category = "Network";						//try to use an existing one
                Info.Version = "";						//versions are optional. leave blank if not needed
                Info.Help = "Send an email via smtp";
                Info.Bugs = "";
                Info.Credits = "";								//give credits to thirdparty code used
                Info.Warnings = "";
                Info.Author = "vux";
                Info.Tags = "smtp,email";

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
        private IStringIn FPinInFrom;
        private IStringIn FPinInTo;
        //private IStringIn FPinInCC;
        private IStringIn FPinInSubject;
        private IStringIn FPinInMessage;
        private IValueIn FPinInDoSend;
        private IValueOut FPinOutSuccess;
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

            
            this.FHost.CreateStringInput("Host", TSliceMode.Single, TPinVisibility.True, out this.FPinInHost);
            this.FPinInHost.SetSubType("", false);

            this.FHost.CreateValueInput("Port", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInPort);
            this.FPinInPort.SetSubType(0, 65000, 1, 25, false, false, true);

            this.FHost.CreateStringInput("Username", TSliceMode.Single, TPinVisibility.True, out this.FPinInUsername);
            this.FPinInUsername.SetSubType("", false);

            this.FHost.CreateStringInput("Password", TSliceMode.Single, TPinVisibility.True, out this.FPinInPassword);
            this.FPinInPassword.SetSubType("", false);
            
            this.FHost.CreateStringInput("From", TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInFrom);
            this.FPinInFrom.SetSubType("", false);

            this.FHost.CreateStringInput("To", TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInTo);
            this.FPinInTo.SetSubType("", false);

            this.FHost.CreateStringInput("Subject", TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInSubject);
            this.FPinInSubject.SetSubType("", false);

            this.FHost.CreateStringInput("Message", TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInMessage);
            this.FPinInMessage.SetSubType("", false);

            
            this.FHost.CreateValueInput("Send", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInDoSend);
            this.FPinInDoSend.SetSubType(0, 1, 1, 0, true, false, false);

            this.FHost.CreateValueOutput("Success", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutSuccess);
            this.FPinOutSuccess.SetSubType(0, 1, 1, 0, false, true, false);
    
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
            double dblsend;
            this.FPinInDoSend.GetValue(0, out dblsend);

            if (dblsend >= 0.5)
            {
                string host, username, pwd ;
                double dblport;
                this.FPinInHost.GetString(0, out host);
                this.FPinInPort.GetValue(0, out dblport);
                this.FPinInUsername.GetString(0, out username);
                this.FPinInPassword.GetString(0, out pwd);

                if (username == null) { username = ""; }
                if (pwd == null) { pwd = ""; }

                SmtpClient emailClient = new SmtpClient(host, Convert.ToInt32(dblport));
                if (username.Length > 0 && pwd.Length > 0)
                {
                    NetworkCredential SMTPUserInfo = new NetworkCredential(username, pwd);
                    emailClient.Credentials = SMTPUserInfo;
                }

                this.FPinOutSuccess.SliceCount = SpreadMax;

                for (int i = 0; i < SpreadMax; i++)
                {
                    string from, to, subject, msg;


                    this.FPinInFrom.GetString(i, out from);
                    this.FPinInTo.GetString(i, out  to);
                    this.FPinInSubject.GetString(i, out  subject);
                    this.FPinInMessage.GetString(i, out msg);

                    try
                    {
                        MailMessage mail = new MailMessage(from, to, subject, msg);
                        emailClient.Send(mail);
                        this.FPinOutSuccess.SetValue(i, 1);
                    }
                    catch (Exception ex)
                    {
                        this.FHost.Log(TLogType.Error, ex.Message);
                        this.FPinOutSuccess.SetValue(i, 0);
                    }

                }
            }
            else
            {
                this.FPinOutSuccess.SliceCount = 0;
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
