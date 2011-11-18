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
	//struct to store a doc and its background color
	public struct SvgDoc
	{
		public SvgDocument Document;
		public Color BackgroundColor;
		
		public SvgDoc(SvgDocument doc, Color col)
		{
			Document = doc;
			BackgroundColor = col;
		}
	}
	
	#region PluginInfo
	[PluginInfo(Name = "Reader",
	            Category = "SVG",
	            Help = "Reads and returns an SVG Document, its elements (layer) and other properties",
	            Tags = "xml")]
	#endregion PluginInfo
	public class DucumentSvgReaderNode : IPluginEvaluate
	{
		#region fields & pins
		[Input("Filename", StringType = StringType.Filename, FileMask = "SVG Files (*.svg)|*.svg")]
		IDiffSpread<string> FFilenameIn;
		
		[Input("Background Color", DefaultColor = new double[] { 0, 0, 0, 0 }, IsSingle = true)]
		IDiffSpread<RGBAColor> FBackgroundIn;
		
		[Input("Reload", IsBang = true)]
		ISpread<bool> FReloadIn;
		
		[Output("Document")]
		ISpread<SvgDoc> FDocOut;

		[Output("Layer")]
		ISpread<SvgElement> FLayerOut;
		
		[Output("View")]
		ISpread<SvgViewBox> FViewOut;
		
		[Output("Size")]
		ISpread<Vector2D> FSizeOut;

		[Import()]
		ILogger FLogger;
		#endregion fields & pins
 
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			if(FFilenameIn.IsChanged || FBackgroundIn.IsChanged || FReloadIn[0])
			{
				FDocOut.SliceCount = SpreadMax;				
				FLayerOut.SliceCount = SpreadMax;
				FSizeOut.SliceCount = SpreadMax;
				FViewOut.SliceCount = SpreadMax;
				
				for(int i=0; i<SpreadMax; i++)
				{
					var doc = SvgDocument.Open(FFilenameIn[i]);
					if(doc != null)
					{
						FDocOut[i] = new SvgDoc(doc, FBackgroundIn[i].Color);
						FLayerOut[i] = (SvgElement)doc.Children[0].Clone();
						FViewOut[i] = doc.ViewBox;
						var size = doc.GetDimensions();
						FSizeOut[i] = new Vector2D(size.Width, size.Height);
					}
				}
			}
				 
			//FLogger.Log(LogType.Debug, "hi tty!");
		}
	}
	
	#region PluginInfo
	[PluginInfo(Name = "Writer",
	            Category = "SVG",
	            Help = "Writes an SVG Document to disc",
	            Tags = "xml",
	            AutoEvaluate = true)]
	#endregion PluginInfo
	public class DucumentSvgWriterNode : IPluginEvaluate
	{
		[Input("Document")]
		ISpread<SvgDoc> FDocIn;
		
		[Input("Filename", DefaultString = "file.svg", FileMask = "SVG Files (*.svg)|*.svg", StringType = StringType.Filename)]
		ISpread<string> FFilenameIn;
		
		[Input("Write", IsBang = true)]
		ISpread<bool> FDoWriteIn;
		
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			//save to disc
			if(FDoWriteIn[0])
			{
				FDocIn[0].Document.Write(FFilenameIn[0]);
			}	
		}
	}
	
	#region PluginInfo
	[PluginInfo(Name = "Renderer",
	            Category = "SVG",
	            Help = "Renders svg layers into a window and outputs the document",
	            Tags = "xml",
	            AutoEvaluate = true,
	            InitialComponentMode = TComponentMode.InAWindow)]
	#endregion PluginInfo
	public class SvgRendererNode : UserControl, IPluginEvaluate
	{
		#region fields & pins
		[Input("Layer")]
		IDiffSpread<SvgElement> FSVGIn;
		
		[Input("View Box", IsSingle = true)]
		IDiffSpread<SvgViewBox> FViewIn;
		
		[Input("Ignore View", IsSingle = true)]
		IDiffSpread<bool> FIgnoreView;
		
		[Input("Background Color, IsSingle = true", DefaultColor = new double[] { 0, 0, 0, 1 }, IsSingle = true)]
		IDiffSpread<RGBAColor> FBackgroundIn;

		[Input("Size", StepSize = 1)]
		IDiffSpread<Vector2> FSizeIn;
		
		[Output("Document")]
		ISpread<SvgDoc> FOutput;
		
		SvgDocument FSVGDoc = new SvgDocument();
		
		SizeF FSize = new SizeF();
		bool FResized = false;
		
		Bitmap FBitMap;
		PictureBox FPicBox = new PictureBox();
		
		[Import]
		INode FThisNode;
		
		#endregion fields & pins
		
		public SvgRendererNode()
		{
			//clear controls in case init is called multiple times
			Controls.Clear();
			FPicBox.Dock = DockStyle.Fill;
			
			Controls.Add(FPicBox);
			
			this.Resize	+= new EventHandler(SvgRendererNode_Resize);
			
		}

		void SvgRendererNode_Resize(object sender, EventArgs e)
		{
			FResized = true;
		}
 
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{

			//update
			if (FSVGIn.IsChanged || FSizeIn.IsChanged || FBackgroundIn.IsChanged ||
			   FIgnoreView.IsChanged || FViewIn.IsChanged || FResized)
			{
				FResized = false;
				
				FSVGDoc = new SvgDocument();
				foreach(var elem in FSVGIn)
				{
					if(elem != null) FSVGDoc.Children.Add(elem);
				}
				
				FSVGDoc.Transforms = new SvgTransformCollection();
				
				//set view
				if(!FIgnoreView[0])
				{
					var view = FViewIn[0];
					
					if(view.Equals(SvgViewBox.Empty))
					{
						view =  new SvgViewBox(-1, -1, 2, 2);
					}
					
					FSVGDoc.ViewBox = view;
				}
				
				//calc size
				FSize.Width = FSizeIn[0].X;
				FSize.Height = FSizeIn[0].Y;
				
				if(FSize == SizeF.Empty)
				{
					FSize = new SizeF(this.Width, this.Height);
				}
				
				//set size and output doc
				FSVGDoc.Width = new SvgUnit(SvgUnitType.User, Math.Max(FSize.Width, 1));
				FSVGDoc.Height = new SvgUnit(SvgUnitType.User, Math.Max(FSize.Height, 1));
				 
				FOutput[0] = new SvgDoc(FSVGDoc, FBackgroundIn[0].Color);
			}
			
			//render to window
			if(FThisNode.Window != null)
			{
				if(FBitMap == null)
				{
					FBitMap = new Bitmap((int)Math.Ceiling(FSVGDoc.Width), (int)Math.Ceiling(FSVGDoc.Height));
				}
				else if(FBitMap.Height != FSVGDoc.Height || FBitMap.Width != FSVGDoc.Width)
				{
					FBitMap.Dispose();
					FBitMap = new Bitmap((int)Math.Ceiling(FSVGDoc.Width), (int)Math.Ceiling(FSVGDoc.Height));
				}
				
				//clear bitmap
				var g = Graphics.FromImage(FBitMap);
				g.Clear(FBackgroundIn[0].Color);
				g.Dispose();
				
				FSVGDoc.Draw(FBitMap);
				FPicBox.Image = FBitMap;
			}
			
		}
	}
	
	
	#region PluginInfo
	[PluginInfo(Name = "SVGTexture", 
	            Category = "EX9.Texture", 
	            Help = "Renders an SVG document into a texture with a given size", 
	            Tags = "")]
	#endregion PluginInfo
	public class EX9_TextureSVGTextureNode : DXTextureOutPluginBase, IPluginEvaluate
	{
		#region fields & pins

		[Input("Document")]
		IDiffSpread<SvgDoc> FSVGIn;
		
		[Input("Transform")]
		IDiffSpread<Matrix> FTransformIn;
		
		[Input("Size", StepSize = 1)]
		IDiffSpread<Vector2> FSizeIn;

		[Import()]
		ILogger FLogger;
		
		List<Bitmap> FBitmaps = new List<Bitmap>();
		List<Size> FSizes = new List<Size>();

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
			
			if(FSVGIn.IsChanged || FTransformIn.IsChanged || FSizeIn.IsChanged)
			{
				FSizes.Clear();
				
				if(FBitmaps.Count > SpreadMax)
				{
					FBitmaps.RemoveRange(SpreadMax, FBitmaps.Count - SpreadMax);
				}
				
				for(int i=0; i<SpreadMax; i++)
				{
					var doc = FSVGIn[i].Document;
					
					//calc size
					var size = new Size((int)FSizeIn[0].X, (int)FSizeIn[0].Y);
					if(size == Size.Empty && doc != null)
					{
						var s = doc.GetDimensions();
						size =  new Size((int)s.Width, (int)s.Height);
					}
					
					//store size for texture creation
					FSizes.Add(size);
					
					if(doc != null)
					{
						//get bitmap
						Bitmap bm;
						if(FBitmaps.Count > i)
						{
							bm = FBitmaps[i];
							if(bm.Size != size)
							{
								bm.Dispose();
								bm = new Bitmap(size.Width, size.Height);
								FBitmaps[i] = bm;
								Reinitialize();
							}
						}
						else //create a new bm
						{
							bm = new Bitmap(size.Width, size.Height);
							FBitmaps.Add(bm);
							Reinitialize();
						}
						
						//clear bitmap
						var g = Graphics.FromImage(bm);
						g.Clear(FSVGIn[i].BackgroundColor);
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
						doc.Width = new SvgUnit(SvgUnitType.Pixel, size.Width);
						doc.Height = new SvgUnit(SvgUnitType.Pixel, size.Height);
						
						//draw into bitmap
						doc.Draw(bm);
						
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
			return TextureUtils.CreateTexture(device, Math.Max(FSizes[Slice].Width, 1), Math.Max(FSizes[Slice].Height, 1));
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
//	[PluginInfo(Name = "SVGTexture", 
//	            Category = "EX9.Texture", 
//	            Help = "Renders an SVG xml-string into a texture with a given size", 
//	            Tags = "xml")]
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
