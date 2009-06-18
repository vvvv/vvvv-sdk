using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Diagnostics;
using System.Text.RegularExpressions;


namespace VVVV.Webinterface.HttpServer
{
    class Request
    {


        private string mRequestType;
        private string mHttpVersion;
        private string mFilename;
        private string mFileLocation;
        private SortedDictionary<string, string> mHeadParameterList = new SortedDictionary<string, string>();
        private SortedDictionary<string, string> mGetPostParameterList = new SortedDictionary<string, string>();
        private string mMessageHead = "";
        private string mMessageBody = "";
        private Dictionary<string,string> mArguments = new Dictionary<string,string>();






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
                return mHeadParameterList;
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

        # endregion public properties








        # region constructor

        public Request(string pRequest)
        {
            String[] lines = pRequest.Split('\n');
            SplitFirstLine(lines[0]);
            SplitHeadParameter(lines);
        }

        #endregion constructor








        #region Analyse Http Head


        private void SplitFirstLine(string pFirstLine)
        {


            // RequestType
            string[] words = pFirstLine.Split(' ');

            mRequestType = words[0];
            Debug.WriteLine("RequestType: " + mRequestType);

            mHttpVersion = words[2].Substring(words[2].LastIndexOf('/') + 1);
            Debug.WriteLine("HttpVersion: " + mHttpVersion);
            

            //FileName & Location
            string p01 = @"[?][\w\W]*$";
            mFileLocation = Regex.Replace(words[1], p01,"");
            mFilename = mFileLocation.Substring(mFileLocation.LastIndexOf('/') + 1);

            //GetProperties
            if(mRequestType =="GET"  && words[1].Contains("?"))
            {
                string p02 = @"[\w\W]+[?]";
                string getProperties = Regex.Replace(words[1], p02, "");

                string[] ParamterPairs = getProperties.Split('&');

                foreach (string pPair in ParamterPairs)
                {
                    string[] pGetPostParameters = pPair.Split('=');
                    mGetPostParameterList.Add(pGetPostParameters[0], pGetPostParameters[1]);
                }
            }
            else if (mRequestType == "POST")
            {

            }
            else
            {

            }
         }


        private void SplitHeadParameter(string[] pParameter)
        {
            int tLength = pParameter.Length;
            for (int i = 1; i < pParameter.Length; i++)
            {
                string line = pParameter[i];
                if (line.Contains(":"))
                {
                    mHeadParameterList.Add(line.Substring(0,line.IndexOf(":")), line.Substring(line.LastIndexOf(":") + 1));
                    Debug.WriteLine(line.Substring(0, line.IndexOf(":")) + ":" + line.Substring(line.LastIndexOf(":") + 1));
                }
            }
        }

        #endregion Analyse Http Head







    }
}
