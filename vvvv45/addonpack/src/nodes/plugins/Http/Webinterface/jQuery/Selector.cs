using System;
using System.Collections.Generic;
using System.Text;

namespace VVVV.Nodes.jQuery
{
	public abstract class Selector : IScriptGenerator
	{
		public static Selector SelectorAll = new RawStringSelector("*");
		
		public string PScript
		{
			get
			{
				return("'" + PSelector + "'");
			}
		}

		protected abstract string PSelector
		{
			get;
		}
	}
}
