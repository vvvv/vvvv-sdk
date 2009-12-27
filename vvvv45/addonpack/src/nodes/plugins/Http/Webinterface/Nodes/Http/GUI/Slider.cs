using System;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V1;
using VVVV.Webinterface.Utilities;
using System.Diagnostics;
using VVVV.Nodes.Http.BaseNodes;

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
                    FPluginInfo.Version = "GUI";

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
            FHost.CreateStringInput("Name", TSliceMode.Dynamic, TPinVisibility.True, out FName);
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

            FHost.CreateValueOutput("Response", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FResponse);
            FResponse.SetSubType(0, 1, 1, 0, false, false, false);
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

                    string currentSliceId = GetSliceId(i);
                    string currentOrientation = String.Empty;
                    string currentName = String.Empty;
                    string SliderId = GetSliceId(i) + i.ToString();
                    string SliderTextfieldId = SliderId + "Value";
                    string SliderSelector = "#" + SliderId;

                    FMin.GetValue(i,out currentMinSlice);
                    FMax.GetValue(i,out currentMaxSlice);
                    FDefault.GetValue(i,out currentDefaultSlice);
                    FStepSize.GetValue(i,out currentStepSize);
                    FName.GetString(i, out currentName);
                    FOrientation.GetString(i, out currentOrientation);


                    currentStepSize *= 10000;
                    currentMinSlice *= 10000;
                    currentMaxSlice *= 10000;

                    FResponse.SliceCount = SpreadMax;
                    string tResponse = ReceivedString[i];
                    

                    if (tResponse == null)
                    {
                        tResponse = currentDefaultSlice.ToString();
                        FSavedResponses[i] = currentDefaultSlice.ToString();
                        FResponse.SetValue(i, Convert.ToDouble(tResponse));
                    }

                    if (ReceivedNewString)
                    {
                        FResponse.SetValue(i, Convert.ToDouble(tResponse));
                    }

                    double currentSliderValue = Convert.ToDouble(tResponse) * 10000;  


                    HtmlDiv tMainContainer = new HtmlDiv();
                    HtmlDiv tSlider = new HtmlDiv(SliderId);
                    HTMLText tText = new HTMLText(currentName, true);

                    
                    TextField tSliderValueText = new TextField(SliderTextfieldId, tResponse);

                    string AttributeTextValue = "position:absolute; right:0%; top:10%; border: hidden;";
                    HTMLAttribute tTextAttributeValue = new HTMLAttribute("style", AttributeTextValue);
                    tSliderValueText.AddAttribute(tTextAttributeValue);

                    string AttributeText = "position:absolute; top:10%; width:80%";
                    HTMLAttribute tTextAttribute = new HTMLAttribute("style", AttributeText);
                    tText.AddAttribute(tTextAttribute);

                    string AttributeSlider = "postion:absolute; top:50%";
                    HTMLAttribute tSliderAttribute = new HTMLAttribute("style", AttributeSlider);
                    tSlider.AddAttribute(tSliderAttribute);


                    tMainContainer.Insert(tText);
                    tMainContainer.Insert(tSliderValueText);
                    tMainContainer.Insert(tSlider);


                    SetTag(i, tMainContainer);





                    string TextfeldJsContent = @"
var value = $(this).val();
var SliderValue = value * 10000;
$('{0}').slider('option', 'value', SliderValue);
var content = '{1}' + '=' + value; 
$.post('ToVVVV.xml',content, null);
";

                    JqueryFunction tTextJS = new JqueryFunction(true, "#" + SliderTextfieldId, "keyup", String.Format(TextfeldJsContent, "#" + SliderId, currentSliceId, currentStepSize));

                    string SliderInitalize =
    @"slider({{
          animate: false,
          max: {6},
          min: {5},
          orientation: '{0}',
          step:{7},
          value: {1},
          slide: function(event,ui){{
                  var id = $(this).attr('id');
                  var value = $('{2}').slider('option', 'value');
                  var SliderValue = value / 10000;
                  var content = '{3}' + '=' + SliderValue; 
                  $.post('ToVVVV.xml',content, null);
                  $('{4}').val(SliderValue);
                  }},
          }})
          ";


                  

                    AddJavaScript(i, new JqueryFunction(true, SliderSelector, String.Format(SliderInitalize, currentOrientation, currentSliderValue.ToString(), SliderSelector, currentSliceId, "#" + SliderTextfieldId, currentMinSlice, currentMaxSlice, currentStepSize)).Text + Environment.NewLine + tTextJS.Text, true);

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
			return (FName.PinIsChanged || FOrientation.PinIsChanged || FMin.PinIsChanged || FMax.PinIsChanged || FDefault.PinIsChanged || FStepSize.PinIsChanged);
		}
	}
}
