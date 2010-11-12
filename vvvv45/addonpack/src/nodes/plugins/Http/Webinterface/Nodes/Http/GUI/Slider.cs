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
    class Slider : GuiNodeDynamic, IPlugin, IDisposable
    {

        #region field declaration

        private bool FDisposed = false;
        private IStringIn FName;
        private IEnumIn FOrientation;
        private IValueOut FResponse;
        private IValueIn FMin;
        private IValueIn FMax;
        private IValueIn FDefault;
        private IValueIn FStepSize;
        private IValueIn FUpdateContinuousValueInput;
        private IValueIn FShowTextfield;


        #endregion field declaration


        #region constructor/destructor

        /// <summary>
        /// the nodes constructor
        /// nothing to declare for this node
        /// </summary>
        public Slider()
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
        ~Slider()
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
                    FPluginInfo.Name = "Slider";
                    //the nodes category: try to use an existing one
                    FPluginInfo.Category = "HTTP";
                    //the nodes version: optional. leave blank if not
                    //needed to distinguish two nodes of the same name and category
                    FPluginInfo.Version = "JQuery";

                    //the nodes author: your sign
                    FPluginInfo.Author = "phlegma";
                    //describe the nodes function
                    FPluginInfo.Help = "Slider node for the Renderer (HTTP)";
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
            FHost.CreateStringInput("Label", TSliceMode.Dynamic, TPinVisibility.True, out FName);
            FName.SetSubType("", false);

            FHost.UpdateEnum("Orientation", "horizontal", new string[] { "horizontal", "vertical" });
            FHost.CreateEnumInput("Orientation", TSliceMode.Single, TPinVisibility.True, out FOrientation);
            FOrientation.SetSubType("Orientation");

            FHost.CreateValueInput("Min", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FMin);
            FMin.SetSubType(double.MinValue, double.MaxValue, 0.01, 0, false, false, false);

            FHost.CreateValueInput("Max", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FMax);
            FMax.SetSubType(double.MinValue, double.MaxValue, 0.01,1, false, false, false);

            FHost.CreateValueInput("Default", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FDefault);
            FDefault.SetSubType(double.MinValue, double.MaxValue, 0.01, 0.5, false, false, false);

            FHost.CreateValueInput("StepSize", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FStepSize);
            FStepSize.SetSubType(double.MinValue, double.MaxValue, 0.01, 0.01, false, false, false);

            FHost.CreateValueInput("Update Continuous", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FUpdateContinuousValueInput);
            FUpdateContinuousValueInput.SetSubType(0.0, 1.0, 1.0, 1, false, true, false);

            FHost.CreateValueInput("Textfield", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FShowTextfield);
            FShowTextfield.SetSubType(0, 1, 1, 0, false, true, true);

            FHost.CreateValueOutput("Response", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FResponse);
            FResponse.SetSubType(double.MinValue, double.MaxValue, 0.01, 0.5, false, false, false);
        }

        #endregion pin creation



        #region Main Loop



        protected override void OnEvaluate(int SpreadMax, bool changedSpreadSize, string NodeId, List<string> SliceId, bool ReceivedNewString, List<string> ReceivedString, List<bool> SendToBrowser)
        {
            if (changedSpreadSize || ReceivedNewString || DynamicPinsAreChanged())
            {
                for (int i = 0; i < SpreadMax; i++)
                {

                    double currentMinSlice;
                    double currentMaxSlice;
                    double currentDefaultSlice;
                    double currentStepSize;
                    double updateContinuousSlice;
                    double currentShowTextfield;

                    

                    string currentSliceId = GetSliceId(i);
                    string currentOrientation = String.Empty;
                    string currentName = String.Empty;

                    string SliderId = SliceId[i] + "Slider" + i.ToString();
                    string SliderTextfieldId = SliceId[i] + "Textfield" + i.ToString();
                    string SliderSelector = "#" + SliderId;

                    FMin.GetValue(i,out currentMinSlice);
                    FMax.GetValue(i,out currentMaxSlice);
                    FDefault.GetValue(i,out currentDefaultSlice);
                    FStepSize.GetValue(i,out currentStepSize);
                    FName.GetString(i, out currentName);
                    FOrientation.GetString(i, out currentOrientation);
                    FUpdateContinuousValueInput.GetValue(i, out updateContinuousSlice);
                    FShowTextfield.GetValue(i, out currentShowTextfield);

                    currentStepSize *= 10000;
                    currentMinSlice *= 10000;
                    currentMaxSlice *= 10000;

                    FResponse.SliceCount = SpreadMax;
                    string tResponse = ReceivedString[i];
                    

                    if (tResponse == null)
                    {
                        FSavedResponses[i] = tResponse = Convert.ToString(currentDefaultSlice).Replace(',','.');
                        FResponse.SetValue(i, double.Parse(tResponse, System.Globalization.NumberFormatInfo.InvariantInfo));
                    }

                    if (ReceivedNewString)
                        if(!String.IsNullOrEmpty(tResponse))
                            FResponse.SetValue(i, double.Parse(tResponse, System.Globalization.NumberFormatInfo.InvariantInfo));

                    double currentSliderValue = Convert.ToDouble(tResponse) * 10000;  



                    HtmlDiv tMainContainer = new HtmlDiv();
                    HtmlDiv tSlider = new HtmlDiv(SliderId);
                    HTMLText tText = new HTMLText(currentName, true);

                    

                    //tSliderValueText.AddAttribute(new HTMLAttribute("position","absolute"));
                    //tSliderValueText.AddAttribute(new HTMLAttribute("right", "0%"));
                    //tSliderValueText.AddAttribute(new HTMLAttribute("top", "10%"));
                    //tSliderValueText.AddAttribute(new HTMLAttribute("border", "hidden"));


                    TextField tSliderValueText = new TextField(SliderTextfieldId, tResponse);
                    string AttributeTextValue,AttributeText,AttributeSlider = String.Empty;

                    if (currentOrientation == "horizontal")
                    {
                        if (currentShowTextfield == 0)
                            AttributeTextValue = "display:none; position:absolute; right:0%; top:10%; border: hidden; width:80%";
                        else
                            AttributeTextValue = "position:absolute; right:0%; top:10%; border: hidden; width:80%";

                        AttributeText = "position:absolute; top:10%; width:80%";
                        AttributeSlider = "postion:absolute; top:50%; width:100%";
                    }
                    else
                    {
                        if (currentShowTextfield == 0)
                            AttributeTextValue = "display:none; position:absolute;  top:110%; border: hidden; width:20%";
                        else
                            AttributeTextValue = "position:absolute;  bottom:-30px; border: hidden; width:20%";

                        AttributeText = "position:absolute; left:0%; top:-35px; width:100%";
                        AttributeSlider = "postion:absolute; left:0%; height:100%";
                    }
                    //string AttributeTextValue = "position:absolute; right:0%; top:10%; border: hidden;";
                    HTMLAttribute tTextAttributeValue = new HTMLAttribute("style", AttributeTextValue);
                    tSliderValueText.AddAttribute(tTextAttributeValue);
    

                    
                    HTMLAttribute tTextAttribute = new HTMLAttribute("style", AttributeText);
                    tText.AddAttribute(tTextAttribute);

                    
                    HTMLAttribute tSliderAttribute = new HTMLAttribute("style", AttributeSlider);
                    tSlider.AddAttribute(tSliderAttribute);

                    tMainContainer.Insert(tText);
                    tMainContainer.Insert(tSliderValueText);
                    tMainContainer.Insert(tSlider);
                    SetTag(i, tMainContainer);




                    //Generates an document.ready block and lsiten for the keyup event of the texzfield
                    //Slecetors for Sliders and Textfield
                    IDSelector SelectorSlider =  new IDSelector(SliderId);
                    IDSelector SelectorTextfield = new IDSelector(SliderTextfieldId);

                    // reads the content form the textfield
                    JavaScriptDeclaration<JavaScriptVariableObject> TextfieldContent = new JavaScriptDeclaration<JavaScriptVariableObject>(new JavaScriptVariableObject("TextfieldContent"), new JQueryExpression(SelectorTextfield).ApplyMethodCall("val").GenerateScript(0,false,false) + " * 10000");
                    //send the value to vvvv
                    JQueryExpression postToServer = new JQueryExpression();
                    postToServer.Post("ToVVVV.xml", new JavaScriptSnippet(JQueryExpression.This().Parent().ApplyMethodCall("attr", "id").GenerateScript(0, false, false) + " + '=' + " + "$(this).val()"), null, null);

                    //set the slider value that it is the same as in the textield
                    JavaScriptSnippet SetSliderValue = new JavaScriptSnippet(String.Format(@"$('#{0}').slider('option', 'value', TextfieldContent)",SliderId));

                    //combines all codes lines to one block
                    JavaScriptCodeBlock Block = new JavaScriptCodeBlock(TextfieldContent, SetSliderValue, postToServer);

                     //creates the keyup funcition and add it to the document ready
                    JavaScriptAnonymousFunction Function = new JavaScriptAnonymousFunction(Block, new string[] { "event" });
                    JQueryExpression DocumentReadyHandler = new JQueryExpression(SelectorTextfield);
                    DocumentReadyHandler.ApplyMethodCall("keyup", Function);

                    //creates the document ready fucniton
                    JQuery DocumentReady = JQuery.GenerateDocumentReady(DocumentReadyHandler);







                    //Generates an document.ready block to initialise the sliders with there option object
                    JavaScriptGenericObject SliderParams = new JavaScriptGenericObject();
                    SliderParams.Set("animate", true);
                    SliderParams.Set("max", currentMaxSlice);
                    SliderParams.Set("min", currentMinSlice);
                    SliderParams.Set("orientation", currentOrientation);
                    SliderParams.Set("step", currentStepSize);
                    SliderParams.Set("value", currentSliderValue);
                    JQueryExpression SliderDocumentReadyHandler = new JQueryExpression(SelectorSlider);
                    SliderDocumentReadyHandler.ApplyMethodCall("slider", SliderParams);
                    //SliderDocumentReadyHandler.Css("position", "absolute");
                    JQuery SliderDocumentReady = JQuery.GenerateDocumentReady(SliderDocumentReadyHandler);






                    //Generates an document.ready block and bind the sider event to the slider divs

                    //Selects the id from the parent div
                    JavaScriptDeclaration<JavaScriptVariableObject> id = new JavaScriptDeclaration<JavaScriptVariableObject>(new JavaScriptVariableObject("id"), JQueryExpression.This().Parent().ApplyMethodCall("attr", "id").GenerateScript(0, false, false));

                    //reads the value from the slider 
                    JavaScriptDeclaration<JavaScriptVariableObject> Value = new JavaScriptDeclaration<JavaScriptVariableObject>(new JavaScriptVariableObject("value"), JQueryExpression.This().ApplyMethodCall("slider", "option", "value").GenerateScript(0, false, false) + " / 10000");
                    JQueryExpression postToServerSlider = new JQueryExpression();

                    //post the values to vvvv
                    postToServerSlider.Post("ToVVVV.xml", new JavaScriptSnippet("id + '=' + value"), null, null);
                    JavaScriptSnippet SetTextfieldValue = new JavaScriptSnippet(String.Format(@"$('#{0}').val( value )", SliderTextfieldId));

                    //combines all codes lines to one block
                    JavaScriptCodeBlock SlideEvent = new JavaScriptCodeBlock(id, Value, postToServerSlider, SetTextfieldValue);
                    
                    JavaScriptAnonymousFunction SlideFunction = new JavaScriptAnonymousFunction(SlideEvent,"event","ui");
                    JQueryExpression SliderEventDocumentReadyHandler = new JQueryExpression(SelectorSlider).Bind(updateContinuousSlice > 0.5 ? "slide" : "slidestop", SlideFunction);
                    JQuery SliderEventDocumentReady = JQuery.GenerateDocumentReady(SliderEventDocumentReadyHandler);



                    AddJavaScript(i, SliderEventDocumentReady.GenerateScript(0, true, true) + Environment.NewLine + SliderDocumentReady.GenerateScript(1, true, true) + Environment.NewLine + DocumentReady.GenerateScript(1, true, true), true);
                    //AddJavaScript(i, new JqueryFunction(true, SliderSelector, String.Format(SliderInitalize, currentOrientation, currentSliderValue.ToString(), SliderSelector, currentSliceId, "#" + SliderTextfieldId, currentMinSlice, currentMaxSlice, currentStepSize)).Text + Environment.NewLine + DocumentReady.GenerateScript(1, true, true), true);

                    string[] tElementSlider = new string[3] { "option", "value", currentSliderValue.ToString() };
                    CreatePollingMessage(i, SliderId, "slider", tElementSlider);
                    string[] tElementTextfield = new string[1] { currentDefaultSlice.ToString() };
                    CreatePollingMessage(i, SliderTextfieldId, "val", tElementTextfield);
                    
                }
            }
        }


        #endregion Main Loop

		protected override bool DynamicPinsAreChanged()
		{
			return (FName.PinIsChanged || FOrientation.PinIsChanged || FMin.PinIsChanged || FMax.PinIsChanged || FDefault.PinIsChanged || FStepSize.PinIsChanged || FShowTextfield.PinIsChanged);
		}
	}
}
