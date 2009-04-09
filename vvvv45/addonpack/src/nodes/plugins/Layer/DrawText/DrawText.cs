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
using VVVV.Utils;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;
using VVVV.Utils.DX;

using SlimDX;
using SlimDX.Direct3D9;

//the vvvv node namespace
namespace VVVV.Nodes
{
	public struct DeviceFont
	{
		public SlimDX.Direct3D9.Font Font;
		public SlimDX.Direct3D9.Sprite Sprite;
		public SlimDX.Direct3D9.Texture Texture;
	}
	
	//class definition
	public class DrawText: IPlugin, IDisposable, IPluginDXLayer
	{
		#region field declaration
		
		//the host (mandatory)
		private IPluginHost FHost;
		//Track whether Dispose has been called.
		private bool FDisposed = false;
		
		private IValueIn FRectInput;
		private IValueIn FItalicInput;
		private IValueIn FBoldInput;
		private IStringIn FTextInput;
		private IEnumIn FFontInput;
		private IValueIn FSizeInput;
		private IEnumIn FNormalizeInput;
		private IColorIn FColorInput;
		private ITransformIn FTranformIn;
		private IEnumIn FHorizontalAlignInput;
		private IEnumIn FVerticalAlignInput;
		private IEnumIn FTextRenderingModeInput;
		//private IEnumIn FTransformSpace;
		private IValueIn FEnabledInput;
		
		private IValueIn FShowBrush;
		private IColorIn FBrushColor;
		
		private IDXLayerIO FLayerOutput;
		private IValueOut FSizeOutput;
		
		private int FSpreadMax;
		
		private Dictionary<int, DeviceFont> FDeviceFonts = new Dictionary<int, DeviceFont>();
		
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
					FPluginInfo.Help = "Draws flat Text";
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
			FHost.CreateTransformInput("Transform", TSliceMode.Dynamic, TPinVisibility.True, out FTranformIn);
			FHost.CreateValueInput("Rectangle", 2, null, TSliceMode.Dynamic, TPinVisibility.True, out FRectInput);
			FRectInput.SetSubType2D(double.MinValue, double.MaxValue, 0.01, 0, 0, false, false, false);
			FHost.CreateStringInput("Text", TSliceMode.Dynamic, TPinVisibility.True, out FTextInput);
			FTextInput.SetSubType("vvvv", false);
			FHost.CreateEnumInput("Font", TSliceMode.Single, TPinVisibility.True, out FFontInput);
			FFontInput.SetSubType("SystemFonts");
			FHost.CreateValueInput("Italic", 1, null, TSliceMode.Single, TPinVisibility.True, out FItalicInput);
			FItalicInput.SetSubType(0, 1, 1, 0, false, true, false);
			FHost.CreateValueInput("Bold", 1, null, TSliceMode.Single, TPinVisibility.True, out FBoldInput);
			FBoldInput.SetSubType(0, 1, 1, 0, false, true, false);
			FHost.CreateValueInput("Size", 1, null, TSliceMode.Single, TPinVisibility.True, out FSizeInput);
			FSizeInput.SetSubType(0, int.MaxValue, 1, 150, false, false, true);
			
			FHost.CreateColorInput("Color", TSliceMode.Dynamic, TPinVisibility.True, out FColorInput);
			FColorInput.SetSubType(VColor.White, true);
			
			//rectangle
			//clip
			
			FHost.CreateColorInput("Brush Color", TSliceMode.Dynamic, TPinVisibility.True, out FBrushColor);
			FBrushColor.SetSubType(VColor.Black, true);
			
			FHost.CreateValueInput("Show Brush", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FShowBrush);
			FShowBrush.SetSubType(0, 1, 1, 0, false, true, false);
			
			FHost.CreateEnumInput("Horizontal Align", TSliceMode.Dynamic, TPinVisibility.True, out FHorizontalAlignInput);
			FHorizontalAlignInput.SetSubType("HorizontalAlign");

			FHost.CreateEnumInput("Vertical Align", TSliceMode.Dynamic, TPinVisibility.True, out FVerticalAlignInput);
			FVerticalAlignInput.SetSubType("VerticalAlign");
			
			FHost.CreateEnumInput("Text Rendering Mode", TSliceMode.Dynamic, TPinVisibility.True, out FTextRenderingModeInput);
			FTextRenderingModeInput.SetSubType("TextRenderingMode");
			
			FHost.CreateEnumInput("Normalize", TSliceMode.Single, TPinVisibility.True, out FNormalizeInput);
			FNormalizeInput.SetSubType("Normalize");

			FHost.CreateValueInput("Enabled", 1, null, TSliceMode.Single, TPinVisibility.True, out FEnabledInput);
			FEnabledInput.SetSubType(0, 1, 1, 1, false, true, false);
			
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
		private void RemoveResource(int OnDevice)
		{
			DeviceFont df = FDeviceFonts[OnDevice];
			FHost.Log(TLogType.Debug, "Destroying Resource...");
			FDeviceFonts.Remove(OnDevice);
			
			df.Font.Dispose();
			df.Sprite.Dispose();
			df.Texture.Dispose();
		}
		
		public void UpdateResource(IPluginOut ForPin, int OnDevice)
		{
			bool needsupdate = false;
			
			try
			{
				DeviceFont df = FDeviceFonts[OnDevice];
				if (FFontInput.PinIsChanged || FSizeInput.PinIsChanged || FBoldInput.PinIsChanged || FItalicInput.PinIsChanged)
				{
					RemoveResource(OnDevice);
					needsupdate = true;
				}
			}
			catch
			{
				//if resource is not yet created on given Device, create it now
				needsupdate = true;
			}
			
			if (needsupdate)
			{
				FHost.Log(TLogType.Debug, "Creating Resource...");
				Device dev = Device.FromPointer(new IntPtr(OnDevice));

				DeviceFont df = new DeviceFont();
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
				df.Sprite = new Sprite(dev);
				df.Texture = new Texture(dev, 1, 1, 1, Usage.None, Format.L8, Pool.Default);// Format.A8R8G8B8, Pool.Default);
				
				FDeviceFonts.Add(OnDevice, df);
				
				//dispose device
				dev.Dispose();
			}
		}
		
		public void DestroyResource(IPluginOut ForPin, int OnDevice, bool OnlyUnManaged)
		{
			//dispose resources that were created on given OnDevice
			try
			{
				RemoveResource(OnDevice);
			}
			catch
			{
				//resource is not available for this device. good. nothing to do then.
			}
		}
		
		public void Render(IDXLayerIO ForPin, IPluginDXDevice DXDevice)
		{
			//concerning the cut characters in some fonts, especially when rendered itallic see:
			//http://www.gamedev.net/community/forums/topic.asp?topic_id=441338
			//seems to be an official bug and we'd need to write our very own font rendering to fix that
			
			double enabled;
			FEnabledInput.GetValue(0, out enabled);
			if (enabled < 0.5)
				return;
			
			//string space;
			//FTransformSpace.GetString(0, out space);
			
			//from the docs: D3DXSPRITE_OBJECTSPACE -> The world, view, and projection transforms are not modified.
			//for view and projection transforms this is exactly what we want: it allows placing the text within the
			//same world as all the other objects. however we don't want to work in object space but in world space
			//that's why we need to to set the world transform to a neutral value: identity
			Device dev = Device.FromPointer(new IntPtr(DXDevice.DevicePointer()));
			dev.SetTransform(TransformState.World, Matrix.Identity);

			DeviceFont df = FDeviceFonts[DXDevice.DevicePointer()];
			//DXDevice.SetSpace(FTranformIn, FTransformSpace);
			FTranformIn.SetRenderSpace();
			df.Sprite.Begin(SpriteFlags.DoNotAddRefTexture | SpriteFlags.ObjectSpace | SpriteFlags.AlphaBlend);
			
			double size;
			FSizeInput.GetValue(0, out size);
			
			int normalize;
			FNormalizeInput.GetOrd(0, out normalize); 
			
			Matrix4x4 preScale = VMath.Scale(1, -1, 1);
			switch (normalize)
			{
				case 0: preScale = VMath.Scale(1, -1, 1); break;  
				//"off" means that text will be in pixels
				
			}
									
			Matrix4x4 world;
			string text;
			RGBAColor textColor, brushColor;
			Rectangle tmpRect = new Rectangle(0, 0, 0, 0);
			
			int hAlign, vAlign, textMode, wi, hi;
			double showBrush, w, h;
			float x, y;
			
			for (int i=0; i<FSpreadMax; i++)
			{
				text = "";
				FTextInput.GetString(i, out text);
				
				if (string.IsNullOrEmpty(text))
					continue;
				
				FColorInput.GetColor(i, out textColor);
								
				DrawTextFormat dtf = DrawTextFormat.NoClip | DrawTextFormat.ExpandTabs;
				
				FHorizontalAlignInput.GetOrd(i, out hAlign);
				switch (hAlign)
				{
						case 0: dtf |= DrawTextFormat.Left; break;
						case 1: dtf |= DrawTextFormat.Center; break;
						case 2: dtf |= DrawTextFormat.Right; break;
				}
				
				FVerticalAlignInput.GetOrd(i, out vAlign);
				switch (vAlign)
				{
						case 0: dtf |= DrawTextFormat.Top; break;
						case 1: dtf |= DrawTextFormat.VerticalCenter; break;
						case 2: dtf |= DrawTextFormat.Bottom; break;
				}
				
				FTextRenderingModeInput.GetOrd(i, out textMode);
				switch (textMode)
				{
						case 0: dtf |= DrawTextFormat.SingleLine; break;
						case 2: dtf |= DrawTextFormat.WordBreak; break;
				}

				FRectInput.GetValue2D(i, out w, out h);
				wi = (int)(w*size*10);
				hi = (int)(h*size*10);
				tmpRect.Width = wi;
				tmpRect.Height = hi;
								
				df.Font.MeasureString(df.Sprite, text, dtf, ref tmpRect);
				FSizeOutput.SetValue2D(i, tmpRect.Width, tmpRect.Height);
				
				FTranformIn.GetRenderWorldMatrix(i, out world);
					
				switch (normalize)
				{
						case 1: preScale = VMath.Scale(1f/tmpRect.Width, -1f/tmpRect.Width, 1); break;
						//"width" means that the texture width will have no influence on the width of the sprite. Width will be always 1.
						
						case 2: preScale = VMath.Scale(1f/tmpRect.Height, -1f/tmpRect.Height, 1); break;
						//"height" means that the texture height will have no influence on the height of the sprite. Height will be always 1.
						
						case 3: preScale = VMath.Scale(1f/tmpRect.Width, -1f/tmpRect.Height, 1); break;
						//"on" means that the particle will always be a unit quad. independant of texture size
				}
				df.Sprite.Transform = VSlimDXUtils.Matrix4x4ToSlimDXMatrix(preScale * world);

				FShowBrush.GetValue(i, out showBrush);
				if (showBrush >= 0.5)
				{
					FBrushColor.GetColor(i, out brushColor);
					x = tmpRect.Width/2;
					y = tmpRect.Height/2;
					if (hAlign == 0)
						x -= x;
					else if (hAlign == 2)
						x += x;
						
					df.Sprite.Draw(df.Texture, new Rectangle(0, 0, tmpRect.Width, tmpRect.Height), new Vector3(x, y, -0.001f), new Vector3(0,0,0), new Color4(brushColor.Color));
				}
				
				df.Font.DrawString(df.Sprite, text, new Rectangle(-wi/2, -hi/2, wi, hi), dtf, textColor.Color.ToArgb());
			}
			
			df.Sprite.End();
		}
		#endregion
	}
}
