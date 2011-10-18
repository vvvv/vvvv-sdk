using System;
using System.Collections.Generic;
using System.Text;

namespace VVVV.Webinterface.jQuery
{
	public class IDSelector : Selector
	{
		public IDSelector(string ID)
		{
			Id = ID;
		}

		public string Id
		{
			set { PValue = "#" + value; }
		}
	}
}