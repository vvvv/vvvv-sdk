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

//here you can change the vertex type
using VertexType = VVVV.Utils.SlimDX.TexturedVertex;

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "Text", Category = "EX9.Texture", Help = "Renders text on textures", Tags = "", Author = "woei")]
	#endregion PluginInfo
	public class TextEX9TextureNode : IPluginEvaluate, IPartImportsSatisfiedNotification
	{
		#region helper class
		class Info
		{
			public int Slice;
			public int Width;
			public int Height;
			
			public string Text;
			public string Font;
			public bool Italic;
			public bool Bold;
			public int Size;
			public RGBAColor Color;
			public RGBAColor Brush;
			public bool ShowBrush;
			public int HAlign;
			public int VAlign;
			public int RenderMode;
			public int Normalize;
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
			FScaleOutput.SliceCount=SpreadMax;
			FSizeOutput.SliceCount=SpreadMax;

			for (int i=0; i<SpreadMax; i++)
			{
				var textureResource = FTextureOut[i];
				var info = textureResource.Metadata;
				
				if (info.Width != FWidthIn[i] || info.Height != FHeightIn[i])
				{
					textureResource.Dispose();
					textureResource = CreateTextureResource(i);
					info = textureResource.Metadata;
				}
				
				if (info.Text != FTextInput[i] || info.Font != FFontInput[i].Name ||
				               info.Italic != FItalicInput[i] || info.Bold != FBoldInput[i] || info.Size != FSizeInput[i] ||
				               info.Color != FColorInput[i] || info.Brush != FBrushColor[i] || info.ShowBrush != FShowBrush[i] ||
				               info.HAlign != FHorizontalAlignInput[i].Index || info.VAlign != FVerticalAlignInput[i].Index || 
				               info.RenderMode != FTextRenderingModeInput[i].Index || info.Normalize != FNormalizeInput[i].Index)
				{
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
					
					textureResource.NeedsUpdate = true;
					
				}
				else
					textureResource.NeedsUpdate = false;
				
				FTextureOut[i] = textureResource;
			}
		}

		TextureResource<Info> CreateTextureResource(int slice)
		{
			var info = new Info() { Slice = slice, Width = FWidthIn[slice], Height = FHeightIn[slice] };
			return TextureResource.Create(info, CreateTexture, UpdateTexture);
		}
		
		Texture CreateTexture(Info info, Device device)
		{
			FLogger.Log(LogType.Debug, "Creating new texture at slice: " + info.Slice);
			return TextureUtils.CreateTexture(device, Math.Max(info.Width, 1), Math.Max(info.Height, 1));
		}
		
		unsafe void UpdateTexture(Info info, Texture texture)
		{
		 	var width = Math.Max(info.Width, 1);
		 	var height = Math.Max(info.Height, 1);
		 			 		
			Bitmap bit = new Bitmap(width, height, PixelFormat.Format32bppArgb);			
			Graphics g = Graphics.FromImage(bit);
			FontStyle style = FontStyle.Regular;
			if (info.Italic)
				style|= FontStyle.Italic;
			if (info.Bold)
				style|= FontStyle.Bold;
			System.Drawing.Font objFont = new System.Drawing.Font(info.Font, 
																info.Size, 
																style, 
																GraphicsUnit.Pixel);		
			
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
	                case 2: layout.Size= new SizeF(width,height);break;
	            }
				
				format.LineAlignment = StringAlignment.Near;
	            switch (info.HAlign)
	            {
	                case 0: format.Alignment = StringAlignment.Near;break;
	                case 1: 
	            		format.Alignment = StringAlignment.Center;
	            		layout.X = width/2;
	            		break;
	                case 2: 
	            		format.Alignment = StringAlignment.Far;
	            		layout.X = width;
	            		break;
	            }
				
				switch (info.VAlign)
	            {
	                case 0: format.LineAlignment = StringAlignment.Near;break;
	                case 1: 
	            		format.LineAlignment = StringAlignment.Center;
	            		layout.Y = height/2;
	            		break;
	                case 2: 
	            		format.LineAlignment = StringAlignment.Far;
	            		layout.Y = height;
	            		break;
	            }
			
				SizeF size = g.MeasureString(text, objFont, layout.Size, format);
				FSizeOutput[info.Slice] = new Vector2D(width/size.Width,height/size.Height);
				
				float scx = 1; float scy = 1;
				switch (info.Normalize)
	            {
	                case 0: break;
	                case 1: scx = width/size.Width;	break;
	                case 2: scy = height/size.Height; break;
	            	case 3:
	            		scx = width/size.Width;	
	            		scy = height/size.Height;
	            		break;
	            }
				FScaleOutput[info.Slice]=new Vector2D(scx,scy);
								
				g.TranslateTransform(layout.X,layout.Y);
				g.ScaleTransform(scx,scy);
				g.TranslateTransform(-layout.X,-layout.Y);
				
				
				if (renderingMode ==2)
					layout.Location = new PointF(0,0);
			
			}
			else
				FScaleOutput[info.Slice]=new Vector2D(0,0);
			
			
			RGBAColor tmpBrush = info.Color;
			tmpBrush.A=0;
			Color brush = tmpBrush.Color;			
			if (info.ShowBrush)
				brush = info.Brush.Color;
			g.Clear(brush);
			g.SmoothingMode = SmoothingMode.AntiAlias;
			g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
			g.DrawString(text, objFont, new SolidBrush(info.Color.Color),layout,format);
			
			TextureUtils.CopyBitmapToTexture(bit, texture);
			bit.Dispose();
			g.Dispose();
		}
	}
}
