using System;
using System.Collections.Generic;
using System.Text;

namespace VVVV.Nodes.jQuery
{
	public class Selector
	{
		public static Selector AllSelector = new RawStringSelector("*");
		public static Selector DocumentSelector = new NameObjectSelector("document");
		public static Selector ThisSelector = new NameObjectSelector("this");
	}
}
