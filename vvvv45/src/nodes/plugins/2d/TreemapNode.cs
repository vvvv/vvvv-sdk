#region usings
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;
using VVVV.Utils.Algorithm;

using VVVV.Core.Logging; 

#endregion usings

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "Treemap",
	Category = "2d", 
	Help = "Returns a spread of packed rectangles whose size corresponds to the input values",
	Tags = "squarify, split, layout, rectangles",
	AutoEvaluate = true)]
	#endregion PluginInfo
	public class C2dTreemapNode : IPluginEvaluate
	{
		#region fields & pins
#pragma warning disable 0649
        [Input("Input")]
        IDiffSpread<double> FInput;

        [Input("Sort", IsSingle = true, DefaultValue = 1)]
        IDiffSpread<bool> FSortIn;

        [Input("Algorithm", IsSingle = true)]
        IDiffSpread<TreemapAlgorithm> FAlgorithmIn;

        [Output("Center")]
        ISpread<Vector2D> FCenterOut;

        [Output("Size")]
        ISpread<Vector2D> FSizeOut;

        [Output("Former Slice")]
        ISpread<int> FFormerSliceOut;
#pragma warning restore
		
		Treemap FTreeMap;
		
		#endregion fields & pins
		
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			if(FInput.IsChanged || FSortIn.IsChanged || FAlgorithmIn.IsChanged)
			{
				var n = MakeTree(SpreadMax);
				FTreeMap = new Treemap(n, FSortIn[0]);
				FTreeMap.Algorithm = FAlgorithmIn[0];
				FTreeMap.DoLayout();
				
				FCenterOut.SliceCount = SpreadMax;
				FSizeOut.SliceCount = SpreadMax;
				FFormerSliceOut.SliceCount = SpreadMax;
				
				var childs = FTreeMap.Root.Children;
				for (int i = 0; i < SpreadMax; i++) 
				{
					var c = childs[i];
					FCenterOut[i] = c.Rect.Center;
					FSizeOut[i] = c.Rect.Size;
					FFormerSliceOut[i] = (int)c.Value;
				}
			}
		}
		
		private void AddToOutput(Node n)
		{
			//convert to output
			foreach(var c in n.Children)
			{
				AddToOutput(c);
			}
		}
		
		private Node MakeTree(int count)
		{
			var root = new Node(count);
				
			for (int i = 0; i < count; i++) 
			{
				var n = new Node(0);
				n.Value = i;
				n.Size = Math.Abs(FInput[i]);
				if(n.Size > 0)
					root.Children.Add(n);
			}
			
			return root;
		}
	}
}




