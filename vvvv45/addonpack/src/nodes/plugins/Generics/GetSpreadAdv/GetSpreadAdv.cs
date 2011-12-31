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

	public class GetSpread<T> : IPluginEvaluate
	{
		#region fields & pins
		[Input("Input")]
		ISpread<ISpread<T>> FInput;
		
		[Input("Offset")]
		ISpread<int> FOffset;
		
		[Input("Count", DefaultValue = 1, MinValue = 0)]
		ISpread<int> FCount;

		[Output("Output")]
		ISpread<ISpread<T>> FOutput;
		#endregion fields & pins

		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			SpreadMax = Math.Max(FInput.SliceCount, FOffset.SliceCount);
			SpreadMax = Math.Max(SpreadMax, FCount.SliceCount);
			FOutput.SliceCount = SpreadMax;
			for (int i=0; i<SpreadMax; i++)
				FOutput[i]=FInput[i].GetRange(FOffset[i],Math.Max(FCount[i],0));

		}
	}
	#region PluginInfo
	[PluginInfo(Name = "GetSpread",
	            Category = "Spreads",
	            Version = "Advanced",
	            Help = "returns sub-spreads from the input specified via offset and count, with Bin Size Option",
	            Tags = "",
	            Author = "woei")]
	#endregion PluginInfo
	public class GetSpreadSpreads : GetSpread<double> {}
	
	#region PluginInfo
	[PluginInfo(Name = "GetSpread",
	            Category = "String",
	            Version = "Advanced",
	            Help = "returns sub-spreads from the input specified via offset and count, with Bin Size Option",
	            Tags = "",
	            Author = "woei")]
	#endregion PluginInfo
	public class GetSpreadString : GetSpread<string> {}
	
	#region PluginInfo
	[PluginInfo(Name = "GetSpread",
	            Category = "Color",
	            Version = "Advanced",
	            Help = "returns sub-spreads from the input specified via offset and count, with Bin Size Option",
	            Tags = "",
	            Author = "woei")]
	#endregion PluginInfo
	public class GetSpreadColor : GetSpread<RGBAColor> {}
	
	#region PluginInfo
	[PluginInfo(Name = "GetSpread",
	            Category = "Transform",
	            Help = "returns sub-spreads from the input specified via offset and count, with Bin Size Option",
	            Tags = "",
	            Author = "woei")]
	#endregion PluginInfo
	public class GetSpreadTransform : GetSpread<Matrix4x4> {}
	
	#region PluginInfo
	[PluginInfo(Name = "GetSpread",
	            Category = "Enumerations",
	            Help = "returns sub-spreads from the input specified via offset and count, with Bin Size Option",
	            Tags = "",
	            Author = "woei")]
	#endregion PluginInfo
	public class GetSpreadEnum : GetSpread<EnumEntry> {}
}
