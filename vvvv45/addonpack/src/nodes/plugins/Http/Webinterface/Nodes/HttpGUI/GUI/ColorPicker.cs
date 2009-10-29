using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Xml;
using VVVV.PluginInterfaces.V1;
using VVVV.Utils.VColor;
using VVVV.Webinterface.Utilities;
using VVVV.Webinterface.jQuery;

using System.Diagnostics;

namespace VVVV.Nodes.HttpGUI
{
	class ColorPicker : GuiNodeDynamic, IPlugin, IDisposable
	{

		#region field declaration

		private bool FDisposed = false;
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
			FHost.CreateColorInput("Default Color", TSliceMode.Dynamic, TPinVisibility.True, out FDefaultColorColorInput);
			FDefaultColorColorInput.SetSubType(new RGBAColor(0.0, 1.0, 0.0, 1.0), false);

            FHost.CreateValueInput("Update Continuous", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FUpdateContinuousValueInput);
            FUpdateContinuousValueInput.SetSubType(0.0, 1.0, 1.0, 1.0, false, true, false);

			FHost.CreateColorOutput("Response", TSliceMode.Dynamic, TPinVisibility.True, out FResponseColorOutput);
			FResponseColorOutput.SetSubType(new RGBAColor(0.0, 1.0, 0.0, 1.0), false);
		}

		#endregion pin creation


		#region Main Loop



		protected override void OnEvaluate(int SpreadMax, bool changedSpreadSize, string NodeId, List<string> SliceId, bool ReceivedNewString, List<string> ReceivedString)
		{

			//check if we received any new data from the web server

			if (changedSpreadSize || DynamicPinsAreChanged() || ReceivedNewString)
			{
				for (int i = 0; i < SpreadMax; i++)
				{

					//set slicecounts for all outputs
					//the incoming int SpreadMax is the maximum slicecount of all input pins, which is a good default
					FResponseColorOutput.SliceCount = SpreadMax;

                    //read data from inputs
					RGBAColor defaultColorSlice;
                    double updateContinuousSlice;
					
					FDefaultColorColorInput.GetColor(i, out defaultColorSlice);
                    FUpdateContinuousValueInput.GetValue(i, out updateContinuousSlice);

					//parse the default color
					string[] rgb = new string[4] {defaultColorSlice.R.ToString(), defaultColorSlice.G.ToString(), defaultColorSlice.B.ToString(), defaultColorSlice.A.ToString()};
					bool validColorReceived = false;

					//if we received a request
					if (ReceivedString[i] != null)
					{
						//parse the color representation that got passed as a POST parameter
                        XmlDocument xmlDocument = new XmlDocument();
                        xmlDocument.LoadXml(HttpUtility.UrlDecode(ReceivedString[i]));
                        XmlNodeList xmlNodeList = xmlDocument.DocumentElement.GetElementsByTagName("value");
						for (int j = 0; j < 4; j++)
						{
							XmlNode xmlNode = xmlNodeList.Item(j);
							if (xmlNode != null)
							{
								rgb[j] = xmlNode.InnerXml;
								validColorReceived = true;
							}
						}
						
					}
				    
                    RGBAColor responseColorSlice ;
					
					if (validColorReceived)
					{
						responseColorSlice = new RGBAColor(double.Parse(rgb[0]), double.Parse(rgb[1]), double.Parse(rgb[2]), double.Parse(rgb[3]));
					}
					else
					{
						responseColorSlice = defaultColorSlice;
					}
					
					//update the output pin with the new response color
					FResponseColorOutput.SetColor(i, responseColorSlice);

					//create a div for the jquery colorpicker
				
					HtmlDiv tColorPicker = new HtmlDiv(SliceId[i]);

					//write the HTML tag using all inserted info
					SetTag(i, tColorPicker);

					//generate JQuery code to create our color picker when the document loads - it will look something like the following block
					/*@"ColorPicker({{
							flat: true,
							color: {{r: {0}, g: {1}, b: {2}}},
							{3}: function (hsb, hex, rgb) {{
								var params = new Object();
								params[$(this).parent().attr('id')] = '<PIN pinname=""Color Input"" slicecount=""1"" values=""|' + rgb.r.toString() + ',' + rgb.g.toString() + ',' + rgb.b.toString() + ',1.00000|""></PIN>'
								$.post('ToVVVV.xml', params);
							}}
					}})";*/


                    JQueryExpression getSliceId = JQueryExpression.This().Parent().ApplyMethodCall("attr", "id");
                    JQueryExpression generateXMLPost = JQueryExpression.Dollars("<PIN></PIN>").Attr("pinname", "Color Input").Attr("slicecount", 1);
					JQueryExpression generateColorValuesXML = new JQueryExpression("<value id=\"r\"></value><value id=\"g\"></value><value id=\"b\"></value><value id=\"a\"></value>");
					JavaScriptObject rgbParam = new JavaScriptVariableObject("rgb");
					generateXMLPost.Append(generateColorValuesXML);
					generateXMLPost.Children("value#r").Append(rgbParam.Member("r").toString()).End();
					generateXMLPost.Children("value#g").Append(rgbParam.Member("g").toString()).End();
					generateXMLPost.Children("value#b").Append(rgbParam.Member("b").toString()).End();
					generateXMLPost.Children("value#a").Append(1.0).End();

					JQueryExpression wrapXMLPost = new JQueryExpression("<XML></XML>").Append(generateXMLPost);
                    wrapXMLPost.ApplyMethodCall("html");

					JQueryExpression createPostParameters = new JQueryExpression(new JavaScriptGenericObject()).Attr(getSliceId, wrapXMLPost).ApplyMethodCall("get", 0);

					JQueryExpression postToServer = new JQueryExpression();
					postToServer.Post("ToVVVV.xml", createPostParameters, null, null);

					//Create the object that sets the colorpicker options
					JavaScriptGenericObject colorPickerParams = new JavaScriptGenericObject();
					colorPickerParams.Set("flat", true);
					//setup the post method to fire at the appropriate time according to the Update Continuous pin
					colorPickerParams.Set(updateContinuousSlice > 0.5 ? "onChange" : "onChangeComplete", new JavaScriptAnonymousFunction(postToServer, "hsb", "hex", "rgb"));
					//set the color picker to the color currently set on the server side
					JavaScriptGenericObject color = new JavaScriptGenericObject();
					color.Set("r", double.Parse(rgb[0])); 
					color.Set("g", double.Parse(rgb[1]));
					color.Set("b", double.Parse(rgb[2]));
					colorPickerParams.Set("color", color);

					//Apply all the colorpicker code to our div
					JQueryExpression documentReadyHandler = new JQueryExpression(new IDSelector(SliceId[i])).ApplyMethodCall("ColorPicker", colorPickerParams);
					//set the newly created colorpicker sub-div to use absolute positioning
					documentReadyHandler.Children().ApplyMethodCall("eq", 0).Css("position", "absolute");
					
					//Assign all our code to the document ready handler
					JQuery onDocumentReady = JQuery.GenerateDocumentReady(documentReadyHandler);

					//insert the JQuery code into the javascript file for this page
					AddJavaScript(i, onDocumentReady.GenerateScript(1, true, true), false);
				}
			}
		}

        #endregion Main Loop

		protected override bool DynamicPinsAreChanged()
		{
			return (FDefaultColorColorInput.PinIsChanged || FUpdateContinuousValueInput.PinIsChanged);
		}
	}
}
