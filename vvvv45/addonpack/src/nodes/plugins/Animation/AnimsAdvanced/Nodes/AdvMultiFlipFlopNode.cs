using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;

namespace VVVV.Nodes
{
	[PluginInfo(Name = "MultiFlipFlop",
	Category = "Animation",
	Version = "Advanced",
	AutoEvaluate = true,
	Tags = "", Author = "vux",Help="Bin size version for MultiFlipFlop"
	)]
	public class AdvMultiFlipFlopNode : IPluginEvaluate
	{
		[Input("Set", DefaultValue = 0.0,IsBang=true)]
		ISpread<ISpread<bool>> FInput;

		[Output("Output")]
		ISpread<double> FOutput;

		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			FOutput.SliceCount = FInput.SliceCount;

			for (int i = 0; i < FInput.SliceCount; i++)
			{
				bool found = false;
				for (int j = 0; j < FInput[i].SliceCount && !found; j++)
				{
					if (FInput[i][j])
					{
						FOutput[i] = j;
						found = true;
					}
				}
			}
		}
	}
}

