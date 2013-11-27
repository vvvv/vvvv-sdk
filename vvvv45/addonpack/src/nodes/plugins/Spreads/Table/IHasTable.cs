using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using System.ComponentModel.Composition;

namespace VVVV.Nodes.Table
{
	abstract public class IHasTable : IPluginEvaluate
	{
		#region fields & pins
		[Input("Table", IsSingle = true)]
		IDiffSpread<Table> FPinInTable;

		protected Table FData;
		protected bool FFreshData = false; //flag is raised when DataChanged event is called
		#endregion

		public void Evaluate(int SpreadMax)
		{
			if (FPinInTable.SliceCount == 0)
			{
				FData = null;
				return;
			}
			if (FPinInTable.IsChanged && FPinInTable[0] != FData)
			{
				if (FData != null)
				{
					FData.DataChanged -= new Table.DataChangedHandler(FData_DataChanged);
				}
				FData = FPinInTable[0];
				if (FData != null)
				{
					FData.DataChanged += new Table.DataChangedHandler(FData_DataChanged);
					FFreshData = true;
				}
				this.FData_Connected();
			}

			if (FData != null)
				this.Evaluate2(SpreadMax);
		}

		protected abstract void Evaluate2(int SpreadMax);

		void FData_Connected() { }
		void FData_DataChanged(object sender, EventArgs e) {
			if (sender != this)
			{
				FFreshData = true;
			}
		}
	}
}
