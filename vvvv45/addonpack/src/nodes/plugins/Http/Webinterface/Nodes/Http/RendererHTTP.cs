
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
    public class RendererHTTP : IPlugin, IDisposable, IPluginConnections
    {


        #region field declaration

        //the host (mandatory)
        private IPluginHost FHost;
        // Track whether Dispose has been called.
        private bool FDisposed = false;

        //input pin declaration
        private IStringIn FHtmlBody;
        private IStringIn FHtmlHead;
        private IStringIn FTitel;
        private IStringIn FUrl;
        private IValueIn FReload;
        private IStringIn FPath;
        private IValueIn FSavePage;
        private IValueIn FOpenBrowser;
        private IValueIn FPlattform;
        private IValueIn FPageWidth;
        private IValueIn FPageHeight;

        //private IEnumIn FPlattform; 

        //output pin declaration
        private IStringOut FWholeHTML;
        private IStringOut FUrlOut;
        private IStringOut FFileName;
        private IStringOut FFileList;
        private WebinterfaceSingelton mWebinterfaceSingelton = WebinterfaceSingelton.getInstance();

        ///<summery>Config Pin</summery>
        private IValueConfig FSartServer;
        private IValueConfig FPort;
        private IValueConfig FReloadServerFolder;
        private IStringConfig FServerFolder;

        //HttpGuiInterface
        private INodeIn FHttpGuiIn;
        private IHttpGUIIO FUpstreamInterface;

        private SortedList<int, string> mGuiTypes;
        private SortedList<int, SortedList<string, string>> mHtmlAttributs;
        private SortedList<int, SortedList<string, string>> mCssStyles;
        private SortedList<string, string> mCssBodyPropertiesIn = new SortedList<string,string>();
        private string[] mHtmlText;
        private SortedList<int, BaseDatenObjekt> mGuiDatenListe;
        
        private INodeIn FCssPropertiesIn;
        private IHttpGUIStyleIO FUpstreamStyleIn;
        
        //Server
        private Server mServer;
        private string mServerFolder;


        //HTML Page
        private string mPageHead;
        private string mPageBody;
        private string mCssFile = "";
        private string mJsFile = "";


        //Builder 
        //private IPhoneBuilder mIPhone;



        #endregion field declaration







        #region constructor/destructor


        /// <summary>
        /// Transformer constructer 
        /// nothing to declar in there
        /// </summary>
        public RendererHTTP()
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
        ~RendererHTTP()
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
                Info.Version = "";						        //versions are optional. leave blank if not needed
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

            FHost.CreateNodeInput("Input", TSliceMode.Dynamic, TPinVisibility.True, out FHttpGuiIn);
            FHttpGuiIn.SetSubType(new Guid[1] { HttpGUIIO.GUID }, HttpGUIIO.FriendlyName);

            FHost.CreateStringInput("Titel", TSliceMode.Single, TPinVisibility.True, out FTitel);
            FTitel.SetSubType("VVVV Webinterface", false);

            FHost.CreateStringInput("Url", TSliceMode.Single, TPinVisibility.True, out FUrl);
            FUrl.SetSubType("index.html", false);

            FHost.CreateValueInput("Browser Width",1, null, TSliceMode.Single, TPinVisibility.True, out FPageWidth);
            FPageWidth.SetSubType(0, double.MaxValue, 1, -1, false, false, true);

            FHost.CreateValueInput("Browser Height",1, null, TSliceMode.Single, TPinVisibility.True, out FPageHeight);
            FPageHeight.SetSubType(0, double.MaxValue, 1, -1, false, false, true);
			
            FHost.CreateNodeInput("Style Properties",  TSliceMode.Single, TPinVisibility.True, out FCssPropertiesIn);
            FCssPropertiesIn.SetSubType(new Guid[1] { HttpGUIStyleIO.GUID }, HttpGUIStyleIO.FriendlyName);
            
            FHost.CreateValueInput("Reload Browser", 1, null, TSliceMode.Single, TPinVisibility.True, out FReload);
            FReload.SetSubType(0, 1, 1, 0, true, false, true);

            FHost.CreateStringInput("HTML Head", TSliceMode.Dynamic, TPinVisibility.OnlyInspector, out FHtmlHead);
            FHtmlHead.SetSubType("", false);

            FHost.CreateStringInput("HTML Body", TSliceMode.Dynamic, TPinVisibility.OnlyInspector, out FHtmlBody);
            FHtmlBody.SetSubType("", false);

            FHost.CreateValueInput("Open Browser", 1, null, TSliceMode.Single, TPinVisibility.OnlyInspector, out FOpenBrowser);
            FOpenBrowser.SetSubType(0, 1, 1, 0, true, false, true);

            FHost.CreateStringInput("File Path", TSliceMode.Single, TPinVisibility.OnlyInspector, out FPath);
            FPath.SetSubType(Application.StartupPath + "\\plugins\\webinterface", true);

            FHost.CreateValueInput("Save", 1, null, TSliceMode.Single, TPinVisibility.OnlyInspector, out FSavePage);
            FSavePage.SetSubType(0, 1, 1, 0, true, false, true);

            //FHost.CreateValueInput("Switch Builder", 1, null, TSliceMode.Dynamic, TPinVisibility.OnlyInspector, out FPlattform);
            //FPlattform.SetSubType(0, double.MaxValue, 1, 0, false, false, true);

            //FHost.CreateValueInput("Reload Interval", 1, null, TSliceMode.Single, TPinVisibility.True, out FReloadInterval);
            //FReloadInterval.SetSubType(0, double.MaxValue, 1, 1000, true, false, true);

            //FHost.CreateStringInput("CSS", TSliceMode.Single, TPinVisibility.True, out FWebAttribute);
            //FWebAttribute.SetSubType("", false);

            
            //create outputs	    	   
            FHost.CreateStringOutput("Files", TSliceMode.Dynamic, TPinVisibility.True, out FFileName);
            FFileName.SetSubType("", true);

            FHost.CreateStringOutput("Server File List", TSliceMode.Dynamic, TPinVisibility.Hidden, out FFileList);
            FFileList.SetSubType("", true);

            FHost.CreateStringOutput("Output", TSliceMode.Dynamic, TPinVisibility.Hidden, out FWholeHTML);
            FWholeHTML.SetSubType("", false);

            

            //create Config Pin
            FHost.CreateValueConfig("Enable Server", 1, null, TSliceMode.Single, TPinVisibility.OnlyInspector, out FSartServer);
            FSartServer.SetSubType(0, 1, 1, 0, false, true, true);

            FHost.CreateValueConfig("Port", 1, null, TSliceMode.Single, TPinVisibility.OnlyInspector, out FPort);
            FPort.SetSubType(1, 65535, 1, 80, false, false, true);

            FHost.CreateStringConfig("ServerFolder", TSliceMode.Single, TPinVisibility.OnlyInspector, out FServerFolder);
            FServerFolder.SetSubType(mServerFolder, true);

            FHost.CreateValueConfig("Reload Server Folder", 1, null, TSliceMode.Single, TPinVisibility.OnlyInspector, out FReloadServerFolder);
            FReloadServerFolder.SetSubType(0, 1, 1, 0, true, false, true);

            
        }

        #endregion pin creation







        #region NodeIO
        public void ConnectPin(IPluginIO Pin)
        {
            //cache a reference to the upstream interface when the NodeInput pin is being connected
            if (Pin == FHttpGuiIn)
            {
                INodeIOBase usI;
                FHttpGuiIn.GetUpstreamInterface(out usI);
                FUpstreamInterface = usI as IHttpGUIIO;
            }else if( Pin == FCssPropertiesIn)
            {
            	INodeIOBase usI;
                FCssPropertiesIn.GetUpstreamInterface(out usI);
                FUpstreamStyleIn = usI as IHttpGUIStyleIO;
            }
        }



        public void DisconnectPin(IPluginIO Pin)
        {
            //reset the cached reference to the upstream interface when the NodeInput is being disconnected
            if (Pin == FHttpGuiIn)
            {
                FUpstreamInterface = null;
            }
            else if (Pin == FCssPropertiesIn)
            {
            	FUpstreamStyleIn = null;
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
            if (Input == FSartServer)
            {
                double pState;
                FSartServer.GetValue(0,out pState);
                if(pState >= 1)
                {
                    mServer = new Server(80, 50, mWebinterfaceSingelton.Subject, "ServerOne",mServerFolder);
                    mWebinterfaceSingelton.AddServhandling(mServer);
                    mServer.ServeFolder(mServerFolder);


                }
                else
                {
                    if (mServer != null)
                    {
                        mWebinterfaceSingelton.DeleteServhandling(mServer);
                        mServer.Dispose();
                    }
                    
                }

            }

            if (Input == FPort)
            {
                if (mServer != null)
                {
                    double tPort;
                    FPort.GetValue(0,out tPort);
                    mServer.Port = Convert.ToInt32(tPort);
                }
                else{return;}
            }

            if (Input == FReloadServerFolder)
            {
                double tValue;
                FReloadServerFolder.GetValue(0, out tValue);
                if (tValue >= 1)
                {
                    mServer.ServeFolder(mServerFolder);
                }
            }
            if (Input == FServerFolder)
            {
                string tFolder;
                FServerFolder.GetString(0,out tFolder);
                mServerFolder = tFolder;
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


            mGuiTypes = new SortedList<int, string>();
            mHtmlAttributs = new SortedList<int, SortedList<string, string>>();
            mCssStyles = new SortedList<int, SortedList<string, string>>();
            mHtmlText = new string[SpreadMax];

            mGuiDatenListe = new SortedList<int, BaseDatenObjekt>();
            int usS;

            //Saves the incoming Html Slices
            if (FUpstreamInterface != null)
            {
                //loop for all slices0

                for (int i = 0; i < FHttpGuiIn.SliceCount; i++)
                {
                    //get upstream slice index

                    FHttpGuiIn.GetUpsreamSlice(i, out usS);

                    BaseDatenObjekt tGuiDaten;
                    FUpstreamInterface.GetDatenObjekt(usS, out tGuiDaten);

                    if (tGuiDaten != null)
                    {
                        //Debug.WriteLine("Objekt Type in Group: " + tGuiDaten.Type);
                        mGuiDatenListe.Add(i, tGuiDaten);
                    }
                }
            }
            


            #endregion Upstream

            if (FReload.PinIsChanged)
            {
                double currentReloadSlice;
                FReload.GetValue(0, out currentReloadSlice);
                if (currentReloadSlice > 0.5)
                {
                    mWebinterfaceSingelton.Reload();
                }
            }



            #region Get HTML Body and Head String

            if ( FHtmlHead.PinIsChanged || FHtmlBody.PinIsChanged)
            {
             
                string tContentBody = "";
                string tContentHead = "";
                double tReload;


                FReload.GetValue(0, out tReload);
                
                // Get HTML Body string
                for (int i = 0; i < FHtmlBody.SliceCount; i++)
                {

                    string currentHtmlBodySlice = "";
                    FHtmlBody.GetString(i, out currentHtmlBodySlice);
                    tContentBody += currentHtmlBodySlice + Environment.NewLine;

                }

                mPageBody = tContentBody;
                
                // Get Html Head String
                for (int i = 0; i < FHtmlHead.SliceCount; i++)
                {
                    string currentHtmlHeadSlice = "";
                    FHtmlHead.GetString(i, out currentHtmlHeadSlice);
                    tContentHead += currentHtmlHeadSlice + Environment.NewLine;
                }

                mPageHead = tContentHead;


            }

            //Upstream Css Properties
            int uSSSytle;
            if (FUpstreamStyleIn != null)
            {
                mCssBodyPropertiesIn.Clear();

                FCssPropertiesIn.GetUpsreamSlice(0, out uSSSytle);
                SortedList<string, string> tStylePropertie = new SortedList<string, string>();
                FUpstreamStyleIn.GetCssProperties(0, out tStylePropertie);

                if (tStylePropertie != null)
                {
                    mCssBodyPropertiesIn = tStylePropertie;
                }
                else
                {
                    tStylePropertie.Add("background-color", "#E0E0E0");
                    mCssBodyPropertiesIn = tStylePropertie;
                }

            }
            else
            {
                SortedList<string, string> tStylePropertie = new SortedList<string, string>();
                tStylePropertie.Add("background-color", "#E0E0E0");
                mCssBodyPropertiesIn = tStylePropertie;
            }

            #endregion Get HTML Body and Head String



            
            #region Build Page

            Page tPage;
            //double currentBuildBang;
            //FBuild.GetValue(0, out currentBuildBang);

            //if (currentBuildBang > 0.5)
            //{

                //string HtmlHead = "<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 4.01 Transitional//EN\" \"http://www.w3.org/TR/html4/loose.dtd\">";
            string HtmlHead = "<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 4.01 Transitional//EN\" \"http://www.w3.org/TR/html4/loose.dtd\">";
                SortedList<string, string> tServerDaten = new SortedList<string, string>();


                string currentSliceUrl = "";
                FUrl.GetString(0, out currentSliceUrl);

                //double currentPlattform;
                //FPlattform.GetValue(0, out currentPlattform);

                

                double currentWidthSlice;
                double currentHeightSlice;
                FPageWidth.GetValue(0, out currentWidthSlice);
                FPageHeight.GetValue(0, out currentHeightSlice);


                string tBrowserWidth = "" + Math.Round(currentWidthSlice);
                string tBrowserHeight = "" + Math.Round(currentHeightSlice);


                JQueryBuilder tJQuery = new JQueryBuilder(mGuiDatenListe, tBrowserWidth, tBrowserHeight, mCssBodyPropertiesIn);
                mJsFile = tJQuery.JsFile;
                mCssFile = tJQuery.CssMainFile;
                tPage = tJQuery.Page;



                tPage.Body.Insert(mPageBody);
                tPage.Head.Insert(mPageHead);


                string currentSliceTitel = "";
                FTitel.GetString(0, out currentSliceTitel);
                tPage.Head.Insert(new Title(currentSliceTitel));
           

                if (mServer != null)
                {
                    mServer.VVVVCssFile = mCssFile;
                    mServer.VVVVJsFile = mJsFile;
                }
                

                tServerDaten.Add(currentSliceUrl, HtmlHead + tPage.Text);
                mWebinterfaceSingelton.ServerDaten = tServerDaten;
                FWholeHTML.SetString(0, HtmlHead + tPage.Text);

                



            #endregion Build Page


            


            #region Save Page

                string tPath;
                double tSave;
                string tUrl;
                
                FPath.GetString(0, out tPath);
                FSavePage.GetValue(0, out tSave);
                FUrl.GetString(0, out tUrl);

                SortedList<string,string> tFiles =  new SortedList<string,string>();
                tFiles.Add(tPath + "\\" + tUrl, tPage.Text);
                tFiles.Add(tPath + "\\" + "VVVV.css", mCssFile);
                tFiles.Add(tPath + "\\" + "VVVV.js", mJsFile);
                

                if (FSavePage.PinIsChanged)
                {
                    if (tSave > 0.5)
                    {
                        
                        foreach(KeyValuePair<string,string> pFile in tFiles)
                        {
                            try
                            {

                                if (File.Exists(pFile.Key))
                                {
                                    File.Delete(pFile.Key);
                                    HTMLToolkit.SavePage(pFile.Key, pFile.Value);
                                    FHost.Log(TLogType.Message, "File: " + tPage + " has been deleted an resaved");
                                }                     
                                else
                                {
                                    HTMLToolkit.SavePage(pFile.Key, pFile.Value);
                                }
                            }   
                            catch
                            {
                                throw new Exception("somthing wrong in here Renderer");
                            }
                        }
                    }
                }

            #endregion Save Page




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




            #region files to serve
            if (mServer != null)
            {
                List<string> tFileList = mServer.FileList;
                List<string> tFileName = mServer.FileNames;

                FFileList.SliceCount = tFileList.Count;
                FFileName.SliceCount = tFileName.Count;

                for (int i = 0; i < tFileList.Count; i++)
                {
                    FFileList.SetString(i, tFileList[i]);

                }

                for (int i = 0; i < tFileName.Count; i++)
                {
                    FFileName.SetString(i, tFileName[i]);
                }
            }

            #endregion files to serve
        }
        

        #endregion mainloop
    }
}
