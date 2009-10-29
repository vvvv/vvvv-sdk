using System;
using System.Collections.Generic;
using System.Text;

namespace VVVV.Webinterface.jQuery
{
	public class JavaScriptVariableObject : JavaScriptObject
	{
		protected string FVariableName;

		public JavaScriptVariableObject(string variableName)
		{
			FVariableName = variableName;
		}
		
		protected override string GenerateObjectScript(int indentSteps, bool breakInternalLines, bool breakAfter)
		{
			return FVariableName;
		}
	}
}
