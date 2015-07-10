using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;

namespace VVVV.Nodes
{
	[PluginInfo(Name = "InputMorph",
	Category = "Value",
	Version = "Advanced",
	Tags = "", Author = "vux", Help = "Bin size version for InputMorph"
	)]
	public class BinputMorphNode : IPluginEvaluate
	{
		[Input("Switch")]
		ISpread<double> FSwitch;

		[Input("Input", DefaultValue = 1.0)]
		ISpread<ISpread<double>> FInput;

		[Output("Output")]
		ISpread<double> FOutput;

		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			
			int outcnt = Math.Max(FSwitch.SliceCount, FInput.SliceCount);
			FOutput.SliceCount = outcnt;

			for (int i = 0; i < outcnt; i++)
			{
				double sw = FSwitch[i] % (double)this.FInput[i].SliceCount;	
				int min = (int)Math.Floor(sw) % this.FInput[i].SliceCount;
				int max = (int)Math.Ceiling(sw) % this.FInput[i].SliceCount;
				this.FOutput[i] = VMath.Map(sw - min, 0.0, 1.0, FInput[i][min], FInput[i][max], TMapMode.Clamp);
			}
		}
	}
}
