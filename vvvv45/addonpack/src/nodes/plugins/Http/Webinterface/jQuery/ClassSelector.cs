using System;
using System.Collections.Generic;
using System.Text;

namespace VVVV.Nodes.jQuery
{
	public class ClassSelector : Selector
	{
		public ClassSelector(string className)
		{
			PClassName = className;
		}

		public string PClassName
		{
			set { PValue = "." + value; }
		}
	}
}
