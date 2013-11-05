#region usings
using System;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;
#endregion usings

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "Disperse", Category = "Spreads", Help = "sequentially increases the slices of the spread from 0 to 1", Author = "woei")]
	#endregion PluginInfo
	public class DisperseFastNode : IPluginEvaluate
	{
		#region fields & pins
		[Input("Input", MinValue = 0, MaxValue=1)]
		public IDiffSpread<double> FInput;

		[Input("Gamma", DefaultValue=1, Visibility = PinVisibility.Hidden)]
		public IDiffSpread<double> FGamma;
		
		[Input("Spread Count", DefaultValue=1)]
		public IDiffSpread<int> FBinSize;
		
		[Output("Output")]
		public IOutStream<double> FOutput;
		#endregion fields & pins

		//called when data for any output pin is requested
		public void Evaluate(int spreadMax)
		{
			if (FInput.IsChanged || FGamma.IsChanged || FBinSize.IsChanged)
			{
				int sliceCount = 0;
				FOutput.Length = 0;
				for (int i=0; i<spreadMax; i++)
				{
					double curBin = (double)FBinSize[i];
					double curGamma = FGamma[i];
					FOutput.Length += FBinSize[i];
					using (var ow = FOutput.GetWriter())
					{
						ow.Position = sliceCount;
						double range = (curBin-1.0)/curBin/curGamma;
						range = 1-range;
						for (int j=0; j<curBin; j++)
	        			{
	        				double start = (double)j/curBin/curGamma;
	        				double val = (FInput[i]-start)/range;
	        				val = Math.Max(Math.Min(val,1.0),0.0);
	        				ow.Write(val);
	        			}
					}
					sliceCount = FOutput.Length;
				}
			}
		}
	}
}
