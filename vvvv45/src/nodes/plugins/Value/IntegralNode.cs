
using System;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Nodes.Value
{
	[PluginInfo(Name = "Integral", 
				Category = "Spreads", 
				Help = "Sums up a spread of values and output the partial sums. The output spread has one slice more than the input.")]
	public class ValueIntegralNode : IPluginEvaluate
	{
		[Input("Input")]
		protected ISpread<ISpread<double>> FInput;

        [Input("Offset")]
        protected ISpread<double> FOffset;

        [Output("Output")]
		protected ISpread<ISpread<double>> FOutput;

		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
            SpreadMax = FInput.CombineWith(FOffset);
            FOutput.SliceCount = SpreadMax;

            for (int i = 0; i < SpreadMax; i++)
			{
				var input = FInput[i];
				var output = FOutput[i];
				
				output.SliceCount = input.SliceCount + 1;
				output[0] = FOffset[i];
				for (int j = 1; j < output.SliceCount; j++)
				{
					output[j] = input[j - 1] + output[j - 1];
				}
			}
		}
	}
}
