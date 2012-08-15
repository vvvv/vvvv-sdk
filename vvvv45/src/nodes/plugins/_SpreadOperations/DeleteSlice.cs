#region usings
using System;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using System.Linq;

using VVVV.Core.Logging;
#endregion usings

namespace VVVV.Nodes
{

	public class DeleteSlice<T> : IPluginEvaluate
	{
		#region fields & pins	
		#pragma warning disable 649
		[Input("Input", BinSize =  1, BinName = "Bin Size")]
		IDiffSpread<ISpread<T>> FInput;
		
		[Input("Index")]
		IDiffSpread<int> FIndex;

		[Output("Output")]
		ISpread<ISpread<T>> FOutput;
		#pragma warning restore
		#endregion fields & pins

		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			if (FInput.IsChanged || FIndex.IsChanged)
			{
	  			FOutput.AssignFrom(FInput);
	  			foreach (int i in FIndex.Select(x => x%FInput.SliceCount).Distinct().OrderByDescending(x => x))
	  				FOutput.RemoveAt(i);
			}
		}
	}
	
	#region PluginInfo
	[PluginInfo(Name = "DeleteSlice",
	            Category = "Spreads",
	            Help = "Delete the slice at the given index.",
	            Tags = "remove, filter",
	            Author = "woei")]
	#endregion PluginInfo
	public class DeleteSliceSpreads : DeleteSlice<double> {}
	
	#region PluginInfo
	[PluginInfo(Name = "DeleteSlice",
	            Category = "String",
	            Help = "Delete the slice at the given index.",
	            Tags = "remove, filter",
	            Author = "woei")]
	#endregion PluginInfo
	public class DeleteSliceString : DeleteSlice<string> {}
	
	#region PluginInfo
	[PluginInfo(Name = "DeleteSlice",
	            Category = "Color",
	            Help = "Delete the slice at the given index.",
	            Tags = "remove, filter",
	            Author = "woei")]
	#endregion PluginInfo
	public class DeleteSliceColor : DeleteSlice<RGBAColor> {}
	
	#region PluginInfo
	[PluginInfo(Name = "DeleteSlice",
	            Category = "Transform",
	            Help = "Delete the slice at the given index.",
	            Tags = "remove, filter",
	            Author = "woei")]
	#endregion PluginInfo
	public class DeleteSliceTransform : DeleteSlice<Matrix4x4> {}
	
	#region PluginInfo
	[PluginInfo(Name = "DeleteSlice",
	            Category = "Enumerations",
	            Help = "Delete the slice at the given index.",
	            Tags = "remove, filter",
	            Author = "woei")]
	#endregion PluginInfo
	public class DeleteSliceEnum : DeleteSlice<EnumEntry> {}
}
