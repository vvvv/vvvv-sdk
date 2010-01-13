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


        private IValueOut FResponse;


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
            FHost.CreateEnumInput("ButtonMode", TSliceMode.Single, TPinVisibility.True, out FMode);
            FMode.SetSubType("ButtonMode");

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


                    string tResponse = ReceivedString.ToArray()[i];

                    if (tResponse == null)
                    {
                        tResponse = CurrentDefaultSlice.ToString();
                    }

                    FResponse.SliceCount = SpreadMax;
                    FResponse.SetValue(i, Convert.ToInt16(tResponse));

                    string currentPathDefault;
                    string currentPathPress;
                    string CurrentMode;

                    FPathDefault.GetString(i,out currentPathDefault);
                    FPathPress.GetString(i, out currentPathPress);
                    FMode.GetString(i, out CurrentMode);



                    FileInfo InfoDefault = new FileInfo(currentPathDefault);
                    FileInfo InfoPress = new FileInfo(currentPathPress);

                    if (InfoDefault.Name != "" && InfoPress.Name != "")
                    {

                        FWebinterfaceSingelton.SetFileToStorage(InfoDefault.Name,File.ReadAllBytes(currentPathDefault));
                        FWebinterfaceSingelton.SetFileToStorage(InfoPress.Name,File.ReadAllBytes(currentPathPress));

                        
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
                            JQueryExpression postToServerTrue = new JQueryExpression();
                            postToServerTrue.Post("ToVVVV.xml", new JavaScriptSnippet(String.Format(@"this.id + '=1'")), null, null);
                            ;
                            
                            JQueryExpression postToServerFalse = new JQueryExpression();
                            postToServerFalse.Post("ToVVVV.xml", new JavaScriptSnippet(String.Format(@"this.id + '=0'")), null, null);
                          

                            JavaScriptAnonymousFunction MouseDown = new JavaScriptAnonymousFunction(new JavaScriptCodeBlock(JQueryExpression.This().Attr("src", InfoPress.Name), postToServerTrue), new string[] { "event" });
                            JavaScriptAnonymousFunction MouseUp = new JavaScriptAnonymousFunction(new JavaScriptCodeBlock(JQueryExpression.This().Attr("src", InfoDefault.Name), postToServerFalse), new string[] { "event" });

                            JQueryExpression DocumentReadyHandler = new JQueryExpression(new ClassSelector(GetNodeId(0)));
                            DocumentReadyHandler.ApplyMethodCall("mousedown", MouseDown);
                            DocumentReadyHandler.ApplyMethodCall("mouseup", MouseUp);
                            DocumentReadyHandler.ApplyMethodCall("mouseleave", MouseUp);


                            JQuery DocumentReady = JQuery.GenerateDocumentReady(DocumentReadyHandler);
                            AddJavaScript(0, DocumentReady.GenerateScript(1, true, true), true);
                        }






                        

//                        if (CurrentMode == "Toggle")
//                        {
//                            #region out
//                            HtmlDiv tDiv = new HtmlDiv();
//                            HtmlDiv tDivInlay = new HtmlDiv();
//                            tDivInlay.Insert("  ");

//                            string AttributeContent = "";
//                            string tContent = "";

//                            if (tResponse == "1")
//                            {
//                                AttributeContent = String.Format(@"position:relative;background-color:rgb({0}%,{1}%,{2}%); width:50%; height:50%;top:25%;left:25%;border-style:solid; border-color:#808080; border-width:thin;", currentOnPressColorSlice.R * 100, currentOnPressColorSlice.G * 100, currentOnPressColorSlice.B * 100);

//                                tContent = String.Format(@"toggle(
//                            function () {{
//                                $('div',this).css({{'background-color':'rgb({0}%,{1}%,{2}%)'}});
//                                var id = $(this).attr('id');
//                                $.post('ToVVVV.xml', id + '=0', null);
//                            }},
//                            function () {{
//                                $('div',this).css({{'background-color':'rgb({3}%,{4}%,{5}%)'}});
//                                var id = $(this).attr('id');
//                                $.post('ToVVVV.xml', id + '=1', null);
//                            }});
//                            ", currentDefaultColorSlice.R * 100, currentDefaultColorSlice.G * 100, currentDefaultColorSlice.B * 100, currentOnPressColorSlice.R * 100, currentOnPressColorSlice.G * 100, currentOnPressColorSlice.B * 100);
//                            }
//                            else
//                            {
//                                AttributeContent = String.Format(@"position:relative;background-color:rgb({0}%,{1}%,{2}%); width:50%; height:50%;top:25%;left:25%;border-style:solid; border-color:#808080; border-width:thin;", currentDefaultColorSlice.R * 100, currentDefaultColorSlice.G * 100, currentDefaultColorSlice.B * 100);

//                                tContent = String.Format(@"toggle(
//                            function () {{
//                                $('div',this).css({{'background-color':'rgb({0}%,{1}%,{2}%)'}});
//                                var id = $(this).attr('id');
//                                $.post('ToVVVV.xml', id + '=1', null);
//                            }},
//                            function () {{
//                                $('div',this).css({{'background-color':'rgb({3}%,{4}%,{5}%)'}});
//                                var id = $(this).attr('id');
//                                $.post('ToVVVV.xml', id + '=0', null);
//                            }}
//                            );
//                            ", currentOnPressColorSlice.R * 100, currentOnPressColorSlice.G * 100, currentOnPressColorSlice.B * 100, currentDefaultColorSlice.R * 100, currentDefaultColorSlice.G * 100, currentDefaultColorSlice.B * 100);
//                            }


//                            HTMLAttribute InlayAttribute = new HTMLAttribute("style", AttributeContent);
//                            tDivInlay.AddAttribute(InlayAttribute);

//                            tDiv.Insert(tDivInlay);

//                            SetTag(i, tDiv);

//                            AddJavaScript(0, new JqueryFunction(true, "." + GetNodeId(0), tContent).Text, true);

//                            #endregion
//                        }
//                        else
//                        {
//                            HtmlDiv tDiv = new HtmlDiv();
//                            HtmlDiv tDivInlay = new HtmlDiv();
//                            tDivInlay.Insert("  ");

//                            string AttributeContent = String.Format(@"position:relative;background-color:rgb({0}%,{1}%,{2}%); width:50%; height:50%;top:25%;left:25%;border-style:solid; border-color:#808080; border-width:thin;", currentDefaultColorSlice.R * 100, currentDefaultColorSlice.G * 100, currentDefaultColorSlice.B * 100);
//                            HTMLAttribute InlayAttribute = new HTMLAttribute("style", AttributeContent);
//                            tDivInlay.AddAttribute(InlayAttribute);

//                            tDiv.Insert(tDivInlay);

//                            SetTag(i, tDiv);


//                            string tContent = String.Format(@"mousedown(function(){{
//                            $('div',this).css({{'background-color':'rgb({0}%,{1}%,{2}%)'}});
//                            var id = $(this).attr('id');
//                            $.post('ToVVVV.xml', id + '=1', null);
//                        }}).mouseup(function(){{
//                            $('div',this).css({{'background-color':'rgb({3}%,{4}%,{5}%)'}});
//                            var id = $(this).attr('id');
//                            $.post('ToVVVV.xml', id + '=0', null);
//                        }}).mouseleave(function(){{
//                            $('div',this).css({{'background-color':'rgb({6}%,{7}%,{8}%)'}});
//                            var id = $(this).attr('id');
//                            $.post('ToVVVV.xml', id + '=0', null);
//                        }});
//                        ", currentOnPressColorSlice.R * 100, currentOnPressColorSlice.G * 100, currentOnPressColorSlice.B * 100, currentDefaultColorSlice.R * 100, currentDefaultColorSlice.G * 100, currentDefaultColorSlice.B * 100, currentDefaultColorSlice.R * 100, currentDefaultColorSlice.G * 100, currentDefaultColorSlice.B * 100);

//                            AddJavaScript(0, new JqueryFunction(true, "." + FGuiDataList[0].NodeId, tContent).Text, true);
//                        }
                    }
                    else
                    {
                        FHost.Log(TLogType.Message, FPluginInfo.Name + ": please loaded Textures");
                    }
                }
            }
        }

        protected override bool DynamicPinsAreChanged()
        {
            return (FDefault.PinIsChanged || FPathDefault.PinIsChanged || FPathPress.PinIsChanged );
        }

        #endregion Main Loop


    }
}
