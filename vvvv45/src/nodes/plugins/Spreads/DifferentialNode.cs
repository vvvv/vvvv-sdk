
using System;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Nodes.Spreads
{
	[PluginInfo(Name = "Differential", 
				Category = "Spreads", 
				Help = "Calculates the differences between one slice and the next. The output spread has one slice less than the input.")]
	public class PluginValueDifferentialNode : IPluginEvaluate
	{
		[Input("Input")]
		protected ISpread<ISpread<double>> FInput;

		[Output("Output")]
		protected ISpread<ISpread<double>> FOutput;

		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			FOutput.SliceCount = FInput.SliceCount;
			
			for (int i = 0; i < FInput.SliceCount; i++)
			{
				var input = FInput[i];
				var output = FOutput[i];
				
				output.SliceCount = input.SliceCount - 1;
				for (int j = 1; j < input.SliceCount; j++)
				{
					output[j - 1] = input[j] - input[j - 1];
				}
			}
		}
	}
}
