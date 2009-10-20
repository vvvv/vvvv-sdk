using System;
using System.Collections.Generic;
using System.Text;

namespace VVVV.Nodes.jQuery
{
	public abstract class StringSelector : Selector
	{
		protected abstract string PSelector
		{
			get;
		}

		public override string PScript(int indentSteps, bool breakInternalLines)
		{
			return ("'" + PSelector + "'");
		}
	}
}