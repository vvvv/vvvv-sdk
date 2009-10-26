using System;
using System.Collections.Generic;
using System.Text;

namespace VVVV.Nodes.jQuery
{
	public class JavaScriptObjectFactory
	{
		private JavaScriptObjectFactory()
		{

		}
		
		public static IJavaScriptObject Create<T>(T value)
		{
            IJavaScriptObject jsObject = value as IJavaScriptObject;
            
            if (jsObject == null)
            {
                jsObject = new JavaScriptValueLiteral<T>(value);
            }

            return jsObject;
		}
	}
}
