using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using BulletSharp;

namespace VVVV.Nodes.Bullet
{
	public abstract class AbstractBodyInteractionNode<T> : IPluginEvaluate
	{
		[Input("Bodies", Order = 0)]
		Pin<T> FInput;

		[Input("Apply", IsBang = true,Order=1000)]
		ISpread<bool> FApply;

		protected abstract void ProcessObject(T obj, int slice);

		public void Evaluate(int SpreadMax)
		{
			if (FInput.PluginIO.IsConnected)
			{
				for (int i = 0; i < SpreadMax; i++)
				{
					if (FApply[i])
					{
						T obj = FInput[i];
						this.ProcessObject(obj, i);
					}
				}
			}
		}
	}
}
