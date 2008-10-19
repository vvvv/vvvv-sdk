#region licence/info

//////project name
//vvvv plugin template with gui

//////description
//basic vvvv plugin template with gui.
//Copy this an rename it, to write your own plugin node.

//////licence
//GNU Lesser General Public License (LGPL)
//english: http://www.gnu.org/licenses/lgpl.html
//german: http://www.gnu.de/lgpl-ger.html

//////language/ide
//C# sharpdevelop

//////dependencies
//VVVV.PluginInterfaces.V1;

//////initial author
//vvvv group

#endregion licence/info

//use what you need
using System;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Collections.Generic;

using VVVV.PluginInterfaces.V1;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

//the vvvv node namespace
namespace VVVV.Nodes
{
	
	//class definition, inheriting from UserControl for the GUI stuff
	public class GUITemplatePlugin: UserControl, IPlugin
	{
		#region field declaration
		
		//the host (mandatory)
		private IPluginHost FHost;
		// Track whether Dispose has been called.
		private bool FDisposed = false;
		
		//input pin declaration
		private IValueIn FMyValueInput;
		private IStringIn FMyStringInput;
		private IColorIn FMyColorInput;
		private ITransformIn FMyTransformInput;
		
		//output pin declaration
		private IValueOut FMyValueOutput;
		private IStringOut FMyStringOutput;
		private IColorOut FMyColorOutput;
		private ITransformOut FMyTransformOutput;
		
		//further fields
		System.Collections.Generic.List<string> FValues = new List<string>();
		System.Collections.Generic.List<string> FStrings = new List<string>();
		System.Collections.Generic.List<RGBAColor> FColors = new List<RGBAColor>();
		System.Collections.Generic.List<Matrix4x4> FTransforms = new List<Matrix4x4>();
		
		#endregion field declaration
		
		#region constructor/destructor
		public GUITemplatePlugin()
		{
			// The InitializeComponent() call is required for Windows Forms designer support.
			InitializeComponent();
		}
		
		// Dispose(bool disposing) executes in two distinct scenarios.
		// If disposing equals true, the method has been called directly
		// or indirectly by a user's code. Managed and unmanaged resources
		// can be disposed.
		// If disposing equals false, the method has been called by the
		// runtime from inside the finalizer and you should not reference
		// other objects. Only unmanaged resources can be disposed.
		protected override void Dispose(bool disposing)
		{
			// Check to see if Dispose has already been called.
			if(!FDisposed)
			{
				if(disposing)
				{
					// Dispose managed resources.
					FValues = null;
					FStrings = null;
					FColors = null;
					FTransforms = null;
				}
				// Release unmanaged resources. If disposing is false,
				// only the following code is executed.
				
				FHost.Log(TLogType.Debug, "PluginGUITemplate is being deleted");
				
				// Note that this is not thread safe.
				// Another thread could start disposing the object
				// after the managed resources are disposed,
				// but before the disposed flag is set to true.
				// If thread safety is necessary, it must be
				// implemented by the client.
			}
			FDisposed = true;
		}
		
		#endregion constructor/destructor
		
		#region node name and infos
		
		//provide node infos
		private static IPluginInfo FPluginInfo;
		public static IPluginInfo PluginInfo
		{
			get
			{
				if (FPluginInfo == null)
				{
					//fill out nodes info
					//see: http://www.vvvv.org/tiki-index.php?page=vvvv+naming+conventions
					FPluginInfo = new PluginInfo();
					
					//the nodes main name: use CamelCaps and no spaces
					FPluginInfo.Name = "Template";
					//the nodes category: try to use an existing one
					FPluginInfo.Category = "Template";
					//the nodes version: optional. leave blank if not
					//needed to distinguish two nodes of the same name and category
					FPluginInfo.Version = "GUI";
					
					//the nodes author: your sign
					FPluginInfo.Author = "vvvv group";
					//describe the nodes function
					FPluginInfo.Help = "Offers a basic code layout to start from when writing a vvvv plugin with GUI";
					//specify a comma separated list of tags that describe the node
					FPluginInfo.Tags = "";
					
					//give credits to thirdparty code used
					FPluginInfo.Credits = "";
					//any known problems?
					FPluginInfo.Bugs = "";
					//any known usage of the node that may cause troubles?
					FPluginInfo.Warnings = "";
					
					//define the nodes initial size in box-mode
					FPluginInfo.InitialBoxSize = new Size(200, 100);
					//define the nodes initial size in window-mode
					FPluginInfo.InitialWindowSize = new Size(400, 300);
					//define the nodes initial component mode
					FPluginInfo.InitialComponentMode = TComponentMode.InAWindow;
					
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
		
		public bool AutoEvaluate
		{
			//return true if this node needs to calculate every frame even if nobody asks for its output
			get {return true;}
		}
		
		#endregion node name and infos
		
		private void InitializeComponent()
		{
			this.SuspendLayout();
			// 
			// GUITemplatePlugin
			// 
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
			this.DoubleBuffered = true;
			this.Name = "GUITemplatePlugin";
			this.Size = new System.Drawing.Size(310, 169);
			this.ResumeLayout(false);
		}
		
		#region pin creation
		
		//this method is called by vvvv when the node is created
		public void SetPluginHost(IPluginHost Host)
		{
			//assign host
			FHost = Host;

			//create inputs
			FHost.CreateValueInput("Value Input", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FMyValueInput);
			FMyValueInput.SetSubType(double.MinValue, double.MaxValue, 0.01, 0.5, false, false, false);
			
			FHost.CreateStringInput("String Input", TSliceMode.Dynamic, TPinVisibility.True, out FMyStringInput);
			FMyStringInput.SetSubType("hello c#", false);

			FHost.CreateColorInput("Color Input", TSliceMode.Dynamic, TPinVisibility.True, out FMyColorInput);
			FMyColorInput.SetSubType(VColor.Green, false);
			
			FHost.CreateTransformInput("Transform Input", TSliceMode.Dynamic, TPinVisibility.True, out FMyTransformInput);
			
			//create outputs
			FHost.CreateValueOutput("Value Output", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FMyValueOutput);
			FMyValueOutput.SetSubType(double.MinValue, double.MaxValue, 0.01, 0, false, false, false);
			
			FHost.CreateStringOutput("String Output", TSliceMode.Dynamic, TPinVisibility.True, out FMyStringOutput);
			FMyStringOutput.SetSubType("", false);

			FHost.CreateColorOutput("Color Output", TSliceMode.Dynamic, TPinVisibility.True, out FMyColorOutput);
			FMyColorOutput.SetSubType(VColor.Green, false);
			
			FHost.CreateTransformOutput("Transform Output", TSliceMode.Dynamic, TPinVisibility.True, out FMyTransformOutput);
			
		}

		#endregion pin creation
		
		#region mainloop
		
		public void Configurate(IPluginConfig Input)
		{
			//nothing to configure in this plugin
			//only used in conjunction with inputs of type cmpdConfigurate
		}
		
		//here we go, thats the method called by vvvv each frame
		//all data handling should be in here
		public void Evaluate(int SpreadMax)
		{
			//if any of the inputs has changed
			//recompute the outputs
			if (FMyValueInput.PinIsChanged || FMyStringInput.PinIsChanged || FMyColorInput.PinIsChanged)
			{
				//first set slicecounts for all outputs
				//the incoming int SpreadMax is the maximum slicecount of all input pins, which is a good default
				FMyValueOutput.SliceCount = SpreadMax;
				FMyStringOutput.SliceCount = SpreadMax;
				FMyColorOutput.SliceCount = SpreadMax;
				FMyTransformOutput.SliceCount = SpreadMax;
				
				FValues.Clear();
				FStrings.Clear();
				FColors.Clear();
				FTransforms.Clear();
				
				//the variables to fill with the input data
				double currentValueSlice;
				string currentStringSlice;
				RGBAColor currentColorSlice;
				Matrix4x4 currentTransformSlice;
				
				//loop for all slices
				for (int i=0; i<SpreadMax; i++)
				{
					//read data from inputs
					FMyValueInput.GetValue(i, out currentValueSlice);
					FMyStringInput.GetString(i, out currentStringSlice);
					FMyColorInput.GetColor(i, out currentColorSlice);
					FMyTransformInput.GetMatrix(i, out currentTransformSlice);

					//your function per slice
					currentValueSlice = currentValueSlice*2;
					currentStringSlice = currentStringSlice.ToUpper();
					currentColorSlice = VColor.Complement(currentColorSlice);
					currentTransformSlice = currentTransformSlice * VMath.Scale(0.5, 2, 2);
					
					FValues.Add(String.Format("{0:F4}", currentValueSlice));
					FStrings.Add(currentStringSlice);
					FColors.Add(currentColorSlice);
					FTransforms.Add(currentTransformSlice);
					
					//write output to pins
					FMyValueOutput.SetValue(i, currentValueSlice);
					FMyStringOutput.SetString(i, currentStringSlice);
					FMyColorOutput.SetColor(i, currentColorSlice);
					FMyTransformOutput.SetMatrix(i, currentTransformSlice);
				}
				
				//redraw gui only if anything changed
				Invalidate();
			}
		}
		
		#endregion mainloop
		
		protected override void OnPaint(PaintEventArgs e)
		{
			System.Drawing.Graphics g = e.Graphics;
			
			Font f = new Font("Verdana", 7);
			SolidBrush b = new SolidBrush(Color.Black);
			SolidBrush d;

			//a vvvv matrix
			Matrix4x4 vMatrix;
			
			for (int i=0; i<FTransforms.Count; i++)
			{
				vMatrix = FTransforms[i];
				
				g.Transform = new Matrix((float) vMatrix.m11, (float) vMatrix.m12, (float) vMatrix.m21, (float) vMatrix.m22, (float) vMatrix.m41, (float) vMatrix.m42);
				
				d = new SolidBrush(FColors[i].Color);
				g.FillRectangle(d, new Rectangle(0, i*12, Width, 12));
				g.DrawString(FValues[i], f, b, 10, i*12);

				g.DrawString(FStrings[i], f, b, 50, i*12);
			}
		}
	}
}
