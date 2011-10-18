using System;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V1;
using VVVV.Webinterface.Utilities;
using System.Diagnostics;
using VVVV.Nodes.Http.BaseNodes;
using VVVV.Webinterface.jQuery;

namespace VVVV.Nodes.Http.GUI
{
    class DatePicker : GuiNodeDynamic, IPlugin, IDisposable
    {

        #region field declaration

        private bool FDisposed = false;
        private IStringOut FResponse;
        private IStringIn FDefault;
        private IStringIn FDateFormat;


        #endregion field declaration


        #region constructor/destructor

        /// <summary>
        /// the nodes constructor
        /// nothing to declare for this node
        /// </summary>
        public DatePicker()
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
                FHost.Log(TLogType.Message, FPluginInfo.Name.ToString() + "Slider (Http Gui) Node is being deleted");
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
        ~DatePicker()
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
                    FPluginInfo.Name = "DatePicker";
                    //the nodes category: try to use an existing one
                    FPluginInfo.Category = "HTTP";
                    //the nodes version: optional. leave blank if not
                    //needed to distinguish two nodes of the same name and category
                    FPluginInfo.Version = "JQuery";

                    //the nodes author: your sign
                    FPluginInfo.Author = "phlegma";
                    //describe the nodes function
                    FPluginInfo.Help = "Datepicker node for the Renderer (HTTP)";
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

            FHost.CreateStringInput("Default", TSliceMode.Dynamic, TPinVisibility.True, out FDefault);
            FDefault.SetSubType("",false);

            FHost.CreateStringInput("Format", TSliceMode.Dynamic, TPinVisibility.True, out FDateFormat);
            FDateFormat.SetSubType("yy-mm-dd", false);

            FHost.CreateStringOutput("Response", TSliceMode.Dynamic, TPinVisibility.True, out FResponse);
            FResponse.SetSubType("", false);
        }

        #endregion pin creation



        #region Main Loop



        protected override void OnEvaluate(int SpreadMax, bool changedSpreadSize, string NodeId, List<string> SliceId, bool ReceivedNewString, List<string> ReceivedString, List<bool> SendToBrowser)
        {
            if (changedSpreadSize || ReceivedNewString || DynamicPinsAreChanged())
            {
                for (int i = 0; i < SpreadMax; i++)
                {
                    string currentDefaultSlice;
                    string currentDateFormat;
                    FDefault.GetString(i, out currentDefaultSlice);
                    FDateFormat.GetString(i, out currentDateFormat);


                    FResponse.SliceCount = SpreadMax;
                    string tResponse = ReceivedString[i];

                    if (tResponse == null)
                    {
                        FSavedResponses[i] = tResponse = currentDefaultSlice;
                        FResponse.SetString(i, tResponse);
                    }

                    if (ReceivedNewString)
                        if (!String.IsNullOrEmpty(tResponse))
                            FResponse.SetString(i,tResponse);

                    TextField Textfield = new TextField(SliceId[i], tResponse);
                    SetTag(i, Textfield);

                    ////Generates an document.ready block and lsiten for the keyup event of the texzfield
                    ////Slecetors for Sliders and Textfield
                    IDSelector SelectorSlider = new IDSelector(SliceId[i]);
 
                    ////send the value to vvvv
                    JQueryExpression postToServer = new JQueryExpression();
                    postToServer.Post("ToVVVV.xml", new JavaScriptSnippet(String.Format(@"'{0}=' + {1}",SliceId[i],"dateText")),null,null);

                    JavaScriptAnonymousFunction Function = new JavaScriptAnonymousFunction(postToServer, new string[] { "dateText", "inst" });

                    ////Generates an document.ready block to initialise the datepicker with there option object
                    JavaScriptGenericObject SliderParams = new JavaScriptGenericObject();
                    SliderParams.Set("autoSize", true);
                    SliderParams.Set("onSelect",Function);
                    SliderParams.Set("dateFormat", currentDateFormat);
                    //SliderParams.Set("orientation", currentOrientation);
                    //SliderParams.Set("step", currentStepSize);
                    //SliderParams.Set("value", currentSliderValue);
                    JQueryExpression SliderDocumentReadyHandler = new JQueryExpression(SelectorSlider);
                    SliderDocumentReadyHandler.ApplyMethodCall("datepicker", SliderParams);
                    JQuery SliderDocumentReady = JQuery.GenerateDocumentReady(SliderDocumentReadyHandler);
                    AddJavaScript(i,SliderDocumentReady.GenerateScript(1, true, true), true);

                    if (FDefault.PinIsChanged)
                    {
                        string[] DefaultValue = new string[3] { "option", "setdate", currentDefaultSlice };
                        CreatePollingMessage(i, SliceId[i], "datepicker", DefaultValue);
                    }

                    if (FDateFormat.PinIsChanged)
                    {
                        string[] DateFormat = new string[3] { "option", "dateFormat", currentDateFormat };
                        CreatePollingMessage(i, SliceId[i], "datepicker", DateFormat);
                    }


                }
            }
        }


        #endregion Main Loop

        protected override bool DynamicPinsAreChanged()
        {
            return (FDefault.PinIsChanged || FDateFormat.PinIsChanged);
        }
    }
}
