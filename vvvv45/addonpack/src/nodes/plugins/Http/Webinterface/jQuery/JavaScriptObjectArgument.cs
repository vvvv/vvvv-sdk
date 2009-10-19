using System;
using System.Collections.Generic;
using System.Text;

namespace VVVV.Nodes.jQuery
{
	class JavaScriptObjectArgument : Argument
	{
		JavaScriptObject FJavaScriptObject;

		public JavaScriptObjectArgument(JavaScriptObject javaScriptObject)
		{
			FJavaScriptObject = javaScriptObject;
		}
		
		public override string PScript
		{
			get { return FJavaScriptObject.PScript; }
		}
	}
}
