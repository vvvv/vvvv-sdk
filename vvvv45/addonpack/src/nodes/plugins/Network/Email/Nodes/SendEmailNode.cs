using System;
using System.ComponentModel.Composition;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Threading;

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

        [Input("Host", DefaultString = "smtp.googlemail.com")]
        ISpread<string> FPinInHost;

        [Input("Port", MinValue = 0, MaxValue = 49151, AsInt = true, DefaultValue=587)]
        IDiffSpread<int> FPinInPort;

        [Input("Username", DefaultString="User@gmail.com")]
        IDiffSpread<string> FPinInUsername;

        [Input("Password", DefaultString="password")]
        IDiffSpread<string> FPinInPassword;

        [Input("Use SSL", DefaultValue=1)]
        IDiffSpread<bool> FPinInSSL;

        [Input("From", DefaultString = "User@gmail.com")]
        IDiffSpread<string> FPinInFrom;

        [Input("To", DefaultString = "User@gmail.com")]
        IDiffSpread<string> FPinInTo;

        [Input("Subject")]
        IDiffSpread<string> FPinInSubject;

        [Input("Message")]
        IDiffSpread<string> FPinInMessage;

        [Input("EmailEncoding", EnumName = "EmailEncoding")]
        IDiffSpread<EnumEntry> FPinInEmailEncoding;

        [Input("Accept Html")]
        IDiffSpread<bool> FPinInIsHtml;

        [Input("Attachment", StringType = StringType.Filename)]
        ISpread<ISpread<string>> FPinInAttachment;

        [Output("Success")]
        ISpread<bool> FPinOutSuccess;

        [Input("Send", IsBang = true, IsSingle=true)]
        IDiffSpread<bool> FPinInDoSend;

        [Import()]
        ILogger FLogger;

        string FError = "";

        [ImportingConstructor]
        public SendEmailNode()
        { 
            var s = new string[]{"Ansi","Ascii","UTF8", "UTF32","Unicode"};
            //Please rename your Enum Type to avoid 
            //numerous "MyDynamicEnum"s in the system
            EnumManager.UpdateEnum("EmailEncoding", "Ansi", s);  
        }

        #region Evaluate
        public void Evaluate(int SpreadMax)
        {
            if (FPinInDoSend.IsChanged)
            {
                SpreadMax = SpreadUtils.SpreadMax(FPinInAttachment, FPinInDoSend, FPinInEmailEncoding, FPinInFrom, FPinInHost, FPinInIsHtml, FPinInMessage, FPinInPassword, FPinInPort, FPinInSSL, FPinInSubject, FPinInTo, FPinInUsername);
                FPinOutSuccess.SliceCount = SpreadMax;
                for (int i = 0; i < SpreadMax; i++)
                {
                    if (FPinInDoSend[i])
                    {
                        string Username = FPinInUsername[i];
                        string Pwd = FPinInPassword[i];
                        if (Username == null) { Username = ""; }
                        if (Pwd == null) { Pwd = ""; }

                        SmtpClient EmailClient = new SmtpClient(FPinInHost[i], FPinInPort[i]);
                        EmailClient.EnableSsl = FPinInSSL[i];
                        EmailClient.SendCompleted += new SendCompletedEventHandler(EmailClient_SendCompleted);

                        if (Username.Length > 0 && Pwd.Length > 0)
                        {
                            NetworkCredential SMTPUserInfo = new NetworkCredential(Username, Pwd);
                            EmailClient.Credentials = SMTPUserInfo;
                        }

                        FPinOutSuccess[i] = false;

                        try
                        {
                            string Message = FPinInMessage[i];
                            string Subject = FPinInSubject[i];

                            MailMessage mail = new MailMessage(FPinInFrom[i], FPinInTo[i]);

                            //Convert the Incomming Message to the corresponding encoding
                            switch (FPinInEmailEncoding[i].Index)
                            {
                                case (0):
                                    mail.BodyEncoding = mail.SubjectEncoding = Encoding.Default;
                                    break;
                                case (1):
                                    mail.BodyEncoding = mail.SubjectEncoding = Encoding.ASCII;
                                    break;
                                case (2):
                                    mail.BodyEncoding = mail.SubjectEncoding = Encoding.UTF8;
                                    break;
                                case (3):
                                    mail.BodyEncoding = mail.SubjectEncoding = Encoding.UTF32;
                                    break;
                                case (4):
                                    mail.BodyEncoding = mail.SubjectEncoding = Encoding.Unicode;
                                    break;
                                default:
                                    mail.BodyEncoding = mail.SubjectEncoding = Encoding.Default;
                                    break;
                            }

                            mail.Subject = Subject;
                            mail.Body = Message;
                            mail.IsBodyHtml = FPinInIsHtml[i];

                            foreach (var filename in FPinInAttachment[i])
                            {
                                if (File.Exists(filename))
                                {
                                    Attachment Attachment = new Attachment(filename);
                                    ContentDisposition Disposition = Attachment.ContentDisposition;
                                    Disposition.Inline = false;
                                    mail.Attachments.Add(Attachment);
                                }
                            }

                            EmailClient.SendAsync(mail, i);
                        }
                        catch (Exception ex)
                        {
                            FLogger.Log(LogType.Error, ex.Message);
                        }
                        finally
                        {
                            EmailClient = null;
                        }
                    }
                }
            }

            if (!String.IsNullOrEmpty(FError))
            {
                FLogger.Log(LogType.Debug, FError);
                FError = "";
            }
        }

        void EmailClient_SendCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            // Get the unique identifier for this asynchronous operation.
            int index = (int)e.UserState;

            if (e.Cancelled)
            {
                FError = "Cancelt";
            }
            if (e.Error != null)
            {
                FError = e.Error.ToString();
            }
            else
            {
                FError = "Message sent.";
            }

            FPinOutSuccess[index] = true;
        }
        #endregion

    }


}
