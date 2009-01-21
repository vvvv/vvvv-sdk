#region licence/info

//////project name
//vvvv plugin template

//////description
//basic vvvv node plugin template.
//Copy this an rename it, to write your own plugin node.

//////licence
//GNU Lesser General Public License (LGPL)
//english: http://www.gnu.org/licenses/lgpl.html
//german: http://www.gnu.de/lgpl-ger.html

//////language/ide
//C# sharpdevelop

//////dependencies
//VVVV.PluginInterfaces.V1;
//VVVV.Utils.VColor;
//VVVV.Utils.VMath;

//////initial author
//vvvv group

#endregion licence/info

//use what you need
using System;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Collections.Generic;
using System.Collections;

using VVVV.PluginInterfaces.V1;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using SlimDX.Direct3D9;

//the vvvv node namespace
namespace VVVV.Nodes
{
	public struct DeviceFont
	{
		public SlimDX.Direct3D9.Font Font;
		public SlimDX.Direct3D9.Device Device;
		public SlimDX.Direct3D9.Sprite Sprite;
	}
	
	//class definition
	public class DrawText: IPlugin, IDisposable, IPluginDXLayer
	{
		#region field declaration
		
		//the host (mandatory)
		private IPluginHost FHost;
		//Track whether Dispose has been called.
		private bool FDisposed = false;
		
		private IValueIn FItalicInput;
		private IValueIn FBoldInput;
		private IValueIn FTexSizeInput;
		private IStringIn FTextInput;
		private IStringIn FFontInput;
		private IValueIn FSizeInput;
		private IColorIn FColorInput;
		private ITransformIn FTranformIn;
		
		private IDXLayerIO FLayerOutput;
		private IValueOut FSizeOutput;
		
		private int FSpreadMax;
		
		private List<DeviceFont> FDeviceFonts = new List<DeviceFont>();
		
		#endregion field declaration
		
		#region constructor/destructor
		
		public DrawText()
		{
			//the nodes constructor
			//nothing to declare for this node
		}
		
		// Implementing IDisposable's Dispose method.
		// Do not make this method virtual.
		// A derived class should not be able to override this method.
		public void Dispose()
		{
			Dispose(true);
			// Take yourself off the Finalization queue
			// to prevent finalization code for this object
			// from executing a second time.
			GC.SuppressFinalize(this);
		}
		
		// Dispose(bool disposing) executes in two distinct scenarios.
		// If disposing equals true, the method has been called directly
		// or indirectly by a user's code. Managed and unmanaged resources
		// can be disposed.
		// If disposing equals false, the method has been called by the
		// runtime from inside the finalizer and you should not reference
		// other objects. Only unmanaged resources can be disposed.
		protected virtual void Dispose(bool disposing)
		{
			// Check to see if Dispose has already been called.
			if(!FDisposed)
			{
				if(disposing)
				{
					// Dispose managed resources.
				}
				// Release unmanaged resources. If disposing is false,
				// only the following code is executed.
				
				FHost.Log(TLogType.Debug, "PluginMeshTemplate is being deleted");
				
				// Note that this is not thread safe.
				// Another thread could start disposing the object
				// after the managed resources are disposed,
				// but before the disposed flag is set to true.
				// If thread safety is necessary, it must be
				// implemented by the client.
			}
			FDisposed = true;
		}

		// Use C# destructor syntax for finalization code.
		// This destructor will run only if the Dispose method
		// does not get called.
		// It gives your base class the opportunity to finalize.
		// Do not provide destructors in types derived from this class.
		~DrawText()
		{
			// Do not re-create Dispose clean-up code here.
			// Calling Dispose(false) is optimal in terms of
			// readability and maintainability.
			Dispose(false);
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
					//see: http://www.vvvv.org/tiki-index.php?page=Conventions.NodeAndPinNaming
					FPluginInfo = new PluginInfo();
					
					//the nodes main name: use CamelCaps and no spaces
					FPluginInfo.Name = "Text";
					//the nodes category: try to use an existing one
					FPluginInfo.Category = "EX9";
					//the nodes version: optional. leave blank if not
					//needed to distinguish two nodes of the same name and category
					FPluginInfo.Version = "";
					
					//the nodes author: your sign
					FPluginInfo.Author = "vvvv group";
					//describe the nodes function
					FPluginInfo.Help = "Draws Text as a Texture on a Sprite in 3D";
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

		public bool AutoEvaluate
		{
			//return true if this node needs to calculate every frame even if nobody asks for its output
			get {return false;}
		}
		
		#endregion node name and infos
		
		#region pin creation
		
		//this method is called by vvvv when the node is created
		public void SetPluginHost(IPluginHost Host)
		{
			//assign host
			FHost = Host;
			
			//create inputs
			FHost.CreateValueInput("Texture Size", 2, null, TSliceMode.Dynamic, TPinVisibility.OnlyInspector, out FTexSizeInput);
			FTexSizeInput.SetSubType2D(0, 8192, 1, 0, 0, false, false, true);
			
			FHost.CreateTransformInput("Transform", TSliceMode.Dynamic, TPinVisibility.True, out FTranformIn);
			
			FHost.CreateStringInput("Text", TSliceMode.Dynamic, TPinVisibility.True, out FTextInput);
			FTextInput.SetSubType("vvvv", false);
			FHost.CreateStringInput("Font", TSliceMode.Single, TPinVisibility.True, out FFontInput);
			FFontInput.SetSubType("Verdana", false);
			FHost.CreateValueInput("Size", 1, null, TSliceMode.Single, TPinVisibility.True, out FSizeInput);
			FSizeInput.SetSubType(0, int.MaxValue, 1, 10, false, false, true);
			FHost.CreateValueInput("Bold", 1, null, TSliceMode.Single, TPinVisibility.True, out FBoldInput);
			FBoldInput.SetSubType(0, 1, 1, 0, false, true, false);
			FHost.CreateValueInput("Italic", 1, null, TSliceMode.Single, TPinVisibility.True, out FItalicInput);
			FItalicInput.SetSubType(0, 1, 1, 0, false, true, false);
			
			FHost.CreateColorInput("Color", TSliceMode.Dynamic, TPinVisibility.True, out FColorInput);
			FColorInput.SetSubType(VColor.White, true);
			
			//rectangle
			//clip
			
			//multiline
			//wordbreak
			
			//center vertical
			//center horizontal
			
			//quality
			
			//create outputs
			FHost.CreateLayerOutput("Layer", TPinVisibility.True, out FLayerOutput);
			FHost.CreateValueOutput("Text Size", 2, null, TSliceMode.Dynamic, TPinVisibility.True, out FSizeOutput);
			FSizeOutput.SetSubType(0, int.MaxValue, 1, 0, false, false, false);
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
			FSpreadMax = SpreadMax;
			FSizeOutput.SliceCount = SpreadMax;
		}
		
		#endregion mainloop
		
		#region DXLayer
		public void UpdateResource(IPluginOut ForPin, int OnDevice)
		{
			DeviceFont f = FDeviceFonts.Find(delegate(DeviceFont df) {return df.Device.ComPointer == (IntPtr)OnDevice;});
			
			if ((f.Font != null) && (FFontInput.PinIsChanged || FSizeInput.PinIsChanged || FBoldInput.PinIsChanged || FItalicInput.PinIsChanged))
			{
				FDeviceFonts.Remove(f);
				f.Font.Dispose();
				f.Font = null;
				f.Sprite.Dispose();
				f.Sprite = null;
			}
			
			//if resource is not yet created for given OnDevice, create it now
			if (f.Font == null)
			{
				FHost.Log(TLogType.Debug, "Creating Resource...");
				
				Device dev = Device.FromPointer(new IntPtr(OnDevice));
				
				DeviceFont df = new DeviceFont();
				df.Device = dev;
				double size;
				FSizeInput.GetValue(0, out size);
				string font;
				FFontInput.GetString(0, out font);
				double italic;
				FItalicInput.GetValue(0, out italic);
				double bold;
				FBoldInput.GetValue(0, out bold);
				FontWeight weight;
				if (bold > 0.5)
					weight = FontWeight.Bold;
				else
					weight = FontWeight.Light;
				
				df.Font = new SlimDX.Direct3D9.Font(dev, (int) size, 0, weight, 0, italic>0.5, CharacterSet.Default, Precision.Default, FontQuality.Default, PitchAndFamily.Default, font);
				//df.Font = new SlimDX.Direct3D9.Font(dev, new System.Drawing.Font(font, (int) size));
				df.Sprite = new Sprite(dev);
				FDeviceFonts.Add(df);
			}
		}
		
		public void DestroyResource(IPluginOut ForPin, int OnDevice, bool OnlyUnManaged)
		{
			//dispose resources that were created for given OnDevice
			DeviceFont f = FDeviceFonts.Find(delegate(DeviceFont df) {return df.Device.ComPointer == (IntPtr)OnDevice;});
			
			if (f.Font != null)
			{
				FHost.Log(TLogType.Debug, "Destroying Resource...");
				FDeviceFonts.Remove(f);
				f.Font.Dispose();
				f.Font = null;
				f.Sprite.Dispose();
				f.Sprite = null;
			}
		}
		
		public void Render(IDXLayerIO ForPin, int OnDevice)
		{
			DeviceFont f = FDeviceFonts.Find(delegate(DeviceFont df) {return df.Device.ComPointer == (IntPtr)OnDevice;});
			
			if (f.Font != null)
			{
				string text;
				RGBAColor c;
				double x, y;
				double sizeX, sizeY;
				
				Matrix4x4 m4x4;
				
				FTexSizeInput.GetValue2D(0, out sizeX, out sizeY);
				if (sizeX == 0)
					FSizeInput.GetValue(0, out sizeX);
				if (sizeY == 0)
					FSizeInput.GetValue(0, out sizeY);
				//compensate texture size
				Matrix4x4 preScale = VMath.Scale(1/sizeX, -1/sizeY, 1);
				
				SlimDX.Matrix m = new SlimDX.Matrix();
				f.Sprite.Begin(SpriteFlags.ObjectSpace | SpriteFlags.DoNotAddRefTexture);// | SpriteFlags.DoNotModifyRenderState | SpriteFlags.DoNotSaveState);
				//f.Device.SetSamplerState(0, SamplerState.MagFilter, TextureFilter.Point);
				
				Rectangle textSize;
				for (int i=0; i<FSpreadMax; i++)
				{
					FColorInput.GetColor(i, out c);
					FTextInput.GetString(i, out text);
					
					FTranformIn.GetMatrix(i, out m4x4);
					
					m4x4 = m4x4 * preScale;
					m.M11 = (float) m4x4.m11;
					m.M12 = (float) m4x4.m12;
					m.M13 = (float) m4x4.m13;
					m.M14 = (float) m4x4.m14;
					
					m.M21 = (float) m4x4.m21;
					m.M22 = (float) m4x4.m22;
					m.M23 = (float) m4x4.m23;
					m.M24 = (float) m4x4.m24;
					
					m.M31 = (float) m4x4.m31;
					m.M32 = (float) m4x4.m32;
					m.M33 = (float) m4x4.m33;
					m.M34 = (float) m4x4.m34;
					
					m.M41 = (float) m4x4.m41;
					m.M42 = (float) m4x4.m42;
					m.M43 = (float) m4x4.m43;
					m.M44 = (float) m4x4.m44;
					
					f.Sprite.Transform = m;
					DrawTextFormat dtf = DrawTextFormat.Center | DrawTextFormat.VerticalCenter | DrawTextFormat.NoClip;
					
					f.Font.DrawString(f.Sprite, text, new Rectangle(0, 0, 0, 0), dtf, c.Color.ToArgb());
					
					textSize = f.Font.MeasureString(f.Sprite, text, dtf);
					FSizeOutput.SetValue2D(i, textSize.Width, textSize.Height);					
				}
				f.Sprite.End();
			}
		}
		#endregion
	}
}
