using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Node10.OOPGenerics
{
	public abstract class AbstractSortComparerNode<T> : IPluginEvaluate
	{
		[Output("Output", IsSingle = true)]
		protected ISpread<IComparer<T>> FOutput;
		
		private IComparer<T> FComparer;

		protected abstract IComparer<T> GetComparer();

		public void Evaluate(int SpreadMax)
		{
			FOutput[0] = GetComparer();
		}
	}
}
