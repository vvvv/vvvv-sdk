using System;
using System.Collections.Generic;
using System.Text;

namespace VVVV.Webinterface.jQuery
{
	public class HTMLElementSelector : Selector
	{
		public HTMLElementSelector(string element)
		{
			Element = element;
		}

		public string Element
		{
			set { PValue = value; }
		}
	}
}