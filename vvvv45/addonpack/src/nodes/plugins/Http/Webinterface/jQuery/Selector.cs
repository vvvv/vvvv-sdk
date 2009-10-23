using System;
using System.Collections.Generic;
using System.Text;

namespace VVVV.Nodes.jQuery
{
	public abstract class Selector : JavaScriptValueLiteral<string>
	{
		public static JavaScriptObject All = JavaScriptObjectFactory.Create("*");
	}
}
