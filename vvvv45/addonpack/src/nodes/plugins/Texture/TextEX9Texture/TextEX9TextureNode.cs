#region usings
using System;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.ComponentModel.Composition;
using System.Runtime.InteropServices;

using SlimDX;
using SlimDX.Direct3D9;
using VVVV.Core.Logging;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V2.EX9;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;
using VVVV.Utils.SlimDX;

#endregion usings

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "Text", Category = "EX9.Texture", Help = "Renders text on textures", Tags = "", Author = "woei")]
	#endregion PluginInfo
	public class TextEX9TextureNode : IPluginEvaluate, IPartImportsSatisfiedNotification
	{
		#region helper class
		private class Info
		{
			public int Slice;
			public int Width;
			public int Height;
			
			private string _text;
			public string Text { get { return _text; } set { if(_text != value) { PropertiesChanged = true; } _text = value; } }
			
			private string _font;
			public string Font { get { return _font; } set { if(_font != value) { PropertiesChanged = true; } _font = value; } }
			
			private bool _italic;
			public bool Italic { get { return _italic; } set { if(_italic != value) { PropertiesChanged = true; } _italic = value; } }
			
			private bool _bold;
			public bool Bold { get { return _bold; } set { if(_bold != value) { PropertiesChanged = true; } _bold = value; } }

			private int _size;			
			public int Size { get { return _size; } set { if(_size != value) { PropertiesChanged = true; } _size = value; } }
			
			private RGBAColor _color;
			public RGBAColor Color { get { return _color; } set { if(_color != value) { PropertiesChanged = true; } _color = value; } }
			
			private RGBAColor _brush;
			public RGBAColor Brush { get { return _brush; } set { if(_brush != value) { PropertiesChanged = true; } _brush = value; } }
			
			private bool _showBrush;
			public bool ShowBrush { get { return _showBrush; } set { if(_showBrush != value) { PropertiesChanged = true; } _showBrush = value; } }
			
			private int _hAlign;
			public int HAlign { get { return _hAlign; } set { if(_hAlign != value) { PropertiesChanged = true; } _hAlign = value; } }
			
			private int _vAlign;
			public int VAlign { get { return _vAlign; } set { if(_vAlign != value) { PropertiesChanged = true; } _vAlign = value; } }
			
			private int _renderMode;
			public int RenderMode { get { return _renderMode; } set { if(_renderMode != value) { PropertiesChanged = true; } _renderMode = value; } }
			
			private int _normalize;
			public int Normalize { get { return _normalize; } set { if(_normalize != value) { PropertiesChanged = true; } _normalize = value; } }
			
			private bool _propertiesChanged;
			public bool PropertiesChanged { get { return _propertiesChanged; } set { _propertiesChanged = value; } }
		}
		#endregion
		
		#region fields & pins
		#pragma warning disable 649
		[Input("Width", DefaultValue = 256)]
		IDiffSpread<int> FWidthIn;

		[Input("Height", DefaultValue = 256)]
		IDiffSpread<int> FHeightIn;
		
 		[Input("Text", DefaultString = "vvvv")]
        IDiffSpread<string> FTextInput;

        [Input("Font", EnumName = "SystemFonts")]
        IDiffSpread<EnumEntry> FFontInput;

        [Input("Italic")]
        IDiffSpread<bool> FItalicInput;

        [Input("Bold")]
        IDiffSpread<bool> FBoldInput;

        [Input("Size", DefaultValue = 150, MinValue = 0)]
        IDiffSpread<int> FSizeInput;

        [Input("Color", DefaultColor = new double[4] { 1, 1, 1, 1 })]
        IDiffSpread<RGBAColor> FColorInput;

        [Input("Brush Color", DefaultColor = new double[4] { 0, 0, 0, 1 })]
        IDiffSpread<RGBAColor> FBrushColor;

        [Input("Show Brush")]
        IDiffSpread<bool> FShowBrush;

        [Input("Horizontal Align", EnumName = "HorizontalAlign")]
        IDiffSpread<EnumEntry> FHorizontalAlignInput;

        [Input("Vertical Align", EnumName = "VerticalAlign")]
        IDiffSpread<EnumEntry> FVerticalAlignInput;

        [Input("Text Rendering Mode", EnumName = "TextRenderingMode")]
        IDiffSpread<EnumEntry> FTextRenderingModeInput;
		
		[Input("Normalize", EnumName = "Normalize", DefaultEnumEntry = "Both")]
        IDiffSpread<EnumEntry> FNormalizeInput;

        
        [Output("Texture Out")]
		ISpread<TextureResource<Info>> FTextureOut;

		[Output("Text Scale")]
        ISpread<Vector2D> FScaleOutput;
		
        [Output("Text Size")]
        ISpread<Vector2D> FSizeOutput;


		[Import()]
		ILogger FLogger;
		
		Spread<Bitmap> FBmpBuffer = new Spread<Bitmap>(0);
		#pragma warning restore
		#endregion fields & pins

		public void OnImportsSatisfied()
		{
			FTextureOut.SliceCount = 0;
		}

		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			FTextureOut.ResizeAndDispose(SpreadMax, CreateTextureResource);
			FBmpBuffer.ResizeAndDispose(SpreadMax, (t) => new Bitmap(FTextureOut[t].Metadata.Width, FTextureOut[t].Metadata.Height, PixelFormat.Format32bppArgb));
			FScaleOutput.SliceCount=SpreadMax;
			FSizeOutput.SliceCount=SpreadMax;

			for (int i=0; i<SpreadMax; i++)
			{
				var textureResource = FTextureOut[i];
				
				textureResource.NeedsUpdate = false;
				
				var info = textureResource.Metadata;
				if (info.Width != FWidthIn[i] || info.Height != FHeightIn[i])
				{
					textureResource.Dispose();
					textureResource = CreateTextureResource(i);
					textureResource.NeedsUpdate = true;
					
					info = textureResource.Metadata;
					FBmpBuffer[i].Dispose();
					FBmpBuffer[i] = new Bitmap(info.Width, info.Height, PixelFormat.Format32bppArgb);
					info.PropertiesChanged = true;
				}
				
				info.Text = FTextInput[i];
				info.Font = FFontInput[i].Name;
				info.Italic = FItalicInput[i];
				info.Bold = FBoldInput[i];
				info.Size = FSizeInput[i];
				info.Color = FColorInput[i];
				info.Brush = FBrushColor[i];
				info.ShowBrush = FShowBrush[i];
				info.HAlign = FHorizontalAlignInput[i].Index;
				info.VAlign = FVerticalAlignInput[i].Index;
				info.RenderMode = FTextRenderingModeInput[i].Index;
				info.Normalize = FNormalizeInput[i].Index;
				
				if (info.PropertiesChanged)
				{
					info.PropertiesChanged = false;
					textureResource.NeedsUpdate = true;
					
					Graphics g = Graphics.FromImage(FBmpBuffer[i]);
					FontStyle style = FontStyle.Regular;
					if (info.Italic)
						style|= FontStyle.Italic;
					if (info.Bold)
						style|= FontStyle.Bold;
					System.Drawing.Font objFont = new System.Drawing.Font(info.Font, info.Size, style, GraphicsUnit.Pixel);		
					
					string text = info.Text;
					
					int renderingMode = info.RenderMode;
					RectangleF layout = new RectangleF(0,0,0,0);
					StringFormat format = new StringFormat();
					if (!string.IsNullOrEmpty(text))
					{
						switch (renderingMode)
			            {
			                case 0: text = text.Replace("\n"," ").Replace("\n",string.Empty); break;
			                case 1: break;
			                case 2: layout.Size= new SizeF(info.Width,info.Height);break;
			            }
						
						format.LineAlignment = StringAlignment.Near;
			            switch (info.HAlign)
			            {
			                case 0: format.Alignment = StringAlignment.Near;break;
			                case 1: 
			            		format.Alignment = StringAlignment.Center;
			            		layout.X = info.Width/2;
			            		break;
			                case 2: 
			            		format.Alignment = StringAlignment.Far;
			            		layout.X = info.Width;
			            		break;
			            }
						
						switch (info.VAlign)
			            {
			                case 0: format.LineAlignment = StringAlignment.Near;break;
			                case 1: 
			            		format.LineAlignment = StringAlignment.Center;
			            		layout.Y = info.Height/2;
			            		break;
			                case 2: 
			            		format.LineAlignment = StringAlignment.Far;
			            		layout.Y = info.Height;
			            		break;
			            }
					
						SizeF size = g.MeasureString(text, objFont, layout.Size, format);
						FSizeOutput[i] = new Vector2D(info.Width/size.Width,info.Height/size.Height);
						
						float scx = 1; float scy = 1;
						switch (info.Normalize)
			            {
			                case 0: break;
			                case 1: scx = info.Width/size.Width; break;
			                case 2: scy = info.Height/size.Height; break;
			            	case 3:
			            		scx = info.Width/size.Width;	
			            		scy = info.Height/size.Height;
			            		break;
			            }
						FScaleOutput[i]=new Vector2D(scx,scy);
										
						g.TranslateTransform(layout.X,layout.Y);
						g.ScaleTransform(scx,scy);
						g.TranslateTransform(-layout.X,-layout.Y);
						
						if (renderingMode ==2)
							layout.Location = new PointF(0,0);
					}
					else
						FScaleOutput[i]=new Vector2D(0,0);
					
					
					RGBAColor tmpBrush = info.Color;
					tmpBrush.A=0;
					Color brush = tmpBrush.Color;			
					if (info.ShowBrush)
						brush = info.Brush.Color;
					g.Clear(brush);
					g.SmoothingMode = SmoothingMode.AntiAlias;
					g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
					g.DrawString(text, objFont, new SolidBrush(info.Color.Color),layout,format);
					g.Dispose();
				}
				FTextureOut[i] = textureResource;
			}
		}

		TextureResource<Info> CreateTextureResource(int slice)
		{
			var info = new Info() { Slice = slice, Width = Math.Max(FWidthIn[slice],1), Height = Math.Max(FHeightIn[slice],1) };
			return TextureResource.Create(info, CreateTexture, UpdateTexture);
		}
		
		Texture CreateTexture(Info info, Device device)
		{
			FLogger.Log(LogType.Debug, "Creating new texture at slice: " + info.Slice);
			return TextureUtils.CreateTexture(device, info.Width, info.Height);
		}
		
		unsafe void UpdateTexture(Info info, Texture texture)
		{
			TextureUtils.CopyBitmapToTexture(FBmpBuffer[info.Slice], texture);
		}
	}
}
