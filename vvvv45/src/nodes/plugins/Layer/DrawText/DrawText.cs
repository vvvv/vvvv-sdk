//////licence
//GNU Lesser General Public License (LGPL)
//english: http://www.gnu.org/licenses/lgpl.html
//german: http://www.gnu.de/lgpl-ger.html

#region usings
//use what you need
using System;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.ComponentModel.Composition;
using System.Linq;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;
using VVVV.Core.Logging;

using SlimDX;
using SlimDX.Direct3D9;
#endregion usings

namespace VVVV.Nodes
{
	// TODO: Implement device handling properly for plugins. They should be able to work with
	// the Device object directly instead of dealing with IntPtr and Dispose.
	
	[PluginInfo(Name = "Text",
	            Category = "EX9",
	            Author = "vvvv group",
	            Help = "Draws flat Text")]
	public class DrawText : IPluginEvaluate, IPluginDXLayer
	{
		class DeviceHelpers : IDisposable
		{
			public readonly Sprite Sprite;
			public readonly Texture Texture;
			
			public DeviceHelpers(Sprite sprite, Texture texture)
			{
				Sprite = sprite;
				Texture = texture;
			}
			
			public void Dispose()
			{
				Sprite.Dispose();
				Texture.Dispose();
			}
		}
		
		#region fields & pins
		[Input("Text", DefaultString = "vvvv")]
		protected ISpread<string> FTextInput;

		[Input("Font", EnumName = "SystemFonts")]
		protected IDiffSpread<EnumEntry> FFontInput;

		[Input("Italic")] //, IsSingle = true)]
		protected IDiffSpread<bool> FItalicInput;

		[Input("Bold")] //, IsSingle = true)]
		protected IDiffSpread<bool> FBoldInput;

		[Input("Size", DefaultValue = 150, MinValue = 0)] //, IsSingle = true)]
		protected IDiffSpread<int> FSizeInput;

		//[Input("Precision", DefaultEnumEntry = "Default")]
		//protected ISpread<Precision> FPrecision;

		[Input("Quality", DefaultEnumEntry = "Default")]
		protected ISpread<FontQuality> FQuality;

		[Input("Color", DefaultColor = new double[4] { 1, 1, 1, 1 })]
		protected ISpread<RGBAColor> FColorInput;

		[Input("Brush Color", DefaultColor = new double[4] { 0, 0, 0, 1 })]
		protected ISpread<RGBAColor> FBrushColor;

		[Input("Show Brush")]
		protected ISpread<bool> FShowBrush;

		[Input("Horizontal Align", EnumName = "HorizontalAlign")]
		protected ISpread<EnumEntry> FHorizontalAlignInput;

		[Input("Vertical Align", EnumName = "VerticalAlign")]
		protected ISpread<EnumEntry> FVerticalAlignInput;

		[Input("Text Rendering Mode", EnumName = "TextRenderingMode")]
		protected ISpread<EnumEntry> FTextRenderingModeInput;

		[Input("Normalize", EnumName = "Normalize", IsSingle = true)]
		protected ISpread<EnumEntry> FNormalizeInput;

		[Input("Width [px] (Multiline Mode)", DefaultValue = 300)]
		protected ISpread<int> FWidth;

		[Input("Enabled", DefaultValue = 1, IsSingle = true)]
		protected ISpread<bool> FEnabledInput;
		
		[Input("Preload Min Character", Visibility = PinVisibility.OnlyInspector, MinValue = 0.0)]
		protected ISpread<int> FPreloadMin;
		
		[Input("Preload Max Character", Visibility = PinVisibility.OnlyInspector, MinValue = 0.0)]
		protected ISpread<int> FPreloadMax;
		
		[Input("Cache Font", DefaultValue = 1.0, Visibility = PinVisibility.OnlyInspector)]
		protected ISpread<bool> FFontCaching;
		
		[Output("Text Size")]
		protected ISpread<Vector2D> FSizeOutput;

		[Import]
		protected ILogger Logger { get; set; }

		private ITransformIn FTransformIn;
		private IDXRenderStateIn FRenderStatePin;
		private IDXLayerIO FLayerOutput;
		private int FSpreadMax;
		private Dictionary<int, SlimDX.Direct3D9.Font> FFonts = new Dictionary<int, SlimDX.Direct3D9.Font>();
		private Dictionary<Device, DeviceHelpers> FDeviceHelpers = new Dictionary<Device, DeviceHelpers>();
		#endregion field declarationPL

		#region constructur
		[ImportingConstructor]
		public DrawText(IPluginHost host)
		{
			host.CreateRenderStateInput(TSliceMode.Single, TPinVisibility.True, out FRenderStatePin);
			FRenderStatePin.Order = -2;
			host.CreateTransformInput("Transform", TSliceMode.Dynamic, TPinVisibility.True, out FTransformIn);
			FTransformIn.Order = -1;
			host.CreateLayerOutput("Layer", TPinVisibility.True, out FLayerOutput);
			FLayerOutput.Order = -1;
		}
		#endregion constructur

		#region mainloop
		public void Evaluate(int spreadMax)
		{
			foreach (var kv in FFonts.ToArray())
				if (kv.Value.Tag == null)
					RemoveFont(kv.Key);

			FSpreadMax = spreadMax;
			FSizeOutput.SliceCount = spreadMax;
			
			if (FEnabledInput.SliceCount > 0 && FEnabledInput[0])
				foreach (var kv in FFonts)
					kv.Value.Tag = null;
			
            if (e != null)
            {
                var e_ = e; 
                e = null;
                throw e_;
            }
		}
		#endregion mainloop

		#region DXLayer
		private void RemoveFont(int id)
		{
			var df = FFonts[id];
			FFonts.Remove(id);
			df.OnLostDevice();
			df.Dispose();
		}

		private static int GetFontKey(Device dev, string name, int size, bool italic, FontWeight weight, FontQuality quality)
		{
			int hashCode = 0;
			
			unchecked
			{
				hashCode += 1000000007 * dev.ComPointer.GetHashCode();
				hashCode += 1000000009 * name.GetHashCode();
				hashCode += 1000000021 * size.GetHashCode();
				hashCode += 1000000033 * italic.GetHashCode();
				hashCode += 1000000087 * weight.GetHashCode();
				hashCode += 1000000093 * quality.GetHashCode();
			}
			
			return hashCode;
		}

		private SlimDX.Direct3D9.Font CreateFont(Device dev, int slice)
		{
			var name = FFontInput[slice].Name;
			var size = FSizeInput[slice];
			var italic = FItalicInput[slice];
			var weight = FBoldInput[slice] ? FontWeight.Bold : FontWeight.Light;
			var quality = FQuality[slice];
			
			var id = GetFontKey(dev, name, size, italic, weight, quality);
			
			SlimDX.Direct3D9.Font font;
			if (!FFonts.TryGetValue(id, out font))
			{
				font = new SlimDX.Direct3D9.Font(
					dev, size, 0, weight, 0, italic, CharacterSet.Default, Precision.Default, //id.Precision,
					quality, PitchAndFamily.Default, name);
				
				font.PreloadCharacters(FPreloadMin[slice], FPreloadMax[slice]);
				
				FFonts.Add(id, font);
			}
			
			if (FFontCaching[slice])
				font.Tag = dev;
			else
				font.Tag = null; // Will be removed by next Evaluate
			
			return font;
		}

		public void UpdateResource(IPluginOut ForPin, Device OnDevice)
		{
			//create device specific helpers on given device if not already present
			if (!FDeviceHelpers.ContainsKey(OnDevice))
			{
				var dh = new DeviceHelpers(
					new Sprite(OnDevice),
					new Texture(OnDevice, 1, 1, 1, Usage.Dynamic, Format.L8, Pool.Default)); // Format.A8R8G8B8, Pool.Default)
				
				//need to fill texture white to be able to set color on sprite later
				DataRectangle tex = dh.Texture.LockRectangle(0, LockFlags.None);
				tex.Data.WriteByte(255);
				dh.Texture.UnlockRectangle(0);

				FDeviceHelpers.Add(OnDevice, dh);
			}
		}

		public void DestroyResource(IPluginOut ForPin, Device OnDevice, bool OnlyUnManaged)
		{
			//dispose resources that were created on given device
			DeviceHelpers dh = null;
			if (FDeviceHelpers.TryGetValue(OnDevice, out dh))
			{
				dh.Dispose();

				var ids = FFonts.Where(kv => kv.Value.Tag == OnDevice).Select(kv => kv.Key).ToArray();
				foreach (var id in ids)
					RemoveFont(id);
				
				FDeviceHelpers.Remove(OnDevice);
			}
		}

		public void SetStates()
		{
			FRenderStatePin.SetRenderState(RenderState.AlphaTestEnable, 1);
			FRenderStatePin.SetRenderState(RenderState.SourceBlend, (int)Blend.SourceAlpha);
			FRenderStatePin.SetRenderState(RenderState.DestinationBlend, (int)Blend.InverseSourceAlpha);
		}

		public void Render(IDXLayerIO ForPin, Device OnDevice)
		{
			//concerning the cut characters in some fonts, especially when rendered italic see:
			//http://www.gamedev.net/community/forums/topic.asp?topic_id=441338
			//seems to be an official bug and we'd need to write our very own font rendering to fix that

			if (!FEnabledInput[0])
				return;

			//from the docs: D3DXSPRITE_OBJECTSPACE -> The world, view, and projection transforms are not modified.
			//for view and projection transforms this is exactly what we want: it allows placing the text within the
			//same world as all the other objects. however we don't want to work in object space but in world space
			//that's why we need to to set the world transform to a neutral value: identity
			OnDevice.SetTransform(TransformState.World, Matrix.Identity);
			FTransformIn.SetRenderSpace();

			//set states that are defined via upstream nodes
			FRenderStatePin.SetSliceStates(0);

			DeviceHelpers dh = FDeviceHelpers[OnDevice];

			dh.Sprite.Begin(SpriteFlags.ObjectSpace | SpriteFlags.DoNotAddRefTexture);
			try
			{
				int normalize = FNormalizeInput[0].Index;
				
				Matrix preScale = Matrix.Scaling(1, -1, 1);
				Matrix world;
				string text;
				Rectangle tmpRect = new Rectangle(0, 0, 0, 0);
				int hAlign, vAlign;
				float x, y;
				int width, height;

				for (int i = 0; i < FSpreadMax; i++)
				{
					var font = CreateFont(OnDevice, i);

					text = FTextInput[i];

					if (string.IsNullOrEmpty(text))
						continue;

					DrawTextFormat format = DrawTextFormat.NoClip | DrawTextFormat.ExpandTabs;

					hAlign = FHorizontalAlignInput[i].Index;
					switch (hAlign)
					{
							case 0: format |= DrawTextFormat.Left; break;
							case 1: format |= DrawTextFormat.Center; break;
							case 2: format |= DrawTextFormat.Right; break;
					}

					vAlign = FVerticalAlignInput[i].Index;
					switch (vAlign)
					{
							case 0: format |= DrawTextFormat.Top; break;
							case 1: format |= DrawTextFormat.VerticalCenter; break;
							case 2: format |= DrawTextFormat.Bottom; break;
					}

					switch (FTextRenderingModeInput[i].Index)
					{
							case 0: format |= DrawTextFormat.SingleLine; break;
							case 2: format |= DrawTextFormat.WordBreak; break;
					}

					tmpRect = new Rectangle(0, 0, FWidth[i], 0);
					font.MeasureString(dh.Sprite, text, format, ref tmpRect);
					width = tmpRect.Width;
					height = tmpRect.Height;
					
					FSizeOutput[i] = new Vector2D(width, height);

					switch (normalize)
					{
							case 1: preScale = Matrix.Scaling(1f / width, -1f / width, 1); break;
							//"width" means that the texture width will have no influence on the width of the sprite. Width will be always 1.

							case 2: preScale = Matrix.Scaling(1f / height, -1f / height, 1); break;
							//"height" means that the texture height will have no influence on the height of the sprite. Height will be always 1.

							case 3: preScale = Matrix.Scaling(1f / width, -1f / height, 1); break;
							//"on" means that the particle will always be a unit quad. independant of texture size
					}

					FTransformIn.GetRenderWorldMatrix(i, out world);
					dh.Sprite.Transform = preScale * world;

					switch (vAlign)
					{
							case 1: y = height / 2; break;
							case 2: y = height; break;
							default: y = 0; break;
					}

					if (FShowBrush[i])
					{
						switch (hAlign)
						{
								case 1: x = width / 2; break;
								case 2: x = width; break;
								default: x = 0; break;
						}
						dh.Sprite.Draw(dh.Texture, new Rectangle(0, 0, width, height),
						               new Vector3(x, y, -0.001f), null, new Color4(FBrushColor[i].Color.ToArgb()));
					}

					width = FWidth[i];
					switch (hAlign)
					{
							case 1: x = width / 2; break;
							case 2: x = width; break;
							default: x = 0; break;
					}
					font.DrawString(dh.Sprite, text, new Rectangle((int)-x, (int)-y, width, height), format, (Color)FColorInput[i]);
				}
			}
			catch (Exception e)
			{
				Logger.Log(e);
                this.e = e;
			}
			finally
			{
				dh.Sprite.End();
			}
		}
        #endregion

        Exception e;
	}
}