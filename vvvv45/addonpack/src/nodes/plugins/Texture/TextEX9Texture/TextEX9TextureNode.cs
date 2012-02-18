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
	public class TextEX9TextureNode : DXTextureOutPluginBase, IPluginEvaluate
	{
		#region fields & pins
		[Input("Width", DefaultValue = 256)]
		IDiffSpread<int> FWidthIn;

		[Input("Height", DefaultValue = 256)]
		IDiffSpread<int> FHeightIn;
		
 		[Input("Text", DefaultString = "vvvv")]
        IDiffSpread<string> FTextInput;

		[Input("Character Encoding", EnumName = "CharEncoding")]
        IDiffSpread<EnumEntry> FCharEncoding;

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


		[Output("Text Scale")]
        ISpread<Vector2D> FScaleOutput;
		
        [Output("Text Size")]
        ISpread<Vector2D> FSizeOutput;
		
		private IDXTextureOut FTextureOutput;


		[Import()]
		ILogger FLogger;

		//track the current texture slice
		int FCurrentSlice;

		#endregion fields & pins

		// import host and hand it to base constructor
		[ImportingConstructor()]
		public TextEX9TextureNode(IPluginHost host) : base(host)
		{
			host.CreateTextureOutput("Texture Out", TSliceMode.Dynamic, TPinVisibility.True, out FTextureOutput);
            FTextureOutput.Order = -1;
		}

		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			SetSliceCount(SpreadMax);
			FScaleOutput.SliceCount=SpreadMax;
			FSizeOutput.SliceCount=SpreadMax;

			//recreate texture if resolution was changed
			if (FWidthIn.IsChanged || FHeightIn.IsChanged) {
				//set new texture size
				Reinitialize();
			}

			//update texture
			if (FTextInput.IsChanged ||
				FCharEncoding.IsChanged ||
				FFontInput.IsChanged ||
				FItalicInput.IsChanged ||
				FBoldInput.IsChanged ||
				FSizeInput.IsChanged ||
				FColorInput.IsChanged ||
				FBrushColor.IsChanged ||
				FShowBrush.IsChanged ||
				FHorizontalAlignInput.IsChanged ||
				FVerticalAlignInput.IsChanged ||
				FTextRenderingModeInput.IsChanged ||
				FNormalizeInput.IsChanged) {
				Update();
			}

		}

		//this method gets called, when Reinitialize() was called in evaluate,
		//or a graphics device asks for its data
		protected override Texture CreateTexture(int Slice, Device device)
		{
			//FLogger.Log(LogType.Debug, "Creating new texture at slice: " + Slice);
			return TextureUtils.CreateTexture(device, Math.Max(FWidthIn[Slice], 1), Math.Max(FHeightIn[Slice], 1));
		}

		//this method gets called, when Update() was called in evaluate,
		//or a graphics device asks for its texture, here you fill the texture with the actual data
		//this is called for each renderer, careful here with multiscreen setups, in that case
		//calculate the pixels in evaluate and just copy the data to the device texture here
		unsafe protected override void UpdateTexture(int Slice, Texture texture)
		{
			var rect = texture.LockRectangle(0, LockFlags.None);
 			//get the pointer to the data
		 	var data = (byte*)rect.Data.DataPointer.ToPointer();
			
		 	//calculate sizes
		 	var byteLength = (int)rect.Data.Length;
		 	var width = rect.Pitch/4;
		 	var height = byteLength/rect.Pitch;		 	
		 			 		
			Bitmap bit = new Bitmap(width, height, PixelFormat.Format32bppArgb);			
			Graphics g = Graphics.FromImage(bit);
			FontStyle style = FontStyle.Regular;
			if (FItalicInput[Slice])
				style|= FontStyle.Italic;
			if (FBoldInput[Slice])
				style|= FontStyle.Bold;
			System.Drawing.Font objFont = new System.Drawing.Font(FFontInput[Slice].Name, 
																FSizeInput[Slice], 
																style, 
																GraphicsUnit.Pixel);		
			
			string text = FTextInput[Slice];
			
			int renderingMode = FTextRenderingModeInput[Slice].Index;
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
				if (FCharEncoding[Slice] == "UTF8")
	            {
	                byte[] utf8bytes = Encoding.Default.GetBytes(text);
	                text = Encoding.UTF8.GetString(utf8bytes);
	            }			
						
				
				format.LineAlignment = StringAlignment.Near;
	            switch (FHorizontalAlignInput[Slice].Index)
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
				
				switch (FVerticalAlignInput[Slice].Index)
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
				FSizeOutput[Slice] = new Vector2D(width/size.Width,height/size.Height);
				
				float scx = 1; float scy = 1;
				switch (FNormalizeInput[Slice].Index)
	            {
	                case 0: break;
	                case 1: scx = width/size.Width;	break;
	                case 2: scy = height/size.Height; break;
	            	case 3:
	            		scx = width/size.Width;	
	            		scy = height/size.Height;
	            		break;
	            }
				FScaleOutput[Slice]=new Vector2D(scx,scy);
								
				g.TranslateTransform(layout.X,layout.Y);
				g.ScaleTransform(scx,scy);
				g.TranslateTransform(-layout.X,-layout.Y);
				
				
				if (renderingMode ==2)
					layout.Location = new PointF(0,0);
			
			}
			else
				FScaleOutput[Slice]=new Vector2D(0,0);
			
			
			RGBAColor tmpBrush = FColorInput[Slice];
			tmpBrush.A=0;
			Color brush = tmpBrush.Color;			
			if (FShowBrush[Slice])
				brush = FBrushColor[Slice].Color;
			g.Clear(brush);
			g.SmoothingMode = SmoothingMode.AntiAlias;
			g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
			g.DrawString(text, objFont, new SolidBrush(FColorInput[Slice].Color),layout,format);
			
			BitmapData bData = bit.LockBits(new Rectangle (new Point(), bit.Size),ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
			int byteCount = bData.Stride * bit.Height; // number of bytes in the bitmap
			byte[] bitBytes = new byte[byteCount];
			Marshal.Copy (bData.Scan0, bitBytes, 0, byteCount);	// Copy the locked bytes from memory	
			bit.UnlockBits (bData); // don't forget to unlock the bitmap!!

			for(int i=0; i<byteLength; i++)
				data[i]=bitBytes[i]; 			
		 		
		 	//unlock texture
		 	texture.UnlockRectangle(0);
			bit.Dispose();
			g.Dispose();
		}
		
	}
}
