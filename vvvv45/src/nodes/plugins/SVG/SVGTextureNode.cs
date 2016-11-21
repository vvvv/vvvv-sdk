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
using VVVV.Utils.ManagedVCL;
using Svg;
using Svg.Transforms;

#endregion usings


namespace VVVV.Nodes
{

	//helper to read/write the custom "VVVVBackgroundColor" attribute
	public static class SvgDocumentExtentions
	{
	    public static void SetVVVVBackgroundColor(this SvgDocument doc, RGBAColor col)
	    {
	        doc.CustomAttributes["VVVVBackgroundColor"] = VColor.Serialze(col);
	    }
	    
	    public static RGBAColor GetVVVVBackgroundColor(this SvgDocument doc)
	    {
	        var col = VColor.Black;
	        var attributeString = "";
	        
	        if(doc.CustomAttributes.TryGetValue("VVVVBackgroundColor", out attributeString))
	            col = VColor.Deserialze(attributeString);
	           
	        return col;
	    }
	}
	
	#region PluginInfo
	[PluginInfo(Name = "AsSVG",
	            Category = "String",
	            Help = "Reads an XML string and returns an SVG document, its elements (layers) and other properties",
	            Tags = "xml")]
	#endregion PluginInfo
	public class DocumentSvgStringReaderNode : DocumentSvgReaderNode
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
				doc = SvgDocument.Open<SvgDocument>(s);
				FSuccess[slice] = true;
			}
			catch(Exception e)
			{
			    FSuccess[slice] = false;
			    FErrorMessage[slice] = e.Message;
			    FError[slice] = true;
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
	public class DocumentSvgFileReaderNode : DocumentSvgReaderNode
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
		        FSuccess[slice] = true;
		    }
		    catch(Exception e)
		    {
		        FSuccess[slice] = false;
		        FErrorMessage[slice] = e.Message;
		        FError[slice] = true;
		    }
		    
		    return doc;
		}
		
		protected override bool InputChanged()
		{
			return FFilenameIn.IsChanged;
		}
	}
	
	public abstract class DocumentSvgReaderNode : IPluginEvaluate
	{
		#region fields & pins
		#pragma warning disable 649,169
		[Input("Background Color", DefaultColor = new double[] { 0, 0, 0, 0 })]
		IDiffSpread<RGBAColor> FBackgroundIn;
		
		[Input("Reload", IsBang = true)]
		ISpread<bool> FReloadIn;
		
		[Output("Document")]
		ISpread<SvgDocument> FDocOut;

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
		
		[Output("Success")]
	    protected ISpread<bool> FSuccess;
	    
	    [Output("Error Message")]
	    protected ISpread<string> FErrorMessage;
	    
	    [Output("Error")]
	    protected ISpread<bool> FError;

		[Import()]
		protected ILogger FLogger;
		#pragma warning restore
		#endregion fields & pins
		
		protected virtual void Reset()
	    {
	        for (int slice = 0; slice < FSuccess.SliceCount; slice++)
	        {
	            FSuccess[slice] = false;
	            FErrorMessage[slice] = "";
	            FError[slice] = false;
	        }
	    }
 
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
		    FSuccess.SliceCount = SpreadMax;
		    FErrorMessage.SliceCount = SpreadMax;
		    FError.SliceCount = SpreadMax;
		    
		    Reset();
		    
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
					    doc.SetVVVVBackgroundColor(FBackgroundIn[i]);
						FDocOut[i] = doc;
						
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
						FDocOut[i] = null;
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
	
	public abstract class DocumentSvgWriterNode : IPluginEvaluate
	{
	    #pragma warning disable 649
		[Input("Document")]
		ISpread<SvgDocument> FDocIn;
		
		[Input("Size", StepSize = 1, Order = 10)]
		ISpread<Vector2> FSizeIn;
		
		[Input("Write", IsBang = true, Order = 20)]
		ISpread<bool> FDoWriteIn;
		
		[Output("Success", Order = 20)]
		protected ISpread<bool> FSuccess;
		
		[Output("Error Message", Order = 30)]
		protected ISpread<string> FErrorMessage;
		
		[Output("Error", Order = 40)]
		protected ISpread<bool> FError;
		
		#pragma warning restore
		
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			SetSlicecount(SpreadMax);
			
			Reset();
			
			for(int i=0; i<SpreadMax; i++)
			{
				//save to disc
				if(FDoWriteIn[i])
				{
					var doc = FDocIn[i];
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
		
		protected virtual void Reset()
		{
		    for (int slice = 0; slice < FSuccess.SliceCount; slice++)
		    {
		        FSuccess[slice] = false;
		        FErrorMessage[slice] = "";
		        FError[slice] = false;
		    }
		}
		
		protected virtual void SetSlicecount(int spreadMax)
		{
		    FSuccess.SliceCount = spreadMax;
		    FErrorMessage.SliceCount = spreadMax;
		    FError.SliceCount = spreadMax;
		}
		
		protected abstract void WriteDoc(SvgDocument doc, int slice);
	}
	
	#region PluginInfo
	[PluginInfo(Name = "Writer",
	            Category = "SVG",
	            Help = "Writes an SVG document to disk",
	            Tags = "xml",
	            AutoEvaluate = true)]
	#endregion PluginInfo
	public class DocumentSvgFileWriterNode : DocumentSvgWriterNode
	{
	    #pragma warning disable 649
	    [Input("Filename", DefaultString = "file.svg", FileMask = "SVG Files (*.svg)|*.svg", StringType = StringType.Filename, Order = 1)]
	    ISpread<string> FFilenameIn;

	    #pragma warning restore
	    
	    protected override void WriteDoc(SvgDocument doc, int slice)
	    {
	        try
	        {
                var newFileName = FFilenameIn[slice];

                Directory.CreateDirectory(Path.GetDirectoryName(newFileName));

                doc.Write(newFileName);
	            FSuccess[slice] = true;
	        }
	        catch(Exception e)
	        {
	            FSuccess[slice] = false;
	            FErrorMessage[slice] = e.Message;
	            FError[slice] = true;
	        }
	    }
	}
	
	#region PluginInfo
	[PluginInfo(Name = "AsString",
	            Category = "SVG",
	            Help = "Writes an SVG document into a string",
	            Tags = "xml",
	            AutoEvaluate = true)]
	#endregion PluginInfo
	public class DocumentSvgStringWriterNode : DocumentSvgWriterNode
	{	
	    #pragma warning disable 649
		[Output("XML")]
		ISpread<string> FStringOut;
		#pragma warning restore
		
		protected override void SetSlicecount(int spreadMax)
		{
		    base.SetSlicecount(spreadMax);
			FStringOut.SliceCount = spreadMax;
		}
		 
		protected override void WriteDoc(SvgDocument doc, int slice)
		{
		    using(var ms = new MemoryStream())
		    {
		        try
		        {
		            doc.Write(ms);
		            ms.Position = 0;
		            var sr = new StreamReader(ms);
		            FStringOut[slice] = sr.ReadToEnd();
		            sr.Close();
		            FSuccess[slice] = true;
		        }
		        catch(Exception e)
		        {
		            FSuccess[slice] = false;
		            FErrorMessage[slice] = e.Message;
		            FError[slice] = true;
		        }
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
        #pragma warning disable 649,169
		[Input("Document")]
		IDiffSpread<SvgDocument> FSVGIn;
		
		[Input("Transform")]
		IDiffSpread<Matrix> FTransformIn;
		
		[Input("Size", StepSize = 1)]
		IDiffSpread<Vector2> FSizeIn;

		[Import()]
		ILogger FLogger;
		#pragma warning restore
		
		List<Bitmap> FBitmaps = new List<Bitmap>();
		List<Size> FSizes = new List<Size>();
		List<SvgDocument> FDocs = new List<SvgDocument>();

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
				if(element != null) FDocs.Add(element);
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
					var doc = FDocs[i];
					
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
						g.Clear(FDocs[i].GetVVVVBackgroundColor().Color);
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
