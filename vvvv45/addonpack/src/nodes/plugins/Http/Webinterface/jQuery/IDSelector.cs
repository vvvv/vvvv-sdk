using System;
using System.Collections.Generic;
using System.Text;

namespace VVVV.Nodes.jQuery
{
	public class IDSelector : JavaScriptValueLiteral<string>
	{
		public IDSelector(string ID) : base(ID)
		{
			Id = ID;
		}

		public string Id
		{
			set { PValue = "#" + value; }
		}
	}
}
