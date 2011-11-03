#region usings
using System;
using System.ComponentModel.Composition;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Xml;
using System.IO;
using System.Text;
using System.Collections.Generic;

using SlimDX;
using SlimDX.Direct3D9;
using VVVV.Core.Logging;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V2.EX9;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;
using VVVV.Utils.SlimDX;
using Svg;
using Svg.Transforms;

#endregion usings


namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "Reader",
	            Category = "SVG",
	            Help = "Reads a SVG Document and outputs it",
	            Tags = "")]
	#endregion PluginInfo
	public class ReaderSvgNode : IPluginEvaluate
	{
		#region fields & pins
		[Input("Filename", StringType = StringType.Filename, FileMask = "SVG Files (*.svg)|*.svg")]
		IDiffSpread<string> FFilenameIn;

		[Output("Document")]
		ISpread<SvgDocument> FOutput;

		[Import()]
		ILogger FLogger;
		#endregion fields & pins
 
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			if(FFilenameIn.IsChanged)
			{
				FOutput.SliceCount = SpreadMax;
				for(int i=0; i<SpreadMax; i++)
				{
					FOutput[i] = SvgDocument.Open(FFilenameIn[i]);
				}
			}
				 
			//FLogger.Log(LogType.Debug, "hi tty!");
		}
	}
	
	#region PluginInfo
	[PluginInfo(Name = "Document",
	            Category = "SVG Split",
	            Help = "Outputs the SVG elements in a SVG document",
	            Tags = "")]
	#endregion PluginInfo
	public class DucumentSvgSplitNode : IPluginEvaluate
	{
		#region fields & pins
		[Input("Document")]
		IDiffSpread<SvgDocument> FDocIn;

		[Output("SVG Layer")]
		ISpread<SvgElement> FOutput;
		
		[Output("Former Index")]
		ISpread<int> FFormerIndexOut;
		
		[Output("Width")]
		ISpread<float> FWidthOut;
		
		[Output("Height")]
		ISpread<float> FHeightOut;

		[Import()]
		ILogger FLogger;
		#endregion fields & pins
 
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			if(FDocIn.IsChanged)
			{
				FOutput.SliceCount = 0;
				FFormerIndexOut.SliceCount = 0;
				FWidthOut.SliceCount = SpreadMax;
				FHeightOut.SliceCount = SpreadMax;
				for(int i=0; i<SpreadMax; i++)
				{
					var doc = FDocIn[i];
					FWidthOut[i] = doc.Width;
					FHeightOut[i] = doc.Height;
					foreach(var c in doc.Children)
					{
						FOutput.Add(c);
						FFormerIndexOut.Add(i);
					}	
				}
			}
				 
			//FLogger.Log(LogType.Debug, "hi tty!");
		}
	}
	
	#region PluginInfo
	[PluginInfo(Name = "Document",
	            Category = "SVG Join",
	            Help = "Outputs the SVG Layers in a SVG document",
	            Tags = "")]
	#endregion PluginInfo
	public class DucumentSvgJoinNode : IPluginEvaluate
	{
		#region fields & pins
		[Input("SVG Layer")]
		IDiffSpread<SvgElement> FSVGIn;
		
		[Input("Transform")]
		IDiffSpread<Matrix> FTransformIn;
		
		[Input("View Box Center ")]
		IDiffSpread<Vector2> FViewCenterIn;
		
		[Input("View Box Size ", DefaultValues = new double[] {2, 2})]
		IDiffSpread<Vector2> FViewSizeIn;

		[Input("Width", DefaultValue = 128)]
		IDiffSpread<int> FWidthIn;

		[Input("Height", DefaultValue = 128)]
		IDiffSpread<int> FHeightIn;
		
		[Input("Filename", DefaultString = "file.svg", FileMask = "SVG Files (*.svg)|*.svg", StringType = StringType.Filename)]
		ISpread<string> FFilenameIn;
		
		[Input("Write", IsBang = true)]
		ISpread<bool> FDoWriteIn;
		
		[Output("Document")]
		ISpread<SvgDocument> FOutput;
		
		SvgDocument FSVGDoc = new SvgDocument();
		
		#endregion fields & pins
 
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			if (FSVGIn.IsChanged)
			{
				FSVGDoc = new SvgDocument();
				foreach(var elem in FSVGIn)
					FSVGDoc.Children.Add(elem);
			}

			//update texture
			if (FSVGIn.IsChanged || FWidthIn.IsChanged || FHeightIn.IsChanged || 
				FTransformIn.IsChanged || FViewCenterIn.IsChanged || FViewSizeIn.IsChanged)
			{
							
				FSVGDoc.Transforms = new SvgTransformCollection();

				var view = new SvgViewBox();
				view.MinX = (float)(FViewCenterIn[0].X - FViewSizeIn[0].X * 0.5);
				view.MinY = (float)(FViewCenterIn[0].Y - FViewSizeIn[0].Y * 0.5);
				view.Width = (float)FViewSizeIn[0].X;
				view.Height = (float)FViewSizeIn[0].Y;
				
			    FSVGDoc.ViewBox = view; 
				var m = FTransformIn[0];
				var mat = new SvgMatrix(new List<float>(){m.M11, m.M12, m.M21, m.M22, m.M41, m.M42});
				FSVGDoc.Transforms.Add(mat);
                FSVGDoc.Width = new SvgUnit(SvgUnitType.Pixel, FWidthIn[0]);
                FSVGDoc.Height = new SvgUnit(SvgUnitType.Pixel, FHeightIn[0]);
				 
				FOutput[0] = FSVGDoc;
			
				//FLogger.Log(LogType.Debug, "hi tty!");
			}
			
			//save to disc
			if(FDoWriteIn[0])
			{
				FSVGDoc.Write(FFilenameIn[0]);
			}
		}
	}
	
	
	#region PluginInfo
	[PluginInfo(Name = "SVGTexture", Category = "SVG", Help = "Creates a SVG Document and renders a SVG graphic into a texture with a given size", Tags = "texture")]
	#endregion PluginInfo
	public class EX9_TextureSVGTextureNode : DXTextureOutPluginBase, IPluginEvaluate
	{
		#region fields & pins

		[Input("Document")]
		IDiffSpread<SvgDocument> FSVGIn;
		
		[Input("Transform")]
		IDiffSpread<Matrix> FTransformIn;
		
		[Input("Background Color", DefaultColor = new double[] { 0, 0, 0, 0 }, IsSingle = true)]
		IDiffSpread<RGBAColor> FBackgroundIn;
		
		[Input("Width", DefaultValue = 128)]
		IDiffSpread<int> FWidthIn;

		[Input("Height", DefaultValue = 128)]
		IDiffSpread<int> FHeightIn;

		[Import()]
		ILogger FLogger;
		
		List<Bitmap> FBitmaps = new List<Bitmap>();
		RectangleF FOriginalDim;

		//track the current texture slice
		int FCurrentSlice;

		#endregion fields & pins

		// import host and hand it to base constructor
		[ImportingConstructor()]
		public EX9_TextureSVGTextureNode(IPluginHost host) : base(host)
		{
		}
		
		//memcopy method
		[DllImport("Kernel32.dll", EntryPoint="RtlMoveMemory", SetLastError=false)]
        static extern void CopyMemory(IntPtr dest, IntPtr src, int size);

		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			SetSliceCount(SpreadMax);

			//recreate texture if doc was changed
			if (FWidthIn.IsChanged || FHeightIn.IsChanged) 
			{
				//set new texture size
				Reinitialize();
				
				//dispose bitmaps
				foreach(var bm in FBitmaps)
					if(bm != null) bm.Dispose();
				
				FBitmaps.Clear();
				//create new bitmaps
				for(int i=0; i<SpreadMax; i++)
				{
					FBitmaps.Add(new Bitmap(FWidthIn[i], FHeightIn[i]));
				}
			}
			
			if(FSVGIn.IsChanged || FTransformIn.IsChanged || FBackgroundIn.IsChanged || FWidthIn.IsChanged || FHeightIn.IsChanged)
			{
				for(int i=0; i<SpreadMax; i++)
				{
					var doc = FSVGIn[i];
					if(doc != null)
					{
						var bm = FBitmaps[i];
						
						//clear bitmap
						var g = Graphics.FromImage(bm);
						g.Clear(FBackgroundIn[i].Color);
						g.Dispose();
						
						//save old values
						var oldW = doc.Width;
						var oldH = doc.Height;
						
						if(doc.Transforms == null)
						{
							doc.Transforms = new SvgTransformCollection();
						}
						
						var m = FTransformIn[i];
						var mat = new SvgMatrix(new List<float>(){m.M11, m.M12, m.M21, m.M22, m.M41, m.M42});
						doc.Transforms.Add(mat);
						doc.Width = new SvgUnit(SvgUnitType.Pixel, FWidthIn[0]);
						doc.Height = new SvgUnit(SvgUnitType.Pixel, FHeightIn[0]);
						
						//draw into bitmap
						FSVGIn[i].Draw(bm);
						
						doc.Width = oldW;
						doc.Height = oldH;
						doc.Transforms.Remove(mat);
						
						Update();
					}
				}
				
			}

		}

		//this method gets called, when Reinitialize() was called in evaluate,
		//or a graphics device asks for its data
		protected override Texture CreateTexture(int Slice, Device device)
		{
			FLogger.Log(LogType.Debug, "Creating new texture at slice: " + Slice);
			return TextureUtils.CreateTexture(device, Math.Max(FWidthIn[Slice], 1), Math.Max(FHeightIn[Slice], 1));
		}

		//this method gets called, when Update() was called in evaluate,
		//or a graphics device asks for its texture, here you fill the texture with the actual data
		//this is called for each renderer, careful here with multiscreen setups, in that case
		//calculate the pixels in evaluate and just copy the data to the device texture here
		unsafe protected override void UpdateTexture(int Slice, Texture texture)
		{
			FCurrentSlice = Slice;
			var bm = FBitmaps[Slice];
			var data = bm.LockBits(new Rectangle(0, 0, bm.Width, bm.Height), ImageLockMode.ReadOnly, bm.PixelFormat);
			var rect = texture.LockRectangle(0, LockFlags.None);
			
			CopyMemory(rect.Data.DataPointer, data.Scan0, data.Stride * data.Height);
			
			texture.UnlockRectangle(0);
			bm.UnlockBits(data);

		}
	}
	
	#region PluginInfo
	[PluginInfo(Name = "SVGTexture", Category = "EX9.Texture XML", Help = "Renders a SVG graphic into a texture with a given size", Tags = "")]
	#endregion PluginInfo
	public class EX9_TextureSVGTextureXMLNode : DXTextureOutPluginBase, IPluginEvaluate
	{
		#region fields & pins

		[Input("XML")]
		IDiffSpread<string> FXMLIn;
		
		[Input("Transform")]
		IDiffSpread<Matrix> FTransformIn;

		[Input("Width", DefaultValue = 128)]
		IDiffSpread<int> FWidthIn;

		[Input("Height", DefaultValue = 128)]
		IDiffSpread<int> FHeightIn;

		[Import()]
		ILogger FLogger;
		
		SvgDocument FSVGDoc;
		Bitmap FBitmap;
		RectangleF FOriginalDim;

		//track the current texture slice
		int FCurrentSlice;

		#endregion fields & pins

		// import host and hand it to base constructor
		[ImportingConstructor()]
		public EX9_TextureSVGTextureXMLNode(IPluginHost host) : base(host)
		{
		}
		
		//memcopy method
		[DllImport("msvcrt.dll", EntryPoint="memcpy", SetLastError=false)]
        static extern void CopyMemory(IntPtr dest, IntPtr src, int size);

		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			SetSliceCount(SpreadMax);

			//recreate texture if resolution was changed
			if (FWidthIn.IsChanged || FHeightIn.IsChanged) {
				//set new texture size
				Reinitialize();
			}
			
			if (FXMLIn.IsChanged)
			{
				var s = new MemoryStream(UTF8Encoding.Default.GetBytes(FXMLIn[0]));
                FSVGDoc = SvgDocument.Open(s, null);
				FOriginalDim = FSVGDoc.GetDimensions();
			}

			//update texture
			if (FXMLIn.IsChanged || FWidthIn.IsChanged || FHeightIn.IsChanged || FTransformIn.IsChanged) {
							
				
				FSVGDoc.Transforms = new SvgTransformCollection();

			    var sx = FWidthIn[0] / (float)FOriginalDim.Width;
				var sy = FHeightIn[0] / (float)FOriginalDim.Height;
				
				sx = Math.Max(sx, sy);
				
				var c = new SvgCircle();
				c.CenterX = 50;
				c.CenterY = 50;
				c.Radius = 100;
				
				//FSVGDoc.Children.Add(c);
				
                FSVGDoc.Transforms.Add(new SvgScale(sx, sx));
				var m = FTransformIn[0];
				var mat = new SvgMatrix(new List<float>(){m.M11, m.M12, m.M21, m.M22, m.M41, m.M42});
				FSVGDoc.Transforms.Add(mat);
                FSVGDoc.Width = new SvgUnit(SvgUnitType.Pixel, FWidthIn[0]);
                FSVGDoc.Height = new SvgUnit(SvgUnitType.Pixel, FHeightIn[0]);
                FBitmap = FSVGDoc.Draw();
				
				Update();
			}

		}

		//this method gets called, when Reinitialize() was called in evaluate,
		//or a graphics device asks for its data
		protected override Texture CreateTexture(int Slice, Device device)
		{
			FLogger.Log(LogType.Debug, "Creating new texture at slice: " + Slice);
			return TextureUtils.CreateTexture(device, Math.Max(FWidthIn[Slice], 1), Math.Max(FHeightIn[Slice], 1));
		}

		//this method gets called, when Update() was called in evaluate,
		//or a graphics device asks for its texture, here you fill the texture with the actual data
		//this is called for each renderer, careful here with multiscreen setups, in that case
		//calculate the pixels in evaluate and just copy the data to the device texture here
		unsafe protected override void UpdateTexture(int Slice, Texture texture)
		{
			FCurrentSlice = Slice;
			var data = FBitmap.LockBits(new Rectangle(0, 0, FBitmap.Width, FBitmap.Height), ImageLockMode.ReadOnly, FBitmap.PixelFormat);
			var rect = texture.LockRectangle(0, LockFlags.None);
			
			CopyMemory(rect.Data.DataPointer, data.Scan0, data.Stride * data.Height);
			
			texture.UnlockRectangle(0);
			FBitmap.UnlockBits(data);

		}
	}
}
