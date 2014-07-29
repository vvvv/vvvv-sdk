using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using System.ComponentModel.Composition;

namespace VVVV.Nodes.Table
{
	#region PluginInfo
	[PluginInfo(Name = "Insert", Category = "Table", Help = "Insert values into a Table (akin to Queue)", Tags = "", Author = "elliotwoods", AutoEvaluate = true)]
	#endregion PluginInfo
	public class InsertNode : IHasTable
	{
		#region fields & pins
		[Input("Input")]
		ISpread<ISpread<double>> FInput;

		[Input("Insert", IsBang = true)]
		ISpread<bool> FInsert;

		[Input("Index")]
		ISpread<int> FIndex;

		#endregion

		protected override void Evaluate2(int SpreadMax)
		{
			if (FIndex.SliceCount > 0)
			{
				for (int i = 0; i < FInsert.SliceCount; i++)
				{
					if (FInsert[i])
					{
						FData.Insert(FInput[i], FIndex[i]);
						FData.OnDataChange(this);
					}
				}
			}
		}
	}
}
