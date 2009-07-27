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


using VVVV.Webinterface.Data;
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





        #region field declaration

        private static volatile WebinterfaceSingelton instance = null;
        private static object m_lock = new Object();
        private ConcreteSubject mSubject;
        private SortedList<string, string> mDaten = new SortedList<string, string>();
        private SortedList<string, SortedList<string, string>> mNodeDaten = new SortedList<string, SortedList<string, string>>();
        private SortedList<string, string> mServerDaten = new SortedList<string, string>();
        private Logger mlogger;
        private XMLHandling mXmlHandling;
        private StartupCheck mStartupCheck;



        #endregion field declaration





        #region Properties

        /// <summary>
        /// node and slice list
        /// </summary>
        public SortedList<string,  string> Daten
        {
            get
            {
                return mDaten;
            }

            set
            {
                mDaten = value;
            }
        }

        /// <summary>
        /// new data from the browser
        /// </summary>
        public SortedList<string, string> ServerDaten
        {
            get
            {

                return mServerDaten;
            }
            set
            {
                mServerDaten = value;
            }
        }

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
        /// the Logger instance
        /// </summary>
        public Logger Logger
        {
            get
            {
                return mlogger;
            }
        }

        /// <summary>
        /// the subject state
        /// </summary>
        public string SubjectState
        {
            get
            {
                return mSubject.SubjectState;
            }
            set
            {
                if (value == "VVVV")
                {
                    mSubject.SubjectState = value;
                    //mTellNodes.NotifyNode();
                    //////Debug.WriteLine("SubjectState: " + value);
                }
                else if (value == "Server")
                {
                    mSubject.SubjectState = value;
                    //////Debug.WriteLine("SubjectState: " + value);
                    //mTellNodes.NotifyNode();
                }
                else
                {
                    //////Debug.WriteLine("SubjectState not set no State found");
                }
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

            mXmlHandling = new XMLHandling(mlogger, mStartupCheck.SartupFolder);

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





        # region Change State


        /// <summary>
        /// Changes the global subject state to "VVVV"
        /// </summary>
        public void setSubbjectStateToVVVV()
        {
            mSubject.SubjectState = "VVVV";
            mSubject.NotifyNode();
            mlogger.log(mlogger.LogType.Info, "Subject State set to VVVV");
            //////Debug.WriteLine("mTellNodes.SubjectState = VVVV");
        }


        /// <summary>
        /// Changes the global subject state to "Server"
        /// </summary>
        /// <param name="pToNode">Node Id which changes value</param>
        public void setSubjectStateToServer(string pToNode)
        {
            mSubject.SubjectState = "Server";
            //////Debug.WriteLine("mTellNodes.SubjectState = Server");
            mlogger.log(mlogger.LogType.Info, "Subject State set to Server");
            mSubject.ToNode = pToNode;
            mSubject.NotifyNode();
        }


        /// <summary>
        /// Changes the global subject state to "VVVVChangedValue"
        /// </summary>
        /// <param name="pNodeId">NodID which changes value</param>
        public void setUserInteraktionNode(string pNodeId)
        {
            mSubject.SubjectState = "VVVVChangedValue";
            mlogger.log(mlogger.LogType.Info, "Subject State set to VVVVChangedValue");
            mSubject.ToHtmlForm = pNodeId;
            //mSubject.NotifyServer();
            //////Debug.WriteLine("mTellNodes.SubjectState = VVVVChangedValue");
        }


        #endregion Change State





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





        #region Daten Handling



        /// <summary>
        /// Calles the subject.Reload() function
        /// </summary>
        public void Reload()
        {
            mSubject.Reload();
        }


        /// <summary>
        /// saves the the XML tree to disc
        /// </summary>
        //public void saveXML()
        //{
        //    mXmlHandling.SaveXml();
        //}


        /// <summary>
        /// creates the Xml Tree
        /// </summary>
        //public void buildXML()
        //{
        //    mXmlHandling.buildXml(mDaten);

        //}


        /// <summary>
        /// gets new data form node an set them to the subject
        /// </summary>
        /// <param name="pNodeId">node ID the new data contains to</param>
        /// <param name="pNewNodeDaten">new node slice data</param>
        public void setNodeDaten(string pSliceId, string pValue)
        {
            SortedList<string, string> tValuePair = new SortedList<string, string>();
            mSubject.NewServerDaten = tValuePair;

            mSubject.ToHtmlForm = pSliceId;
            saveNewNodeDaten(pSliceId, pValue);
        }


        /// <summary>
        /// saves the new data node data to the data list
        /// </summary>
        /// <param name="pKey">Node ID</param>
        /// <param name="pNewNodeDaten">whole node slice list</param>
        public void saveNewNodeDaten(string pKey, string pNewNodeDaten)
        {

            mlogger.log(mlogger.LogType.Debug, "Add new Node Daten to mDaten List ");

            if (mDaten.ContainsKey(pKey))
            {
                mDaten.Remove(pKey);
                mDaten.Add(pKey, pNewNodeDaten);
            }
            else
            {
                mDaten.Add(pKey, pNewNodeDaten);
            }


            // Hier mDaten Locken 
            //mXmlHandling.buildXml(mDaten);
            // Hier mDaten entlockenLocken 
        }



        


        /// <summary>
        /// set data from browser to the data list
        /// </summary>
        /// <param name="pNewData">new value pair</param>
        private void setNewBrowserDaten(string[] pNewData)
        {

            mlogger.log(mlogger.LogType.Debug, "New Browser Daten set to mDaten List");
            int tLength = pNewData.Length;
            int tWordLength = pNewData[0].Length;
            string tName = pNewData[0].Replace("?", "");
            //////Debug.WriteLine("New Browser Content in DataWarehouse: " + pNewData);


            // here mDaten Loggen 


            if (mDaten.ContainsKey(tName))
            {
                string tValue;
                mDaten.TryGetValue(tName, out tValue);
                mDaten.Remove(tName);
                mDaten.Add(tName, pNewData[1]);
                // here mDaten entLoggen 
                setSubjectStateToServer(tName);
            }
            else
            {
                mDaten.Add(tName, pNewData[1]);
                setSubjectStateToServer(tName);
                //////Debug.WriteLine("no SliceId found in SaveNewBrowserData");
                //mlogger.log(mlogger.LogType.Debug, "Replaced SliceKey: " + pNewData[1].ToString() + " with Value: " + pNewData[2].ToString());
            }
        }



        /// <summary>
        /// sets the new Browser data
        /// </summary>
        /// <param name="pContent">content string with value pairs separated by and / variable and value separated by =</param>
        public void setNewBrowserDaten(string pSliceID, string pValue)
        {

            Monitor.Enter(mDaten);
            
            if (mDaten.ContainsKey(pSliceID))
            {
                mDaten.Remove(pSliceID);
                mDaten.Add(pSliceID, pValue);
            }
            else
            {
                mDaten.Add(pSliceID, pValue);
            }

            Monitor.Exit(mDaten);
        }


        public void getNewBrowserData(string pSliceId, out string Response)
        {

            if (Monitor.TryEnter(mDaten))
            {
                try
                {
                    if (mDaten.ContainsKey(pSliceId))
                    {
                        mDaten.TryGetValue(pSliceId, out Response);
                        mDaten.Remove(pSliceId);
                    }
                    else
                    {
                        Response = "";
                    }
                }
                finally
                {
                    //Debug.WriteLine("Locked");
                    Monitor.Exit(mDaten);
                }
            }
            else
            {
                Response = "";
            }
        }

         

        

        # endregion Daten Handling



    }
}
