
#region licence/info

//////project name


//////description

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
//phlegma

#endregion licence/info




//use what you need
using System;
using System.Drawing;
using VVVV.PluginInterfaces.V1;
using System.IO;
using System.Collections.Generic;
using System.Collections;
using System.Windows.Forms;
using System.Diagnostics;

using VVVV.Webinterface;
using VVVV.Webinterface.Utilities;
using VVVV.Webinterface.Data;
using VVVV.Nodes.Http;
using VVVV.Nodes.HttpGUI;
using VVVV.Nodes.HttpGUI.Datenobjekte;


//the vvvv node namespace
namespace VVVV.Nodes.Http
{

    /// <summary>
    /// node to put all html nodes to one html page
    /// </summary>
    public class RendererHtmlPage : IPlugin, IDisposable, IPluginConnections, IHttpPageIO
    {


        #region field declaration

        //the host (mandatory)
        private IPluginHost FHost;
        // Track whether Dispose has been called.
        private bool FDisposed = false;


        //input pin
        private IStringIn FHtmlBody;
        private IStringIn FHtmlHead;
        private IStringIn FTitel;
        private IStringIn FUrl;
        private IValueIn FReload;
        private IStringIn FPath;
        private IValueIn FSavePage;
        private IEnumIn FCommunication;
        private IValueIn FPageWidth;
        private IValueIn FPageHeight;

        private INodeIn FHttpGuiIn;
        private IHttpGUIIO FUpstreamInterface;

        private INodeOut FHttpPageOut;


        //output pin
        private IStringOut FWholeHTML;
        private WebinterfaceSingelton mWebinterfaceSingelton = WebinterfaceSingelton.getInstance();

        //HttpGuiInterface  
        private SortedList<int, string> mGuiTypes;
        private SortedList<int, SortedList<string, string>> mHtmlAttributs;
        private SortedList<int, SortedList<string, string>> mCssStyles;
        private SortedList<int,SortedList<string, string>> mCssPropertiesSpread = new SortedList<int,SortedList<string,string>>();
        private string[] mHtmlText;
        private SortedList<int, BaseDatenObjekt> mGuiDatenListe;

        private INodeIn FCssPropertiesIn;
        private IHttpGUIStyleIO FUpstreamStyleIn;

        //HTML Page
        private Page mPage;
        private string mPageName;
        private string mUrl = String.Empty;
        private string mPageHead = String.Empty;
        private string mPageBody = String.Empty;
        private string mCssFile = String.Empty;
        private string mJsFile = String.Empty;



        #endregion field declaration







        #region constructor/destructor

        /// <summary>
        /// Transformer constructer 
        /// nothing to declar in there
        /// </summary>
        public RendererHtmlPage()
        {
           
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

                FHost.Log(TLogType.Debug, "Page (HTTP) Node is being deleted");

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
        ~RendererHtmlPage()
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
                Info.Name = "Page";							//use CamelCaps and no spaces
                Info.Category = "HTTP";						    //try to use an existing one
                Info.Version = "";						//versions are optional. leave blank if not needed
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


            //Input
            FHost.CreateNodeInput("Input", TSliceMode.Dynamic, TPinVisibility.True, out FHttpGuiIn);
            FHttpGuiIn.SetSubType(new Guid[1] { HttpGUIIO.GUID }, HttpGUIIO.FriendlyName);

            FHost.CreateStringInput("Titel", TSliceMode.Single, TPinVisibility.True, out FTitel);
            FTitel.SetSubType("VVVV Webinterface", false);

            FHost.CreateStringInput("Url", TSliceMode.Single, TPinVisibility.True, out FUrl);
            FUrl.SetSubType("index.html", false);

            FHost.CreateNodeInput("CSS", TSliceMode.Dynamic, TPinVisibility.True, out FCssPropertiesIn);
            FCssPropertiesIn.SetSubType(new Guid[1] { HttpGUIStyleIO.GUID }, HttpGUIStyleIO.FriendlyName);

            FHost.CreateValueInput("Browser Width", 1, null, TSliceMode.Single, TPinVisibility.True, out FPageWidth);
            FPageWidth.SetSubType(0, double.MaxValue, 1, -1, false, false, true);

            FHost.CreateValueInput("Browser Height", 1, null, TSliceMode.Single, TPinVisibility.True, out FPageHeight);
            FPageHeight.SetSubType(0, double.MaxValue, 1, -1, false, false, true);

            FHost.CreateValueInput("Reload Browser", 1, null, TSliceMode.Single, TPinVisibility.True, out FReload);
            FReload.SetSubType(0, 1, 1, 0, true, false, true);

            FHost.UpdateEnum("Communication", "Manual", new string[] { "Manual", "Polling", "Comet" });
            FHost.CreateEnumInput("Communication", TSliceMode.Single, TPinVisibility.True, out FCommunication);
            FCommunication.SetSubType("Communication");

            FHost.CreateStringInput("HTML Head", TSliceMode.Dynamic, TPinVisibility.OnlyInspector, out FHtmlHead);
            FHtmlHead.SetSubType("", false);

            FHost.CreateStringInput("HTML Body", TSliceMode.Dynamic, TPinVisibility.OnlyInspector, out FHtmlBody);
            FHtmlBody.SetSubType("", false);

            FHost.CreateStringInput("File Path", TSliceMode.Single, TPinVisibility.OnlyInspector, out FPath);
            FPath.SetSubType(Application.StartupPath + "\\plugins\\webinterface", true);

            FHost.CreateValueInput("DoSave", 1, null, TSliceMode.Single, TPinVisibility.OnlyInspector, out FSavePage);
            FSavePage.SetSubType(0, 1, 1, 0, true, false, true);


            //create outputs
            FHost.CreateNodeOutput("Output GuiElements", TSliceMode.Dynamic, TPinVisibility.True, out FHttpPageOut);
            FHttpPageOut.SetSubType(new Guid[1] { HttpPageIO.GUID }, HttpPageIO.FriendlyName);
            FHttpPageOut.SetInterface(this);


            FHost.CreateStringOutput("Output Page", TSliceMode.Dynamic, TPinVisibility.Hidden, out FWholeHTML);
            FWholeHTML.SetSubType("", false);


            string PatchPath = String.Empty;
            FHost.GetNodePath(true, out PatchPath);
            mPageName = "Page" + HTMLToolkit.CreatePageID(PatchPath);
            
        }

        #endregion pin creation






        #region NodeIO


        public void GetPage(out Page Page, out string CssFile, out string JsFile, out string PageName, out string FileName)
        {
            Page = mPage;
            CssFile = mCssFile;
            JsFile = mJsFile;
            PageName = mPageName;
            FileName = mUrl;
        }


        public void ConnectPin(IPluginIO Pin)
        {
            //cache a reference to the upstream interface when the NodeInput pin is being connected
            if (Pin == FHttpGuiIn)
            {
                INodeIOBase usI;
                FHttpGuiIn.GetUpstreamInterface(out usI);
                FUpstreamInterface = usI as IHttpGUIIO;
            }
            else if (Pin == FCssPropertiesIn)
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

                    List<GuiDataObject> tGuiDaten;
                    FUpstreamInterface.GetDatenObjekt(usS, out tGuiDaten);

                    //if (tGuiDaten != null)
                    //{
                    //    ////Debug.WriteLine("Objekt Type in Group: " + tGuiDaten.Type);
                    //    mGuiDatenListe.Add(i, tGuiDaten);
                    //}
                }
            }



            #endregion Upstream




            #region Read Url

            FUrl.GetString(0, out mUrl);

            #endregion Read Url




            #region Reload Page

            if (FReload.PinIsChanged)
            {
                double currentReloadSlice;
                FReload.GetValue(0, out currentReloadSlice);
                if (currentReloadSlice > 0.5)
                {
                    mWebinterfaceSingelton.Reload();
                }
            }
            #endregion Reload Page




            #region Get HTML Body and Head String

            if (FHtmlHead.PinIsChanged || FHtmlBody.PinIsChanged)
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


            #endregion Get HTML Body and Head String




            #region Body CSS Properties


            //Upstream Css Properties
            int uSSSytle;
            if (FUpstreamStyleIn != null)
            {
                mCssPropertiesSpread.Clear();

                for (int i = 0; i < FCssPropertiesIn.SliceCount; i++)
                {
                    FCssPropertiesIn.GetUpsreamSlice(i, out uSSSytle);
                    SortedList<string, string> tStylePropertie = new SortedList<string, string>();
                    FUpstreamStyleIn.GetCssProperties(uSSSytle, out tStylePropertie);

                    mCssPropertiesSpread.Add(i, tStylePropertie);
                }
            }
            else
            {
                mCssPropertiesSpread.Clear();
                SortedList<string, string> tStylePropertie = new SortedList<string, string>();
                tStylePropertie.Add("background-color", "#E0E0E0");
                mCssPropertiesSpread.Add(0,tStylePropertie);
            }

            #endregion Body CSS Properties



            #region Build Page

            Page tPage = new Page(true);

            PageBuilder tPageBuilder = new PageBuilder(mGuiDatenListe);


            //New Builer only ObjectList Input input
            //JQueryBuilder tJQuery = new JQueryBuilder(mGuiDatenListe, "-1", "-1", mCssPropertiesSpread, mPageName);
            //mJsFile = tJQuery.JsFile;
            //mCssFile = tJQuery.CssMainFile;
            //tPage = tJQuery.Page;

            tPage.Body.Insert(mPageBody);
            tPage.Head.Insert(mPageHead);

            string currentSliceTitel = "";
            FTitel.GetString(0, out currentSliceTitel);
            tPage.Head.Insert(new Title(currentSliceTitel));

            string t = tPage.Text;
            FWholeHTML.SetString(0, tPage.Text);

            mPage = tPage;

            //Communication Type
            string tCommunicationType;
            FCommunication.GetString(0, out tCommunicationType);
            if (tCommunicationType == "Polling")
            {
                mPage.Head.Insert(JSToolkit.Polling("100","'HIer sollte ein XML stehen :-)'"));

            }else if(tCommunicationType == "Comet")
            {
                mPage.Body.Insert(JSToolkit.Comet());
            
            }
            


            #endregion Build Page



            #region Browser Window


            if(FPageWidth.PinIsChanged || FPageHeight.PinIsChanged)
            {
                double currentWidthSlice;
                double currentHeightSlice;
                FPageWidth.GetValue(0, out currentWidthSlice);
                FPageHeight.GetValue(0, out currentHeightSlice);

                string tBrowserWidth = "" + Math.Round(currentWidthSlice);
                string tBrowserHeight = "" + Math.Round(currentHeightSlice);


                if (currentWidthSlice != -1 || currentHeightSlice != -1)
                {
                    mPage.Head.Insert(JSToolkit.ResizeBrowser(currentWidthSlice.ToString(),currentHeightSlice.ToString()));
                    
                }
            }

            #endregion Browser Window



            #region Save Page

            string tPath;
            double tSave;
            string tUrl;

            FPath.GetString(0, out tPath);
            FSavePage.GetValue(0, out tSave);
            FUrl.GetString(0, out tUrl);

            SortedList<string, string> tFiles = new SortedList<string, string>();
            tFiles.Add(tPath + "\\" + tUrl, tPage.Text);
            tFiles.Add(tPath + "\\" + "VVVV.css", mCssFile);
            tFiles.Add(tPath + "\\" + "VVVV.js", mJsFile);


            if (FSavePage.PinIsChanged)
            {
                if (tSave > 0.5)
                {

                    foreach (KeyValuePair<string, string> pFile in tFiles)
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



        }


        #endregion mainloop
    }
}
