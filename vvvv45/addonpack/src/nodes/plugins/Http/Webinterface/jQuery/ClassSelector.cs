using System;
using System.Collections.Generic;
using System.Text;

namespace VVVV.Nodes.jQuery
{
	public class ClassSelector : StringSelector
	{
		protected string FClassName;

		public ClassSelector(string ClassName)
		{
			FClassName = ClassName;
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
