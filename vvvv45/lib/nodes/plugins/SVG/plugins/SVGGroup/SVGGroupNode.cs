#region usings
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;
using VVVV.Utils.SlimDX;

using VVVV.Core.Logging;
using Svg;
using Svg.Transforms;
using SlimDX;
#endregion usings

namespace VVVV.Nodes
{	
	public enum StrokeMode
	{
		Color,
		None
	}

	#region Base Nodes
	public abstract class SVGVisualElementNode<T> : IPluginEvaluate where T : SvgVisualElement
	{
		#region fields & pins
		[Input("Transform", Order = 0)]
		protected IDiffSpread<Matrix> FTransformIn;
		
		[Input("Stroke", Order = 10, DefaultColor = new double[] { 0, 0, 0, 1 })]
		protected IDiffSpread<RGBAColor> FStrokeIn;
		
		[Input("Stroke Mode", Order = 11, Visibility = PinVisibility.OnlyInspector)]
		protected IDiffSpread<StrokeMode> FStrokeModeIn;
		
		[Input("Stroke Width", Order = 12)]
		protected IDiffSpread<float> FStrokeWidthIn;
		
		[Input("Enabled", Order = 30, DefaultValue = 1)]
		protected IDiffSpread<bool> FEnabledIn;
		
		[Output("Layer")]
		ISpread<T> FOutput;
		
		bool FFirstFrame = true;
		
		List<T> FElements = new List<T>();
		
		#endregion fields & pins
		
		public void Evaluate(int SpreadMax)
		{
			if (FFirstFrame) FElements.Add(CreateElement());
			
			SpreadMax = CalcSpreadMax(SpreadMax);
			
			FOutput.SliceCount = SpreadMax;
			
			//set slice count
            if (FElements.Count > SpreadMax)
            {
                FElements.RemoveRange(SpreadMax, FOutput.SliceCount - SpreadMax);
            }
            else if (FElements.Count < SpreadMax)
            {
                for (int i = FElements.Count; i < SpreadMax; i++)
                    FElements.Add(CreateElement());
            }
			
			if(PinsChanged())
			{
				for(int i=0; i<SpreadMax; i++)
				{
					var elem = FElements[i];
					SetTransform(elem, i);
					SetFill(elem, i);
					SetStroke(elem, i);
					elem.Visible = FEnabledIn[i];
					FOutput.AssignFrom(FElements);
				}
			}
			
			FFirstFrame = false;
		}
		
		//calc spread max, override in sub class
		protected virtual int CalcSpreadMax(int max)
		{
			return max;
		}
		
		//check if any pin is changed
		protected virtual bool PinsChanged()
		{
			return FTransformIn.IsChanged || FStrokeIn.IsChanged || FStrokeWidthIn.IsChanged
				|| FStrokeModeIn.IsChanged || FEnabledIn.IsChanged;
		}
		
		
		protected void SetTransform(T elem, int slice)
		{
			//decompose matrix
			Vector3 trans;
			Vector3 scale;
			Quaternion rot;
			FTransformIn[slice].Decompose(out scale, out rot, out trans);

			var rotvec = VMath.QuaternionToEulerYawPitchRoll(rot.ToVector4D());
			
			//add rotation
			elem.Transforms = new SvgTransformCollection();
			elem.Transforms.Add(new SvgTranslate(trans.X, trans.Y));
			elem.Transforms.Add(new SvgRotate((float)(rotvec.z*VMath.RadToDeg)));
			
			
			
			//calc geometry
			CalcGeometry(elem, new Vector2(trans.X, trans.Y), new Vector2(scale.X, scale.Y), slice);
		}
		
		//stroke color
		protected void SetStroke(T elem, int slice)
		{
			if(FStrokeModeIn[slice] != StrokeMode.None)
			{
				elem.Stroke = new SvgColourServer(GetRGB(FStrokeIn[slice]));
				elem.StrokeOpacity = (float)FStrokeIn[slice].A;
				elem.StrokeWidth = FStrokeWidthIn[slice];
			}
			else
			{
				elem.Stroke = null;
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
	
	public abstract class SVGVisualElementFillNode<T> : SVGVisualElementNode<T> where T : SvgVisualElement
	{
		#region fields & pins
		
		[Input("Fill", Order = 20, DefaultColor = new double[] { 1, 1, 1, 1 })]
		protected IDiffSpread<RGBAColor> FFillIn;
		
		[Input("Fill Mode", Order = 21, Visibility = PinVisibility.OnlyInspector)]
		protected IDiffSpread<SvgFillRule> FFillModeIn;
		
		#endregion fields & pins
		
		protected override bool PinsChanged()
		{
			return base.PinsChanged() || FFillIn.IsChanged || FFillModeIn.IsChanged;
		}
		
		protected override void SetFill(T elem, int index)
		{
			elem.Fill = new SvgColourServer(GetRGB(FFillIn[index]));
			elem.FillOpacity = (float)FFillIn[index].A;
			elem.FillRule = FFillModeIn[index];
		}
	}
	#endregion Base Nodes
	
	//QUAD----------------------------------------------------------------------
	
	#region PluginInfo
	[PluginInfo(Name = "Quad", Category = "SVG", Help = "Svg Quad", Tags = "")]
	#endregion PluginInfo
	public class SvgRectNode : SVGVisualElementFillNode<SvgRectangle>
	{
		[Input("Corner Radius ", Order = 22)]
		IDiffSpread<Vector2> FCornerRadiusIn;
		
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
			elem.Transforms.Add(new SvgTranslate(-scale.X * 0.5f, -scale.Y * 0.5f));
			elem.Width = (float)scale.X;
			elem.Height = (float)scale.Y;
			
			elem.CornerRadiusX = Math.Max(FCornerRadiusIn[slice].X, 0);
			elem.CornerRadiusY = Math.Max(FCornerRadiusIn[slice].Y, 0);
		}
	}
	
	//ELLIPSE-------------------------------------------------------------------
	
	#region PluginInfo
	[PluginInfo(Name = "Circle", Category = "SVG", Help = "Svg Ellipse", Tags = "Circle")]
	#endregion PluginInfo
	public class SvgEllipseNode : SVGVisualElementFillNode<SvgEllipse>
	{
		
		protected override SvgEllipse CreateElement()
		{
			return new SvgEllipse();
		}
		
		protected override void CalcGeometry(SvgEllipse elem, Vector2 trans, Vector2 scale, int slice)
		{
			elem.RadiusX = scale.X*0.5f;
			elem.RadiusY = scale.Y*0.5f;

		}
	}
	
	//POLYGON-------------------------------------------------------------------
	#region PluginInfo
	[PluginInfo(Name = "Polygon", Category = "SVG", Help = "Svg Polygon from a list or vertices", Tags = "")]
	#endregion PluginInfo
	public class SvgPolygonNode : SVGVisualElementFillNode<SvgPolygon>
	{
		[Input("Vertices ", Order = -1)]
		IDiffSpread<ISpread<Vector2>> FVerticesIn;
		
		protected override SvgPolygon CreateElement()
		{
			var p = new SvgPolygon();
			p.Points = new SvgUnitCollection();
			return p;
		}
		
		protected override int CalcSpreadMax(int max)
		{
			max = Math.Max(FTransformIn.SliceCount, FStrokeIn.SliceCount);
			max = Math.Max(max, FStrokeModeIn.SliceCount);
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
			elem.Points.Clear();
			foreach(var v in verts)
			{
				elem.Points.Add(v.X * scale.X);
				elem.Points.Add(v.Y * scale.Y);
			}
		}
	}
	
	//GROUP---------------------------------------------------------------------
	
	#region PluginInfo
	[PluginInfo(Name = "Group", Category = "SVG", Help = "Groups multiple SVG Layers into one", Tags = "")]
	#endregion PluginInfo
	public class SVGGroupNode : IPluginEvaluate
	{
		#region fields & pins
		[Input("Transform", IsSingle = true)]
		IDiffSpread<Matrix> FTransformIn;
		
		[Input("Input", IsPinGroup=true)]
		IDiffSpread<ISpread<SvgElement>> FInput;

		[Output("Layer")]
		ISpread<SvgElement> FOutput;

		[Import()]
		ILogger FLogger;
		
		List<SvgElement> FGroups = new List<SvgElement>();
		#endregion fields & pins
		
		public SVGGroupNode()
		{
			var g = new SvgGroup();
			FGroups.Add(g);
		}

		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
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
			if(FInput.IsChanged || FTransformIn.IsChanged)
			{
				foreach (var g in FGroups)
				{
					g.Children.Clear();
					for(int i=0; i<FInput.SliceCount; i++)
					{
						var pin = FInput[i];
						for(int j=0; j<pin.SliceCount; j++)
						{ 
							var elem = pin[j];
							if(elem != null)
								g.Children.Add(elem);
						}
					}
					
				}
				//write groups to output
				FOutput.AssignFrom(FGroups);
			}
		}
	}
}
