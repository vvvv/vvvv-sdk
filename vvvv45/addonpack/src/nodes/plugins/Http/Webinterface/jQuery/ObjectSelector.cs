using System;
using System.Collections.Generic;
using System.Text;

namespace VVVV.Nodes.jQuery
{
	public abstract class ObjectSelector
	{
		
		protected abstract string PSelector
		{
			get;
		}

		public string PScript(int indentSteps, bool breakInternalLines)
		{
			return (PSelector);
		}
	}
}
