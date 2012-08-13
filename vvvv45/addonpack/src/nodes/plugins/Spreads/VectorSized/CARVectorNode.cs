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
	[PluginInfo(Name = "CAR", Category = "Spreads", Version = "Vector", Help = "CAR (Spreads) with bin and vector size", Author = "woei")]
	#endregion PluginInfo
	public class CARVectorNode : IPluginEvaluate
	{
		#region fields & pins
		#pragma warning disable 649
		[Input("Input")]
		IInStream<double> FInput;

		[Input("Vector Size", MinValue = 1, DefaultValue = 1, IsSingle = true)]
		IInStream<int> FVec;
		
		[Input("Bin Size", DefaultValue = -1)]
		IInStream<int> FBin;
		
		[Output("First Slice")]
		IOutStream<double> FFirst;
		
		[Output("Remainder")]
		IOutStream<double> FRemainder;
		#pragma warning restore
		#endregion fields & pins
		
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			if (FVec.Length>0)
			{
				int vecSize = Math.Max(1,FVec.GetReader().Read());
				VecBinSpread<double> spread = new VecBinSpread<double>(FInput,vecSize,FBin);
			
				FFirst.Length = spread.Count * vecSize;
				if (FFirst.Length == spread.ItemCount)
				{
					FRemainder.Length=0;
					FFirst.AssignFrom(FInput);
				}
				else
				{
					FRemainder.Length = spread.ItemCount-FFirst.Length;
					using (var fWriter = FFirst.GetWriter())
			        using (var rWriter = FRemainder.GetWriter())
					{
						for (int b = 0; b < spread.Count; b++)
						{
							fWriter.Write(spread.GetBinRow(b,0).ToArray(), 0, vecSize);
							rWriter.Write(spread[b], vecSize, spread[b].Length-vecSize);
						}
					}
				}
			}
			else
				FFirst.Length = FRemainder.Length = 0;
		}
	}
	
	public class CARBinNode<T> : IPluginEvaluate
	{
		#region fields & pins
		#pragma warning disable 649
		[Input("Input")]
		IInStream<T> FInput;
		
		[Input("Bin Size", DefaultValue = -1)]
		IInStream<int> FBin;
		
		[Output("First Slice")]
		IOutStream<T> FFirst;
		
		[Output("Remainder")]
		IOutStream<T> FRemainder;
		#pragma warning restore
		#endregion fields & pins
		
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			VecBinSpread<T> spread = new VecBinSpread<T>(FInput,1,FBin);				
			
			FFirst.Length = spread.Count;
			if (FFirst.Length == spread.ItemCount)
			{
				FRemainder.Length=0;
				FFirst.AssignFrom(FInput);
			}
			else
			{
				FRemainder.Length = spread.ItemCount-FFirst.Length;
				using (var fWriter = FFirst.GetWriter())
		        using (var rWriter = FRemainder.GetWriter())
				{
					for (int b = 0; b < spread.Count; b++)
					{
						fWriter.Write(spread[b][0]);
						rWriter.Write(spread[b], 1, spread[b].Length-1);
					}
				}
			}
		}
	}
	
	#region PluginInfo
	[PluginInfo(Name = "CAR", Category = "String", Version = "Bin", Help = "CAR (String) with bin size", Author = "woei")]
	#endregion PluginInfo
	public class CARStringBin : CARBinNode<string> {}
	
	#region PluginInfo
	[PluginInfo(Name = "CAR", Category = "Color", Version = "Bin", Help = "CAR (Color) with bin size", Author = "woei")]
	#endregion PluginInfo
	public class CARColorBin : CARBinNode<RGBAColor> {}
	
	#region PluginInfo
	[PluginInfo(Name = "CAR", Category = "Transform", Version = "Bin", Help = "Splits the spread into the first element and the rest, with bin size", Author = "woei")]
	#endregion PluginInfo
	public class CARTransformBin : CARBinNode<Matrix4x4> {}
	
	#region PluginInfo
	[PluginInfo(Name = "CAR", Category = "Enumerations", Version = "Bin", Help = "Splits the spread into the first element and the rest, with bin size", Author = "woei")]
	#endregion PluginInfo
	public class CAREnumBin : CARBinNode<EnumEntry> {}
	
	
}
