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
            //Status Code
            //mAttributeNames.Add(0, "HTTP Status Code");
           // mAttributeValues.Add(0,"HTTP/1.1 " + pStatusCode);
            mStatusCode = pStatusCode;
            //Date
            //Culture Info setzen für die Ländereinstellung
            CultureInfo ci = new CultureInfo("en-US");

            //Neues DateTime Objekt für den 04.07.2006

            DateTime pNow = DateTime.Now;
            DateTime pTime = DateTime.Now;
            //Als Label im Format 07/04/2006 (englisch) speichern
            string currentDate = pNow.ToString("d", ci);
            string currentTime = pTime.ToLongTimeString();


            //string currentTime = usdate.ToShortDateString();

            mAttributeNames.Add(0, "Date");
            mAttributeValues.Add(0, currentDate + " " + currentTime); 

            //Server
            mAttributeNames.Add(1, "Server");
            mAttributeValues.Add(1, "VVVV Webinterface Server 1beta1");

            ////Cache Controle
            //mAttributeNames.Add(3, "Cache-Control");
            //mAttributeValues.Add(3, "no-cache,must-revalidate");

            ////Content-Type
            //mAttributeNames.Add(4, "Content-Type");
            //mAttributeValues.Add(4, "text/html; charset=utf-8");
        }

        public ResponseHeader(string pStatusCode, string pFilename)
        {
            //Status Code
            mStatusCode = pStatusCode;
            
            //Date
            //Culture Info setzen für die Ländereinstellung
            CultureInfo ci = new CultureInfo("en-US");

            //Neues DateTime Objekt für den 04.07.2006
            
            DateTime pNow = DateTime.Now;
            DateTime pTime = DateTime.Now;
            //Als Label im Format 07/04/2006 (englisch) speichern
            string currentDate = pNow.ToString("d", ci);
            string currentTime = pTime.ToLongTimeString();

            
            //string currentTime = usdate.ToShortDateString();

            mAttributeNames.Add(0, "Date");
            mAttributeValues.Add(0, currentDate + " " + currentTime); 

            //Server
            mAttributeNames.Add(1,"Server");
            mAttributeValues.Add(1, "VVVV Webinterface Server 1.0");

            //location
            mAttributeNames.Add(2,"location");
            mAttributeValues.Add(2, pFilename);

            ////conenction
            //mAttributeNames.Add(3,"Connection");
            //mAttributeValues.Add(3, "keep-alive");


            ////Content-Type
            //mAttributeNames.Add(4,"Content-Type");
            //mAttributeValues.Add(4, "text/html; charset=utf-8");

            ////Cache Controle
            //mAttributeNames.Add(5, "Cache-Control");
            //mAttributeValues.Add(5, "no-cache,must-revalidate");
            
        }


        public void SetAttribute(string pName, string pValue)
        {
            if(mAttributeNames.ContainsValue(pName) == true)
            {
                int tIndex;
                tIndex = mAttributeNames.IndexOfValue(pName);

                mAttributeValues.Remove(tIndex);
                mAttributeValues.Add(tIndex, pValue);
                Debug.WriteLine("Attribute: " + pName + " already exist and value replaced by:" + pValue);
                
            }
            else
            {

                int tIndex = mAttributeNames.Count;
                mAttributeNames.Add(tIndex, pName);
                mAttributeValues.Add(tIndex, pValue);
                Debug.WriteLine("Attribute: " + pName + " doesn't exist and add at index:" + tIndex);
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
                Debug.WriteLine("Attribute: " + pName + " doesn't exist in Request Header class");
                return "Attribute: " + pName +  " doesn't exist in Request Header class";
            }
        }

        public void DeletAttribute(string pName)
        {

        }
    }
}
