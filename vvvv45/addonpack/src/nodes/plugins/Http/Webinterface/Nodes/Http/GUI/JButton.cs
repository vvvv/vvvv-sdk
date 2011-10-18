using System;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V1;
using VVVV.Utils.VColor;
using VVVV.Webinterface.Utilities;
using VVVV.Nodes.Http.BaseNodes;
using VVVV.Webinterface.jQuery;

namespace VVVV.Nodes.Http.GUI
{
    class JButton : GuiNodeDynamic, IPlugin, IDisposable
    {

        #region field declaration

        private bool FDisposed = false;

        private IValueIn FDefault;
        private IStringIn FLabel;
        private IEnumIn FMode;
        private IValueIn FButtonCount;



        private IValueOut FResponse;


        #endregion field declaration


        #region constructor/destructor

        /// <summary>
        /// the nodes constructor
        /// nothing to declare for this node
        /// </summary>
        public JButton()
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
        ~JButton()
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
                    FPluginInfo.Version = "JQuery";

                    //the nodes author: your sign
                    FPluginInfo.Author = "phlegma";
                    //describe the nodes function
                    FPluginInfo.Help = "Button (HTTP Jquery) node for the Renderer";
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

            FHost.CreateStringInput("Label", TSliceMode.Dynamic, TPinVisibility.True, out FLabel);
            FLabel.SetSubType("Label", false);

            FHost.CreateValueInput("Default", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FDefault);
            FDefault.SetSubType(0, 1, 1, 0, false, true, true);

            FHost.CreateValueInput("Count", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FButtonCount);
            FButtonCount.SetSubType(0,double.MaxValue,1,1,false,false,true);

            FHost.UpdateEnum("ButtonMode", "Toggle", new string[] { "Toggle", "Bang" });
            FHost.CreateEnumInput("Mode", TSliceMode.Single, TPinVisibility.True, out FMode);
            FMode.SetSubType("ButtonMode");


            //output
            FHost.CreateValueOutput("Response", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FResponse);
            FResponse.SetSubType(0, 1, 1, 0, false, false, true);
        }

        #endregion pin creation


        #region Main Loop


        protected override void OnEvaluate(int SpreadMax, bool changedSpreadSize, string NodeId, List<string> SliceId, bool ReceivedNewString, List<string> ReceivedString, List<bool> SendToBrowser)
        {

            FResponse.SliceCount = SpreadMax;

 
            for (int i = 0; i < SpreadMax; i++)
            {
                string CurrentMode;
                FMode.GetString(i, out CurrentMode);

                if(CurrentMode == "Bang" && ReceivedNewString == false)
                {
                    FResponse.SetValue(i, 0);
                }

                if (changedSpreadSize || ReceivedNewString || DynamicPinsAreChanged())
                {

                    double CurrentDefaultSlice;
                    FDefault.GetValue(i, out CurrentDefaultSlice);

                    string Label;
                    FLabel.GetString(i, out Label);
                    
                    double ButtonCountSlice;
                    FButtonCount.GetValue(i, out ButtonCountSlice);


                    
                    if (CurrentMode == "Toggle")
                    {
                        //Response Handling
                        FSaveLastResponse = true;
                        string tResponse = ReceivedString[i];
                        if (tResponse == null)
                            tResponse = CurrentDefaultSlice.ToString();
                     

                        FResponse.SliceCount = SpreadMax;
                        if (tResponse == "true")
                            FResponse.SetValue(i, 1);
                        else
                            FResponse.SetValue(i, 0);


                        //Create Html Tags
                        HtmlDiv HtmlContainer = new HtmlDiv();
                        
                        string CheckID = "check" + SliceId[i];
                        CheckBox Input = new CheckBox(CheckID);
                        Label InputLabel = new Label(CheckID);
                        InputLabel.Insert(Label);

                        HtmlContainer.Insert(Input);
                        HtmlContainer.Insert(InputLabel);
                        SetTag(i, HtmlContainer);

                        //Create Javascript
                        JQueryExpression GetState = new JQueryExpression(new BlankSelector("this"));
                        GetState.Children(new BlankSelector("'label'"));
                        GetState.Attr("aria-pressed");

                        JQueryExpression postTrueToServer = new JQueryExpression();
                        postTrueToServer.Post("ToVVVV.xml", new JavaScriptSnippet(String.Format(@"this.id + '=' + ") + GetState.GenerateScript(1,true,true)), null, null);


                        JavaScriptAnonymousFunction func = new JavaScriptAnonymousFunction(postTrueToServer, null);
                       
                        IDSelector NodeIDSelector = new IDSelector(SliceId[i]);
                        JQueryExpression Click = new JQueryExpression(NodeIDSelector);
                        Click.ApplyMethodCall("click", func);
                        
                        //Click.ApplyMethodCall("onmouseup", func2);

                        JQuery JQuerySend = JQuery.GenerateDocumentReady(Click);

                        JQueryExpression Expr = new JQueryExpression(NodeIDSelector);
                        Expr.AddChainedMethodCall("buttonset", null);
                        //Expr.Css("Position", "absolute");
                        JQuery JQueryCreate = JQuery.GenerateDocumentReady(Expr);

                        AddJavaScript(i, JQueryCreate.GenerateScript(0, true, true) + JQuerySend.GenerateScript(0, true, true), true);

                    }
                    else
                    {
                        //Response Handling
                        FSaveLastResponse = false;
                        string tResponse = ReceivedString[i];
                        if (tResponse == null)
                            tResponse = CurrentDefaultSlice.ToString();

                        FResponse.SliceCount = SpreadMax;
                        FResponse.SetValue(i, Convert.ToInt16(tResponse));


                        Webinterface.Utilities.Button Button = new Webinterface.Utilities.Button(false);
                        Button.Insert(Label);
                        SetTag(i, Button);

                        JQueryExpression postTrueToServer = new JQueryExpression();
                        postTrueToServer.Post("ToVVVV.xml", new JavaScriptSnippet(String.Format(@"this.id + '=1'")), null, null);

                        JQueryExpression postFalseToServer = new JQueryExpression();
                        postFalseToServer.Post("ToVVVV.xml", new JavaScriptSnippet(String.Format(@"this.id + '=0'")), null, null);

                        JavaScriptAnonymousFunction func = new JavaScriptAnonymousFunction(postTrueToServer, null);
                        JavaScriptAnonymousFunction func2 = new JavaScriptAnonymousFunction(postFalseToServer, null);

                        IDSelector NodeIDSelector = new IDSelector(SliceId[i]);
                        JQueryExpression Click = new JQueryExpression(NodeIDSelector);
                        Click.ApplyMethodCall("click", func);
                        //Click.ApplyMethodCall("onmouseup", func2);

                        JQuery JQuerySend = JQuery.GenerateDocumentReady(Click);
                        JQueryExpression Expr = new JQueryExpression(NodeIDSelector);
                        Expr.AddChainedMethodCall("button", null);
                        Expr.Css("Position", "absolute");
                        JQuery JQueryCreate = JQuery.GenerateDocumentReady(Expr);

                        AddJavaScript(i, JQueryCreate.GenerateScript(0, true, true) + JQuerySend.GenerateScript(0, true, true), true);
                    }
                }
            }
            
        }

        protected override bool DynamicPinsAreChanged()
        {
            return (FMode.PinIsChanged || FDefault.PinIsChanged || FLabel.PinIsChanged || FButtonCount.PinIsChanged || FMode.PinIsChanged);
        }

        #endregion Main Loop


    }
}
