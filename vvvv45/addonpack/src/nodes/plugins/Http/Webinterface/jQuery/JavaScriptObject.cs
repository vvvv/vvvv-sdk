using System;
using System.Collections.Generic;
using System.Text;

namespace VVVV.Webinterface.jQuery
{
	public abstract class JavaScriptObject : JavaScriptExpression
	{
		#region IScriptGenerator Members

		protected abstract override string GenerateObjectScript(int indentSteps, bool breakInternalLines, bool breakAfter);

		#endregion
	}
}
