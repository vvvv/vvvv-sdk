using System;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V1;
using VVVV.Webinterface.Utilities;
using VVVV.Nodes.Http.BaseNodes;
using System.IO;

namespace VVVV.Nodes.Http.GUI
{
    /// <summary>
    /// Creates an Link Node
    /// </summary>
    class Link : GuiNodeDynamic, IPlugin, IDisposable
    {

        #region field declaration

        private bool FDisposed = false;
        private IStringIn FLink;
        private IStringIn FText;
        private IEnumIn FTarget;
        private IStringIn FKey;
        private IStringIn FPath;

        #endregion field declaration


        #region constructor/destructor

        /// <summary>
        /// the nodes constructor
        /// nothing to declare for this node
        /// </summary>
        public Link()
        {
        }

        /// <summary>
        /// Implementing IDisposable's Dispose method.
        /// Do not make this method virtual.
        /// A derived class should not be able to override this method.
        /// </summary>
        /// 

        #region Dispose

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
            if (FDisposed == false)
            {
                if (disposing)
                {
                    // Dispose managed resources.
                }
                // Release unmanaged resources. If disposing is false,
                // only the following code is executed.
                //mWebinterfaceSingelton.DeleteNode(mObserver);
                FHost.Log(TLogType.Message, FPluginInfo.Name.ToString() + "(Http Gui) Node is being deleted");

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
        /// Do not provide destructors in WebTypes derived from this class.
        /// </summary>
        ~Link()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }

        #endregion dispose

        #endregion constructor/destructor



        #region Pugin Information

        public static IPluginInfo FPluginInfo;

        public static IPluginInfo PluginInfo
        {
            get
            {
                if (FPluginInfo == null)
                {
                    //fill out nodes info
                    //see: http://www.vvvv.org/tiki-index.php?page=Conventions.NodeAndPinNaming
                    FPluginInfo = new PluginInfo();

                    //the nodes main name: use CamelCaps and no spaces
                    FPluginInfo.Name = "Link";
                    //the nodes category: try to use an existing one
                    FPluginInfo.Category = "HTTP";
                    //the nodes version: optional. leave blank if not
                    //needed to distinguish two nodes of the same name and category
                    FPluginInfo.Version = "";

                    //the nodes author: your sign
                    FPluginInfo.Author = "phlegma";
                    //describe the nodes function
                    FPluginInfo.Help = "Link node for the Renderer (HTTP)";
                    //specify a comma separated list of tags that describe the node
                    FPluginInfo.Tags = "";

                    //give credits to thirdparty code used
                    FPluginInfo.Credits = "";
                    //any known problems?
                    FPluginInfo.Bugs = "";
                    //any known usage of the node that may cause troubles?
                    FPluginInfo.Warnings = "";



                    //leave below as is
                    System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(true);
                    System.Diagnostics.StackFrame sf = st.GetFrame(0);
                    System.Reflection.MethodBase method = sf.GetMethod();
                    FPluginInfo.Namespace = method.DeclaringType.Namespace;
                    FPluginInfo.Class = method.DeclaringType.Name;
                    //leave above as is
                }

                return FPluginInfo;
            }
        }


        #endregion Plugin Information



        #region pin creation

        protected override void OnSetPluginHost()
        {
            // create required pins
            FHost.CreateStringInput("Link", TSliceMode.Dynamic, TPinVisibility.True, out FLink);
            FLink.SetSubType("", false);

            FHost.CreateStringInput("Text", TSliceMode.Dynamic, TPinVisibility.True, out FText);
            FText.SetSubType("", false);

            FHost.CreateStringInput("Filename", TSliceMode.Dynamic, TPinVisibility.True, out FPath);
            FPath.SetSubType("", true);

            FHost.CreateEnumInput("Target", TSliceMode.Dynamic, TPinVisibility.True, out FTarget);
            FTarget.SetSubType("Target");
            FHost.UpdateEnum("Target", "_self", new string[] { "_self", "_blank", "_parent", "_top" });

            FHost.CreateStringInput("Key", TSliceMode.Dynamic, TPinVisibility.True, out FKey);
            FKey.SetSubType("", false);
        }

        #endregion pin creation



        #region Main Loop



        protected override void OnEvaluate(int SpreadMax, bool changedSpreadSize, string NodeId, List<string> SliceId, bool ReceivedNewString, List<string> ReceivedString, List<bool> SendToBrowser)
        {
            if (DynamicPinsAreChanged() || changedSpreadSize || ReceivedNewString)
            {
                for (int i = 0; i < SpreadMax; i++)
                {
                    string currentLink = String.Empty;
                    string currentText = String.Empty;
                    string currentTarget = String.Empty;
                    string currentKey = String.Empty;
                    string currentPath = String.Empty;

                    FLink.GetString(i, out currentLink);
                    FText.GetString(i, out currentText);
                    FTarget.GetString(i, out currentTarget);
                    FKey.GetString(i, out currentKey);
                    FPath.GetString(i, out currentPath);

                    if (currentLink != null && currentLink != "")
                    {
                        // create the an Link from the Type Tag
                        Webinterface.Utilities.Link HtmlLink = new Webinterface.Utilities.Link(currentLink);

                        //insert an text if exist
                        if (currentText != null && currentText != "")
                        {
                            HtmlLink.Insert(currentText);
                        }

                        //insert Attribute target 
                        if (currentTarget != null && currentTarget != "")
                        {
                            HtmlLink.AddAttribute(new HTMLAttribute("target", currentTarget));
                        }

                        //insert Key which kann be pressed to click the link
                        if (currentKey != null && currentKey != "")
                        {
                            HtmlLink.AddAttribute(new HTMLAttribute("accesskey", currentKey));
                        }

                        string Filename = new FileInfo(currentPath).Name;
                        bool FileExist = File.Exists(currentPath);


                        if (!String.IsNullOrEmpty(currentPath) && FileExist)
                        {
                            AddTextureToMemory(currentPath);
                            Img Image = new Img(Filename);
                            Image.AddAttribute(" width=100%");
                            Image.AddAttribute(" height=100%");
                            HtmlLink.Insert(Image);
                        }

                        //set the tag to the Gui Element Object
                        SetTag(i, HtmlLink);
                    }
                    else
                    {
                        FHost.Log(TLogType.Message, "Please insert a Link");
                    }
                }
            }
        }


        protected override bool DynamicPinsAreChanged()
        {
            //checks if any input pin is changeds
            return (FLink.PinIsChanged || FText.PinIsChanged || FTarget.PinIsChanged || FKey.PinIsChanged || FPath.PinIsChanged);
        }


        #endregion Main Loop



    }
}
