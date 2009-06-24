
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

using VVVV.Webinterface;
using VVVV.Webinterface.Utilities;
using VVVV.Webinterface.Data;
using VVVV.Webinterface.HttpServer;
using VVVV.Nodes.HTTP;
using VVVV.Nodes.HttpGUI;
using VVVV.Nodes.HttpGUI.Datenobjekte;


//the vvvv node namespace
namespace VVVV.Nodes.HTTP
{

    /// <summary>
    /// node to put all html nodes to one html page
    /// </summary>
    public class Renderer : IPlugin, IDisposable, IPluginConnections
    {





        #region field declaration

        //the host (mandatory)
        private IPluginHost FHost;
        // Track whether Dispose has been called.
        private bool FDisposed = false;



        //input pin 
        private IStringIn FDirectories;
        private IValueIn FEnableServer;
        private IValueIn FOpenBrowser;
        private IValueIn FPageWidth;
        private IValueIn FPageHeight;

        private INodeIn FHttpPageIn;
        private IHttpPageIO FUpstreamInterface;


        //output pin 
        //private IStringOut FFileName;
        //private IStringOut FFileList;
        

        //Config Pin
        private IValueIn FPort;


        //Server
        private VVVV.Webinterface.HttpServer.Server mServer;
        private string mServerFolder;
        private WebinterfaceSingelton mWebinterfaceSingelton = WebinterfaceSingelton.getInstance();
        private SortedList<string, byte[]> mHtmlPageList = new SortedList<string, byte[]>();
        private List<string> PageNames = new List<string>();

        #endregion field declaration







        #region constructor/destructor


        /// <summary>
        /// Transformer constructer 
        /// nothing to declar in there
        /// </summary>
        public Renderer()
        {
            mServerFolder = mWebinterfaceSingelton.FolderToServ;
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

                mServer.Dispose();
                FHost.Log(TLogType.Debug, "Renderer (HTML) Node is being deleted");

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
        ~Renderer()
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
                Info.Version = "Server";						//versions are optional. leave blank if not needed
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

            //create inputs

            //FHost.Log(TLogType.Message, "my ID is: " + m_ID.ToString());
            //FHost.Log(TLogType.Message, "in the Webpage Render Array are: " + m_DataWareHouse.WebpageRendererNodeCount.ToString() + " Elements");

            FHost.CreateNodeInput("Input", TSliceMode.Dynamic, TPinVisibility.True, out FHttpPageIn);
            FHttpPageIn.SetSubType(new Guid[1] { HttpPageIO.GUID }, HttpPageIO.FriendlyName);

            FHost.CreateStringInput("Directories", TSliceMode.Dynamic, TPinVisibility.True, out FDirectories);
            FDirectories.SetSubType(mServerFolder, false);

            FHost.CreateValueInput("Browser Width", 1, null, TSliceMode.Single, TPinVisibility.True, out FPageWidth);
            FPageWidth.SetSubType(0, double.MaxValue, 1, -1, false, false, true);

            FHost.CreateValueInput("Browser Height", 1, null, TSliceMode.Single, TPinVisibility.True, out FPageHeight);
            FPageHeight.SetSubType(0, double.MaxValue, 1, -1, false, false, true);

            FHost.CreateValueInput("Open Browser", 1, null, TSliceMode.Single, TPinVisibility.OnlyInspector, out FOpenBrowser);
            FOpenBrowser.SetSubType(0, 1, 1, 0, true, false, true);

            FHost.CreateValueInput("Enable", 1, null, TSliceMode.Single, TPinVisibility.Hidden, out FEnableServer);
            FEnableServer.SetSubType(0, 1, 1, 1, false, true, true);

            //create outputs	    	   
            //FHost.CreateStringOutput("Files", TSliceMode.Dynamic, TPinVisibility.True, out FFileName);
            //FFileName.SetSubType("", true);

            //FHost.CreateStringOutput("Server File List", TSliceMode.Dynamic, TPinVisibility.Hidden, out FFileList);
            //FFileList.SetSubType("", true);

            //create Config Pin
            FHost.CreateValueInput("Port", 1, null, TSliceMode.Single, TPinVisibility.OnlyInspector, out FPort);
            FPort.SetSubType(1, 65535, 1, 80, false, false, true);


        }

        #endregion pin creation





        #region NodeIO
        public void ConnectPin(IPluginIO Pin)
        {
            //cache a reference to the upstream interface when the NodeInput pin is being connected
            if (Pin == FHttpPageIn)
            {
                INodeIOBase usI;
                FHttpPageIn.GetUpstreamInterface(out usI);
                FUpstreamInterface = usI as IHttpPageIO;
            }
        }



        public void DisconnectPin(IPluginIO Pin)
        {
            //reset the cached reference to the upstream interface when the NodeInput is being disconnected
            if (Pin == FHttpPageIn)
            {
                FUpstreamInterface = null;
            }
        }
        #endregion NodeIO





        #region mainloop


        /// <summary>
        /// nothing to configure in this plugin
        /// only used in conjunction with inputs of type cmpdConfigurate
        /// </summary>
        /// <param name="Input"></param>
        public void Configurate(IPluginConfig Input)
        {
            if (Input == FPort)
            {
                if (mServer != null)
                {
                    double tPort;
                    FPort.GetValue(0, out tPort);
                    mServer.Port = Convert.ToInt32(tPort);
                }
                else { return; }
            }

        }



        /// <summary>
        /// here we go, thats the method called by vvvv each frame
        /// all data handling should be in here
        /// </summary>
        /// <param name="SpreadMax"></param>
        public void Evaluate(int SpreadMax)
        {


            #region Upstream


            int usS;

            //Saves the incoming Html Slices
            if (FUpstreamInterface != null)
            {
                //loop for all slices0

                for (int i = 0; i < FHttpPageIn.SliceCount; i++)
                {
                    //get upstream slice index

                    FHttpPageIn.GetUpsreamSlice(i, out usS);

                    string PageName;
                    Page tPage;
                    string tJsFile;
                    string tCssFile;
                    string tUrl;
                    FUpstreamInterface.GetPage( out tPage, out tCssFile, out tJsFile, out PageName);
                    FUpstreamInterface.GetUrl( out tUrl);

                   

                    if (PageNames.Contains(PageName))
                    {
                        PageNames.Remove(PageName);
                        PageNames.Add(PageName);

                        mHtmlPageList.Remove(tUrl);
                        mHtmlPageList.Add(tUrl, Encoding.UTF8.GetBytes(tPage.Text));

                        mHtmlPageList.Remove("VVVV.css");
                        mHtmlPageList.Add("VVVV.css", Encoding.UTF8.GetBytes(tCssFile));

                        mHtmlPageList.Remove("VVVV.js");
                        mHtmlPageList.Add("VVVV.js", Encoding.UTF8.GetBytes(tJsFile));

                    }
                    else
                    {
                        PageNames.Add(PageName);
                        mHtmlPageList.Add(tUrl, Encoding.UTF8.GetBytes(tPage.Text));
                        mHtmlPageList.Add("VVVV.css", Encoding.UTF8.GetBytes(tCssFile));
                        mHtmlPageList.Add("VVVV.js", Encoding.UTF8.GetBytes(tJsFile));

                    }


                    if (mServer != null)
                    {
                        mServer.HtmlPages = mHtmlPageList;
                    }
                }
            }



            #endregion Upstream




            #region Enable Server

            double pState;
            FEnableServer.GetValue(0, out pState);



            if (FEnableServer.PinIsChanged)
            {
                if (pState > 0.5)
                {
                    mServer = new VVVV.Webinterface.HttpServer.Server(80, 50, mWebinterfaceSingelton.Subject, "ServerOne", mServerFolder);
                    mWebinterfaceSingelton.AddServhandling(mServer);
                    //mServer.ServeFolder(mServerFolder);
                    mServer.Start();
                }
                else
                {
                    if (mServer != null)
                    {
                        mWebinterfaceSingelton.DeleteServhandling(mServer);
                        mServer.Stop();
                        mServer = null;
                    }

                }
            }


            #endregion Enable Server




            #region Browser Window

            {
                double currentWidthSlice;
                double currentHeightSlice;
                FPageWidth.GetValue(0, out currentWidthSlice);
                FPageHeight.GetValue(0, out currentHeightSlice);

                string tBrowserWidth = "" + Math.Round(currentWidthSlice);
                string tBrowserHeight = "" + Math.Round(currentHeightSlice);
            }

            #endregion Browser Window




            #region Open Browser

            if (FOpenBrowser.PinIsChanged)
            {
                double currentValue;
                FOpenBrowser.GetValue(1, out currentValue);

                if (currentValue == 1)
                {
                    System.Diagnostics.Process.Start("http://localhost/index.html");
                }
            }

            #endregion Open Browser




            #region Directories

            if (FDirectories.PinIsChanged)
            {
                List<string> tDirectories = new List<string>();

                for (int i = 0; i < FDirectories.SliceCount; i++)
                {
                    string tCurrentDirectories;
                    FDirectories.GetString(i, out tCurrentDirectories);
                    tDirectories.Add(tCurrentDirectories);
                }

                mServer.FoldersToServ = tDirectories;
            }

            #endregion Directories




            #region files to serve

            if (mServer != null)
            {
                //List<string> tFileList = mServer.FileList;
                //List<string> tFileName = mServer.FileNames;

                //FFileList.SliceCount = tFileList.Count;
                //FFileName.SliceCount = tFileName.Count;

                //for (int i = 0; i < tFileList.Count; i++)
                //{
                //    FFileList.SetString(i, tFileList[i]);

                //}

                //for (int i = 0; i < tFileName.Count; i++)
                //{
                //    FFileName.SetString(i, tFileName[i]);
                //}
            }

            #endregion files to serve

        }

        #endregion mainloop
    }
}
