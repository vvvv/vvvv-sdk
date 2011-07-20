using System;
using System.ComponentModel.Composition;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;

using VVVV.Core.Logging;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using System.Text;

namespace VVVV.Nodes
{

    #region PluginInfo
    [PluginInfo(Name = "SendEmail",
                Category = "Network",
                Help = "Send an email via smtp",
                Author = "phlegma",
                Credits = "vux",
                Tags = "email,Smtp",
                AutoEvaluate = true)]
    #endregion PluginInfo
    public class SendEmailNode : IPluginEvaluate
    {

        [Input("Host", IsSingle = true, DefaultString = "smtp.googlemail.com")]
        ISpread<string> FPinInHost;

        [Input("Port", IsSingle = true, MinValue = 0, MaxValue = 49151, AsInt = true, DefaultValue=587)]
        IDiffSpread<int> FPinInPort;

        [Input("Username", IsSingle = true, DefaultString="User@gmail.com")]
        IDiffSpread<string> FPinInUsername;

        [Input("Password", IsSingle = true , DefaultString="password")]
        IDiffSpread<string> FPinInPassword;

        [Input("Use SSL", IsSingle= true, DefaultValue=1)]
        IDiffSpread<bool> FPinInSSL;

        [Input("From", DefaultString="User@gmail.com")]
        IDiffSpread<string> FPinInFrom;

        [Input("To", DefaultString = "User@gmail.com")]
        IDiffSpread<string> FPinInTo;

        [Input("Subject")]
        IDiffSpread<string> FPinInSubject;

        [Input("Message")]
        IDiffSpread<string> FPinInMessage;

        [Input("EmailEncoding", EnumName = "EmailEncoding")]
        IDiffSpread<EnumEntry> FEmailEncoding;

        [Input("AsHtml")]
        IDiffSpread<bool> FPinInIsHtml;

        [Input("Attachment", StringType = StringType.Filename)]
        IDiffSpread<string> FPinInAttachment;

        [Output("Success")]
        ISpread<bool> FPinOutSuccess;

        [Input("Send", IsBang = true, IsSingle=true)]
        IDiffSpread<bool> FPinInDoSend;

        [Import()]
        ILogger FLogger;

        [ImportingConstructor]
        public SendEmailNode()
		{ 
			var s = new string[]{"US-ASCII","UTF8", "UTF32","Unicode"};
			//Please rename your Enum Type to avoid 
			//numerous "MyDynamicEnum"s in the system
		    EnumManager.UpdateEnum("EmailEncoding", "US-ASCII", s);  
		}

        #region Evaluate
        public void Evaluate(int SpreadMax)
        {
            if (FPinInDoSend.IsChanged && FPinInDoSend[0])
            {
                string Username = FPinInUsername[0];
                string Pwd = FPinInPassword[0];
                if (Username == null) { Username = ""; }
                if (Pwd == null) { Pwd = ""; }

                SmtpClient EmailClient = new SmtpClient(FPinInHost[0], FPinInPort[0]);
                EmailClient.EnableSsl = FPinInSSL[0];

                if (Username.Length > 0 && Pwd.Length > 0)
                {
                    NetworkCredential SMTPUserInfo = new NetworkCredential(Username, Pwd);
                    EmailClient.Credentials = SMTPUserInfo;
                }

                FPinOutSuccess.SliceCount = SpreadMax;

                for (int i = 0; i < SpreadMax; i++)
                {
                    try
                    {
                        string message = FPinInMessage[i];


                        UTF8Encoding utf8 = new UTF8Encoding();
                        byte[] byteArray = Encoding.Unicode.GetBytes(FPinInMessage[i]);
                        byte[] utf8Array = Encoding.Convert(Encoding.Unicode, Encoding.UTF8, byteArray);
                        string finalString = utf8.GetString(utf8Array);


                        MailMessage mail = new MailMessage(FPinInFrom[i], FPinInTo[i], FPinInSubject[i], finalString);
                        mail.IsBodyHtml = FPinInIsHtml[i];

                        switch (FEmailEncoding[i].Index)
                        {
                            case (0):
                                mail.BodyEncoding = Encoding.ASCII;
                                break;
                            case (1):
                                mail.BodyEncoding = Encoding.UTF8;
                                break;
                            case (2):
                                mail.BodyEncoding = Encoding.UTF32;
                                break;
                            case (3):
                                mail.BodyEncoding = Encoding.Unicode;
                                break;
                            default:
                                mail.BodyEncoding = Encoding.ASCII;
                                break;
                        }

                        if (!String.IsNullOrEmpty(FPinInAttachment[i]))
                        {
                            Attachment Attachment = new Attachment(FPinInAttachment[i]);
                            ContentDisposition Disposition = Attachment.ContentDisposition;
                            Disposition.Inline = false;
                            mail.Attachments.Add(Attachment);
                        }


                        EmailClient.Send(mail);
                        FPinOutSuccess[i] = true;
                    }
                    catch (Exception ex)
                    {
                        FLogger.Log(LogType.Error, ex.Message);
                        FPinOutSuccess[i] = false;
                    }

                }
            }
            else
            {
                this.FPinOutSuccess.SliceCount = 0;
            }
        }
        #endregion

    }


}
