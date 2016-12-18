#region usings
using System;
using System.ComponentModel.Composition;
using VVVV.Nodes.Generic;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

#endregion usings

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "Reverse", Category = "Spreads", Version = "Vector", Help = "Reverse (Spreads) with Bin and Vector Size.", Author = "woei")]
	#endregion PluginInfo
	public class ReverseVectorNode : IPluginEvaluate
	{
        #region fields & pins
        readonly VecBinSpread<double> spread = new VecBinSpread<double>();
        
        #pragma warning disable 649
        [Input("Input", CheckIfChanged = true, AutoValidate = false)]
		IInStream<double> FInput;

		[Input("Vector Size", MinValue = 1, DefaultValue = 1, IsSingle = true, CheckIfChanged = true, AutoValidate = false)]
		IInStream<int> FVec;
		
		[Input("Bin Size", DefaultValue = -1, CheckIfChanged = true, AutoValidate = false)]
		IInStream<int> FBin;
		
		[Input("Reverse", DefaultBoolean = true)]
		IDiffSpread<bool> FReverse;
		
		[Output("Output")]
		IOutStream<double> FOutput;
		#pragma warning restore
		#endregion fields & pins
		
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			FInput.Sync(); 
			FVec.Sync();
			FBin.Sync();
			
			if (FInput.IsChanged || FVec.IsChanged || FBin.IsChanged || FReverse.IsChanged)
			{
				if (FVec.Length>0)
				{
					int vecSize = Math.Max(1,FVec.GetReader().Read());
                    spread.Sync(FInput,vecSize,FBin, FReverse.SliceCount);			
		    
					FOutput.Length = spread.ItemCount;
					using (var dataWriter = FOutput.GetWriter())
					{
						for (int b = 0; b < spread.Count; b++)
						{
							if (FReverse[b])
							{
								for (int r =spread[b].Length-vecSize; r>=0; r-=vecSize)
								{
									dataWriter.Write(spread[b], r, vecSize);
								}
							}
							else
							{
								dataWriter.Write(spread[b], 0, spread[b].Length);
							}
						}
					}
				}
				else
					FOutput.Length = 0;
			}
		}
	}
	
	
	#region PluginInfo
	[PluginInfo(Name = "Reverse", Category = "String", Version = "Bin", Help = "Reverse (String) with Bin Size.", Author = "woei")]
	#endregion PluginInfo
	public class ReverseStringBin : ReverseBin<string> {}
	
	#region PluginInfo
	[PluginInfo(Name = "Reverse", Category = "Color", Version = "Bin", Help = "Reverse (Color) with Bin Size.", Author = "woei")]
	#endregion PluginInfo
	public class ReverseColorBin : ReverseBin<RGBAColor> {}
	
	#region PluginInfo
	[PluginInfo(Name = "Reverse", Category = "Transform", Version = "Bin", Help = "Reverses the order of the slices in the Spread. With Bin Size.", Author = "woei")]
	#endregion PluginInfo
	public class ReverseTransformBin : ReverseBin<Matrix4x4> {}
	
	#region PluginInfo
	[PluginInfo(Name = "Reverse", Category = "Enumerations", Version = "Bin", Help = "Reverses the order of the slices in the Spread. With Bin Size.", Author = "woei")]
	#endregion PluginInfo
	public class ReverseEnumBin : ReverseBin<EnumEntry> {}
	
	#region PluginInfo
	[PluginInfo(Name = "Reverse", Category = "Raw", Version = "Bin", Help = "Reverses the order of the slices in the Spread. With Bin Size.", Author = "woei")]
	#endregion PluginInfo
	public class ReverseRawBin : ReverseBin<System.IO.Stream> {}
}
