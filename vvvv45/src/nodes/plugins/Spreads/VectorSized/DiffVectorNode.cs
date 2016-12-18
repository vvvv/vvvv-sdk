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
	[PluginInfo(Name = "Differential", Category = "Spreads", Version = "Vector", Help = "Differential (Spreads) with vector size", Author = "woei")]
	#endregion PluginInfo
	public class DifferentialVectorNode : IPluginEvaluate
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
		
		[Output("Output")]
		IOutStream<double> FOutput;
		
		[Output("Output Bin Size")]
		IOutStream<int> FOutBin;
		
		[Output("Offset")]
		IOutStream<double> FOffset;
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
                    spread.Sync(FInput,vecSize,FBin);
					
					FOutput.Length = Math.Max(spread.ItemCount-(spread.NonEmptyBinCount*vecSize),0);
					FOutBin.Length = spread.Count;
					FOffset.Length = spread.Count * vecSize;
					using (var offWriter = FOffset.GetWriter())
					using (var binWriter = FOutBin.GetWriter())
					using (var dataWriter = FOutput.GetWriter())
					{
						int incr = 0;
						for (int b = 0; b < spread.Count; b++)
						{
							if (spread[b].Length>0)
							{
							    for (int v = 0; v < vecSize; v++)
							    {
								    dataWriter.Position = incr+v;
								    var column = spread.GetBinColumn(b,v);
								    for (int s=0; s<column.Count-1;s++)
								    {
									    dataWriter.Write(column[s+1]-column[s],vecSize);
								    }
							    }
							    incr+=spread[b].Length-vecSize;
							    binWriter.Write((spread[b].Length/vecSize)-1,1);
							
							    offWriter.Write(spread[b],0,vecSize);
							}
							else
							{
								binWriter.Write(0,1);
								double[] zero = new double[vecSize];
								offWriter.Write(zero,0,vecSize);
							}
						}
					}
				}
				else
					FOutput.Length = FOutBin.Length = FOffset.Length;
			}
		}
	}
}
