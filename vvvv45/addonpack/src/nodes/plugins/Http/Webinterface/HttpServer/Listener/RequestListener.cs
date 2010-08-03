using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Xml;
using System.IO;
using System.Net;



namespace VVVV.Webinterface.HttpServer
{

    /// <summary>
    /// Handles and analyse a HTTP Request.
    /// </summary>
    class RequestListener
    {


        private string mRequestType;
        private string mHttpVersion;
        private string mFilename;
        private string mFileLocation;
        private HttpListenerRequest FRequest;
        private SortedDictionary<string, string> mRequestHeadParameterList = new SortedDictionary<string, string>();
        private SortedDictionary<string, string> mRequestBodyParameterList = new SortedDictionary<string, string>();
        
        private string mMessageHead = "";
        private string mMessageBody = "";
        private Dictionary<string,string> mArguments = new Dictionary<string,string>();
        private Response mResponse;
        private WebinterfaceSingelton mWebinterfaceSingelton = WebinterfaceSingelton.getInstance();

        private SortedList<string, byte[]> mHtmlPages;
        List<string> mFolderToServ;
        private SortedList<string, string> mPostMessages = new SortedList<string, string>();

        # region public properties

        public string RequestType
        {
            get
            {
                return mRequestType;
            }
        }

        public string HttpVersion
        {
            get
            {
                return mHttpVersion;
            }
        }

        public string FileName
        {
            get
            {
                return mFilename;
            }
        }

        public string FileLocation
        {
            get
            {
                return mFileLocation;
            }
        }

        public SortedDictionary<string,string> ParameterList
        {
            get 
            {
                return mRequestHeadParameterList;
            }

        }

        public string MessageHead
        {
            get
            {
                return mMessageHead;
            }
        }

        public string MessageBody
        {
            get
            {
                return mMessageBody;
            }
        }

        public Response Response
        {
            get
            {
                return mResponse;
            }
        }

        # endregion public properties








        # region constructor

        public RequestListener(List<string> pFolderToServ, HttpListenerRequest pRequest, SortedList<string,string> pPostMessages)
        {



            this.mFolderToServ = pFolderToServ;
            this.FRequest = pRequest;
            this.mPostMessages = pPostMessages;

            this.mFileLocation = pRequest.Url.LocalPath;
                
            int SegementLength = pRequest.Url.Segments.Length;
            this.mFilename = pRequest.Url.Segments[SegementLength - 1];
            this.mRequestType = pRequest.HttpMethod;
            //mMessageHead = pRequest.Substring(0,pRequest.IndexOf("\r\n\r\n"));
            mMessageBody = ReadRequestContent(pRequest);
            //mMessageBody = mMessageBody.TrimStart(new Char[] { '\n', '\r', '?' });
            //string[] tHeadLines = mMessageHead.Split('\n');
            //SplitHeadParameter(tHeadLines);
            //SplitFirstLine(tHeadLines[0]);





            if (FRequest.HttpMethod == "GET" || FRequest.HttpMethod == "OPTIONS")
            {
                GetRequest();
            }

            else if (FRequest.HttpMethod == "POST")
            {

                PostRequest();
            }
            else
            {
                mResponse = new Response(mFilename, Encoding.UTF8.GetBytes("Error in Request Handling"), new HTTPStatusCode("").Code500);
            }    
        }


        private string ReadRequestContent(HttpListenerRequest request)
        {


            if (!request.HasEntityBody)
            {
                //Debug.WriteLine("No client data was sent with the request.");
                return "No client data was sent with the request.";
            }
            System.IO.Stream body = request.InputStream;
            System.Text.Encoding encoding = request.ContentEncoding;
            System.IO.StreamReader reader = new System.IO.StreamReader(body, encoding);
            if (request.ContentType != null)
            {
                //Debug.WriteLine("Client data content type {0}", request.ContentType);
            }
            //Debug.WriteLine(String.Format("Client data content length {0}", request.ContentLength64));

            //Debug.WriteLine("Start of client data:");
            // Convert the data to a string and display it on the Debug.
            string RequestContent = reader.ReadToEnd();
            //Debug.WriteLine(RequestContent);
            //Debug.WriteLine("End of client data:");
            body.Close();
            reader.Close();




            return RequestContent;
            // If you are finished with the request, it should be closed also.
        }

        #endregion constructor

        #region Analyse Http Head


        private void SplitHeadParameter(string[] pParameter)
        {

                int tLength = pParameter.Length;
                for (int i = 1; i < pParameter.Length; i++)
                {
                    string line = pParameter[i];
                    if (line.Contains(":"))
                    {
 
                        try
                        {
                        mRequestHeadParameterList.Add(line.Substring(0, line.IndexOf(":")), line.Substring(line.LastIndexOf(":") + 1));
                        //////Debug.WriteLine(line.Substring(0, line.IndexOf(":")) + ":" + line.Substring(line.LastIndexOf(":") + 1));
                        }catch(Exception ex)
                        {
                                    
                            Debug.WriteLine(" Error in Reading page Header " + Environment.NewLine + ex.Message);
                        }

                    }
                }
        }

        private void SplitFirstLine(string pFirstLine)
        {
            // RequestType
            string[] words = pFirstLine.Split(' ');


            mRequestType = words[0];
            //////Debug.WriteLine("RequestType: " + mRequestType);

            mHttpVersion = words[2].Substring(words[2].LastIndexOf('/') + 1);
            //////Debug.WriteLine("HttpVersion: " + mHttpVersion);

            if (words[1] == "/" && words[0] == "GET")
            {
                mFileLocation = words[1];
                mFilename = "index.html";
            }
            else
            {
                //FileName & Location
                string p01 = @"[?][\w\W]*$";
                mFileLocation = Regex.Replace(words[1], p01, "");
                mFilename = mFileLocation.Substring(mFileLocation.LastIndexOf('/') + 1);
            }

            //////Debug.WriteLine("mFilename: " +  mFilename);
            //////Debug.WriteLine("mFileLocation: " + mFileLocation);

            //GetProperties
            if(mRequestType =="GET" || mRequestType == "OPTIONS")
            {
                if (words[1].Contains("?"))
                {
                    string p02 = @"[\w\W]+[?]";
                    string getProperties = Regex.Replace(words[1], p02, "");
                    mMessageBody = getProperties;

                    string[] ParamterPairs = getProperties.Split('&');

                    foreach (string pPair in ParamterPairs)
                    {
                        string[] pGetPostParameters = pPair.Split('=');
                        if (pGetPostParameters.Length > 1)
                        {
                            mRequestBodyParameterList.Add(pGetPostParameters[0], pGetPostParameters[1]);
                        }
                    }
                }
                else
                {
                    return;
                }
            }
         }

        private string DetectedFileExtension(string pContenType)
        {
            string tReqeustedFileExtension = "unknown";

            Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey("MIME\\Database\\Content Type");

            foreach (string keyName in key.GetSubKeyNames())
            {
                Microsoft.Win32.RegistryKey temp = key.OpenSubKey(keyName);

                if (pContenType.Equals(keyName))
                {
                    if (temp.GetValue("Extension") != null)
                    {
                       tReqeustedFileExtension = temp.GetValue("Extension").ToString();
                    }
                }
            }

            return tReqeustedFileExtension;
        }




        

        #endregion Analyse Http Head





        #region GET Request


        private void GetRequest()
        {
             mWebinterfaceSingelton.setResponseMessage(mMessageBody, mRequestType);
             mResponse = new Response(mFilename,new LoadSelectContent(mFilename, mFileLocation, mFolderToServ).ContentAsBytes, new HTTPStatusCode("").Code200);
             
        }


        #endregion GET Request



        #region POST Request


        private void PostRequest()
        {
            //string tContentTypeHeader;
            //mRequestHeadParameterList.TryGetValue("Content-Type", out tContentTypeHeader);

            //string[] tValues = tContentTypeHeader.Split(';');

            //string ContentType = tValues[0];
            //ContentType = ContentType.Trim();

            string tReqeustedFileExtension = String.Empty;


            mWebinterfaceSingelton.setResponseMessage(mMessageBody, mRequestType);
            //string tContentType = String.Empty;


            //mRequestHeadParameterList.TryGetValue("Content-Type", out tContentType);
            mResponse = new Response(mFilename, FRequest.Headers.Get("Content-Type"), Encoding.UTF8.GetBytes("VVVV Received POST Request, but file not found"), new HTTPStatusCode("").Code404);
            
            string tRemoteIPAdresse = FRequest.RemoteEndPoint.ToString();
            tRemoteIPAdresse = tRemoteIPAdresse.Split(':')[0];
            string[] tVVVVParameter = mMessageBody.Split('&');

            switch (mFilename)
            {

                case "ToVVVV.xml":
                        //string tRemoteIPAdresse = mSocketInformation.ClientSocket.RemoteEndPoint.ToString();
                        //tRemoteIPAdresse = tRemoteIPAdresse.Split(':')[0];
                        //string[] tVVVVParameter = mMessageBody.Split('&');

                        foreach (string tValuePair in tVVVVParameter)
                        {
                            string[] tValue = tValuePair.Split('=');

                            if (tValue.Length > 1)
                            {
                                mWebinterfaceSingelton.setNewBrowserDaten(tValue[0], tValue[1]);
                            }
                        }
                        mResponse = new Response(mFilename, FRequest.Headers.Get("Content-Type"), Encoding.UTF8.GetBytes("VVVV Received Post Request"), new HTTPStatusCode("").Code200);
                    break;




                case "MakeMeMaster.xml":
                    //string tRemoteIPAdresse = mSocketInformation.ClientSocket.RemoteEndPoint.ToString();
                    //tRemoteIPAdresse = tRemoteIPAdresse.Split(':')[0];
                    //string[] tVVVVParameter = mMessageBody.Split('&');

                    mWebinterfaceSingelton.SetMaster(tRemoteIPAdresse, tVVVVParameter[0].Split('=')[1]);

                    foreach (string tValuePair in tVVVVParameter)
                    {
                        string[] tValue = tValuePair.Split('=');

                        if (tValue.Length > 1)
                        {
                            mWebinterfaceSingelton.setNewBrowserDaten(tValue[0], tValue[1]);
                        }
                    }

                    mResponse = new Response(mFilename, FRequest.Headers.Get("Content-Type"), Encoding.UTF8.GetBytes("You are Master: " + tRemoteIPAdresse), new HTTPStatusCode("").Code200);
                break;





                case "CheckIfSlave.xml":
                        //string tRemoteIPAdresse = mSocketInformation.ClientSocket.RemoteEndPoint.ToString();
                        //tRemoteIPAdresse = tRemoteIPAdresse.Split(':')[0];
                        //string[] tVVVVParameter = mMessageBody.Split('&');

                        string tResponse = mWebinterfaceSingelton.CheckIfSlave(tRemoteIPAdresse, tVVVVParameter[0].Split('=')[1]);

                        if (tResponse == "Master")
                        {
                            mResponse = new Response(mFilename, FRequest.Headers.Get("Content-Type"), Encoding.UTF8.GetBytes(tResponse), new HTTPStatusCode("").Code200);

                            foreach (string tValuePair in tVVVVParameter)
                            {
                                string[] tValue = tValuePair.Split('=');

                                if (tValue.Length > 1)
                                {
                                    mWebinterfaceSingelton.setNewBrowserDaten(tValue[0], tValue[1]);
                                }
                            }
                        }
                        else
                        {
                            mResponse = new Response(mFilename, FRequest.Headers.Get("Content-Type"), Encoding.UTF8.GetBytes(tResponse), new HTTPStatusCode("").Code200);
                        }
                    break;





                case "polling.xml":
                    XmlDocument tMessage;
                    mWebinterfaceSingelton.getPollingMessage(out tMessage);

                    if (tMessage != null)
                    {
                        StringWriter sw = new StringWriter();
                        XmlTextWriter xw = new XmlTextWriter(sw);

                        // Save Xml Document to Text Writter.
                        tMessage.WriteTo(xw);
                        System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();

                        // Convert Xml Document To Byte Array.
                        byte[] docAsBytes = encoding.GetBytes(sw.ToString());

                        mResponse = new Response(mFilename, "text/xml", docAsBytes, new HTTPStatusCode("").Code200);
                    }
                    else
                    {
                        mResponse = new Response(mFilename, "text/xml", Encoding.UTF8.GetBytes("NoNewData"), new HTTPStatusCode("").Code200);
                    }
                break;



                case "uploadify.php":

                    mResponse = new Response(mFilename, "text/xml", Encoding.UTF8.GetBytes("1"), new HTTPStatusCode("").Code200);
                    break;




                default:
                        if (mPostMessages.ContainsKey(mFilename))
                        {
                            string PostResponse;
                            mPostMessages.TryGetValue(mFilename, out PostResponse);
                            mResponse = new Response(mFilename, "text/xml", Encoding.UTF8.GetBytes(PostResponse), new HTTPStatusCode("").Code200);
                        }
                        else
                        {
                            tReqeustedFileExtension = DetectedFileExtension(FRequest.Headers.Get("Content-Type"));
                            if (tReqeustedFileExtension == "unknown" && FRequest.Headers.Get("Content-Type").Contains("javascript"))
                            {
                                tReqeustedFileExtension = ".js";
                            }
                        }
                    break;
            }


            //if (mFilename == "ToVVVV.xml")
            //{

            //    string tRemoteIPAdresse = mSocketInformation.ClientSocket.RemoteEndPoint.ToString();
            //    tRemoteIPAdresse = tRemoteIPAdresse.Split(':')[0];
            //    string[] tVVVVParameter = mMessageBody.Split('&');


            //    foreach (string tValuePair in tVVVVParameter)
            //    {
            //        string[] tValue = tValuePair.Split('=');
                    
            //        if (tValue.Length > 1)
            //        {
            //            mWebinterfaceSingelton.setNewBrowserDaten(tValue[0], tValue[1]);
            //        }
            //    }
            //    mResponse = new Response(mFilename, tContentType, Encoding.UTF8.GetBytes("VVVV Received Post Request"), new HTTPStatusCode("").Code200);
            //}
            //else if (mFilename == "MakeMeMaster.xml")
            //{
            //    string tRemoteIPAdresse = mSocketInformation.ClientSocket.RemoteEndPoint.ToString();
            //    tRemoteIPAdresse = tRemoteIPAdresse.Split(':')[0];
            //    string[] tVVVVParameter = mMessageBody.Split('&');

            //    mWebinterfaceSingelton.SetMaster(tRemoteIPAdresse, tVVVVParameter[0].Split('=')[1]);

            //    foreach (string tValuePair in tVVVVParameter)
            //    {
            //        string[] tValue = tValuePair.Split('=');

            //        if (tValue.Length > 1)
            //        {
            //            mWebinterfaceSingelton.setNewBrowserDaten(tValue[0], tValue[1]);
            //        }
            //    }

            //    mResponse = new Response(mFilename, tContentType, Encoding.UTF8.GetBytes("You are Master: " + tRemoteIPAdresse), new HTTPStatusCode("").Code200);
            //}
            //else if (mFilename == "CheckIfSlave.xml")
            //{
            //    string tRemoteIPAdresse = mSocketInformation.ClientSocket.RemoteEndPoint.ToString();
            //    tRemoteIPAdresse = tRemoteIPAdresse.Split(':')[0];
            //    string[] tVVVVParameter = mMessageBody.Split('&');

            //    string tResponse = mWebinterfaceSingelton.CheckIfSlave(tRemoteIPAdresse,tVVVVParameter[0].Split('=')[1]);

            //    if (tResponse == "Master")
            //    {
            //        mResponse = new Response(mFilename, tContentType, Encoding.UTF8.GetBytes(tResponse), new HTTPStatusCode("").Code200);

            //        foreach (string tValuePair in tVVVVParameter)
            //        {
            //            string[] tValue = tValuePair.Split('=');

            //            if (tValue.Length > 1)
            //            {
            //                mWebinterfaceSingelton.setNewBrowserDaten(tValue[0], tValue[1]);
            //            }
            //        }
            //    }
            //    else
            //    {
            //        mResponse = new Response(mFilename, tContentType, Encoding.UTF8.GetBytes(tResponse), new HTTPStatusCode("").Code200);
            //    }

                
            //}
            //else if (mFilename == "polling.xml")
            //{
            //    XmlDocument tMessage;
            //    mWebinterfaceSingelton.getPollingMessage(out tMessage);

            //    if (tMessage != null)
            //    {
            //        StringWriter sw = new StringWriter();
            //        XmlTextWriter xw = new XmlTextWriter(sw);

            //        // Save Xml Document to Text Writter.
            //        tMessage.WriteTo(xw);
            //        System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();

            //        // Convert Xml Document To Byte Array.
            //        byte[] docAsBytes = encoding.GetBytes(sw.ToString());

            //        mResponse = new Response(mFilename, "text/xml", docAsBytes, new HTTPStatusCode("").Code200);
            //    }
            //    else
            //    {
            //        mResponse = new Response(mFilename, "text/xml", Encoding.UTF8.GetBytes("NoNewData"), new HTTPStatusCode("").Code200);
            //    }


            //}
            //else if (mPostMessages.ContainsKey(mFilename))
            //{
            //    string PostResponse;
            //    mPostMessages.TryGetValue(mFilename, out PostResponse);
            //    mResponse = new Response(mFilename, "text/xml", Encoding.UTF8.GetBytes(PostResponse), new HTTPStatusCode("").Code200);
            //}
            //else
            //{
            //    tReqeustedFileExtension = DetectedFileExtension(ContentType);
            //    if (tReqeustedFileExtension == "unknown" && ContentType.Contains("javascript"))
            //    {
            //        tReqeustedFileExtension = ".js";
            //    }
            //}
        }

        #endregion POST Request


        private void SetReceivedData(string pContent)
        {

        }

    }
}
