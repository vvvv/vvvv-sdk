#region usings
using System;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using VVVV.Core.Logging;
#endregion usings

namespace VVVV.Nodes
{

	public class SetSlice<T> : IPluginEvaluate
	{
		#region fields & pins
#pragma warning disable 0649
        [Input("Spread", BinName = "Bin Size", BinSize = 1, BinOrder = 1)]
        ISpread<ISpread<T>> FSpread;

        [Input("Input")]
        ISpread<T> FInput;

        [Input("Index", Order = 2)]
        ISpread<int> FIndex;

        [Output("Output")]
        ISpread<ISpread<T>> FOutput; 
#pragma warning restore
		#endregion fields & pins
		
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			FOutput.AssignFrom(FSpread);

			int incr=0;
			for (int i=0; i<FIndex.SliceCount; i++)
			{
				int ind = FIndex[i];
				for (int s=0; s<FOutput[ind].SliceCount; s++)
					FOutput[ind][s]=FInput[incr+s];
				incr+=FOutput[ind].SliceCount;
			}
		}
	}
	
	#region PluginInfo
	[PluginInfo(Name = "SetSlice",
	            Category = "Transform",
	            Help = "Replace individual slices of the spread with the given input",
	            Tags = "",
	            Author = "woei")]
	#endregion PluginInfo
	public class SetSliceTransform : SetSlice<Matrix4x4> {}
	
	#region PluginInfo
	[PluginInfo(Name = "SetSlice",
	            Category = "Enumerations",
	            Help = "Replace individual slices of the spread with the given input",
	            Tags = "",
	            Author = "woei")]
	#endregion PluginInfo
	public class SetSliceEnum : SetSlice<EnumEntry> {}
}
