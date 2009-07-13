using System;
using System.IO;
using System.Xml;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Xml.XPath;

using VVVV.Webinterface.Utilities;

namespace VVVV.Webinterface.Data
{

    /// <summary>
    /// XML Handling class definition 
    /// 
    /// </summary>
    class XMLHandling
    {

        private XmlDocument mXml = new XmlDocument();
        private string mPath;
        private Logger mLogger;


        /// <summary>
        /// the actuall xml tree
        /// </summary>
        public XmlDocument XmlBaum
        {
            get
            {
                return mXml;
            }

        }

        /// <summary>
        /// XMLHandling consructor
        /// </summary>
        /// <param name="pLogger">ogger instance</param>
        public XMLHandling(Logger pLogger, string pPath)
        {
			this.mLogger = pLogger;
            this.mPath = pPath;
        }


        /// <summary>
        /// build en XML tress aout of the data modell
        /// </summary>
        /// <param name="data">actuell data modell</param>
        public void buildXml(SortedList<string, SortedList<string,string>> data)
        {
            System.Xml.XmlDocument tXml = new XmlDocument();
            
            tXml.AppendChild(tXml.CreateElement("", "webinterface", ""));
            XmlElement tRoot = tXml.DocumentElement;
            XmlElement tNode;
            XmlElement tSlice;



            foreach (KeyValuePair<string, SortedList<string, string>> p in data)
            {

                
                tNode = tXml.CreateElement("", "node", "");
                tNode.SetAttribute("nodeID", p.Key.ToString());


                SortedList<string, string> tSliceList;
                data.TryGetValue(p.Key, out tSliceList);  

                foreach (KeyValuePair<string,string> tSliceContent in tSliceList)
                {
                    tSlice = tXml.CreateElement("", "slice", "");
                    tSlice.SetAttribute("value", tSliceContent.Value);
                    tSlice.SetAttribute("id", tSliceContent.Key);
                    tNode.AppendChild(tSlice);
                }

                tRoot.AppendChild(tNode);
            }

            mXml = tXml;
            SaveXml();
        }


        /// <summary>
        /// saves the xml tree to disc
        /// </summary>
        public void SaveXml()
        {
            //mXml.Save("webinterface.xml");
            //mLogger.log(mLogger.LogType.Info, "Xml File saved");
        }
    }
}
