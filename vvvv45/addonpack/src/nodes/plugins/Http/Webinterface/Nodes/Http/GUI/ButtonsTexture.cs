using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using VVVV.PluginInterfaces.V1;
using VVVV.Utils.VColor;
using VVVV.Webinterface.Utilities;
using VVVV.Nodes.Http.BaseNodes;
using VVVV.Webinterface.jQuery;

namespace VVVV.Nodes.Http.GUI
{
    class ButtonTexture : GuiNodeDynamic, IPlugin, IDisposable
    {

        #region field declaration

        private bool FDisposed = false;

        private IValueIn FDefault;
        private IStringIn FPathDefault;
        private IStringIn FPathPress;
        private IEnumIn FMode;
        private IValueIn FReload;
        private IValueOut FResponse;

        private List<bool> FReceivedThisFrame = new List<bool>();


        #endregion field declaration


        #region constructor/destructor

        /// <summary>
        /// the nodes constructor
        /// nothing to declare for this node
        /// </summary>
        public ButtonTexture()
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
        ~ButtonTexture()
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
                    FPluginInfo.Name = "Button";
                    //the nodes category: try to use an existing one
                    FPluginInfo.Category = "HTTP";
                    //the nodes version: optional. leave blank if not
                    //needed to distinguish two nodes of the same name and category
                    FPluginInfo.Version = "Texture";

                    //the nodes author: your sign
                    FPluginInfo.Author = "phlegma";
                    //describe the nodes function
                    FPluginInfo.Help = "Button node for the Renderer (HTTP)";
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
            //input
            FHost.CreateValueInput("Default", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FDefault);
            FDefault.SetSubType(0, 1, 1, 0, false, true, true);

            FHost.CreateStringInput("Released", TSliceMode.Dynamic, TPinVisibility.True, out FPathDefault);
            FPathDefault.SetSubType("", true);

            FHost.CreateStringInput("Press", TSliceMode.Dynamic, TPinVisibility.True, out FPathPress);
            FPathPress.SetSubType("", true);

            FHost.UpdateEnum("ButtonMode", "Toggle", new string[] { "Toggle", "Bang" });
            FHost.CreateEnumInput("Mode", TSliceMode.Single, TPinVisibility.True, out FMode);
            FMode.SetSubType("ButtonMode");

            FHost.CreateValueInput("Reload", 1, null, TSliceMode.Single, TPinVisibility.True, out FReload);
            FReload.SetSubType(0, 1, 1, 0, true, false, true);

            //output
            FHost.CreateValueOutput("Response", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FResponse);
            FResponse.SetSubType(0, 1, 1, 0, false, false, true);
        }

        #endregion pin creation


        #region Main Loop


        protected override void OnEvaluate(int SpreadMax, bool changedSpreadSize, string NodeId, List<string> SliceId, bool ReceivedNewString, List<string> ReceivedString, List<bool> SendToBrowser)
        {

            if (changedSpreadSize || ReceivedNewString || DynamicPinsAreChanged())
            {

                for (int i = 0; i < SpreadMax; i++)
                {

                    double CurrentDefaultSlice;
                    FDefault.GetValue(i, out CurrentDefaultSlice);

                    string currentPathDefault;
                    string currentPathPress;
                    string CurrentMode;

                    FPathDefault.GetString(i, out currentPathDefault);
                    FPathPress.GetString(i, out currentPathPress);
                    FMode.GetString(i, out CurrentMode);


                    bool DirectoryExist = Directory.Exists(currentPathDefault) && Directory.Exists(currentPathPress);
                    bool FileExist = File.Exists(currentPathDefault) && File.Exists(currentPathPress);

                    if (FileExist)
                    {

                        FileInfo InfoDefault = new FileInfo(currentPathDefault);
                        FileInfo InfoPress = new FileInfo(currentPathPress);

                        AddTextureToMemory(InfoDefault.FullName);
                        AddTextureToMemory(InfoPress.FullName);

                        Img Image = new Img();
                        HTMLAttribute tSource;

                        if (ReceivedString[i] == null)
                        {
                            ReceivedString[i] = CurrentDefaultSlice.ToString();
                        }


                        if (ReceivedString[i] == "1")
                        {
                            tSource = new HTMLAttribute("src", InfoPress.Name);

                        }
                        else
                        {
                            tSource = new HTMLAttribute("src", InfoDefault.Name);
                        }

                        Image.AddAttribute(tSource);

                        SetTag(i, Image);

                        string[] tElementSlider;
                        if (ReceivedString[i] == "0")
                        {
                            tElementSlider = new string[2] { "src", InfoDefault.Name };
                        }
                        else
                        {
                            tElementSlider = new string[2] { "src", InfoPress.Name };
                        }
                        CreatePollingMessage(i, SliceId[i], "attr", tElementSlider);


                        if (CurrentMode == "Toggle")
                        {
                            if (ReceivedString[i] == null)
                            {
                                ReceivedString[i] = CurrentDefaultSlice.ToString();
                            }

                            FResponse.SliceCount = SpreadMax;
                            FResponse.SetValue(i, Convert.ToInt16(ReceivedString[i]));


                            JQueryExpression postToServerTrue = new JQueryExpression();
                            postToServerTrue.Post("ToVVVV.xml", new JavaScriptSnippet(String.Format(@"this.id + '=1'")), null, null);

                            JQueryExpression postToServerFalse = new JQueryExpression();
                            postToServerFalse.Post("ToVVVV.xml", new JavaScriptSnippet(String.Format(@"this.id + '=0'")), null, null);

                            JavaScriptCodeBlock IfStatement = new JavaScriptCodeBlock(JQueryExpression.This().Attr("src", InfoPress.Name), postToServerTrue);
                            JavaScriptCodeBlock ElseStatement = new JavaScriptCodeBlock(JQueryExpression.This().Attr("src", InfoDefault.Name), postToServerFalse);
                            JavaScriptCodeBlock Block = new JavaScriptCodeBlock(new JavaScriptIfCondition(new JavaScriptCondition(JQueryExpression.This().Attr("src"), "==", InfoDefault.Name), IfStatement, ElseStatement));

                            JavaScriptAnonymousFunction Function = new JavaScriptAnonymousFunction(Block, new string[] { "event" });
                            JQueryExpression DocumentReadyHandler = new JQueryExpression(new ClassSelector(GetNodeId(0)));
                            DocumentReadyHandler.ApplyMethodCall("click", Function);

                            JQuery DocumentReady = JQuery.GenerateDocumentReady(DocumentReadyHandler);
                            AddJavaScript(0, DocumentReady.GenerateScript(1, true, true), true);
                        }
                        else
                        {
                            FResponse.SliceCount = SpreadMax;
                            if (ReceivedString[i] != null)
                            {

                                FResponse.SetValue(i, Convert.ToInt16(ReceivedString[i]));
                                FReceivedString[i] = null;
                            }
                            JQueryExpression postToServerTrue = new JQueryExpression();
                            postToServerTrue.Post("ToVVVV.xml", new JavaScriptSnippet(String.Format(@"this.id + '=1'")), null, null);

                            JavaScriptAnonymousFunction MouseDown = new JavaScriptAnonymousFunction(new JavaScriptCodeBlock(JQueryExpression.This().Attr("src", InfoPress.Name), postToServerTrue), new string[] { "event" });
                            JavaScriptAnonymousFunction MouseUp = new JavaScriptAnonymousFunction(new JavaScriptCodeBlock(JQueryExpression.This().Attr("src", InfoDefault.Name)), new string[] { "event" });

                            JQueryExpression DocumentReadyHandler = new JQueryExpression(new ClassSelector(GetNodeId(0)));
                            DocumentReadyHandler.ApplyMethodCall("mousedown", MouseDown);
                            DocumentReadyHandler.ApplyMethodCall("mouseup", MouseUp);
                            DocumentReadyHandler.ApplyMethodCall("mouseleave", MouseUp);


                            JQuery DocumentReady = JQuery.GenerateDocumentReady(DocumentReadyHandler);
                            AddJavaScript(0, DocumentReady.GenerateScript(1, true, true), true);
                        }
                    }
                    else
                    {
                        SetTag(i, new Img("", "noPic"));
                        FHost.Log(TLogType.Message, FPluginInfo.Name + ": please loaded Textures");
                    }
                }
            }
            else
            {
                for (int i = 0; i < SpreadMax; i++)
                {

                    string currentMode;
                    FMode.GetString(i, out currentMode);
                    if (currentMode == "Bang")
                    {
                        FResponse.SetValue(i, 0);
                    }

                }
            }
        }

        protected override bool DynamicPinsAreChanged()
        {
            return (FDefault.PinIsChanged || FPathDefault.PinIsChanged || FPathPress.PinIsChanged || FMode.PinIsChanged || FReload.PinIsChanged );
        }

        #endregion Main Loop


    }
}
