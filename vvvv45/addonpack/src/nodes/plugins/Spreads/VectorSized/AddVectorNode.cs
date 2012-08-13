#region usings
using System;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;

using System.Linq;
#endregion usings

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "Add", Category = "Value", Version = "Spectral Vector", Help = "+ (Value Spectral) with vector size", Author = "woei")]
	#endregion PluginInfo
	public class AddVectorNode : IPluginEvaluate
	{
		#region fields & pins
		#pragma warning disable 649
		[Input("Input")]
		IInStream<double> FInput;

		[Input("Vector Size", MinValue = 1, DefaultValue = 1, IsSingle = true)]
		IInStream<int> FVec;
		
		[Input("Bin Size", DefaultValue = -1)]
		IInStream<int> FBin;
		
		[Output("Output")]
		IOutStream<double> FOutput;
		#pragma warning restore
		#endregion fields & pins
		
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			if (FInput.Length>0 && FVec.Length>0 && FBin.Length>0)
			{
				int vecSize = Math.Max(1,FVec.GetReader().Read());
				VecBinSpread<double> spread = new VecBinSpread<double>(FInput,vecSize,FBin); 
				
				FOutput.Length = spread.Count*vecSize;
				using (var dataWriter = FOutput.GetWriter())
					for (int b = 0; b < spread.Count; b++)
						for (int v = 0; v < vecSize; v++)
							dataWriter.Write(spread.GetBinColumn(b,v).Sum(),1);
			}
			else
				FOutput.Length = 0;
		}
	}
}
