using System;
using System.Collections.Generic;
using System.Text;

namespace VVVV.Nodes.jQuery
{
	public abstract class Selector : JavaScriptValueLiteral<string>
	{
		public static JavaScriptObject AllSelector = JavaScriptValueLiteralFactory.Create("*");
		public static JavaScriptObject DocumentSelector = new JavaScriptVariableObject("document");
		public static JavaScriptObject ThisSelector = new JavaScriptVariableObject("this");
	}
}
