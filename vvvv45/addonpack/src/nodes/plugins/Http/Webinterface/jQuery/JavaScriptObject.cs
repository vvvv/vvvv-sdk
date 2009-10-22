using System;
using System.Collections.Generic;
using System.Text;

namespace VVVV.Nodes.jQuery
{
	public abstract class JavaScriptObject : IScriptGenerator
	{
		public static JavaScriptObject ConvertToJavaScriptObject(object o)
		{
			if (o is JavaScriptObject)
			{
				return (JavaScriptObject)o;
			}
			else
			{
				return JavaScriptValueLiteralFactory.Create(o);
			}
		}

		#region IScriptGenerator Members

		public abstract string PScript(int indentSteps, bool breakInternalLines, bool breakAfter);

		#endregion
	}
}
