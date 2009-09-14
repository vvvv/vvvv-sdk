#region licence/info

//////project name
//vvvv plugin template

//////description
//basic vvvv node plugin template.
//Copy this an rename it, to write your own plugin node.

//////licence
//GNU Lesser General Public License (LGPL)
//english: http://www.gnu.org/licenses/lgpl.html
//german: http://www.gnu.de/lgpl-ger.html

//////language/ide
//C# sharpdevelop 

//////dependencies
//VVVV.PluginInterfaces.V1;
//VVVV.Utils.VColor;
//VVVV.Utils.VMath;

//////initial author
//vvvv group

#endregion licence/info

using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Threading;
using System.IO;
using System.Diagnostics;
using System.Xml;
using System.Xml.XPath;
using System.Windows.Forms;



using VVVV.Webinterface.Data;
using VVVV.Nodes.HttpGUI;
using VVVV.Nodes.Http;
using VVVV.Webinterface.HttpServer;
using VVVV.Webinterface.Utilities;



namespace VVVV.Webinterface
{
    /// <summary>
    /// singelton of the webinterface 
    /// initalising the following Components
    /// </summary>
    /// <list type="table">
    /// <item>
    ///	 <term><see cref="VVVV.Webinterface.Utilities.Logger"/></term>
    ///	 <description>creates an instance of the logger class</description>
    /// </item>
    /// <item>
    /// 	 <term><see cref="VVVV.Webinterface.Data.XMLHandling"/></term>
    ///	 <description>creates an Iinstance of the XMLHandling class </description>
    /// </item>
    /// <item>
    /// 	 <term><see cref="VVVV.Webinterface.Data.ConcreteSubject"/></term>
    ///	 <description>creates an cnstance of the ConcretSunject class</description>
    /// </item>
    ///  </list>
    sealed class WebinterfaceSingelton
    {

        public struct PageValues
        {
            public string PageName;
            public Page Page;
            public List<GuiDataObject> GuiList;
            public string Url;
        }


        #region field declaration



        
        private static object m_lock = new Object();
        private ConcreteSubject mSubject;
        
        private SortedList<string, SortedList<string, string>> mNodeDaten = new SortedList<string, SortedList<string, string>>();
        private SortedList<string, string> mServerDaten = new SortedList<string, string>();

        private Logger mlogger;
        private StartupCheck mStartupCheck;


        //New 
        private static volatile WebinterfaceSingelton instance = null;
        private SortedList<string, string> mNodeData = new SortedList<string, string>();
        private List<string> mGetMessages = new List<string>();
        private List<string> mPostMessages = new List<string>();
        private SortedList<string,string> mBrowserPolling = new SortedList<string,string>();
        private SortedList<string,string> mNodePolling = new SortedList<string,string>();
        private SortedList<string, PageValues> mGuiLists = new SortedList<string,PageValues>();
        private List<string> mConnectedPages = new List<string>();
        private SortedList<string, string> mServerFiles = new SortedList<string,string>();
        private List<string> mUrls = new List<string>();
        private SortedList<string,SortedList<int,string>> mValuesToSave = new SortedList<string,SortedList<int,string>>();
        private string mHostPath;
        SortedList<string, SortedList<int, string>> mLoadedValues = new SortedList<string, SortedList<int, string>>();

        #endregion field declaration





        #region Properties

        /// <summary>
        /// the ConcretSubject instance
        /// </summary>
        public ConcreteSubject Subject
        {
            get
            {
                return mSubject;
            }
        }


        /// <summary>
        /// inforamtion to which node the data should be send
        /// </summary>
        public string ToNode
        {
            set
            {
                mSubject.ToNode = value;
            }
        }

        /// <summary>
        /// the information to which html form the data should be send
        /// </summary>
        public string ToHtml
        {
            set
            {
                mSubject.ToHtmlForm = value;
            }
        }

        /// <summary>
        /// gets the Folder to Serv;
        /// </summary>
        public string FolderToServ
        {
            get
            {
                return mStartupCheck.SartupFolder;
            }
        }

        public List<string> ServerFilesUrl
        {
            get
            {
                return mUrls;
            }
        }


        public SortedList<string,PageValues> GuiLists
        {
            get
            {
                return mGuiLists;
            }
        }


        public SortedList<string, string> ServerFiles
        {
            get
            {
                return mServerFiles;
            }
        }


        public string HostPath
        {

            set
            {
                mHostPath = value;
                LoadDataFromFile();
            }
        }


        #endregion  Properties



        #region construtor


        /// <summary>
        /// Constructor of the Singelton Class 
        /// </summary>
        private WebinterfaceSingelton()
        {


            mStartupCheck = new StartupCheck();
            mStartupCheck.SartupFolder = "plugins\\webinterface";
            mStartupCheck.StartupSubFolder = "assets";
            mStartupCheck.StartupSubFolder = "log";
            mStartupCheck.StartupSubFolder = "lib";
            mStartupCheck.CheckifStartupPathExist();


            if(Directory.Exists(mStartupCheck.getSubFolderPath("log")))
            {
                mlogger = new Logger(Path.Combine(mStartupCheck.getSubFolderPath("log"), System.DateTime.Today.ToShortDateString() + ".log"));
            }
            else
            {
                mlogger = new Logger(System.DateTime.Today.ToShortDateString() + ".log");
            }
            
            
            mlogger.log(mlogger.LogType.Info, "VVVV Webinterface Singelton erstellt");


            TextWriterTraceListener tr2 = new TextWriterTraceListener(System.IO.File.CreateText("Debug.txt"));
            Debug.Listeners.Add(tr2);

            mSubject = new ConcreteSubject();
            //////Debug.WriteLine(mSubject, " Subject");

            mServerDaten.Add("", "");
        }

        /// <summary>
        /// Function to get the WebinterfaceSingleton
        /// Only creates the instance once. Checks by every call if the instance is allready created an returns the existing one. 
        /// </summary>
        /// <returns>Singelton iinstance</returns>
        public static WebinterfaceSingelton getInstance()
        {
            if (instance == null)
            {
                lock (m_lock)
                {
                    if (instance == null)
                    {
                        instance = new WebinterfaceSingelton();
                    }
                }
            }

            return instance;
        }


        #endregion constructor



        #region ObserverHandling



        /// <summary>
        /// add a node to the webinterface and returns an observer instance
        /// </summary>
        /// <param name="pTypeName">Node ID</param>
        /// <returns><see cref="VVVV.Webinterface.Data.NodeObserver"/></returns>
        public NodeObserver AddNode(string pNodeId)
        {

            NodeObserver tObserver = new NodeObserver(mSubject, pNodeId);
            mSubject.AttachNode(tObserver);

            //////Debug.WriteLine("Observer erzeug: " + tObserver.ID);
            mlogger.log(mlogger.LogType.Info, "Add Textfield Node to Subject Oserver List");

            return tObserver;
        }




        /// <summary>
        /// deletes an observer from the node observer list in the subject instance if a node is deleted
        /// </summary>
        /// <param name="pObserver">the node observer to deleted see(<see cref="VVVV.Webinterface.Data.NodeObserver"/>)</param>
        public void DeleteNode(NodeObserver pObserver)
        {
            mSubject.DetachNode(pObserver);
        }



        /// <summary>
        /// attches an server instance to the server observer list int the subject instance
        /// </summary>
        /// <param name="pServhandling">the server instance to add</param>
        public void AddServhandling(Server pServhandling)
        {
            mSubject.AttachServerhandling(pServhandling);
        }



        /// <summary>
        /// deletes a server instance form the server observer list
        /// </summary>
        /// <param name="pServer">the server instance to delete</param>
        public void DeleteServhandling(Server pServer)
        {
            mSubject.DetachServhandling(pServer);
        }



        /// <summary>
        /// notify the server that there new data from vvvv
        /// </summary>
        public void NotifyServer(string pData)
        {
            mSubject.NotifyServer(pData);
        }

        #endregion ObserverHandling



        #region Build HtmlPages


        public void AddConnectedPage(string pPageName)
        {
            if (mConnectedPages.Contains(pPageName) == false)
            {
                mConnectedPages.Add(pPageName);
            }
        }


        public void RemoveDisconnectedPage(string pPageName, string pUrl)
        {
            if (mConnectedPages.Contains(pPageName))
            {
                mConnectedPages.Remove(pPageName);
                mUrls.Remove(pUrl);
            }
        }




        public void setHtmlPageData(string pPageName, string pUrl,Page pPage, List<GuiDataObject> pGuiList)
        {
            if (Monitor.TryEnter(mGuiLists))
            {
                try
                {
                    if (mConnectedPages.Contains(pPageName))
                    {
                        PageValues tPagesValues = new PageValues();
                        tPagesValues.PageName = pPageName;
                        tPagesValues.Page = pPage;
                        tPagesValues.Url = pUrl;
                        tPagesValues.GuiList = pGuiList;
                        if (mGuiLists.ContainsKey(pUrl))
                        {
                            mGuiLists.Remove(pUrl);
                            mGuiLists.Add(pUrl, tPagesValues);
                        }
                        else
                        {
                            mUrls.Add(pUrl);
                            mUrls.Add(pPageName + ".css");
                            mUrls.Add(pPageName + ".js");
                            mGuiLists.Add(pUrl, tPagesValues);
                        }
                    }
                }
                finally
                {
                    //Debug.WriteLine("Locked");
                    Monitor.Exit(mGuiLists);
                }
            }
        }


        public void BuildPages(string pUrl)
        {
            if (mGuiLists.ContainsKey(pUrl))
            {

                PageValues tPageValues;
                Monitor.Enter(mGuiLists);
                mGuiLists.TryGetValue(pUrl, out tPageValues);
                Monitor.Exit(mGuiLists);

                PageBuilder tPageBuilder = new PageBuilder();
                tPageBuilder.UpdateGuiList(tPageValues.GuiList, tPageValues.Page);
                tPageBuilder.Build();

                Monitor.Enter(mServerFiles);
                mServerFiles.Remove(pUrl);
                mServerFiles.Remove(tPageValues.PageName + ".css");
                mServerFiles.Remove(tPageValues.PageName + ".js");
                mServerFiles.Add(pUrl, tPageBuilder.HtmlFile);
                mServerFiles.Add(tPageValues.PageName + ".css", tPageBuilder.CssFile.ToString());
                mServerFiles.Add(tPageValues.PageName + ".js", tPageBuilder.JsFile.ToString());

                Monitor.Exit(mServerFiles);
            }
        }

        private void GetPageList()
        {

        }


        //private void HandlePageList(string pPageName, Page pPage, string pCssFile, string pJsFile, string pUrl)
        //{

        //    if (PageNames.Contains(pPageName))
        //    {
        //        PageNames.Remove(pPageName);
        //        PageNames.Add(pPageName);

        //        mHtmlPageList.Remove(pUrl);
        //        mHtmlPageList.Add(pUrl, Encoding.UTF8.GetBytes(pPage.Text));

        //        mHtmlPageList.Remove(pPageName + ".css");
        //        mHtmlPageList.Add(pPageName + ".css", Encoding.UTF8.GetBytes(pCssFile));

        //        mHtmlPageList.Remove(pPageName + ".js");
        //        mHtmlPageList.Add(pPageName + ".js", Encoding.UTF8.GetBytes(pJsFile));

        //    }
        //    else
        //    {
        //        PageNames.Add(pPageName);
        //        mHtmlPageList.Add(pUrl, Encoding.UTF8.GetBytes(pPage.Text));
        //        mHtmlPageList.Add(pPageName + ".css", Encoding.UTF8.GetBytes(pCssFile));
        //        mHtmlPageList.Add(pPageName + ".js", Encoding.UTF8.GetBytes(pJsFile));

        //    }
        //}

        //private void RemovePageFormList(string pPageName, Page pPage, string pCssFile, string pJsFile, string pUrl)
        //{

        //    if (PageNames.Contains(pPageName))
        //    {
        //        PageNames.Remove(pPageName);
        //        mHtmlPageList.Remove(pUrl);
        //        mHtmlPageList.Remove(pPageName + ".css");
        //        mHtmlPageList.Remove(pPageName + ".js");

        //    }
        //}

        #endregion Build HtmlPages



        #region Daten Handling

        /// <summary>
        /// sets the new Browser data
        /// </summary>
        /// <param name="pContent">content string with value pairs separated by and / variable and value separated by =</param>
        public void setNewBrowserDaten(string pSliceID, string pValue)
        {

            Monitor.Enter(mNodeData);
            
            if (mNodeData.ContainsKey(pSliceID))
            {
                mNodeData.Remove(pSliceID);
                mNodeData.Add(pSliceID, pValue);
            }
            else
            {
                mNodeData.Add(pSliceID, pValue);
            }

            Monitor.Exit(mNodeData);
        }


        public void getNewBrowserData(string pSliceId,string pHostPath,int Slice,out string Response)
        {

            if (Monitor.TryEnter(mNodeData))
            {
                try
                {
                    if (mNodeData.ContainsKey(pSliceId))
                    {
                        mNodeData.TryGetValue(pSliceId, out Response);
                    }
                    else if(mLoadedValues.ContainsKey(pHostPath))
                    {
                        string tResponse = "";
                        SortedList<int, string> tSpreadValues = new SortedList<int, string>();
                        mLoadedValues.TryGetValue(pHostPath, out tSpreadValues);
                        if (tSpreadValues.ContainsKey(Slice))
                        {
                            tSpreadValues.TryGetValue(Slice, out tResponse);
                        }
                        Response = tResponse;
                    }else
                    {
                        Response = "";
                    }
                }
                finally
                {
                    //Debug.WriteLine("Locked");
                    Monitor.Exit(mNodeData);
                }
            }
            else
            {
                Response = "";
            }
        }


        public void setResponseMessage(string pContent, string pType)
        {
            if (pType == "GET")
            {
                Monitor.Enter(mGetMessages);
                mGetMessages.Add(pContent);
                Monitor.Exit(mGetMessages);
            }
            else if (pType == "POST")
            {
                Monitor.Enter(mPostMessages);
                mPostMessages.Add(pContent);
                Monitor.Exit(mPostMessages);
            }
        }


        public void getRequestMessage(out List<string> GetMessages, out List<string> PostMessages)
        {
            if (Monitor.TryEnter(mGetMessages) && Monitor.TryEnter(mPostMessages))
            {
                try
                {
                    GetMessages = new List<string>(mGetMessages);
                    mGetMessages.Clear();
                    PostMessages = new List<string>(mPostMessages);
                    mPostMessages.Clear();
                }
                finally
                {
                    //Debug.WriteLine("Locked");
                    Monitor.Exit(mGetMessages);
                    Monitor.Exit(mPostMessages);
                }
            }
            else
            {
                GetMessages = null;
                PostMessages = null;
            }
        }


        public void getPollingMessage(out XmlDocument pContent)
        {
            if (Monitor.TryEnter(mNodePolling) && Monitor.TryEnter(mBrowserPolling))
            {
                try
                {
                    if (mBrowserPolling.Count > 0 || mNodePolling.Count > 0)
                    {
                        XmlDocument tXml = CreatePollingXml();
                        pContent = tXml;
                        mBrowserPolling.Clear();
                        mNodePolling.Clear();
                    }
                    else
                    {

                        pContent = null;
                    }
                }
                finally
                {
                    //Debug.WriteLine("Locked");
                    Monitor.Exit(mNodePolling);
                    Monitor.Exit(mBrowserPolling);
                }
            }
            else
            {
                pContent = null;
            }
        }


        public void setPollingMessage(string SliceId, string pContent)
        {
            Monitor.Enter(mNodePolling);
            
            if (mNodePolling.ContainsKey(SliceId) == false)
            {
                mNodePolling.Add(SliceId, pContent);
            }
            else
            {
                mNodePolling.Remove(SliceId);
                mNodePolling.Add(SliceId, pContent);
            }

            Monitor.Exit(mNodePolling);
        }

        public void setPollingMessage(string pCommand, string pContent, bool CommandAsTagName)
        {
            Monitor.Enter(mBrowserPolling);
            if (CommandAsTagName)
            {
                if (mBrowserPolling.ContainsKey(pCommand) == false)
                {
                    mBrowserPolling.Add(pCommand, pContent);
                }
                else
                {
                    mBrowserPolling.Remove(pCommand);
                    mBrowserPolling.Add(pCommand, pContent);
                }
            }

            Monitor.Exit(mBrowserPolling);
        }


        public XmlDocument CreatePollingXml()
        {

            System.Xml.XmlDocument tXml = new XmlDocument();

            tXml.AppendChild(tXml.CreateElement("", "polling", ""));
            XmlElement tRoot = tXml.DocumentElement;
            XmlElement tBrowser = tXml.CreateElement("", "browser", "");
            XmlElement tNodes = tXml.CreateElement("", "nodes", "");
           
            if(mBrowserPolling.Count > 0)
            {
                foreach (KeyValuePair<string, string> tPair in mBrowserPolling)
                {
                    XmlElement tCommand;
                    tCommand = tXml.CreateElement("", tPair.Key, "");
                    tCommand.InnerText =  tPair.Value;
                    tBrowser.AppendChild(tCommand);
                }
            }

            if(mNodePolling.Count > 0)
            {
                foreach(KeyValuePair<string,string> tPair in mNodePolling)
                {
                    XmlElement tNodeSlice;
                    tNodeSlice = tXml.CreateElement("", "node", "");
                    tNodeSlice.SetAttribute("SliceId",tPair.Key);
                    tNodes.AppendChild(tNodeSlice);
                }
            }

            tRoot.AppendChild(tBrowser);
            tRoot.AppendChild(tNodes);
            return tXml;
        }
         


        # endregion Daten Handling



        #region Client Response Handling

        string mMasterIP = String.Empty;
        bool mSetNewMaster = true;
        int mTimeOut = 2000;
        private object _SyncCheck = new object();
        //Create a timer that waits one minute, then invokes every 5 minutes.
        System.Threading.Timer mMasterTimer;

        public string CheckIfMaster(string IPAdress)
        {
            lock (_SyncCheck)
            {
                if (mMasterTimer == null)
                {
                    TimerCallback timerDelegate = new TimerCallback(this.TimerCall);
                    mMasterTimer = new System.Threading.Timer(timerDelegate, null, mTimeOut, System.Threading.Timeout.Infinite);
                }

                if (mSetNewMaster == true)
                {
                    mMasterIP = IPAdress;
                    mSetNewMaster = false;
                    Debug.WriteLine(IPAdress + " is Master");
                    return "Master";
                }
                else if (mSetNewMaster == false && mMasterIP == IPAdress)
                {
                    mMasterTimer.Change(mTimeOut, System.Threading.Timeout.Infinite);
                    Debug.WriteLine(IPAdress + " is Master");
                    return "Master";
                }
                else
                {
                    Debug.WriteLine(IPAdress + " is Slave");
                    return "Slave";
                }
            }
        }

        private void TimerCall(object eventState)
        {
            mMasterIP = String.Empty;
            mSetNewMaster = true;
        }


        #endregion Client Response Handling




        #region  SaveDataToFile

        public void AddListOnDestroy(string NodeID, SortedList<int,string> SpreadList)
        {
            if (mValuesToSave.ContainsKey(NodeID))
            {
                mValuesToSave.Remove(NodeID);
                mValuesToSave.Add(NodeID, SpreadList);
            }
            else
            {
                mValuesToSave.Add(NodeID, SpreadList);
            }
        }


        public void SaveDataToFile()
        {
            int tSubBegin = mHostPath.LastIndexOf('\\') + 1;
            string tHostPath = mHostPath.Substring(0, tSubBegin);
            tHostPath += "webinterface.xml";

            FileStream fs = new FileStream(tHostPath, FileMode.Create);
            XmlTextWriter w = new XmlTextWriter(fs, Encoding.UTF8);
            w.Formatting = Formatting.Indented;

            w.WriteStartDocument();
            w.WriteStartElement("root");


            foreach (KeyValuePair<string, SortedList<int, string>> p in mValuesToSave)
            {
                SortedList<int, string> tSliceList;
                mValuesToSave.TryGetValue(p.Key, out tSliceList);
                StringBuilder tSpread = new StringBuilder();
                foreach (KeyValuePair<int, string> tSliceContent in tSliceList)
                {
                    tSpread.Append(tSliceContent.Value + ";");
                }

                w.WriteStartElement("Node");
                w.WriteAttributeString("id", p.Key.ToString());
                w.WriteElementString("Spread", tSpread.ToString());
                w.WriteEndElement();


            }

            w.WriteEndElement();
            w.WriteEndDocument();
            w.Flush();
            fs.Close();
        }


        public void LoadDataFromFile()
        {
            int tSubBegin = mHostPath.LastIndexOf('\\') + 1;
            string tHostPath = mHostPath.Substring(0, tSubBegin);
            tHostPath += "webinterface.xml";

            System.Xml.XmlDocument tXml = new XmlDocument();

            


            if (File.Exists(tHostPath))
            {


                XmlDocument tdoc = new XmlDocument();
                tdoc.Load(new XmlTextReader(tHostPath));

                XmlNodeList tNodes = tdoc.GetElementsByTagName("Node");
                
                for(int i = 0; i < tNodes.Count; i++)
                {
                    XmlNode tNode = tNodes.Item(i);
                    XmlAttributeCollection tAttributes = tNode.Attributes;
                    string tNodeID = tAttributes[0].Value;

                    XmlNode tSpread = tNode.FirstChild;
                    string tSpreadContent = tSpread.InnerText;
                    string[] tSpreadList = tSpreadContent.Split(';');

                    SortedList<int, string> tSpreadValues = new SortedList<int, string>();
                    for (int j = 0; j < tSpreadList.Length; j++)
                    {
                        tSpreadValues.Add(j, tSpreadList[j]);
                    }

                    mLoadedValues.Add(tNodeID, tSpreadValues);
                }
            }
        }


        public SortedList<int, string> GetLoadedValues(string pNodeId)
        {
            SortedList<int, string> tSpread = new SortedList<int,string>();

            if(mLoadedValues.ContainsKey(pNodeId))
            {  
                mLoadedValues.TryGetValue(pNodeId, out tSpread);
            }

            return tSpread;
        }

        #endregion  SaveDataToFile







    }

    
}
