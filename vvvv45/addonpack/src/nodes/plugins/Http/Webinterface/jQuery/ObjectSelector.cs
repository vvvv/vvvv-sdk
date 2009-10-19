using System;
using System.Collections.Generic;
using System.Text;

namespace VVVV.Nodes.jQuery
{
	public abstract class ObjectSelector : Selector
	{
		public override string PScript
		{
			get
			{
				return (PSelector);
			}
		}

		protected abstract string PSelector
		{
			get;
		}
	}
}
