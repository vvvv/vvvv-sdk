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
	[PluginInfo(Name = "AsSVG",
	            Category = "String",
	            Help = "Reads an XML string and returns an SVG document, its elements (layers) and other properties",
	            Tags = "xml")]
	#endregion PluginInfo
	public class DucumentSvgStringReaderNode : DucumentSvgReaderNode
	{
	    #pragma warning disable 649
		[Input("XML", DefaultString = "<SVG></SVG>")]
		IDiffSpread<string> FXMLIn;
		#pragma warning restore
		
		protected override SvgDocument ReadDocument(int slice)
		{
			SvgDocument doc = null;
			try
			{
				var s = new MemoryStream(UTF8Encoding.Default.GetBytes(FXMLIn[slice]));
				doc = SvgDocument.Open(s, null);
			}
			catch (Exception e)
			{
				FLogger.Log(e);
			}
			
			return doc;
		}
		
		protected override bool InputChanged()
		{
			return FXMLIn.IsChanged;
		}
	}
	
	#region PluginInfo
	[PluginInfo(Name = "Reader",
	            Category = "SVG",
	            Help = "Reads and returns an SVG document, its elements (layers) and other properties",
	            Tags = "xml")]
	#endregion PluginInfo
	public class DucumentSvgFileReaderNode : DucumentSvgReaderNode
	{
	    #pragma warning disable 649
		[Input("Filename", StringType = StringType.Filename, DefaultString = "file.svg", FileMask = "SVG Files (*.svg)|*.svg")]
		IDiffSpread<string> FFilenameIn;
		#pragma warning restore
		
		protected override SvgDocument ReadDocument(int slice)
		{
			SvgDocument doc = null;
			try
			{
				doc = SvgDocument.Open(FFilenameIn[slice]);
			}
			catch (Exception e)
			{
				FLogger.Log(e);
			}
			
			return doc;
		}
		
		protected override bool InputChanged()
		{
			return FFilenameIn.IsChanged;
		}
	}
	
	public abstract class DucumentSvgReaderNode : IPluginEvaluate
	{
		#region fields & pins
		#pragma warning disable 649,169
		[Input("Background Color", DefaultColor = new double[] { 0, 0, 0, 0 })]
		IDiffSpread<RGBAColor> FBackgroundIn;
		
		[Input("Reload", IsBang = true)]
		ISpread<bool> FReloadIn;
		
		[Output("Document")]
		ISpread<SvgDoc> FDocOut;

		[Output("Layer")]
		ISpread<ISpread<SvgElement>> FLayerOut;
		
		[Output("View")]
		ISpread<SvgViewBox> FViewOut;
		
		[Output("Has View", Visibility = PinVisibility.Hidden)]
		ISpread<bool> FHasViewOut;
		
		[Output("Size")]
		ISpread<Vector2D> FSizeOut;
		
		[Output("Has Size", Visibility = PinVisibility.Hidden)]
		ISpread<bool> FHasSizeOut;

		[Import()]
		protected ILogger FLogger;
		#pragma warning restore
		#endregion fields & pins
 
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			if(InputChanged() || FBackgroundIn.IsChanged || FReloadIn[0])
			{
				FDocOut.SliceCount = SpreadMax;				
				FLayerOut.SliceCount = SpreadMax;
				FViewOut.SliceCount = SpreadMax;
				FHasViewOut.SliceCount = SpreadMax;
				FSizeOut.SliceCount = SpreadMax;
				FHasSizeOut.SliceCount = SpreadMax;
				
				for(int i=0; i<SpreadMax; i++)
				{
					var doc = ReadDocument(i);
					
					if(doc != null)
					{
						FDocOut[i] = new SvgDoc(doc, FBackgroundIn[i].Color);
						
						var spread = FLayerOut[i];
						spread.SliceCount = doc.Children.Count;
						
						for(int j=0; j<spread.SliceCount; j++)
						{
							spread[j] = doc.Children[j];
						}
						
						//check view and size
						var noView = doc.ViewBox.Equals(SvgViewBox.Empty);
						
						var hp = new SvgUnit(SvgUnitType.Percentage, 100);
						var noSize = (doc.Width == hp && doc.Height == hp);
						
						var view = doc.ViewBox;
						
						if(noView)
						{
							if(noSize)
							{
								view = doc.Bounds;
							}
							else
							{
								view = doc.GetDimensions();
							}
							
							doc.ViewBox = view;
						}
						
						if(noSize)
						{
							doc.Width = new SvgUnit(SvgUnitType.User, view.Width);
							doc.Height = new SvgUnit(SvgUnitType.User, view.Height);
						}
						
						FViewOut[i] = view;
						FHasViewOut[i] = !noView;
						var size = doc.GetDimensions();
						FSizeOut[i] = new Vector2D(size.Width, size.Height);
						
						FHasSizeOut[i] = !noSize;
					}
					else
					{
						FDocOut[i] = new SvgDoc();
						FLayerOut[i].SliceCount = 0;
						
						FViewOut[i] = new SvgViewBox();
						FHasViewOut[i] = false;
						
						FSizeOut[i] = new Vector2D();
						FHasSizeOut[i] = false;
					}
				}
			}
			//FLogger.Log(LogType.Debug, "hi tty!");
		}
		
		protected abstract SvgDocument ReadDocument(int slice);
		protected abstract bool InputChanged();
	}
	
	public abstract class DucumentSvgWriterNode : IPluginEvaluate
	{
	    #pragma warning disable 649
		[Input("Document")]
		ISpread<SvgDoc> FDocIn;
		
		[Input("Size", StepSize = 1, Order = 10)]
		ISpread<Vector2> FSizeIn;
		
		[Input("Write", IsBang = true, Order = 20)]
		ISpread<bool> FDoWriteIn;
		#pragma warning restore
		
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			SetSlicecount(SpreadMax);
			
			for(int i=0; i<SpreadMax; i++)
			{
				//save to disc
				if(FDoWriteIn[i])
				{
					var doc = FDocIn[i].Document;
					var oldW = doc.Width;
					var oldH = doc.Height;
					
					if(FSizeIn[i] != Vector2.Zero)
					{
						doc.Width = Math.Max(FSizeIn[i].X, 1);
						doc.Height = Math.Max(FSizeIn[i].Y, 1);
					}
					
					WriteDoc(doc, i);
					
					doc.Width = oldW;
					doc.Height = oldH;
				}
			}
		}
		
		protected abstract void SetSlicecount(int spreadMax);
		protected abstract void WriteDoc(SvgDocument doc, int slice);
	}
	
	#region PluginInfo
	[PluginInfo(Name = "Writer",
	            Category = "SVG",
	            Help = "Writes an SVG document to disk",
	            Tags = "xml",
	            AutoEvaluate = true)]
	#endregion PluginInfo
	public class DucumentSvgFileWriterNode : DucumentSvgWriterNode
	{	
	    #pragma warning disable 649
		[Input("Filename", DefaultString = "file.svg", FileMask = "SVG Files (*.svg)|*.svg", StringType = StringType.Filename, Order = 1)]
		ISpread<string> FFilenameIn;
		#pragma warning restore
		
		protected override void SetSlicecount(int spreadMax)
		{
			//nothing to do
		}
		
		protected override void WriteDoc(SvgDocument doc, int slice)
		{
			doc.Write(FFilenameIn[slice]);
		}
	}
	
	#region PluginInfo
	[PluginInfo(Name = "AsString",
	            Category = "SVG",
	            Help = "Writes an SVG document into a string",
	            Tags = "xml",
	            AutoEvaluate = true)]
	#endregion PluginInfo
	public class DucumentSvgStringWriterNode : DucumentSvgWriterNode
	{	
	    #pragma warning disable 649
		[Output("XML")]
		ISpread<string> FStringOut;
		#pragma warning restore
		
		protected override void SetSlicecount(int spreadMax)
		{
			FStringOut.SliceCount = spreadMax;
		}
		 
		protected override void WriteDoc(SvgDocument doc, int slice)
		{
			using(var ms = new MemoryStream())
            {
                doc.Write(ms);
                ms.Position = 0;
                var sr = new StreamReader(ms);
                FStringOut[slice] = sr.ReadToEnd();
                sr.Close();
            }
		}
	}
	
	#region PluginInfo
	[PluginInfo(Name = "Renderer",
	            Category = "SVG",
	            Help = "Renders SVG layers into a window and returns the document",
	            Tags = "xml",
	            AutoEvaluate = true,
	            InitialBoxWidth = 160,
                InitialBoxHeight = 120,
                InitialWindowWidth = 400,
                InitialWindowHeight = 300,
	            InitialComponentMode = TComponentMode.InAWindow)]
	#endregion PluginInfo
    public class SvgRendererNode : UserControl, IPluginEvaluate, IUserInputWindow
	{
		#region fields & pins
		#pragma warning disable 649,169
		[Input("Layers")]
		IDiffSpread<SvgElement> FSVGIn;
		
		[Input("View Box", IsSingle = true)]
		IDiffSpread<SvgViewBox> FViewIn;
		
		[Input("Ignore View", IsSingle = true, Visibility = PinVisibility.OnlyInspector)]
		IDiffSpread<bool> FIgnoreView;
		
		[Input("Background Color", DefaultColor = new double[] { 0, 0, 0, 1 }, IsSingle = true)]
		IDiffSpread<RGBAColor> FBackgroundIn;

		[Input("Size", StepSize = 1)]
		IDiffSpread<Vector2> FSizeIn;
		
		[Output("Document")]
		ISpread<SvgDoc> FOutput;
		
		[Output("Size")]
		ISpread<Vector2> FSizeOutput;
		
		SvgDocument FSVGDoc = new SvgDocument();
		
		SizeF FSize = new SizeF();
		bool FResized = false;
		
		Bitmap FBitMap;
		PictureBox FPicBox = new PictureBox();
		
		[Import]
		INode FThisNode;
		#pragma warning restore
		
		#endregion fields & pins
		
		public SvgRendererNode()
		{
			//clear controls in case init is called multiple times
			Controls.Clear();
			FPicBox.Dock = DockStyle.Fill;
			
			FPicBox.SizeMode = PictureBoxSizeMode.StretchImage;
			
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
				FSizeOutput[0] = new Vector2(FSize.Width, FSize.Height);
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

        public IntPtr InputWindowHandle
        {
            get { return FPicBox.Handle; }
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
        #pragma warning disable 649,169
		[Input("Document")]
		IDiffSpread<SvgDoc> FSVGIn;
		
		[Input("Transform")]
		IDiffSpread<Matrix> FTransformIn;
		
		[Input("Size", StepSize = 1)]
		IDiffSpread<Vector2> FSizeIn;

		[Import()]
		ILogger FLogger;
		#pragma warning restore
		
		List<Bitmap> FBitmaps = new List<Bitmap>();
		List<Size> FSizes = new List<Size>();
		List<SvgDoc> FDocs = new List<SvgDoc>();

		//track the current texture slice
		int FCurrentSlice;

		#endregion fields & pins

		// import host and hand it to base constructor
		[ImportingConstructor()]
		public EX9_TextureSVGTextureNode(IPluginHost host) : base(host)
		{
		}
        
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			FDocs.Clear();
			foreach (var element in FSVGIn) 
			{
				if(element.Document != null) FDocs.Add(element);
			}
			
			SpreadMax = FDocs.Count;
			SetSliceCount(SpreadMax);
			
			if(FSVGIn.IsChanged || FTransformIn.IsChanged || FSizeIn.IsChanged)
			{
				FSizes.Clear();
				
				if(FBitmaps.Count > SpreadMax)
				{
					//dispose unused
					for (int i = SpreadMax; i < FBitmaps.Count; i++) 
					{
						FBitmaps[i].Dispose();
					}
					
					//remove from list
					FBitmaps.RemoveRange(SpreadMax, FBitmaps.Count - SpreadMax);
				}
				
				for(int i=0; i<SpreadMax; i++)
				{
					var doc = FDocs[i].Document;
					
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
						g.Clear(FDocs[i].BackgroundColor);
						g.Dispose();
						
						//save old values
						var oldW = doc.Width;
						var oldH = doc.Height;
						
						if(doc.Transforms == null)
						{
							doc.Transforms = new SvgTransformCollection();
						}
						
						var m = FTransformIn[i];
						
						if(m.M11 != 0 && m.M22 !=0)
						{
							var mat = new SvgMatrix(new List<float>(){m.M11, m.M12, m.M21, m.M22, m.M41, m.M42});
							doc.Transforms.Add(mat);
							doc.Width = new SvgUnit(SvgUnitType.Pixel, size.Width);
							doc.Height = new SvgUnit(SvgUnitType.Pixel, size.Height);
							
							//draw into bitmap
							doc.Draw(bm);
							
							doc.Width = oldW;
							doc.Height = oldH;
							doc.Transforms.Remove(mat);
						}
						
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
			TextureUtils.CopyBitmapToTexture(FBitmaps[Slice], texture);
		}
	}
}
