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

	public class SetSpread<T> : IPluginEvaluate
	{
		#region fields & pins
		[Input("Spread")]
		ISpread<ISpread<T>> FSpread;
		
		[Input("Input", BinSize =  1, BinName = "Count", BinOrder = 1)]
		ISpread<ISpread<T>> FInput;
		
		[Input("Offset")]
		ISpread<int> FOffset;

		[Output("Output")]
		ISpread<ISpread<T>> FOutput;
		#endregion fields & pins


		
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			
			FOutput.SliceCount = Math.Max(FSpread.SliceCount,Math.Max(FInput.SliceCount,FOffset.SliceCount));
			for (int i=0; i<FInput.SliceCount; i++)
			{
				FOutput[i].AssignFrom(FSpread[i]);
				for (int s=0; s<FInput[i].SliceCount; s++)
					FOutput[i][s+FOffset[i]]=FInput[i][s];
			}
		}
	}
	#region PluginInfo
	[PluginInfo(Name = "SetSpread",
	            Category = "Spreads",
	            Help = "SetSpread with Bin Size",
	            Tags = "",
	            Author = "woei")]
	#endregion PluginInfo
	public class SetSpreadSpreads : SetSpread<double> {}
	
	#region PluginInfo
	[PluginInfo(Name = "SetSpread",
	            Category = "String",
	            Help = "SetSpread with Bin Size",
	            Tags = "",
	            Author = "woei")]
	#endregion PluginInfo
	public class SetSpreadString : SetSpread<string> {}
	
	#region PluginInfo
	[PluginInfo(Name = "SetSpread",
	            Category = "Color",
	            Help = "SetSpread with Bin Size",
	            Tags = "split",
	            Author = "woei")]
	#endregion PluginInfo
	public class SetSpreadColor : SetSpread<RGBAColor> {}
	
	#region PluginInfo
	[PluginInfo(Name = "SetSpread",
	            Category = "Transform",
	            Help = "SetSpread with Bin Size",
	            Tags = "",
	            Author = "woei")]
	#endregion PluginInfo
	public class SetSpreadTransform : SetSpread<Matrix4x4> {}
	
	#region PluginInfo
	[PluginInfo(Name = "SetSpread",
	            Category = "Enumerations",
	            Help = "SetSpread with Bin Size",
	            Tags = "",
	            Author = "woei")]
	#endregion PluginInfo
	public class SetSpreadEnum : SetSpread<EnumEntry> {}
	
	#region PluginInfo
	[PluginInfo(Name = "SetSpread",
	            Category = "Raw",
	            Help = "SetSpread with Bin Size",
	            Tags = "",
	            Author = "woei")]
	#endregion PluginInfo
	public class SetSpreadRaw : SetSpread<System.IO.Stream> {}
}
