
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

//use what you need
using System;
using System.Drawing;
using VVVV.PluginInterfaces.V1;
//using VVVV.Utils.VColor;
//using VVVV.Utils.VMath;
using System.IO;
using System.Collections.Generic;
using System.Collections;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;
using System.Text;
using System.Management;
using System.Net;
using System.Net.Sockets;

using VVVV.Webinterface;
using VVVV.Webinterface.Utilities;
using VVVV.Webinterface.HttpServer;
using VVVV.Nodes.Http;
using VVVV.Nodes.HttpGUI;


//the vvvv node namespace
namespace VVVV.Nodes.Http
{

    /// <summary>
    /// node to put all html nodes to one html page
    /// </summary>
    public class RendererHTTPListener : IPlugin, IDisposable, IPluginConnections
    {





        #region field declaration

        //the host (mandatory)
        private IPluginHost FHost;
        // Track whether Dispose has been called.
        private bool FDisposed = false;



        //input pin 
        private IStringIn FDirectories;
        private IValueIn FEnableServer;
        private INodeIn FHttpPageIn;
        private IValueIn FOpenBrowser;
        private IValueIn FSaveState;
        private IStringIn FPostFilename;
        private IStringIn FPostMessage;
        private IStringIn FLink;

        //output pin 
        private IStringOut FGetMessages;
        private IStringOut FPostMessages;
        private IStringOut FFileName;
        private IValueIn FTimeOut;
        
        //Config Pin
        private IValueConfig FPort;
        private IValueConfig FPageCount;


        private List<INodeIn> FInputPinList = new List<INodeIn>();
        private List<INodeIOBase> FUpstreamInterfaceList = new List<INodeIOBase>();
        private SortedList<string, IHttpPageIO> FNodeUpstream = new SortedList<string,IHttpPageIO>();


        //Server
        private int mPortNumber = 80;
        private int mBacklog = 500;
        //private VVVV.Webinterface.HttpServer.Server mServer;
        private ListenerTest FServer;
        private WebinterfaceSingelton mWebinterfaceSingelton = WebinterfaceSingelton.getInstance();
        private SortedList<string, byte[]> mHtmlPageList = new SortedList<string, byte[]>();
        private SortedList<string, string> mPostMessages = new SortedList<string, string>();

        private List<string> PageNames = new List<string>();
        List<string> mDirectories = new List<string>();

        private bool FAddPage = false;

        #endregion field declaration




        #region constructor/destructor


        /// <summary>
        /// Transformer constructer 
        /// nothing to declar in there
        /// </summary>
        public RendererHTTPListener()
        {
            //mServer = new VVVV.Webinterface.HttpServer.Server(mPortNumber, mBacklog);
            FServer = new ListenerTest(mPortNumber);
        }



        /// <summary>
        /// Implementing IDisposable's Dispose method.
        /// Do not make this method virtual.
        /// A derived class should not be able to override this method.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            // Take yourself off the Finalization queue
            // to prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }


        /// <summary>
        /// Dispose(bool disposing) executes in two distinct scenarios.
        /// If disposing equals true, the method has been called directly
        /// or indirectly by a user's code. Managed and unmanaged resources
        /// can be disposed.
        /// If disposing equals false, the method has been called by the
        /// runtime from inside the finalizer and you should not reference
        /// other objects. Only unmanaged resources can be disposed.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!FDisposed)
            {
                if (disposing)
                {


                    // Dispose managed resources.
                }
                // Release unmanaged resources. If disposing is false,
                // only the following code is executed.

                if (FServer.Running)
                {
                    FServer.Stop();
                    FServer.Close();
                }

                
                FServer = null;

                FHost.Log(TLogType.Debug, "Renderer (HTTP) Node is being deleted");

                // Note that this is not thread safe.
                // Another thread could start disposing the object
                // after the managed resources are disposed,
                // but before the disposed flag is set to true.
                // If thread safety is necessary, it must be
                // implemented by the client.
            }
            FDisposed = true;
        }


        /// <summary>
        /// Use C# destructor syntax for finalization code.
        /// This destructor will run only if the Dispose method
        /// does not get called.
        /// It gives your base class the opportunity to finalize.
        /// Do not provide destructors in types derived from this class.
        /// </summary>
        ~RendererHTTPListener()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }


        #endregion constructor/destructor




        #region node name and infos

        /// <summary>
        /// provide node infos 
        /// </summary>
        public static IPluginInfo PluginInfo
        {
            get
            {
                //fill out nodes info
                //see: http://www.vvvv.org/tiki-index.php?page=vvvv+naming+conventions
                IPluginInfo Info = new PluginInfo();
                Info.Name = "Renderer";							//use CamelCaps and no spaces
                Info.Category = "HTTP";						    //try to use an existing one
                Info.Version = "C# Listener";						//versions are optional. leave blank if not needed
                Info.Help = "";
                Info.Bugs = "";
                Info.Credits = "";								//give credits to thirdparty code used
                Info.Warnings = "";

                //leave below as is
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(true);
                System.Diagnostics.StackFrame sf = st.GetFrame(0);
                System.Reflection.MethodBase method = sf.GetMethod();
                Info.Namespace = method.DeclaringType.Namespace;
                Info.Class = method.DeclaringType.Name;
                return Info;
                //leave above as is
            }
        }

        /// <summary>
        /// Function AutoEvaluate is callrd from the Host VVVV to get Status true or fals 
        /// </summary>
        /// <returns></returns>
        public bool AutoEvaluate
        {
            //return true if this node needs to calculate every frame even if nobody asks for its output
            get { return true; }
        }

        #endregion node name and infos




        #region pin creation


        /// <summary>
        /// this method is called by vvvv when the node is created
        /// </summary>
        /// <param name="Host"></param>
        public void SetPluginHost(IPluginHost Host)
        {
            //assign host
            FHost = Host;

            try
            {
                string HostPath;
                FHost.GetHostPath(out HostPath);
                mWebinterfaceSingelton.HostPath = HostPath;

                //conig
                FHost.CreateValueConfig("Port", 1, null, TSliceMode.Single, TPinVisibility.OnlyInspector, out FPort);
                FPort.SetSubType(0, 65535, 1, 80, false, false, true);

                FHost.CreateValueConfig("Pagecount", 1, null, TSliceMode.Single, TPinVisibility.OnlyInspector, out FPageCount);
                FPageCount.SetSubType(1, double.MaxValue, 1, 1, false, false, true);


                //inputs
                FHost.CreateStringInput("Directories", TSliceMode.Dynamic, TPinVisibility.True, out FDirectories);
                FDirectories.SetSubType("", false);

                //FHost.CreateValueInput("Save", 1, null, TSliceMode.Single, TPinVisibility.True, out FSaveState);
                //FSaveState.SetSubType(0, 1, 1, 0, true, false, true);

                FHost.CreateValueInput("Enable", 1, null, TSliceMode.Single, TPinVisibility.True, out FEnableServer);
                FEnableServer.SetSubType(0, 1, 1, 1, false, true, true);

                FHost.CreateStringInput("POST Filename", TSliceMode.Dynamic, TPinVisibility.OnlyInspector, out FPostFilename);
                FPostFilename.SetSubType("", false);

                FHost.CreateStringInput("POST Message", TSliceMode.Dynamic, TPinVisibility.OnlyInspector, out FPostMessage);
                FPostMessage.SetSubType("", false);

                FHost.CreateValueInput("Open Browser", 1, null, TSliceMode.Single, TPinVisibility.OnlyInspector, out FOpenBrowser);
                FOpenBrowser.SetSubType(0, 1, 1, 0, true, false, true);

                FHost.CreateStringInput("Link", TSliceMode.Single, TPinVisibility.OnlyInspector, out FLink);
                FLink.SetSubType("http://127.0.0.1/index.html", false);

                //FHost.CreateValueInput("TimeOut", 1, null, TSliceMode.Single, TPinVisibility.OnlyInspector, out FTimeOut);
                //FTimeOut.SetSubType(0, double.MaxValue, 1, 0, false, false, true);

                FHost.CreateNodeInput("Input1", TSliceMode.Dynamic, TPinVisibility.True, out FHttpPageIn);
                FHttpPageIn.SetSubType(new Guid[1] { HttpPageIO.GUID }, HttpPageIO.FriendlyName);
                FInputPinList.Add(FHttpPageIn);


                //outputs	 
                FHost.CreateStringOutput("Files", TSliceMode.Dynamic, TPinVisibility.True, out FFileName);
                FFileName.SetSubType("", true);

                FHost.CreateStringOutput("GET", TSliceMode.Dynamic, TPinVisibility.Hidden, out FGetMessages);
                FGetMessages.SetSubType("", false);

                FHost.CreateStringOutput("POST", TSliceMode.Dynamic, TPinVisibility.Hidden, out FPostMessages);
                FPostMessages.SetSubType("", false);

            }
            catch (Exception ex)
            {
                FHost.Log(TLogType.Error, "Error Renderer (Http) by Pin Initialisation" + Environment.NewLine + ex.Message);
            }


        }

        #endregion pin creation




        #region NodeIO

        public void ConnectPin(IPluginIO Pin)
        {
            //cache a reference to the upstream interface when the NodeInput pin is being connected
            foreach (INodeIn pNodeIn in FInputPinList)
            {
                if (Pin == pNodeIn)
                {
                    INodeIOBase usI;
                    IHttpPageIO FUpstreamInterface;

                    pNodeIn.GetUpstreamInterface(out usI);
                    FUpstreamInterface = usI as IHttpPageIO;
                    FUpstreamInterfaceList.Add(usI);

                    FNodeUpstream.Add(Pin.Name, FUpstreamInterface);

                    string tPageName;
                    string tUrl;
                    ReadPageName(Pin.Name, out tUrl, out tPageName);

                    if (tPageName != null)
                    {
                        mWebinterfaceSingelton.AddConnectedPage(tPageName);
                    }
                }
            }

        }



        public void DisconnectPin(IPluginIO Pin)
        {
            //reset the cached reference to the upstream interface when the NodeInput is being disconnected
            foreach (INodeIn pNodeIn in FInputPinList)
            {
                if (Pin == pNodeIn)
                {
                    string tNodeName = Pin.Name;

                    string tPageName;
                    string tUrl;
                    ReadPageName(Pin.Name, out tUrl, out tPageName);

                    if (tPageName != null)
                    {
                        mWebinterfaceSingelton.RemoveDisconnectedPage(tPageName, tUrl);
                    }
                    
                    FNodeUpstream.Remove(Pin.Name);

                }
            }
            
        }
        #endregion NodeIO




        #region mainloop


        public void Configurate(IPluginConfig Input)
        {

            if (Input == FPageCount)
            {
                double count;
                FPageCount.GetValue(0, out count);

                int diff = FInputPinList.Count - (int)Math.Round(count);

                if (diff > 0) //delete pins
                {
                    for (int i = 0; i < diff; i++)
                    {
                        INodeIn pinToDelete = FInputPinList[FInputPinList.Count - 1];
                        FInputPinList.Remove(pinToDelete);
                        FNodeUpstream.Remove(pinToDelete.Name);
                        FHost.DeletePin(pinToDelete);
                        pinToDelete = null;
                    }

                }
                else if (diff < 0) //create pins
                {
                    for (int i = 0; i > diff; i--)
                    {
                        INodeIn newPin;
                        FHost.CreateNodeInput("Input" + (FInputPinList.Count + 1), TSliceMode.Dynamic, TPinVisibility.True, out newPin);
                        newPin.SetSubType(new Guid[1] { HttpPageIO.GUID }, HttpPageIO.FriendlyName);
                        FInputPinList.Add(newPin);
                    }
                }
            }
            else if (Input == FPort)
            {
                double PortNumber;
                FPort.GetValue(0, out PortNumber);
                mPortNumber = (int)PortNumber;

                if (FServer.Running)
                {
                    FServer.Stop();
                    FServer.Port = mPortNumber;
                    FServer.Start();
                }
                else
                {
                    FServer.Port = mPortNumber;
                }
            }
        }



        /// <summary>
        /// here we go, thats the method called by vvvv each frame
        /// all data handling should be in here
        /// </summary>
        /// <param name="SpreadMax"></param>
        public void Evaluate(int SpreadMax)
        {

            #region Enable Server 

            //double pState;
            //FEnableServer.GetValue(0, out pState);


            //if (FEnableServer.PinIsChanged)
            //{
            //    if (pState > 0.5)
            //    {
            //        mServer.Start();
            //        if (mServer.Running == false)
            //        {
            //            FHost.Log(TLogType.Error, String.Format("Http Port {0} taken by an other Application like Skype etc.. Please choose an other Port and Reenable the Render(HTTP).", mPortNumber));
            //        }
            //    }
            //    else if (pState < 0.5)
            //    {
            //        if (mServer.Running == true)
            //        {
            //            mServer.Stop();
            //        }
            //    }
            //}


            double pState;
            FEnableServer.GetValue(0, out pState);


            if (FEnableServer.PinIsChanged)
            {
                if (pState > 0.5)
                {
                    FServer.Start();
                    if (FServer.Running == false)
                    {
                        FHost.Log(TLogType.Error, String.Format("Http Port {0} taken by an other Application like Skype etc.. Please choose an other Port and Reenable the Render(HTTP).", mPortNumber));
                    }
                }
                else if (pState < 0.5)
                {
                    if (FServer.Running == true)
                    {
                        FServer.Stop();
                    }
                }
            }
                
            


            #endregion Enable Server


            #region Open Browser

            if (FOpenBrowser.PinIsChanged || FLink.PinIsChanged)
            {
                double currentValue;
                FOpenBrowser.GetValue(0, out currentValue);
                string currentLink;
                FLink.GetString(0, out currentLink);

                if (currentValue > 0.5)
                {
                    System.Diagnostics.Process.Start(currentLink);
                }
            }

            #endregion Open Browser


            #region SaveState

            //if (FSaveState.PinIsChanged)
            //{
            //    double Save;
            //    FSaveState.GetValue(0, out Save);
            //    if (Save > 0.5)
            //    {
            //        mWebinterfaceSingelton.SaveDataToFile();
            //    }

            //}

            #endregion SaveState


            #region Directories

            if (FDirectories.PinIsChanged)
            {
                List<string> tDirectories = new List<string>();
                List<string> tFiles = new List<string>();

                for (int i = 0; i < FDirectories.SliceCount; i++)
                {
                    string tCurrentDirectories;
                    FDirectories.GetString(i, out tCurrentDirectories);
                    if (!String.IsNullOrEmpty(tCurrentDirectories))
                    {
                        if (Directory.Exists(tCurrentDirectories))
                        {

                            tDirectories.Add(tCurrentDirectories);

                            DirectoryInfo tInfo = new DirectoryInfo(tCurrentDirectories);
                            FileInfo[] tFileInfos = tInfo.GetFiles();

                            for (int j = 0; j < tFileInfos.Length; j++)
                            {
                                tFiles.Add(tFileInfos[j].Name);
                            }
                        }
                        else
                        {
                            FHost.Log(TLogType.Message, "Definded Directory for Renderer (HTTP) does not exist: " + Environment.NewLine + tCurrentDirectories);
                        }
                    }
                }

                Ressources Ressource = new Ressources();
                tFiles.AddRange(Ressource.Files);



                FServer.FoldersToServ = tDirectories;
                

                if (tFiles.Count > 0)
                {
                    FFileName.SliceCount = tFiles.Count;
                    for (int i = 0; i < tFiles.Count; i++)
                    {
                        FFileName.SetString(i, tFiles[i]);
                    }
                }
                else
                {
                    FFileName.SliceCount = 0; 
                }

            }


            

            #endregion Directories


            #region Get Post Messages


            try
            {

                if (FServer != null)
                {
                    List<string> tGetMessages;
                    List<string> tPostMessages;
                    mWebinterfaceSingelton.getRequestMessage(out tGetMessages, out tPostMessages);


                    if (tGetMessages != null && tGetMessages.Count > 0)
                    {
                        FGetMessages.SliceCount = tGetMessages.Count;
                        for (int i = 0; i < tGetMessages.Count; i++)
                        {
                            FGetMessages.SetString(i, tGetMessages[i]);
                        }
                    }
                    else
                    {
                        FGetMessages.SliceCount = 0;

                    }

                    if (tPostMessages != null && tPostMessages.Count > 0)
                    {
                        FPostMessages.SliceCount = tPostMessages.Count;
                        for (int i = 0; i < tPostMessages.Count; i++)
                        {
                            FPostMessages.SetString(i, tPostMessages[i]);
                        }
                    }
                    else
                    {
                        FPostMessages.SliceCount = 0;
                    }

                }
            }
            catch (Exception ex)
            {
                FHost.Log(TLogType.Error, "Error in Renderer (Http) -  GET POST Handling" + Environment.NewLine + ex.Message);
            }



            #endregion Get Post Messages


            //#region TimeOut
            //if (FTimeOut.PinIsChanged)
            //{
            //   double TimeOut = 0;
            //   FTimeOut.GetValue(0,out TimeOut);
            //   mWebinterfaceSingelton.TimeOut = (int)TimeOut;
            //}


            //#endregion TimeOut


            #region POST Input 

            if (FPostMessage.PinIsChanged || FPostFilename.PinIsChanged)
            {

                mPostMessages.Clear();
                for (int i = 0; i < SpreadMax; i++)
                {
                    string currentPostFileName = String.Empty;
                    string currentPostMessage = String.Empty;

                    FPostFilename.GetString(i, out currentPostFileName);
                    FPostMessage.GetString(i, out currentPostMessage);

                    if (currentPostFileName != null)
                    {
                        if (mPostMessages.ContainsKey(currentPostFileName))
                        {
                            mPostMessages.Remove(currentPostFileName);
                            mPostMessages.Add(currentPostFileName, currentPostMessage);
                        }
                        else
                        {
                            mPostMessages.Add(currentPostFileName, currentPostMessage);
                        }
                    }
                }

                if (FServer != null)
                {
                    FServer.PostMessages = mPostMessages;
                }
            }
            #endregion POST Input

            #region ReadErrorMessages
            if (FServer != null)
            {
                List<string> Messages = FServer.ErrorMessages;
                if (Messages.Count > 0)
                {
                    foreach (string Message in Messages)
                        FHost.Log(TLogType.Error, Message);
                }
            }
            

            #endregion ReadErrorMessages

        }

        #endregion mainloop



        #region HandleUpstream



        private void ReadPageName(string pNodeName, out string pUrl, out string PageName)
        {
            IHttpPageIO FUpstream;
            FNodeUpstream.TryGetValue(pNodeName, out FUpstream);
            string tPageName = "";
            string tFileName;

            if (FUpstream != null)
            {
                FUpstream.GetPage( out tPageName, out tFileName);
                PageName = tPageName;
                pUrl = tFileName;
            }
            else
            {
                PageName = null;
                pUrl = null;
            }


        }


        #endregion HandleUpstream
    }
}
