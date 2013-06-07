
using System;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Nodes.Value
{
	[PluginInfo(Name = "Differential", 
				Category = "Spreads", 
				Help = "Calculates the differences between one slice and the next. The output spread has one slice less than the input.")]
	public class ValueDifferentialNode : IPluginEvaluate
	{
		[Input("Input")]
		protected ISpread<ISpread<double>> FInput;

        [Output("Output")]
        protected ISpread<ISpread<double>> FOutput;

        [Output("Offset")]
        protected ISpread<double> FOffset;

        //called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
            FOutput.SliceCount = FInput.SliceCount;
            FOffset.SliceCount = FInput.SliceCount;
			
			for (int i = 0; i < FInput.SliceCount; i++)
			{
				var input = FInput[i];
				var output = FOutput[i];

				FOffset[i] = input.SliceCount > 0 ? input[0] : 0;

				output.SliceCount = input.SliceCount - 1;
				for (int j = 1; j < input.SliceCount; j++)
				{
					output[j - 1] = input[j] - input[j - 1];
				}
			}
		}
	}
}
