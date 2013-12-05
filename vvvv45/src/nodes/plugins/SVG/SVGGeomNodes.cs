#region usings
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text.RegularExpressions;

using SlimDX;
using Svg;
using Svg.Transforms;
using VVVV.Core.Logging;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.SlimDX;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

#endregion usings

namespace VVVV.Nodes
{	

	#region Base Nodes
	public abstract class SVGVisualElementNode<T> : IPluginEvaluate where T : SvgElement
	{
		#region fields & pins
		#pragma warning disable 649
		[Input("ID", Order = -2, Visibility = PinVisibility.OnlyInspector)]
		protected IDiffSpread<string> FIDIn;
		
		[Input("Class", Order = -1, Visibility = PinVisibility.OnlyInspector)]
		protected IDiffSpread<string> FClassIn;
		
		[Input("Transform", Order = 0)]
		protected IDiffSpread<SlimDX.Matrix> FTransformIn;
		
		[Input("Enabled", Order = 1000000, DefaultValue = 1)]
		protected IDiffSpread<bool> FEnabledIn;
		
		[Output("Layer")]
		protected Pin<T> FOutput;
		
		[Import]
		protected ILogger FLogger;
		#pragma warning restore
		#endregion fields & pins
		
		void LogIDFix(SvgElement elem, string oldID, string newID)
		{
			var msg = "ID of " + elem + " was changed from " + oldID + " to " + newID;
			FLogger.Log(LogType.Warning, msg);
		}
		
		public abstract void Evaluate(int SpreadMax);
		
		protected void SetIDs()
		{
			//assumes that base nodes have calculated the output spread
			//and call this at the end of their Evaluate
			for (int i = 0; i < FOutput.SliceCount; i++)
			{
				var elem = FOutput[i];
				
				//set class
				if(!string.IsNullOrWhiteSpace(FClassIn[i]))
				{
					elem.CustomAttributes["class"] = FClassIn[i];
				}
				
				//set id
				var idToSet = FIDIn[i];
				if(!string.IsNullOrWhiteSpace(idToSet))
				{
					elem.SetAndFixID(idToSet, true, LogIDFix);
				}
			}
			
			//out pin changed hack
			if(FIDIn.IsChanged || FClassIn.IsChanged)
				FOutput[0] = FOutput[0];
		}
	}
	
	
	public abstract class SVGVisualElementNodeStroke<T> : SVGVisualElementNode<T> where T : SvgVisualElement
	{
		#region fields & pins
		#pragma warning disable 649
		
		[Input("Stroke", Order = 20, DefaultColor = new double[] { 0, 0, 0, 1 })]
		protected IDiffSpread<RGBAColor> FStrokeIn;
		
		[Input("Stroke Width", DefaultValue = 0.1, Order = 22, MinValue = 0)]
		protected IDiffSpread<float> FStrokeWidthIn;
		
		#pragma warning restore
		
		bool FFirstFrame = true;
		
		List<T> FElements = new List<T>();
		
		#endregion fields & pins
		
		public sealed override void Evaluate(int SpreadMax)
		{
			if (FFirstFrame) FElements.Add(CreateElement());
			
			SpreadMax = CalcSpreadMax(SpreadMax);
			
			FOutput.SliceCount = SpreadMax;
			
			//set slice count
            if (FElements.Count > SpreadMax)
            {
                FElements.RemoveRange(SpreadMax, FElements.Count - SpreadMax);
            }
            else if (FElements.Count < SpreadMax)
            {
                for (int i = FElements.Count; i < SpreadMax; i++)
                    FElements.Add(CreateElement());
            }
            
            var pinsChanged = PinsChanged();
			if(pinsChanged)
			{
				for(int i=0; i<SpreadMax; i++)
				{
					var elem = FElements[i];
					SetTransform(elem, i);
					SetFill(elem, i);
					SetStroke(elem, i);
					elem.Visible = FEnabledIn[i];
					FOutput[i] = elem;
				}
			}
			
			FFirstFrame = false;
			
			//set id and class to elements
			if(pinsChanged || FIDIn.IsChanged || FClassIn.IsChanged)
				base.SetIDs();
		}
		
		//calc spread max, override in sub class
		protected virtual int CalcSpreadMax(int max)
		{
			return max;
		}
		
		//check if any pin is changed
		protected virtual bool PinsChanged()
		{
			return FTransformIn.IsChanged || FStrokeIn.IsChanged || FStrokeWidthIn.IsChanged || FEnabledIn.IsChanged;
		}
		
		protected void SetTransform(T elem, int slice)
		{
			//decompose matrix
			Vector2 trans;
			Vector2D scale;
			double rotate;
				
			var m = FTransformIn[slice];
			trans.X = m.M41;
			trans.Y = m.M42;
			
			var m2d = new Matrix2x2(m.ToMatrix4x4());
			m2d.Decompose(out scale, out rotate);
			
			//add rotation
			elem.Transforms = new SvgTransformCollection();
			elem.Transforms.Add(new SvgTranslate(trans.X, trans.Y));
			elem.Transforms.Add(new SvgRotate((float)(rotate*VMath.RadToDeg)));
			
			//calc geometry
			CalcGeometry(elem, trans, scale.ToSlimDXVector(), slice);
		}
		
		//stroke color
		protected void SetStroke(T elem, int slice)
		{
			if(FStrokeIn[slice].A != 0)
			{
				elem.Stroke = new SvgColourServer(GetRGB(FStrokeIn[slice]));
				elem.StrokeOpacity = (float)FStrokeIn[slice].A;
				elem.StrokeWidth = FStrokeWidthIn[slice];
			}
			else
			{
				elem.Stroke = null;
				elem.StrokeOpacity = 0;
			}
		}
		
		//set fill if needed
		protected virtual void SetFill(T elem, int slice)
		{
		}
		
		//create element
		protected abstract T CreateElement();
		
		//calc geometry
		protected abstract void CalcGeometry(T elem, Vector2 trans, Vector2 scale, int slice);
		
		//get rgb c# color with full alpha
		protected static Color GetRGB(RGBAColor c)
		{
			return Color.FromArgb(255, (int)(c.R*255), (int)(c.G*255), (int)(c.B*255));
		}
	}
	
	//FILL----------------------------------------------------------------------
	
	public abstract class SVGVisualElementFillNode<T> : SVGVisualElementNodeStroke<T> where T : SvgVisualElement
	{
		#region fields & pins
		
		[Input("Fill", Order = 10, DefaultColor = new double[] { 1, 1, 1, 1 })]
		protected IDiffSpread<RGBAColor> FFillIn;
		
		[Input("Fill Mode", Order = 12, Visibility = PinVisibility.OnlyInspector)]
		protected IDiffSpread<SvgFillRule> FFillModeIn;
		
		#endregion fields & pins
		
		protected override bool PinsChanged()
		{
			return base.PinsChanged() || FFillIn.IsChanged || FFillModeIn.IsChanged;
		}
		
		protected override void SetFill(T elem, int index)
		{
			elem.FillRule = FFillModeIn[index];
			if(FFillIn[index].A != 0)
			{
				elem.Fill = new SvgColourServer(GetRGB(FFillIn[index]));
				elem.FillOpacity = (float)FFillIn[index].A;
			}
			else
			{
				elem.Fill = null;
				elem.FillOpacity = 0;
			}
		}
	}
	
	public abstract class SVGVisualElementFillXYWH<T> : SVGVisualElementFillNode<T> where T : SvgVisualElement
	{
		#region fields & pins
		
		[Input("Position", Order = 6, Visibility = PinVisibility.OnlyInspector)]
		protected IDiffSpread<Vector2> FPositionIn;
		
		[Input("Size", DefaultValues = new double[] { 1, 1 }, Order = 8, Visibility = PinVisibility.OnlyInspector)]
		protected IDiffSpread<Vector2> FSizeIn;
		
		#endregion fields & pins
		
		protected override bool PinsChanged()
		{
			return base.PinsChanged() || FPositionIn.IsChanged || FSizeIn.IsChanged;
		}
	}
	
	#endregion Base Nodes
	
	//QUAD----------------------------------------------------------------------
	#region PluginInfo
	[PluginInfo(Name = "Quad", 
	            Category = "SVG", 
	            Help = "Renders a rectangle into a Renderer (SVG)", 
	            Tags = "rectangle, square, primitive, 2d, vector")]
	#endregion PluginInfo
	public class SvgRectNode : SVGVisualElementFillXYWH<SvgRectangle>
	{
	    #pragma warning disable 649
		[Input("Corner Radius ", Order = 23, MinValue = 0, MaxValue = 1)]
		IDiffSpread<Vector2> FCornerRadiusIn;
		#pragma warning restore
		
		protected override SvgRectangle CreateElement()
		{
			return new SvgRectangle();
		}
		
		protected override bool PinsChanged()
		{
			return base.PinsChanged() || FCornerRadiusIn.IsChanged;
		}
		
		protected override void CalcGeometry(SvgRectangle elem, Vector2 trans, Vector2 scale, int slice)
		{
			
			var pos = FPositionIn[slice];
			var size = FSizeIn[slice];
			elem.X = pos.X;
			elem.Y = pos.Y;
			elem.Width = scale.X * size.X;
			elem.Height = scale.Y * size.Y;
			
			elem.Transforms.Add(new SvgTranslate(-elem.Width * 0.5f, -elem.Height * 0.5f));
			
			elem.CornerRadiusX = Math.Max(FCornerRadiusIn[slice].X * elem.Width * 0.5f, 0);
			elem.CornerRadiusY = Math.Max(FCornerRadiusIn[slice].Y * elem.Height * 0.5f, 0);
		}
	}
	
	//ELLIPSE-------------------------------------------------------------------
	#region PluginInfo
	[PluginInfo(Name = "Circle", 
	            Category = "SVG", 
	            Help = "Renders an ellipse into a Renderer (SVG)", 
	            Tags = "ellipse, primitive, 2d, vector")]
	#endregion PluginInfo
	public class SvgEllipseNode : SVGVisualElementFillXYWH<SvgEllipse>
	{
		protected override SvgEllipse CreateElement()
		{
			return new SvgEllipse();
		}
		
		protected override void CalcGeometry(SvgEllipse elem, Vector2 trans, Vector2 scale, int slice)
		{
			var pos = FPositionIn[slice];
			var size = FSizeIn[slice];
			elem.CenterX = pos.X;
			elem.CenterY = pos.Y;
			elem.RadiusX = scale.X*0.5f*size.X;
			elem.RadiusY = scale.Y*0.5f*size.Y;
		}
	}
	
	//PATH------------------------------------------------------------------
	#region PluginInfo
	[PluginInfo(Name = "Path", 
	            Category = "SVG", 
	            Help = "Renders a path from a list of vertices into a Renderer (SVG)", 
	            Tags = "primitive, 2d, vector")]
	#endregion PluginInfo
	public class SvgPathNode : SVGVisualElementFillNode<SvgPath>
	{
	    #pragma warning disable 649
		[Input("Vertices", Order = -50)]
		IDiffSpread<ISpread<Vector2>> FVerticesIn;
		
		[Input("Control 1", Order = -40)]
		IDiffSpread<ISpread<Vector2>> FControl1In;
		
		[Input("Control 2", Order = -30)]
		IDiffSpread<ISpread<Vector2>> FControl2In;
		
		[Input("Arc Rotation", Order = -20)]
		IDiffSpread<ISpread<float>> FArcRotationIn;
		
		[Input("Command", Order = -10, MaxChars = 1, DefaultString = "L")]
		IDiffSpread<ISpread<string>> FCommandIn;
		
		[Input("Closed Path", Order = -15, DefaultValue = 1)]
		IDiffSpread<bool> FClosedIn;
		#pragma warning restore
		
		protected override SvgPath CreateElement()
		{
			var p = new SvgPath();
			return p;
		}
		
		protected override int CalcSpreadMax(int max)
		{
			max = Math.Max(FTransformIn.SliceCount, FStrokeIn.SliceCount);
			max = Math.Max(max, FStrokeWidthIn.SliceCount);
			max = Math.Max(max, FEnabledIn.SliceCount);
			max = Math.Max(max, FFillIn.SliceCount);
			max = Math.Max(max, FFillModeIn.SliceCount);
			max = Math.Max(max, FVerticesIn.SliceCount);
			max = Math.Max(max, FControl1In.SliceCount);
			max = Math.Max(max, FControl2In.SliceCount);
			max = Math.Max(max, FArcRotationIn.SliceCount);
			max = Math.Max(max, FCommandIn.SliceCount);
			max = Math.Max(max, FClosedIn.SliceCount);
			return max;
		}
		
		protected override bool PinsChanged()
		{
			return base.PinsChanged() || FVerticesIn.IsChanged || FCommandIn.IsChanged || FControl1In.IsChanged || FControl2In.IsChanged || FArcRotationIn.IsChanged || FClosedIn.IsChanged;
		}
		
		protected override void CalcGeometry(SvgPath elem, Vector2 trans, Vector2 scale, int slice)
		{
			elem.PathData.Clear();
			
			var verts = FVerticesIn[slice];
			var cont1 = FControl1In[slice];
			var cont2 = FControl2In[slice];
			var comms = FCommandIn[slice];
			var arc = FArcRotationIn[slice];
			
			var coords = new List<float>(7);
			
			for (int i = 0; i < verts.SliceCount; i++)
			{
				coords.Clear();
				
				var c = comms[i][0];
				
				//make sure the first and last command fits the specification
				if(i == 0)
				{
					c = 'M';
				}
				
				//fill in params
				switch (c)
				{
					case 'm': // relative moveto
					case 'M': // moveto
						
						coords.Add(verts[i].X);
						coords.Add(verts[i].Y);
						break;
					case 'a':
					case 'A':

						coords.Add(cont1[i].X);
						coords.Add(cont1[i].Y);
						
						coords.Add(arc[i] * 360);
						
						coords.Add(cont2[i].X);
						coords.Add(cont2[i].Y);
						
						coords.Add(verts[i].X);
						coords.Add(verts[i].Y);
						
						break;
					case 'l': // relative lineto
					case 'L': // lineto
						
						coords.Add(verts[i].X);
						coords.Add(verts[i].Y);
						break;
					case 'H': // horizontal lineto
					case 'h': // relative horizontal lineto
						
						coords.Add(verts[i].X);
						break;
					case 'V': // vertical lineto
					case 'v': // relative vertical lineto
						
						coords.Add(verts[i].Y);
						break;
					case 'Q': // curveto
					case 'q': // relative curveto
						
						coords.Add(cont1[i].X);
						coords.Add(cont1[i].Y);
						
						coords.Add(verts[i].X);
						coords.Add(verts[i].Y);
						break;
					case 'T': // shorthand/smooth curveto
					case 't': // relative shorthand/smooth curveto
						
						coords.Add(verts[i].X);
						coords.Add(verts[i].Y);
						break;
					case 'C': // curveto
					case 'c': // relative curveto
						
						coords.Add(cont1[i].X);
						coords.Add(cont1[i].Y);
						
						coords.Add(cont2[i].X);
						coords.Add(cont2[i].Y);
						
						coords.Add(verts[i].X);
						coords.Add(verts[i].Y);

						break;
					case 'S': // shorthand/smooth curveto
					case 's': // relative shorthand/smooth curveto

						coords.Add(cont1[i].X);
						coords.Add(cont1[i].Y);
						
						coords.Add(verts[i].X);
						coords.Add(verts[i].Y);
						break;
				}
				
				SvgPathBuilder.CreatePathSegment(c, elem.PathData, coords, char.IsLower(c));
				
				if (i == verts.SliceCount - 1 && FClosedIn[i])
				{
					c = 'Z';
					SvgPathBuilder.CreatePathSegment(c, elem.PathData, coords, char.IsLower(c));
				}
			}
			
		}
		

	}
	
	//POLYLINE------------------------------------------------------------------
	#region PluginInfo
	[PluginInfo(Name = "Polyline", 
	            Category = "SVG", 
	            Help = "Renders an open polyline from a list of vertices into a Renderer (SVG)", 
	            Tags = "primitive, 2d, vector")]
	#endregion PluginInfo
	public class SvgPolylineNode : SVGVisualElementFillNode<SvgPolyline>
	{
	    #pragma warning disable 649
		[Input("Vertices", Order = -1)]
		IDiffSpread<ISpread<Vector2>> FVerticesIn;
		#pragma warning restore
		
		protected override SvgPolyline CreateElement()
		{
			var p = new SvgPolyline();
			p.Points = new SvgUnitCollection();
			return p;
		}
		
		protected override int CalcSpreadMax(int max)
		{
			max = Math.Max(FTransformIn.SliceCount, FStrokeIn.SliceCount);
			max = Math.Max(max, FStrokeWidthIn.SliceCount);
			max = Math.Max(max, FEnabledIn.SliceCount);
			max = Math.Max(max, FFillIn.SliceCount);
			max = Math.Max(max, FFillModeIn.SliceCount);
			max = Math.Max(max, FVerticesIn.SliceCount);
			return max;
		}
		
		protected override bool PinsChanged()
		{
			return base.PinsChanged() || FVerticesIn.IsChanged;
		}
		
		protected override void CalcGeometry(SvgPolyline elem, Vector2 trans, Vector2 scale, int slice)
		{
			var verts = FVerticesIn[slice];
			elem.Points = new SvgUnitCollection();
			foreach(var v in verts)
			{
				elem.Points.Add(v.X * scale.X);
				elem.Points.Add(v.Y * scale.Y);
			}
		}
	}
	
	//POLYGON-------------------------------------------------------------------
	#region PluginInfo
	[PluginInfo(Name = "Polygon", 
	            Category = "SVG", 
	            Help = "Renders a closed polygon from a list of vertices into a Renderer (SVG)", 
	            Tags = "primitive, 2d, vector")]
	#endregion PluginInfo
	public class SvgPolygonNode : SVGVisualElementFillNode<SvgPolygon>
	{
	    #pragma warning disable 649
		[Input("Vertices", Order = -1)]
		IDiffSpread<ISpread<Vector2>> FVerticesIn;
		#pragma warning restore
		
		protected override SvgPolygon CreateElement()
		{
			var p = new SvgPolygon();
			p.Points = new SvgUnitCollection();
			return p;
		}
		
		protected override int CalcSpreadMax(int max)
		{
			max = Math.Max(FTransformIn.SliceCount, FStrokeIn.SliceCount);
			max = Math.Max(max, FStrokeWidthIn.SliceCount);
			max = Math.Max(max, FEnabledIn.SliceCount);
			max = Math.Max(max, FFillIn.SliceCount);
			max = Math.Max(max, FFillModeIn.SliceCount);
			max = Math.Max(max, FVerticesIn.SliceCount);
			return max;
		}
		
		protected override bool PinsChanged()
		{
			return base.PinsChanged() || FVerticesIn.IsChanged;
		}
		
		protected override void CalcGeometry(SvgPolygon elem, Vector2 trans, Vector2 scale, int slice)
		{
			var verts = FVerticesIn[slice];
			elem.Points = new SvgUnitCollection();
			foreach(var v in verts)
			{
				elem.Points.Add(v.X * scale.X);
				elem.Points.Add(v.Y * scale.Y);
			}
		}
	}
	
	//TEXT----------------------------------------------------------------------
	#region PluginInfo
	[PluginInfo(Name = "Text", 
	            Category = "SVG", 
	            Help = "Renders text into a Renderer (SVG)", 
	            Tags = "primitive, 2d, vector")]
	#endregion PluginInfo
	public class SvgTextNode : SVGVisualElementFillNode<SvgText>
	{
	    #pragma warning disable 649
		[Input("Text", Order = 1, DefaultString = "vvvv")]
		IDiffSpread<string> FTextIn;
		
		[Input("Font", EnumName = "SystemFonts", Order = 2)]
        IDiffSpread<EnumEntry> FFontIn;
		
		[Input("Size", DefaultValue = 1, Order = 3)]
		IDiffSpread<float> FTextSizeIn;
		
		[Input("Anchor", Order = 4)]
		IDiffSpread<SvgTextAnchor> FTextAnchorIn;
		#pragma warning restore
		
		protected override SvgText CreateElement()
		{
			return new SvgText();
		}
		
		protected override bool PinsChanged()
		{
			return base.PinsChanged() || FTextIn.IsChanged || FTextSizeIn.IsChanged || FFontIn.IsChanged || FTextAnchorIn.IsChanged;
		}
		
		protected override void CalcGeometry(SvgText elem, Vector2 trans, Vector2 scale, int slice)
		{
			elem.Text = FTextIn[slice];
			elem.FontSize = FTextSizeIn[slice];
			elem.TextAnchor = FTextAnchorIn[slice];
			try
			{
				elem.FontFamily = (new Font(FFontIn[slice].Name, 1)).FontFamily.Name;
				
			}
			catch (Exception)
			{
				elem.FontFamily = (new Font("Arial", 1)).FontFamily.Name;
			}
			
			elem.Transforms.Add(new SvgScale(scale.X, scale.Y));	
		}
	}
	
	//VIEWBOX---------------------------------------------------------------------------------------
	#region PluginInfo
	[PluginInfo(Name = "Camera", 
	            Category = "SVG",
	            Version = "Join",
	            Help = "Sets the visible rectangle of an SVG scene", 
	            Tags = "viewbox")]
	#endregion PluginInfo
	public class SVGCameraNode : IPluginEvaluate
	{
		#region fields & pins
		#pragma warning disable 649, 169
		[Input("Center")]
		IDiffSpread<Vector2> FViewCenterIn;
		
		[Input("Size", DefaultValues = new double[] {2, 2})]
		IDiffSpread<Vector2> FViewSizeIn;

		[Output("View Box")]
		ISpread<SvgViewBox> FOutput;

		[Import()]
		ILogger FLogger;
		#pragma warning restore
		
		#endregion fields & pins

		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{	
			if(FViewCenterIn.IsChanged || FViewSizeIn.IsChanged)
			{
				FOutput.SliceCount = SpreadMax;
				
				//create views
				for(int i=0; i<SpreadMax; i++)
				{
					FOutput[i] = new SvgViewBox(FViewCenterIn[i].X - FViewSizeIn[i].X * 0.5f,
					                            FViewCenterIn[i].Y - FViewSizeIn[i].Y * 0.5f,
					                            FViewSizeIn[i].X,
					                            FViewSizeIn[i].Y);
				}
			}
		}
	}
	
	#region PluginInfo
	[PluginInfo(Name = "Camera", 
	            Category = "SVG",
	            Version = "Split",
	            Help = "Returns the values of the viewbox",
	            Tags = "viewbox")]
	#endregion PluginInfo
	public class SVGCameraSplitNode : IPluginEvaluate
	{
		#region fields & pins
		#pragma warning disable 649, 169
		[Input("View Box")]
		IDiffSpread<SvgViewBox> FInput;
		
		[Output("Center")]
		ISpread<Vector2> FViewCenterOut;
		
		[Output("Size")]
		ISpread<Vector2> FViewSizeOut;

		[Import()]
		ILogger FLogger;
		#pragma warning restore
		
		#endregion fields & pins

		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{	
			if(FInput.IsChanged)
			{
				FViewCenterOut.SliceCount = SpreadMax;
				FViewSizeOut.SliceCount = SpreadMax;
				
				//get view values
				for(int i=0; i<SpreadMax; i++)
				{
					
					FViewSizeOut[i] = new Vector2(FInput[i].Width, FInput[i].Height);
					FViewCenterOut[i] = new Vector2(FInput[i].MinX + FInput[i].Width * 0.5f,
					                                FInput[i].MinY + FInput[i].Height * 0.5f);
					
				}
			}
		}
	}
	
	//GETPATH-------------------------------------------------------------------
	#region PluginInfo
	[PluginInfo(Name = "GetPath", 
	            Category = "SVG", 
	            Help = "Returns the path data from SVG layers", 
	            Tags = "")]
	#endregion PluginInfo
	public class SVGGetPathNode : IPluginEvaluate
	{
	    #pragma warning disable 649
		[Input("Layer")]
		ISpread<SvgElement> FInput;
		
		[Input("Flatten", DefaultValue = 1)]
		ISpread<bool> FFlattenInput;
		
		[Input("Max Flatten Error", DefaultValue = 0.25)]
		ISpread<float> FMaxFlattenInput;
		
		[Input("Update", IsBang = true, IsSingle = true)]
		ISpread<bool> FUpdateInput;
		
		[Output("Path")]
		ISpread<ISpread<Vector2D>> FPathOutput;
		
		[Output("Path Type")]
		ISpread<ISpread<int>> FPathTypeOutput;
		#pragma warning restore
		
		public void Evaluate(int SpreadMax)
		{
			if(FUpdateInput[0])
			{
				FPathOutput.SliceCount = SpreadMax;
				FPathTypeOutput.SliceCount = SpreadMax;
				
				for(int i=0; i<SpreadMax; i++)
				{
					
					var elem = FInput[i];
					var po = FPathOutput[i];
					var pto = FPathTypeOutput[i];
					
					if(elem is SvgVisualElement || elem is SvgFragment)
					{
						GraphicsPath p;
						if(elem is SvgGroup) p = ((SvgGroup)elem).Path;
						else if(elem is SvgVisualElement) p = (GraphicsPath)((SvgVisualElement)elem).Path.Clone();
						else p = ((SvgFragment)elem).Path;
						
						if(FFlattenInput[i])
						{
							p.Flatten(new System.Drawing.Drawing2D.Matrix(), FMaxFlattenInput[i] * 0.1f);
						}
							          
						po.SliceCount = p.PointCount;
						pto.SliceCount = p.PointCount;
						
						for(int j=0; j<p.PointCount; j++)
						{
							po[j] = new Vector2D(p.PathPoints[j].X, p.PathPoints[j].Y);
							pto[j] = p.PathTypes[j];
						}
					}
					else
					{
						po.SliceCount = 0;
						pto.SliceCount = 0;
					}
				}
			}
		}
	}
	
	//GETELEMENTS-------------------------------------------------------------------
	#region PluginInfo
	[PluginInfo(Name = "GetElements", 
	            Category = "SVG", 
	            Help = "Returns all elements in the SVG tree as a flat spread", 
	            Tags = "")]
	#endregion PluginInfo
	public class SVGGetElementsNode : IPluginEvaluate
	{
	    #pragma warning disable 649
		[Input("Layer")]
		IDiffSpread<SvgElement> FInput;
		
		[Output("Element")]
		ISpread<SvgElement> FElementsOut;
		
		[Output("ID")]
		ISpread<string> FElementIDOut;
		
		[Output("Class")]
		ISpread<string> FElementClassOut;
		
		[Output("Type")]
		ISpread<string> FElementTypeOut;
		
		[Output("Level")]
		ISpread<int> FElementLevelOut;
		#pragma warning restore
		
		public void Evaluate(int SpreadMax)
		{
			if(FInput.IsChanged)
			{
				FElementsOut.SliceCount = 0;
				FElementTypeOut.SliceCount = 0;
				FElementLevelOut.SliceCount = 0;
				FElementIDOut.SliceCount = 0;
				FElementClassOut.SliceCount = 0;
				
				for(int i=0; i<SpreadMax; i++)
				{
					if(FInput[i] != null)
						FillRecursive(FInput[i], 0);
				}
			}
		}
		
		//fill the pins recursive with data
		private void FillRecursive(SvgElement elem, int level)
		{
			FElementsOut.Add(elem);
			FElementTypeOut.Add(elem.GetType().Name.Replace("Svg", ""));
			FElementLevelOut.Add(level);
			FElementIDOut.Add(elem.ID);
			var className = "";
			if(elem.CustomAttributes.ContainsKey("class"))
				className = elem.CustomAttributes["class"];
			FElementClassOut.Add(className);
			
			foreach(var child in elem.Children)
			{
				FillRecursive(child, level + 1);
			}
			
		}
	}

	//NORMALIZE---------------------------------------------------------------------

	public enum SvgNormalizeMode
	{
		None,
		Width,
		Height,
		Both
	}
	
	#region PluginInfo
	[PluginInfo(Name = "Normalize", 
	            Category = "SVG", 
	            Help = "Takes a layer and transforms it into unit space", 
	            Tags = "")]
	#endregion PluginInfo
	public class SVGNormalizeNode : IPluginEvaluate
	{
		#region fields & pins
		#pragma warning disable 649,169
		[Input("Transform")]
		IDiffSpread<SlimDX.Matrix> FTransformIn;
		
		[Input("Input")]
		IDiffSpread<SvgElement> FInput;
		
		[Input("Mode", DefaultEnumEntry = "Both")]
		IDiffSpread<SvgNormalizeMode> FModeIn;

		[Output("Layer")]
		ISpread<SvgElement> FOutput;

		[Import()]
		ILogger FLogger;
		#pragma warning restore
		
		List<SvgGroup> FGroups = new List<SvgGroup>();
		#endregion fields & pins
		
		public SVGNormalizeNode()
		{
			var g = new SvgGroup();
			FGroups.Add(g);
		}
		
		void LogIDFix(SvgElement elem, string oldID, string newID)
		{
			var msg = "ID of " + elem + " was changed from " + oldID + " to " + newID;
			FLogger.Log(LogType.Warning, msg);
		}

		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			//add all elements to each group
			if(FInput.IsChanged || FTransformIn.IsChanged || FModeIn.IsChanged)
			{
				//assign size and clear group list
				FOutput.SliceCount = FTransformIn.SliceCount;
				FGroups.Clear();
				
				//create groups and add matrix to it
				for(int i=0; i<FTransformIn.SliceCount; i++)
				{
					var g = new SvgGroup();
					g.Transforms = new SvgTransformCollection();
					
					var m = FTransformIn[i];
					var mat = new SvgMatrix(new List<float>(){m.M11, m.M12, m.M21, m.M22, m.M41, m.M42});
					
					g.Children.Clear();
					
					for(int j=0; j<FInput.SliceCount; j++)
					{
						var elem = FInput[j];
						if(elem != null)
							g.Children.AddAndFixID(elem, true, true, LogIDFix);
					}
					
					var b = FModeIn[i] == SvgNormalizeMode.None ? new RectangleF() : g.Path.GetBounds();
					
					switch (FModeIn[i])
					{
						case SvgNormalizeMode.Both:
							
							if (b.Height > 0 && b.Width > 0)
							{
								var sx = 1/b.Width;
								var sy = 1/b.Height;
								var ox = -b.X * sx - 0.5f;
								var oy = -b.Y * sy - 0.5f;
								
								g.Transforms.Add(new SvgMatrix(new List<float>(){sx, 0, 0, sy, ox, oy}));
							}
							break;
							
						case SvgNormalizeMode.Width:
							
							if (b.Width > 0)
							{
								var sx = 1/b.Width;
								var ox = -b.X * sx - 0.5f;
								
								g.Transforms.Add(new SvgMatrix(new List<float>(){sx, 0, 0, 1, ox, 0}));
							}
							break;
							
						case SvgNormalizeMode.Height:
							
							if (b.Height > 0)
							{
								var sy = 1/b.Height;
								var oy = -b.Y * sy - 0.5f;
								
								g.Transforms.Add(new SvgMatrix(new List<float>(){1, 0, 0, sy, 0, oy}));
							}
							break;
							
						default:
							break;
					}
					
					g.Transforms.Add(mat);
					
					//add to group list
					FGroups.Add(g);
				}
				
				//write groups to output
				FOutput.AssignFrom(FGroups);
			}
		}
	}
	
	//GROUP---------------------------------------------------------------------
	#region PluginInfo
	[PluginInfo(Name = "Group", 
	            Category = "SVG", 
	            Help = "Groups multiple SVG layers to be rendered one after the other", 
	            Tags = "")]
	#endregion PluginInfo
	public class SVGGroupNode : SVGVisualElementNode<SvgElement>
	{
		#region fields & pins
		#pragma warning disable 649,169
		
		[Input("Layer", IsPinGroup=true)]
		IDiffSpread<ISpread<SvgElement>> FInput;

		#pragma warning restore
		
		List<SvgElement> FGroups = new List<SvgElement>();
		#endregion fields & pins
		
		public SVGGroupNode()
		{
			var g = new SvgGroup();
			FGroups.Add(g);
		}
		
		void LogIDFix(SvgElement elem, string oldID, string newID)
		{
			var msg = "ID of " + elem + " was changed from " + oldID + " to " + newID;
			FLogger.Log(LogType.Warning, msg);
		}

		//called when data for any output pin is requested
		public override void Evaluate(int SpreadMax)
		{
			//check transforms
			if(FTransformIn.IsChanged)
			{
				//assign size and clear group list
				FOutput.SliceCount = FTransformIn.SliceCount;
				FGroups.Clear();
				
				//create groups and add matrix to it
				for(int i=0; i<FTransformIn.SliceCount; i++)
				{
					var g = new SvgGroup();
					g.Transforms = new SvgTransformCollection();
					
					var m = FTransformIn[i];
					var mat = new SvgMatrix(new List<float>(){m.M11, m.M12, m.M21, m.M22, m.M41, m.M42});
					
					g.Transforms.Add(mat);	
					
					FGroups.Add(g);
				}
			}
			
			//add all elements to each group
			var pinsChanged = FInput.IsChanged || FTransformIn.IsChanged || FEnabledIn.IsChanged;
			if(pinsChanged)
			{
				foreach (var g in FGroups)
				{
					g.Children.Clear();
					
					if(FEnabledIn[0])
					{
						for(int i=0; i<FInput.SliceCount; i++)
						{
							var pin = FInput[i];
							for(int j=0; j<pin.SliceCount; j++)
							{
								var elem = pin[j];
								if(elem != null)
									g.Children.AddAndFixID(elem, true, true, LogIDFix);
							}
						}
					}
					
				}
				//write groups to output
				FOutput.AssignFrom(FGroups);
			}
			
			//set id and class to elements
			if(pinsChanged || FIDIn.IsChanged || FClassIn.IsChanged)
				base.SetIDs();

		}
	}
}
