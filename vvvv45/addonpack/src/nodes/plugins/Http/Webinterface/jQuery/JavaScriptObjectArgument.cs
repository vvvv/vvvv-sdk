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

		public override string PScript(int indentSteps, bool breakInternalLines)
		{
			return FJavaScriptObject.PScript(indentSteps, breakInternalLines);
		}
	}
}
