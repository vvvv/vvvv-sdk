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

	public class DeCons<T> : IPluginEvaluate
	{
		#region fields & pins
		[Input("Input")]
		ISpread<ISpread<T>> FInput;

		[Output("Output", IsPinGroup = true)]
		ISpread<ISpread<T>> FOutput;
		#endregion fields & pins

		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			FOutput.SliceCount = FInput.SliceCount;
			for (int i=0; i<FInput.SliceCount; i++)
			{
				FOutput[i].AssignFrom(FInput[i]);
			}
		}
	}
	#region PluginInfo
	[PluginInfo(Name = "DeCons",
	            Category = "Spreads",
	            Help = "Reverse Function to the Cons Node",
	            Tags = "split",
	            Author = "woei")]
	#endregion PluginInfo
	public class DeConsSpreads : DeCons<double> {}
	
	#region PluginInfo
	[PluginInfo(Name = "DeCons",
	            Category = "String",
	            Help = "Reverse Function to the Cons Node",
	            Tags = "split",
	            Author = "woei")]
	#endregion PluginInfo
	public class DeConsString : DeCons<string> {}
	
	#region PluginInfo
	[PluginInfo(Name = "DeCons",
	            Category = "Color",
	            Help = "Reverse Function to the Cons Node",
	            Tags = "split",
	            Author = "woei")]
	#endregion PluginInfo
	public class DeConsColor : DeCons<RGBAColor> {}
	
	#region PluginInfo
	[PluginInfo(Name = "DeCons",
	            Category = "Transform",
	            Help = "Reverse Function to the Cons Node",
	            Tags = "split",
	            Author = "woei")]
	#endregion PluginInfo
	public class DeConsTransform : DeCons<Matrix4x4> {}
	
	#region PluginInfo
	[PluginInfo(Name = "DeCons",
	            Category = "Enumeration",
	            Help = "Reverse Function to the Cons Node",
	            Tags = "split",
	            Author = "woei")]
	#endregion PluginInfo
	public class DeConsEnum : DeCons<EnumEntry> {}
}
