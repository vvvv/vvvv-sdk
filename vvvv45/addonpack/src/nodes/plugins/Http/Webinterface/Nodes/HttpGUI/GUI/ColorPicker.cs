using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Xml;
using VVVV.PluginInterfaces.V1;
using VVVV.Utils.VColor;
using VVVV.Webinterface.Utilities;
using VVVV.Nodes.jQuery;

using System.Diagnostics;

namespace VVVV.Nodes.HttpGUI
{
	class ColorPicker : GuiNodeDynamic, IPlugin, IDisposable
	{

		#region field declaration

		private bool FDisposed = false;
		private IStringIn FNameStringInput;
        private IValueIn FUpdateContinuousValueInput;
		private IColorIn FDefaultColorColorInput;
		private IColorOut FResponseColorOutput;

		#endregion field declaration


		#region constructor/destructor

		/// <summary>
		/// the nodes constructor
		/// nothing to declare for this node
		/// </summary>
		public ColorPicker()
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
				FHost.Log(TLogType.Message, FPluginInfo.Name.ToString() + "ColorPicker (Http Gui) Node is being deleted");
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
		~ColorPicker()
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
					FPluginInfo.Name = "ColorPicker";
					//the nodes category: try to use an existing one
					FPluginInfo.Category = "HTTP";
					//the nodes version: optional. leave blank if not
					//needed to distinguish two nodes of the same name and category
					FPluginInfo.Version = "GUI";

					//the nodes author: your sign
					FPluginInfo.Author = "iceberg";
					//describe the nodes function
					FPluginInfo.Help = "Color picker node for the Renderer (HTTP)";
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
			FHost.CreateStringInput("Name", TSliceMode.Dynamic, TPinVisibility.True, out FNameStringInput);
			FNameStringInput.SetSubType("", false);
            
            FHost.CreateColorInput("Default Color", TSliceMode.Dynamic, TPinVisibility.True, out FDefaultColorColorInput);
			FDefaultColorColorInput.SetSubType(new RGBAColor(0.0, 1.0, 0.0, 1.0), false);

            FHost.CreateValueInput("Update Continuous", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FUpdateContinuousValueInput);
            FUpdateContinuousValueInput.SetSubType(0.0, 1.0, 1.0, 1.0, false, true, false);

			FHost.CreateColorOutput("Response", TSliceMode.Dynamic, TPinVisibility.True, out FResponseColorOutput);
			FResponseColorOutput.SetSubType(new RGBAColor(0.0, 1.0, 0.0, 1.0), false);
		}

		#endregion pin creation


		#region Main Loop



		protected override void OnEvaluate(int SpreadMax, string NodeId, List<string> SliceId, bool ReceivedNewString, List<string> ReceivedString)
		{

			//check if we received any new data from the web server

			if (FChangedSpreadSize || FNameStringInput.PinIsChanged || FDefaultColorColorInput.PinIsChanged || FUpdateContinuousValueInput.PinIsChanged || ReceivedNewString)
			{
				for (int i = 0; i < SpreadMax; i++)
				{

					//set slicecounts for all outputs
					//the incoming int SpreadMax is the maximum slicecount of all input pins, which is a good default
					FResponseColorOutput.SliceCount = SpreadMax;

                    //read data from inputs
                    string nameStringSlice = String.Empty;
					RGBAColor defaultColorSlice;
                    double updateContinuousSlice;
					
					FNameStringInput.GetString(i, out nameStringSlice);
					FDefaultColorColorInput.GetColor(i, out defaultColorSlice);
                    FUpdateContinuousValueInput.GetValue(i, out updateContinuousSlice);

					//check for request data
                    RGBAColor responseColorSlice;
					string[] rgb;

					//if we received a request
					if (ReceivedString[i] != null)
					{
						//parse the color representation that got passed as a POST parameter
                        XmlDocument xmlDocument = new XmlDocument();
                        xmlDocument.LoadXml(HttpUtility.UrlDecode(ReceivedString[i]));
                        //the color value comes in the same xml representation that is used in a vvvv patch file
                        string colorValues = xmlDocument.DocumentElement.Attributes["values"].InnerXml;
                        rgb = colorValues.Trim(new char[] { '|' }).Split(new char[] { ',' });
						if (rgb.Length >= 3)
						{
							responseColorSlice = new RGBAColor(double.Parse(rgb[0]), double.Parse(rgb[1]), double.Parse(rgb[2]), 1.0);
                        }
						else
						{
							//parse the default color
                            responseColorSlice = defaultColorSlice;
                            rgb = new string[3] { defaultColorSlice.R.ToString(), defaultColorSlice.G.ToString(), defaultColorSlice.B.ToString() };
                            
						}
					}
					else
					{
                        //parse the default color
                        responseColorSlice = defaultColorSlice;
                        rgb = new string[3] {defaultColorSlice.R.ToString(), defaultColorSlice.G.ToString(), defaultColorSlice.B.ToString()};
					}
					
					//update the output pin with the new response color
					FResponseColorOutput.SetColor(i, responseColorSlice);

					//create a div for the jquery colorpicker
					HtmlDiv tColorPicker = new HtmlDiv(SliceId[i]);

					//write the HTML tag using all inserted info
					SetTag(i, tColorPicker);

					//generate JQuery code to create our color picker when the document loads
					string colorPickerInitializeCode =
						@"ColorPicker({{
							flat: true,
							color: {{r: {0}, g: {1}, b: {2}}},
							{3}: function (hsb, hex, rgb) {{
								var params = new Object();
								params[$(this).parent().attr('id')] = '<PIN pinname=""Color Input"" slicecount=""1"" values=""|' + rgb.r.toString() + ',' + rgb.g.toString() + ',' + rgb.b.toString() + ',1.00000|""></PIN>'
								$.post('ToVVVV.xml', params);
							}}
						}})";


					JavaScriptGenericObject color = new JavaScriptGenericObject();
					color.Set("r", new JavaScriptNumberObject(double.Parse(rgb[0])));
					color.Set("g", new JavaScriptNumberObject(double.Parse(rgb[1])));
					color.Set("b", new JavaScriptNumberObject(double.Parse(rgb[2])));
					JavaScriptGenericObject jgo = new JavaScriptGenericObject();
					jgo.Set("flat", new JavaScriptBooleanObject(true));
					jgo.Set(updateContinuousSlice > 0.5 ? "onChange" : "onChangeComplete", new JavaScriptAnonymousFunction(new JQuery(), "hsb", "hex", "rgb"));
					jgo.Set("color", color);
					JQueryExpression ex = new JQueryExpression(new IDSelector(SliceId[i])).ApplyMethodCall("ColorPicker", jgo);
					


					JQuery dr = JQuery.GenerateDocumentReady(new JQuery(ex));

					/*JQuery jq = new JQuery();
					JQueryExpression ex = new JQueryExpression(Selector.DocumentSelector);
					MethodCall mc = new MethodCall(new Method("ready"));
					JavaScriptAnonymousFunction jaf = new JavaScriptAnonymousFunction();

					JQueryExpression sb = new JQueryExpression(new RawStringSelector("body"));
					MethodCall mc2 = new MethodCall(new Method("css"));
					mc2.AddArgument(new JavaScriptObjectArgument(new JavaScriptStringObject("background-color")));
					mc2.AddArgument(new JavaScriptObjectArgument(new JavaScriptStringObject("#FF0000")));
					sb.AddMethodCall(mc2);
					JQuery jq2 = new JQuery();
					jq2.AddStatement(sb);
					jaf.PJQuery = jq2;

					mc.AddArgument(new JavaScriptObjectArgument(jaf));
					ex.AddMethodCall(mc);
					jq.AddStatement(ex);*/

					//set the color picker to the color currently set on the server side, and setup the post method to fire
                    //at the appropriate time according to the Update Continuous pin
					colorPickerInitializeCode = String.Format(colorPickerInitializeCode, rgb[0], rgb[1], rgb[2], updateContinuousSlice > 0.5 ? "onChange" : "onChangeComplete");

					//set the newly created colorpicker sub-div to use absolute positioning
					string colorPickerPositionAbsoluteCode = "children().eq(0).css('position', 'absolute')";

					//insert the JQuery code into the javascript file for this page
					JqueryFunction colorPickerInitializeFunction = new JqueryFunction(true, "#" + SliceId[i], colorPickerInitializeCode);
					JqueryFunction colorPickerPositionAbsoluteFunction = new JqueryFunction(true, "#" + SliceId[i], colorPickerPositionAbsoluteCode);
					SetJavaScript(i, dr.PScript(1, true) + colorPickerPositionAbsoluteFunction.Text);
				}
			}
		}

        #endregion Main Loop
    }
}
