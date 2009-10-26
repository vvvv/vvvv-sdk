using System;
using System.Collections.Generic;
using System.Text;

namespace VVVV.Nodes.jQuery
{
	public class JavaScriptVariableObject : IJavaScriptObject
	{
		protected string FVariableName;

		private JavaScriptVariableObject()
		{

		}
		
		public JavaScriptVariableObject(string variableName)
		{
			FVariableName = variableName;
		}
		
		public string PScript(int indentSteps, bool breakInternalLines, bool breakAfter)
		{
			return FVariableName;
		}
	}
}
