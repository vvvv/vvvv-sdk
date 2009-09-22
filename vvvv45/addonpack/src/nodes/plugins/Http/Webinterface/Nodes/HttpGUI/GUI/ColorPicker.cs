using System;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V1;
using VVVV.Utils.VColor;
using VVVV.Webinterface.Utilities;

using System.Diagnostics;

namespace VVVV.Nodes.HttpGUI
{
	class ColorPicker : GuiNodeDynamic, IPlugin, IDisposable
	{

		#region field declaration

		private bool FDisposed = false;
		private IStringIn FNameStringInput;
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
			
			FHost.CreateColorOutput("Response", TSliceMode.Dynamic, TPinVisibility.True, out FResponseColorOutput);
			FResponseColorOutput.SetSubType(new RGBAColor(0.0, 1.0, 0.0, 1.0), false);
		}

		#endregion pin creation


		#region Main Loop



		protected override void OnEvaluate(int SpreadMax)
		{

			//check if we received any new data from the web server
			bool ReceivedResponse = CheckIfNodeReceivedData();

			if (mChangedSpreadSize || FNameStringInput.PinIsChanged || ReceivedResponse)
			{
				for (int i = 0; i < SpreadMax; i++)
				{

					//set slicecounts for all outputs
					//the incoming int SpreadMax is the maximum slicecount of all input pins, which is a good default
					FResponseColorOutput.SliceCount = SpreadMax;

					
					
					//read the new data we received from the server
					
					if (ReceivedResponse)
					{
						string tResponse;
						GetNewDataFromServer(i, out tResponse);
						if (tResponse == null)
						{
							//tResponse = currentDefaultSlice.ToString();
						}
						string [] rgb = tResponse.Split(new char [] {'.'});
						FResponseColorOutput.SetColor(i, new RGBAColor(double.Parse(rgb[0]) / 255.0, double.Parse(rgb[1]) / 255.0, double.Parse(rgb[2]) / 255.0, 1.0));
					}


					string nameStringSlice = String.Empty;

					//read data from inputs
					FNameStringInput.GetString(i, out nameStringSlice);
					
					//create a container div to house our custom color picker widget
					
					HtmlDiv tMainContainer = new HtmlDiv();
					
					//create a div for the actual jquery colorpicker
					string ColorPickerId = GetSliceId(i) + i.ToString();
					HtmlDiv tColorPicker = new HtmlDiv(ColorPickerId);
					//style the colorpicker
					HTMLAttribute tColorPickerStyle = new HTMLAttribute("style", "postion:absolute; top:50%");
					tColorPicker.AddAttribute(tColorPickerStyle);
					HTMLAttribute tColorPickerClass = new HTMLAttribute("class", "colorpickervvvv");
					tColorPicker.AddAttribute(tColorPickerClass);
					
					//insert into the container div
					tMainContainer.Insert(tColorPicker);

					//create a text label for the widget
					HTMLText tLabel = new HTMLText(nameStringSlice, true);
					//style the label
					HTMLAttribute tLabelStyle = new HTMLAttribute("style", "position:absolute; top:10%; width:80%");
					tLabel.AddAttribute(tLabelStyle);
					//insert into the container div
					tMainContainer.Insert(tLabel);
					
					
					//write the container HTML tag using all inserted info
					SetTag(i, tMainContainer, "ColorPicker");

					string colorPickerInitializeCode =
						@"ColorPicker({
							flat: true,
							onChange: function (hsb, hex, rgb) {
								var params = new Object();
								params[$(this).parent().attr('id')] = rgb.r.toString() + '.' + rgb.g.toString() + '.' + rgb.b.toString();
								$.post('ToVVVV.xml', params);
							}
						})";

					JqueryFunction colorPickerInitializeFunction = new JqueryFunction(true, ".colorpickervvvv", colorPickerInitializeCode);
					
					SetJavaScript(i, colorPickerInitializeFunction.Text);
				}
			}
		}


		#endregion Main Loop
	}
}
