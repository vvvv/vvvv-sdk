#region usings
using System;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;

using VVVV.Utils.VMath;
using VVVV.Utils.VColor;
#endregion usings

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "Reverse", Category = "Spreads", Version = "Vector", Help = "Reverse (Spreads) with bin and vector size", Author = "woei")]
	#endregion PluginInfo
	public class ReverseVectorNode : IPluginEvaluate
	{
		#region fields & pins
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
					VecBinSpread<double> spread = new VecBinSpread<double>(FInput,vecSize,FBin, FReverse.SliceCount);			
		    
					FOutput.Length = spread.ItemCount;
					using (var dataWriter = FOutput.GetWriter())
					{
						for (int b = 0; b < spread.Count; b++)
						{
							if (FReverse[b])
							{
								for (int r = (spread[b].Length/vecSize)-1; r>=0; r--)
								{
									dataWriter.Write(spread.GetBinRow(b,r).ToArray(), 0, vecSize);
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
	
	public class ReverseBinNode<T> : IPluginEvaluate
	{
		#region fields & pins
		#pragma warning disable 649
		[Input("Input", CheckIfChanged = true, AutoValidate = false)]
		IInStream<T> FInput;
		
		[Input("Bin Size", DefaultValue = -1, CheckIfChanged = true, AutoValidate = false)]
		IInStream<int> FBin;
		
		[Input("Reverse", DefaultBoolean = true)]
		IDiffSpread<bool> FReverse;
		
		[Output("Output")]
		IOutStream<T> FOutput;
		#pragma warning restore
		#endregion fields & pins
		
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			FInput.Sync(); 
			FBin.Sync();
			
			if (FInput.IsChanged || FBin.IsChanged)
			{
				VecBinSpread<T> spread = new VecBinSpread<T>(FInput,1,FBin, FReverse.SliceCount);			
	    
				FOutput.Length = spread.ItemCount;
				using (var dataWriter = FOutput.GetWriter())
				{
					for (int b = 0; b < spread.Count; b++)
					{
						T[] bin = spread[b];
						if (FReverse[b])
							Array.Reverse(bin);
						dataWriter.Write(bin, 0, bin.Length);
					}
				}
			}
		}
	}
	
	#region PluginInfo
	[PluginInfo(Name = "Reverse", Category = "String", Version = "Bin", Help = "Reverse (String) with bin size", Author = "woei")]
	#endregion PluginInfo
	public class ReverseStringBin : ReverseBinNode<string> {}
	
	#region PluginInfo
	[PluginInfo(Name = "Reverse", Category = "Color", Version = "Bin", Help = "Reverse (Color) with bin size", Author = "woei")]
	#endregion PluginInfo
	public class ReverseColorBin : ReverseBinNode<RGBAColor> {}
	
	#region PluginInfo
	[PluginInfo(Name = "Reverse", Category = "Transform", Version = "Bin", Help = "Reverses the order of the slices in the spread, with bin size", Author = "woei")]
	#endregion PluginInfo
	public class ReverseTransformBin : ReverseBinNode<Matrix4x4> {}
	
	#region PluginInfo
	[PluginInfo(Name = "Reverse", Category = "Enumerations", Version = "Bin", Help = "Reverses the order of the slices in the spread, with bin size", Author = "woei")]
	#endregion PluginInfo
	public class ReverseEnumBin : ReverseBinNode<EnumEntry> {}
	
	#region PluginInfo
	[PluginInfo(Name = "Reverse", Category = "Raw", Version = "Bin", Help = "Reverses the order of the slices in the spread, with bin size", Author = "woei")]
	#endregion PluginInfo
	public class ReverseRawBin : ReverseBinNode<System.IO.Stream> {}
}
