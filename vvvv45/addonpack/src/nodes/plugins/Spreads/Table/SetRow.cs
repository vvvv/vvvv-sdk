using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using System.ComponentModel.Composition;

namespace VVVV.Nodes.Table
{
	#region PluginInfo
	[PluginInfo(Name = "SetRow", Category = "Table", Help = "Set values in a Table (akin to S+H)", Tags = "", Author = "elliotwoods", AutoEvaluate = true)]
	#endregion PluginInfo
	public class SetNode : IHasTable
	{
		#region fields & pins
		[Input("Input")]
		ISpread<ISpread<double>> FInput;

		[Input("Index")]
		ISpread<int> FIndex;

		[Input("Set", IsBang=true)]
		ISpread<ISpread<bool>> FSet;
		#endregion

		protected override void Evaluate2(int SpreadMax)
		{
			for (int i = 0; i < SpreadMax; i++)
			{
				//check whether any are high
				if (SpectralOr(FSet[i]))
				{
					if (FIndex.SliceCount > 0)
						FData.Set(FInput[i], FIndex[i], FSet[i]);
					else
						FData.Set(FInput[i], 0, FSet[i]);
					FData.OnDataChange(this);
				}
			}
		}

		bool SpectralOr(ISpread<bool> spread)
		{
			bool value = false;
			foreach (bool slice in spread)
				value |= slice;
			return value;
		}
	}
}
