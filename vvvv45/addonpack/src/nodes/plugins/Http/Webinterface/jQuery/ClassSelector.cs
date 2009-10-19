using System;
using System.Collections.Generic;
using System.Text;

namespace VVVV.Nodes.jQuery
{
	class ClassSelector : Selector
	{
		protected string FClassName;

		public ClassSelector(string ClassName)
		{
			PClassName = ClassName;
		}

		public string PClassName
		{
			set { FClassName = value; }
		}
	

		protected override string PSelector
		{
			get
			{
				return "." + FClassName;
			}
		}
	}
}
