using System;
using System.Collections.Generic;
using System.Text;

namespace VVVV.Nodes.jQuery
{
	public abstract class Selector
	{
		public static JavaScriptObject AllSelector = new JavaScriptValueLiteral<string>("document");
		public static JavaScriptObject DocumentSelector = new JavaScriptValueLiteral<string>("document");
		public static JavaScriptObject ThisSelector = new JavaScriptVariableObject("this");
	}
}