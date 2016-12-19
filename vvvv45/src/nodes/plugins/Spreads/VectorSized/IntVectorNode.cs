#region usings
using System;
using System.ComponentModel.Composition;
using VVVV.Nodes.Generic;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;

#endregion usings

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "Integral", Category = "Spreads", Version = "Vector", Help = "Integral (Spreads) with vector size", Author = "woei")]
	#endregion PluginInfo
	public class IntegralVectorNode : IPluginEvaluate
	{
        #region fields & pins
        readonly VecBinSpread<double> spread = new VecBinSpread<double>();

        #pragma warning disable 649
        [Input("Input", CheckIfChanged = true, AutoValidate = false)]
		IInStream<double> FInput;

		[Input("Vector Size", MinValue = 1, DefaultValue = 1, IsSingle = true, CheckIfChanged = true, AutoValidate = false)]
		IInStream<int> FVec;
		
		[Input("Input Bin Size", DefaultValue = -1, CheckIfChanged = true, AutoValidate = false)]
		IInStream<int> FBin;
		
		[Input("Offset")]
		IDiffSpread<double> FOffset;
				
		[Input("Append Offset", DefaultBoolean = true, Visibility = PinVisibility.OnlyInspector)]
		IDiffSpread<bool> FInclOffset;
		
		[Output("Output")]
		IOutStream<double> FOutput;
		
		[Output("Output Bin Size")]
		IOutStream<int> FOutBin;
		#pragma warning restore
		#endregion fields & pins
		
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			FInput.Sync(); 
			FVec.Sync();
			FBin.Sync();
			
			if (FInput.IsChanged || FVec.IsChanged || FBin.IsChanged || FOffset.IsChanged || FInclOffset.IsChanged)
			{
				if (FInput.Length>0 && FVec.Length>0 && FBin.Length>0 && FOffset.SliceCount>0 && FInclOffset.SliceCount>0)
				{						 
					int vecSize = Math.Max(1,FVec.GetReader().Read());
					int offsetCount = (int)Math.Ceiling(FOffset.SliceCount/(double)vecSize);
					offsetCount = offsetCount.CombineWith(FInclOffset);
                    spread.Sync(FInput,vecSize,FBin,offsetCount);				
					
					FOutBin.Length = spread.Count;
					FOutput.Length = spread.ItemCount+(spread.Count*spread.VectorSize);
					using (var binWriter = FOutBin.GetWriter())
					using (var dataWriter = FOutput.GetWriter())
					{
						int incr = 0;
						for (int b = 0; b < spread.Count; b++)
						{
							for (int v = 0; v < vecSize; v++)
							{
								dataWriter.Position = incr+v;
								double sum = FOffset[b*vecSize+v];
								if (FInclOffset[b])
									dataWriter.Write(sum, vecSize);
								double[] column = spread.GetBinColumn(b,v).ToArray();
								for (int s=0; s<column.Length;s++)
								{
									sum += column[s];
									dataWriter.Write(sum,vecSize);
								}
							}
							incr+=spread[b].Length+(FInclOffset[b] ? vecSize:0);
							binWriter.Write((spread[b].Length/vecSize)+1,1);
						}
						FOutput.Length=incr;
					}
				}
				else
					FOutput.Length = FOutBin.Length = 0;
			}
		}
	}
}
