using System;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V1;
using VVVV.Webinterface.Utilities;
using VVVV.Nodes.Http.BaseNodes;
using VVVV.Webinterface.jQuery;

namespace VVVV.Nodes.HttpGUI
{
    class Textfield : GuiNodeDynamic, IPlugin, IDisposable
    {

        #region field declaration

        private bool FDisposed = false;
        private IStringOut FResponse;
        private IStringIn FDefault;
        private IValueIn FPasswort;
        private IValueIn FMultiline;
        private IValueIn FUpdateContinuousValueInput;
             
        #endregion field declaration


        #region constructor/destructor

        /// <summary>
        /// the nodes constructor
        /// nothing to declare for this node
        /// </summary>
        public Textfield()
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
                FHost.Log(TLogType.Message, FPluginInfo.Name.ToString() + "Textfield (Http Gui) Node is being deleted");

                

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
        ~Textfield()
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
                    FPluginInfo.Name = "Textfield";
                    //the nodes category: try to use an existing one
                    FPluginInfo.Category = "HTTP";
                    //the nodes version: optional. leave blank if not
                    //needed to distinguish two nodes of the same name and category
                    FPluginInfo.Version = "HTML";

                    //the nodes author: your sign
                    FPluginInfo.Author = "phlegma";
                    //describe the nodes function
                    FPluginInfo.Help = "Textfield node for the Renderer (HTTP)";
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
            FHost.CreateStringInput("Default", TSliceMode.Dynamic, TPinVisibility.True, out FDefault);
            FDefault.SetSubType("", false);

            FHost.CreateStringOutput("Response", TSliceMode.Dynamic, TPinVisibility.True, out FResponse);
            FResponse.SetSubType("", false);

            FHost.CreateValueInput("Mulitline", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FMultiline);
            FMultiline.SetSubType(0, 1, 1, 0, false, true, true);
            
            FHost.CreateValueInput("Password" ,1,null, TSliceMode.Dynamic, TPinVisibility.True, out FPasswort);
            FPasswort.SetSubType(0,1,1,0,false,true,true);

            FHost.CreateValueInput("UpdateContinuous", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FUpdateContinuousValueInput);
            FUpdateContinuousValueInput.SetSubType(0, 1, 1, 1, false, true, true);
        }



        #endregion pin creation



        #region Main Loop



        protected override void OnEvaluate(int SpreadMax, bool changedSpreadSize, string NodeId, List<string> SliceId, bool ReceivedNewString, List<string> ReceivedString, List<bool> SendToBrowser)
        {

            if (changedSpreadSize || ReceivedNewString || DynamicPinsAreChanged())
            {
                for (int i = 0; i < SpreadMax; i++)
                {

                    string tResponse = ReceivedString[i];

                    double currentPassword;
                    string currentDefault;
                    double currentMulitline;
                    double currentUpdateContinouse;

                    FDefault.GetString(i,out currentDefault);
                    FPasswort.GetValue(i, out currentPassword);
                    FMultiline.GetValue(i, out currentMulitline);
                    FUpdateContinuousValueInput.GetValue(i, out currentUpdateContinouse);


                    //Response Handling
                    FResponse.SliceCount = SpreadMax;
                    if (tResponse == null)
                    {
                        if(!String.IsNullOrEmpty(currentDefault))
                            tResponse = currentDefault;
                    }

                    FResponse.SetString(i, tResponse);


                    // Create HTML Tags and Attributes
                    HtmlDiv Container = new HtmlDiv();

                    if (currentMulitline == 0)
                    {
                        TextField tTextfield = new TextField();
                        tTextfield.AddAttribute(new HTMLAttribute("id", SliceId[i] + "Textfield"));

                        if (currentPassword > 0.5)
                        {
                            tTextfield.AddAttribute(new HTMLAttribute("type", "password"));
                        }
                        tTextfield.AddAttribute(new HTMLAttribute("Value", tResponse));
                        tTextfield.AddAttribute(new HTMLAttribute("style", "width:100%;height:100%"));

                        Container.Insert(tTextfield);

                        if (currentUpdateContinouse < 0.5)
                        {
                            Button SubmitButtons = new Button(true);
                            SubmitButtons.AddAttribute(new HTMLAttribute("id", SliceId[i] + "Submit"));
                            SubmitButtons.AddAttribute(new HTMLAttribute("type", "submit"));
                            SubmitButtons.AddAttribute(new HTMLAttribute("Value", "Submit"));
                            SubmitButtons.AddAttribute(new HTMLAttribute("style", "bottom:-20px"));
                            Container.Insert(SubmitButtons);
                        }
                    }
                    else
                    {
                        TextArea tTextfield = new TextArea();
                        tTextfield.AddAttribute(new HTMLAttribute("id", SliceId[i] + "Textfield"));

                        if (currentPassword > 0.5)
                        {
                            tTextfield.AddAttribute(new HTMLAttribute("type", "password"));
                        }
                        tTextfield.Insert(tResponse);
                        tTextfield.AddAttribute(new HTMLAttribute("style", "width:100%;height:100%"));

                        Container.Insert(tTextfield);

                        if (currentUpdateContinouse < 0.5)
                        {
                            Button SubmitButtons = new Button(true);
                            SubmitButtons.AddAttribute(new HTMLAttribute("id",SliceId[i] + "Submit"));
                            SubmitButtons.AddAttribute(new HTMLAttribute("type", "submit"));
                            SubmitButtons.AddAttribute(new HTMLAttribute("Value", "Submit"));
                            SubmitButtons.AddAttribute(new HTMLAttribute("style", "bottom:-20px"));
                            Container.Insert(SubmitButtons);
                        }
                    }

                    SetTag(i, Container);

                    
                   

                    //Generate the JavaScript an add it to the GUIDataObject
                    if (currentUpdateContinouse > 0.5)
                    {
                        JQueryExpression postToServer = new JQueryExpression();
                        postToServer.Post("ToVVVV.xml", new JavaScriptSnippet(String.Format("'{0}=' + ", SliceId[i]) + new JQueryExpression(new IDSelector(SliceId[i] + "Textfield")).Attr("value").GenerateScript(1, true, true)), null, null);

                        JavaScriptCodeBlock Block = new JavaScriptCodeBlock(postToServer);
                        JavaScriptAnonymousFunction Function = new JavaScriptAnonymousFunction(Block, new string[] { "event" });
                        JQueryExpression DocumentReadyHandler = new JQueryExpression(new IDSelector(SliceId[i] + "Textfield"));
                        DocumentReadyHandler.ApplyMethodCall("keyup", Function);
                        JQuery DocumentReady = JQuery.GenerateDocumentReady(DocumentReadyHandler);

                        AddJavaScript(i, DocumentReady.GenerateScript(0, true, true), true);
                    }
                    else
                    {
                        JQueryExpression postToServer = new JQueryExpression();
                        postToServer.Post("ToVVVV.xml", new JavaScriptSnippet(String.Format("'{0}=' + ", SliceId[i]) + new JQueryExpression(new IDSelector(SliceId[i] + "Textfield")).Attr("value").GenerateScript(1, true, true)), null, null);

                        JavaScriptCodeBlock Block = new JavaScriptCodeBlock(postToServer);
                        JavaScriptAnonymousFunction Function = new JavaScriptAnonymousFunction(Block, new string[] { "event" });
                        JQueryExpression DocumentReadyHandler = new JQueryExpression(new IDSelector(SliceId[i] + "Submit"));
                        DocumentReadyHandler.ApplyMethodCall("click", Function);
                        JQuery DocumentReady = JQuery.GenerateDocumentReady(DocumentReadyHandler);

                        AddJavaScript(i, DocumentReady.GenerateScript(0, true, true), true);
                    }
                }
            }
        }


        #endregion Main Loop

		protected override bool DynamicPinsAreChanged()
		{
			return FDefault.PinIsChanged || FPasswort.PinIsChanged || FMultiline.PinIsChanged || FUpdateContinuousValueInput.PinIsChanged ;
		}
	}
}
