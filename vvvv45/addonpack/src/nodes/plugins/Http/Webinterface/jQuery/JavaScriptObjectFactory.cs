using System;
using System.Collections.Generic;
using System.Text;

namespace VVVV.Nodes.jQuery
{
	public class JavaScriptObjectFactory
	{
		public static JavaScriptObject Create<T>(T value)
		{
            JavaScriptObject jsObject = value as JavaScriptObject;
            
            if (jsObject == null)
            {
                jsObject = new JavaScriptValueLiteral<T>(value);
            }

            return jsObject;
		}
	}
}
