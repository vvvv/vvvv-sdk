using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using System.ComponentModel.Composition;

namespace VVVV.Nodes.Table
{
	#region PluginInfo
	[PluginInfo(Name = "GetValues", Category = "Table", Version = "Value", Help = "Convert Table to Values", Tags = "", Author = "elliotwoods", AutoEvaluate = true)]
	#endregion PluginInfo
	public class GetValuesNode : IHasTable
	{
		#region fields & pins
		[Output("Output")]
		ISpread<ISpread<double>> FOutput;

		[Output("Count")]
		ISpread<int> FCount;
		#endregion

		protected override void Evaluate2(int SpreadMax)
		{
			if (FData == null)
			{
				FOutput.SliceCount = 0;
				FCount[0] = 0;
				return;
			}

			if (FFreshData)
			{
				FData.GetSpread(FOutput);
				FCount[0] = FOutput.SliceCount;
			}
		}
	}
}
