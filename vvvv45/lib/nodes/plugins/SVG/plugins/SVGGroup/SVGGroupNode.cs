#region usings
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

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
				
				//write groups to output
				FOutput.AssignFrom(FGroups);
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
			}
		}
	}
}
