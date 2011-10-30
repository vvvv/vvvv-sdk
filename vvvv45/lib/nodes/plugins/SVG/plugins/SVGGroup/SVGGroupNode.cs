#region usings
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

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

	public abstract class SVGVisualElementNode<T> : IPluginEvaluate where T : SvgVisualElement
	{
		#region fields & pins
		[Input("Transform", Order = 0)]
		IDiffSpread<Matrix> FTransformIn;
		
		[Input("Stroke", Order = 10, DefaultColor = new double[] { 0, 0, 0, 1 })]
		IDiffSpread<RGBAColor> FStrokeIn;
		
		[Input("Stroke Mode", Order = 11, Visibility = PinVisibility.OnlyInspector)]
		IDiffSpread<StrokeMode> FStrokeModeIn;
		
		[Input("Stroke Width", Order = 12)]
		IDiffSpread<float> FStrokeWidthIn;
		
		[Input("Enabled", Order = 30, DefaultValue = 1)]
		IDiffSpread<bool> FEnabledIn;
		
		[Output("Layer")]
		ISpread<SvgVisualElement> FOutput;
		
		bool FFirstFrame = true;
		
		List<SvgVisualElement> FElements = new List<SvgVisualElement>();
		
		#endregion fields & pins
		
		public void Evaluate(int SpreadMax)
		{
			if (FFirstFrame) FElements.Add(CreateElement());
			FOutput.SliceCount = SpreadMax;
			
			//set slice count
            if (FElements.Count > SpreadMax)
            {
                FElements.RemoveRange(SpreadMax, FOutput.SliceCount - SpreadMax);
            }
            else if (FOutput.SliceCount < SpreadMax)
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
		
		protected virtual bool PinsChanged()
		{
			return FTransformIn.IsChanged || FStrokeIn.IsChanged || FStrokeWidthIn.IsChanged
				|| FStrokeModeIn.IsChanged || FEnabledIn.IsChanged;
		}
		
		protected void SetTransform(SvgVisualElement elem, int slice)
		{
			elem.Transforms = new SvgTransformCollection();
			elem.Transforms.Add(new SvgTranslate(-0.5f, -0.5f));
			var m = FTransformIn[slice];
			var mat = new SvgMatrix(new List<float>(){m.M11, m.M12, m.M21, m.M22, m.M41, m.M42});
			
			elem.Transforms.Add(mat);
		}
		
		protected void SetStroke(SvgVisualElement elem, int slice)
		{
			if(FStrokeModeIn[slice] != StrokeMode.None)
			{
				elem.Stroke = new SvgColourServer(FStrokeIn[slice].Color);
				elem.StrokeOpacity = (float)FStrokeIn[slice].A;
				elem.StrokeWidth = FStrokeWidthIn[slice];
			}
			else
			{
				elem.Stroke = null;
			}
		}
		
		protected virtual void SetFill(SvgVisualElement elem, int slice)
		{
		}
		
		protected abstract T CreateElement();
	}
	
	public abstract class SVGVisualElementFillNode<T> : SVGVisualElementNode<T> where T : SvgVisualElement
	{
		#region fields & pins
		
		[Input("Fill", Order = 20, DefaultColor = new double[] { 1, 1, 1, 1 })]
		IDiffSpread<RGBAColor> FFillIn;
		
		[Input("Fill Mode", Order = 21, Visibility = PinVisibility.OnlyInspector)]
		IDiffSpread<SvgFillRule> FFillModeIn;
		
		#endregion fields & pins
		
		protected override bool PinsChanged()
		{
			return base.PinsChanged() || FFillIn.IsChanged || FFillModeIn.IsChanged;
		}
		
				protected override void SetFill(SvgVisualElement elem, int index)
		{
			elem.Fill = new SvgColourServer(FFillIn[index].Color);
			elem.FillOpacity = (float)FFillIn[index].A;
			elem.FillRule = FFillModeIn[index];
		}
	}
	
	#region PluginInfo
	[PluginInfo(Name = "Quad", Category = "SVG", Help = "Svg Quad", Tags = "")]
	#endregion PluginInfo
	public class SvgRect : SVGVisualElementFillNode<SvgRectangle>
	{
		protected override SvgRectangle CreateElement()
		{
			var elem = new SvgRectangle();
			elem.Width = 1;
			elem.Height = 1;
			return elem;
		}
	}
	
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
