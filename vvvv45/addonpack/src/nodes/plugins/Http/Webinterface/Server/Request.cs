using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Diagnostics;

namespace VVVV.Webinterface.HttpServer
{
    class Request
    {


        private string mRequestType;
        private string mHttpVersion;
        private string mFilename;
        private string mFileLocation;
        private SortedDictionary<string, string> mParameterList = new SortedDictionary<string, string>();
        private string mMessageHead = "";
        private string mMessageBody = "";







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
                return mParameterList;
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
            SplitParameter(lines);
        }

        #endregion constructor








        #region Analyse Http Head


        private void SplitFirstLine(string pFirstLine)
        {
            // RequestType
            string[] words = pFirstLine.Split(' ');
            mRequestType = words[0];
            Debug.WriteLine("RequestType: " + mRequestType);
            
            //FileName & Location
            if (words[1].LastIndexOf('/') == 0)
            {
                mFileLocation = "/";
            }
            else
            {
                mFileLocation = words[1].Substring(0, words[1].LastIndexOf('/'));
            }
            Debug.WriteLine("mFileLocation: " + mFileLocation);

            mFilename = words[1].Substring(words[1].LastIndexOf('/') + 1);
            Debug.WriteLine("mFileName: " + mFilename);
 
            mHttpVersion = words[2].Substring(words[2].LastIndexOf('/') + 1);
            Debug.WriteLine("HttpVersion: " + mHttpVersion);
        }


        private void SplitParameter(string[] pParameter)
        {
            int tLength = pParameter.Length;
            for (int i = 1; i < pParameter.Length; i++)
            {
                string line = pParameter[i];
                if (line.Contains(":"))
                {
                    mParameterList.Add(line.Substring(0,line.IndexOf(":")), line.Substring(line.LastIndexOf(":") + 1));
                    Debug.WriteLine(line.Substring(0, line.IndexOf(":")) + ":" + line.Substring(line.LastIndexOf(":") + 1));
                }
            }
        }

        #endregion Analyse Http Head







    }
}
