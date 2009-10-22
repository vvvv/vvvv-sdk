using System;
using System.Collections.Generic;
using System.Text;

namespace VVVV.Nodes.jQuery
{
	public class Selector
	{
		public static JavaScriptObject AllSelector = JavaScriptValueLiteralFactory.Create("*");
		public static JavaScriptObject DocumentSelector = new JavaScriptVariableObject("document");
		public static JavaScriptObject ThisSelector = new JavaScriptVariableObject("this");
	}
}
