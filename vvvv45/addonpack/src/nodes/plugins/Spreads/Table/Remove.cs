using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using System.ComponentModel.Composition;

namespace VVVV.Nodes.Table
{
	#region PluginInfo
	[PluginInfo(Name = "Remove", Category = "Table", Help = "Remove rows from a Table", Tags = "", Author = "elliotwoods", AutoEvaluate = true)]
	#endregion PluginInfo
	public class RemoveNode : IHasTable
	{
		#region fields & pins
		[Input("Remove", IsBang = true)]
		ISpread<bool> FRemove;

		[Input("Index")]
		ISpread<int> FIndex;
		#endregion

		protected override void Evaluate2(int SpreadMax)
		{
			if (FIndex.SliceCount > 0)
			{
				for (int i = 0; i < FRemove.SliceCount; i++)
				{
					if (FRemove[i])
					{
						if (FData.Rows.Count > 0)
						{
							int index = VVVV.Utils.VMath.VMath.Zmod(FIndex[i], FData.Rows.Count);
							FData.Rows.RemoveAt(index);
							FData.OnDataChange(this);
						}
					}
				}
			}
		}
	}
}
