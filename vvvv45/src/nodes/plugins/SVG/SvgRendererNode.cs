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
using System.Drawing.Drawing2D;
using System.Drawing.Text;

#endregion usings


namespace VVVV.Nodes
{
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
	public class SvgRendererNode : TopControl, IPluginEvaluate, IUserInputWindow, IBackgroundColor
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

        [Input("Smoothing Mode", DefaultEnumEntry = "AntiAlias", Visibility = PinVisibility.OnlyInspector)]
        IDiffSpread<SmoothingMode> FSmoothingModeIn;

        [Input("Compositing Quality", DefaultEnumEntry = "HighQuality", Visibility = PinVisibility.OnlyInspector)]
        IDiffSpread<CompositingQuality> FCompositingQualityIn;

        [Input("Compositing Mode", DefaultEnumEntry = "SourceOver", Visibility = PinVisibility.OnlyInspector)]
        IDiffSpread<CompositingMode> FCompositingModeIn;

        [Input("Pixel Offset Mode", DefaultEnumEntry = "Half", Visibility = PinVisibility.OnlyInspector)]
        IDiffSpread<PixelOffsetMode> FPixelOffsetModeIn;

        [Input("Interpolation Mode", DefaultEnumEntry = "Default", Visibility = PinVisibility.OnlyInspector)]
        IDiffSpread<InterpolationMode> FInterpolationModeIn;

        [Input("Text Rendering Hint", DefaultEnumEntry = "AntiAlias", Visibility = PinVisibility.OnlyInspector)]
        IDiffSpread<TextRenderingHint> FTextRenderingHintIn;

        [Input("Text Contrast", DefaultValue = 1, Visibility = PinVisibility.OnlyInspector)]
        IDiffSpread<int> FTextContrastIn;

        [Output("Document")]
		ISpread<SvgDocument> FOutput;
		
		[Output("Size")]
		ISpread<Vector2> FSizeOutput;
		
		SvgDocument FSVGDoc = new SvgDocument();
		
		SizeF FSize = new SizeF();
		bool FResized = false;
		
		Bitmap FBitMap;
		PictureBox FPicBox = new PictureBox();
		
		[Import]
		INode FThisNode;
		
		[Import]
		ILogger FLogger;
		#pragma warning restore
		
		#endregion fields & pins
		
		[ImportingConstructor]
		public SvgRendererNode()
		{
			//clear controls in case init is called multiple times
			Controls.Clear();
			FPicBox.Dock = DockStyle.Fill;
			FPicBox.SizeMode = PictureBoxSizeMode.StretchImage;
			FPicBox.BackColor = Color.Black;
			Controls.Add(FPicBox);
			
			Resize += SvgRendererNode_Resize;
		}
		
		void SvgRendererNode_Resize(object sender, EventArgs e)
		{
			FResized = true;
		}
		
		void LogIDFix(SvgElement elem, string oldID, string newID)
		{
			var msg = "ID of " + elem + " was changed from " + oldID + " to " + newID;
			FLogger.Log(LogType.Warning, msg);
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
					if(elem != null) FSVGDoc.Children.AddAndForceUniqueID(elem, true, true, LogIDFix);
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
				
				FSVGDoc.SetVVVVBackgroundColor(FBackgroundIn[0]);
				FOutput[0] = FSVGDoc;
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

				//also set controls backcolor so it does not flash when going fullscreen
				if (FBackgroundIn.IsChanged)
					this.BackColor = FBackgroundIn[0].Color;

                Draw(FSVGDoc, FBitMap);

				FPicBox.Image = FBitMap;
			}
		}
		
		public IntPtr InputWindowHandle
		{
			get { return FPicBox.Handle; }
		}
		
		public RGBAColor BackgroundColor
		{
			get { return FBackgroundIn[0]; }
		}

        /// <summary>
        /// Renders the <see cref="SvgDocument"/> into a given Bitmap <see cref="Bitmap"/>.
        /// </summary>
        public virtual void Draw(SvgDocument doc, Bitmap bitmap)
        {
            //Trace.TraceInformation("Begin Render");

            using (var g = Graphics.FromImage(bitmap))
            {
                g.Clear(FBackgroundIn[0].Color);

                g.TextRenderingHint = FTextRenderingHintIn[0];
                g.TextContrast = FTextContrastIn[0];
                g.PixelOffsetMode = FPixelOffsetModeIn[0];
                g.CompositingQuality = FCompositingQualityIn[0];
                g.CompositingMode = FCompositingModeIn[0];
                g.SmoothingMode = FSmoothingModeIn[0];
                g.InterpolationMode = FInterpolationModeIn[0];

                doc.Draw(g);
            }

            //Trace.TraceInformation("End Render");
        }

    }
	
	#region PluginInfo
	[PluginInfo(Name = "AsDocument",
	            Category = "SVG",
	            Help = "Creates a SVG document for each element slice",
	            Tags = "xml")]
	#endregion PluginInfo
	public class SvgAsDocumentNode : IPluginEvaluate
	{
		#region fields & pins
		#pragma warning disable 649,169
		[Input("Layers")]
		IDiffSpread<SvgElement> FSVGIn;
		
		[Input("View Box")]
		IDiffSpread<SvgViewBox> FViewIn;
		
		[Input("Ignore View", Visibility = PinVisibility.OnlyInspector)]
		IDiffSpread<bool> FIgnoreView;
		
		[Input("Background Color", DefaultColor = new double[] { 0, 0, 0, 1 })]
		IDiffSpread<RGBAColor> FBackgroundIn;
		
		[Input("Size", StepSize = 1, DefaultValues = new double[] { 400, 300 })]
		IDiffSpread<Vector2> FSizeIn;
		
		[Output("Document")]
		ISpread<SvgDocument> FOutput;
		
		[Import]
		ILogger FLogger;
		#pragma warning restore
		
		#endregion fields & pins
		
		void LogIDFix(SvgElement elem, string oldID, string newID)
		{
			var msg = "ID of " + elem + " was changed from " + oldID + " to " + newID;
			FLogger.Log(LogType.Warning, msg);
		}
		
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			//update
			if (FSVGIn.IsChanged || FSizeIn.IsChanged || FBackgroundIn.IsChanged ||
			    FIgnoreView.IsChanged || FViewIn.IsChanged)
			{
				FOutput.SliceCount = SpreadMax;
				for(int i=0; i<SpreadMax; i++)
				{
					var doc = new SvgDocument();
					var elem = FSVGIn[i];

					if(elem != null)
						doc.Children.AddAndForceUniqueID(elem, true, true, LogIDFix);
					
					doc.Transforms = new SvgTransformCollection();
					
					//set view
					if(!FIgnoreView[i])
					{
						var view = FViewIn[i];
						
						if(view.Equals(SvgViewBox.Empty))
						{
							view =  new SvgViewBox(-1, -1, 2, 2);
						}
						
						doc.ViewBox = view;
					}
					
					//set size and output doc
					doc.Width = new SvgUnit(SvgUnitType.User, Math.Max(FSizeIn[i].X, 1));
					doc.Height = new SvgUnit(SvgUnitType.User, Math.Max(FSizeIn[i].Y, 1));
					
					doc.SetVVVVBackgroundColor(FBackgroundIn[i]);
					FOutput[i] = doc;
				}
			}
		}
	}
}
