using System;
using System.Collections.Generic;
using System.Text;

namespace VVVV.Webinterface.jQuery
{
	public abstract class Selector : JavaScriptValueLiteral<string>
	{
		public static IJavaScriptObject All = JavaScriptObjectFactory.Create("*");
	}
}
