using System;
using System.ComponentModel.Composition;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.IO;
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

        [Input("From", IsSingle = true, DefaultString = "User@gmail.com")]
        IDiffSpread<string> FPinInFrom;

        [Input("To", IsSingle = true, DefaultString = "User@gmail.com")]
        IDiffSpread<string> FPinInTo;

        [Input("Subject", IsSingle = true)]
        IDiffSpread<string> FPinInSubject;

        [Input("Message", IsSingle = true)]
        IDiffSpread<string> FPinInMessage;

        [Input("Message Path", IsSingle = true, StringType = StringType.Filename)]
        IDiffSpread<string> FPinMassagePath;

        [Input("EmailEncoding", IsSingle = true, EnumName = "EmailEncoding")]
        IDiffSpread<EnumEntry> FEmailEncoding;

        [Input("AsHtml", IsSingle = true)]
        IDiffSpread<bool> FPinInIsHtml;

        [Input("Attachment", IsSingle = true, StringType = StringType.Filename)]
        IDiffSpread<string> FPinInAttachment;

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
                EmailClient.SendCompleted += new SendCompletedEventHandler(EmailClient_SendCompleted);

                if (Username.Length > 0 && Pwd.Length > 0)
                {
                    NetworkCredential SMTPUserInfo = new NetworkCredential(Username, Pwd);
                    EmailClient.Credentials = SMTPUserInfo;
                }

                FPinOutSuccess.SliceCount = SpreadMax;

                //for (int i = 0; i < SpreadMax; i++)
                //{
                try
                {
                    //string message = FPinInMessage[0];

                    //Debug.WriteLine(message);

                    //Encoding VVVV = Encoding.UTF8;
                    //byte[] byteArray = VVVV.GetBytes(message);
                    //byte[] ConvertedArray = Encoding.Convert(VVVV, Encoding.Unicode, byteArray);
                    //string finalString = Encoding.Unicode.GetString(ConvertedArray);
                    //Debug.WriteLine(finalString);


                    //// Convert the UTF-16 encoded source string to UTF-8 and ASCII.
                    //byte[] utf8String = Encoding.UTF8.GetBytes(message);
                    //byte[] asciiString = Encoding.ASCII.GetBytes(message);

                    //// Write the UTF-8 and ASCII encoded byte arrays. 
                    //Debug.WriteLine("UTF-8  Bytes: {0}", BitConverter.ToString(utf8String));
                    //Debug.WriteLine("ASCII  Bytes: {0}", BitConverter.ToString(asciiString));


                    //// Convert UTF-8 and ASCII encoded bytes back to UTF-16 encoded  
                    //// string and write.
                    //Debug.WriteLine("UTF-8  Text : {0}", Encoding.UTF8.GetString(utf8String));
                    //Debug.WriteLine("ASCII  Text : {0}", Encoding.ASCII.GetString(asciiString));

                    //Console.WriteLine(Encoding.UTF8.GetString(asciiString));
                    //Console.WriteLine(Encoding.ASCII.GetString(asciiString));

                    string Message = "";

                    if (File.Exists(FPinMassagePath[0]))
                    {
                        FileStream MessageFile = new FileStream(FPinMassagePath[0], FileMode.Open);
                        StreamReader reader = new StreamReader(MessageFile);
                        Message = reader.ReadToEnd();
                        //Debug.WriteLine(Message);
                        reader.Close();
                        MessageFile.Close();
                    }
                    else
                    {
                        Message = FPinInMessage[0];
                    }


                    MailMessage mail = new MailMessage(FPinInFrom[0], FPinInTo[0], FPinInSubject[0], Message);
                    mail.IsBodyHtml = FPinInIsHtml[0];

                    switch (FEmailEncoding[0].Index)
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
                            mail.BodyEncoding = Encoding.UTF8;
                            break;
                    }

                    if (File.Exists(FPinInAttachment[0]))
                    {
                        Attachment Attachment = new Attachment(FPinInAttachment[0]);
                        ContentDisposition Disposition = Attachment.ContentDisposition;
                        Disposition.Inline = false;
                        mail.Attachments.Add(Attachment);
                    }



                    EmailClient.SendAsync(mail,0);

                }
                catch (Exception ex)
                {
                    FLogger.Log(LogType.Error, ex.Message);
                    //FPinOutSuccess[i] = false;
                }
                finally
                {
                    EmailClient = null;
                }



                //}
            }
            else
            {
                this.FPinOutSuccess.SliceCount = 0;
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
            int token = (int)e.UserState;

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

            FPinOutSuccess[0] = true;
        }
        #endregion

    }


}
