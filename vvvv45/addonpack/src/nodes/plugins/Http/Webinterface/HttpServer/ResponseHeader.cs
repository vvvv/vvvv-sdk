using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Globalization;
using VVVV.Webinterface.HttpServer;

namespace VVVV.Webinterface.HttpServer
{

    /// <summary>
    /// Creates an HTTP Response Header
    /// </summary>
    class ResponseHeader
    {
        /// <summary>
        /// Ever add or delete Values on the same index on both Listes
        /// </summary>

        
        SortedList<int, string> mAttributeNames= new SortedList<int,string>();
        SortedList<int, string> mAttributeValues = new SortedList<int, string>();
        private string mStatusCode;

        public string Text
        {
            get
            {
                string tHeaderText = "";
                int tIndex = mAttributeNames.Count;
                for(int i = 0; i < tIndex; i ++)
                {
                    string tName;
                    string tValue;

                    mAttributeNames.TryGetValue(i, out tName);
                    mAttributeValues.TryGetValue(i, out tValue);


                    tHeaderText += tName + ": "+ tValue + Environment.NewLine;
                }

                return "HTTP/1.1 "  + mStatusCode  + Environment.NewLine + tHeaderText + Environment.NewLine;
            }
        }


        public ResponseHeader()
        {
        }

        public ResponseHeader(string pStatusCode)
        {
            mStatusCode = pStatusCode;

            //Date
            //Culture Info setzen für die Ländereinstellung
            CultureInfo ci = new CultureInfo("en-US");
            DateTime pNow = DateTime.Now;
            DateTime pTime = DateTime.Now;
            //Als Label im Format 07/04/2006 (englisch) speichern
            string currentDate = pNow.ToString("d", ci);
            string currentTime = pTime.ToLongTimeString();

            mAttributeNames.Add(0, "Date");
            mAttributeValues.Add(0, currentDate + " " + currentTime); 

            //Server
            mAttributeNames.Add(1, "Server");
            mAttributeValues.Add(1, "VVVV Webinterface Server 1beta1");

        }

        public ResponseHeader(string pStatusCode, string pFilename)
        {
            //Status Code
            mStatusCode = pStatusCode;
            
            //Date
            //Culture Info setzen für die Ländereinstellung
            CultureInfo ci = new CultureInfo("en-US");
            
            DateTime pNow = DateTime.Now;
            DateTime pTime = DateTime.Now;

            string currentDate = pNow.ToString("d", ci);
            string currentTime = pTime.ToLongTimeString();

            mAttributeNames.Add(0, "Date");
            mAttributeValues.Add(0, currentDate + " " + currentTime); 

            //Server
            mAttributeNames.Add(1,"Server");
            mAttributeValues.Add(1, "VVVV Webinterface Server 1.0");

            //location
            mAttributeNames.Add(2,"location");
            mAttributeValues.Add(2, pFilename);
            
        }


        public void SetAttribute(string pName, string pValue)
        {
            if(mAttributeNames.ContainsValue(pName) == true)
            {
                int tIndex;
                tIndex = mAttributeNames.IndexOfValue(pName);

                mAttributeValues.Remove(tIndex);
                mAttributeValues.Add(tIndex, pValue);
                ////Debug.WriteLine(String.Format("Attribute {0} already exist and value replaced by {1}:",pName,pValue));
            }
            else
            {
                int tIndex = mAttributeNames.Count;
                mAttributeNames.Add(tIndex, pName);
                mAttributeValues.Add(tIndex, pValue);
                ////Debug.WriteLine(String.Format("Attribute {0} add at Index {1} with Value {2}:",pName, tIndex, pValue));
            }
        }

        public string GetAttribute(string pName)
        {     
            if (mAttributeNames.ContainsValue(pName) == true)
            {
                int tIndex;
                string tValue;
                tIndex =  mAttributeNames.IndexOfValue(pName);
                mAttributeValues.TryGetValue(tIndex, out tValue);

                return pName + ": " + tValue;
            }
            else
            {
                ////Debug.WriteLine( String.Format("Attribute: {0}  doesn't exist in Request Header class", pName ));
                return "Attribute: " + pName +  " doesn't exist in Request Header class";
            }
        }
    }
}
