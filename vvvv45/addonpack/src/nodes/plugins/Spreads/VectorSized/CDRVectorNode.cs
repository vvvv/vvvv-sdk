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
	[PluginInfo(Name = "CDR", Category = "Spreads", Version = "Vector", Help = "CDR (Spreads) with bin and vector size", Author = "woei")]
	#endregion PluginInfo
	public class CDRVectorNode : IPluginEvaluate
	{
		#region fields & pins
		#pragma warning disable 649
		[Input("Input", CheckIfChanged = true, AutoValidate = false)]
		IInStream<double> FInput;

		[Input("Vector Size", MinValue = 1, DefaultValue = 1, IsSingle = true, CheckIfChanged = true, AutoValidate = false)]
		IInStream<int> FVec;
		
		[Input("Bin Size", DefaultValue = -1, CheckIfChanged = true, AutoValidate = false)]
		IInStream<int> FBin;
		
		[Output("Remainder")]
		IOutStream<double> FRemainder;
		
		[Output("Last Slice")]
		IOutStream<double> FLast;
		#pragma warning restore
		#endregion fields & pins
		
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			FInput.Sync(); 
			FVec.Sync();
			FBin.Sync();
			
			if (FInput.IsChanged || FVec.IsChanged || FBin.IsChanged)
			{
				if (FInput.Length>0 && FVec.Length>0 && FBin.Length>0)
				{
					int vecSize = Math.Max(1,FVec.GetReader().Read());
					VecBinSpread<double> spread = new VecBinSpread<double>(FInput,vecSize,FBin);
					
					FLast.Length = spread.Count * vecSize;
					if (FLast.Length == spread.ItemCount || spread.ItemCount == 0)
					{
						FRemainder.Length = 0;
						if (spread.ItemCount!=0)
							FLast.AssignFrom(FInput);
						else
							FLast.Length=0;
					}
					else
					{
						FRemainder.Length = spread.ItemCount-FLast.Length;
						using (var rWriter = FRemainder.GetWriter())
						using (var lWriter = FLast.GetWriter())
						{
							for (int b = 0; b < spread.Count; b++)
							{
								int rLength = spread[b].Length-vecSize;
								rWriter.Write(spread[b], 0, rLength);
								
								lWriter.Write(spread.GetBinRow(b,rLength/vecSize).ToArray(), 0, vecSize);
							}
						}
					}
				}
				else
					FRemainder.Length = FLast.Length = 0;
			}
		}
	}
	
	public class CDRNode<T> : IPluginEvaluate
	{
		#region fields & pins
		#pragma warning disable 649
		[Input("Input", CheckIfChanged = true, AutoValidate = false)]
		IInStream<T> FInput;
		
		[Input("Bin Size", DefaultValue = -1, CheckIfChanged = true, AutoValidate = false)]
		IInStream<int> FBin;
		
		[Output("Remainder")]
		IOutStream<T> FRemainder;
		
		[Output("Last Slice")]
		IOutStream<T> FLast;
		#pragma warning restore
		#endregion fields & pins
		
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			FInput.Sync(); 
			FBin.Sync();
			
			if (FInput.IsChanged || FBin.IsChanged)
			{
				VecBinSpread<T> spread = new VecBinSpread<T>(FInput,1,FBin);
				
				FLast.Length = spread.Count;
				if (FLast.Length == spread.ItemCount || spread.ItemCount==0)
				{
					FRemainder.Length = 0;
					if (spread.ItemCount!=0)
						FLast.AssignFrom(FInput);
					else
						FLast.Length=0;
				}
				else
				{
					FRemainder.Length = spread.ItemCount-FLast.Length;
					using (var rWriter = FRemainder.GetWriter())
					using (var lWriter = FLast.GetWriter())
					{
						for (int b = 0; b < spread.Count; b++)
						{
							int rLength = spread[b].Length-1;
							rWriter.Write(spread[b], 0, rLength);
							
							lWriter.Write(spread[b][rLength]);
						}
					}
				}
			}
		}
	}
	
	#region PluginInfo
	[PluginInfo(Name = "CDR", Category = "String", Version = "Bin", Help = "CDR (String) with bin size", Author = "woei")]
	#endregion PluginInfo
	public class CDRStringBin : CDRNode<string> {}
	
	#region PluginInfo
	[PluginInfo(Name = "CDR", Category = "Color", Version = "Bin", Help = "CDR (Color) with bin size", Author = "woei")]
	#endregion PluginInfo
	public class CDRColorBin : CDRNode<RGBAColor> {}
	
	#region PluginInfo
	[PluginInfo(Name = "CDR", Category = "Transform", Version = "Bin", Help = "Splits the spread into the last element and the rest, with bin size", Author = "woei")]
	#endregion PluginInfo
	public class CDRTransformBin : CDRNode<Matrix4x4> {}
	
	#region PluginInfo
	[PluginInfo(Name = "CDR", Category = "Enumerations", Version = "Bin", Help = "Splits the spread into the last element and the rest, with bin size", Author = "woei")]
	#endregion PluginInfo
	public class CDREnumBin : CDRNode<EnumEntry> {}
	
	#region PluginInfo
	[PluginInfo(Name = "CDR", Category = "Raw", Version = "Bin", Help = "Splits the spread into the last element and the rest, with bin size", Author = "woei")]
	#endregion PluginInfo
	public class CDRRawBin : CDRNode<System.IO.Stream> {}
	
}
