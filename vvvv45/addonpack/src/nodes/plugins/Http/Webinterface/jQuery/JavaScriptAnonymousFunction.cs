using System;
using System.Collections.Generic;
using System.Text;

namespace VVVV.Webinterface.jQuery
{
	public class JavaScriptAnonymousFunction : JavaScriptObject
	{
		protected JQuery FJQuery;
		protected Queue<JavaScriptVariableObject> FArgumentNames;

		public JavaScriptAnonymousFunction() : this(new JQuery())
		{

		}
		
		public JavaScriptAnonymousFunction(JQuery jQuery, params string[] argumentNames)
		{
			FJQuery = jQuery;
			FArgumentNames = new Queue<JavaScriptVariableObject>();
			for (int i = 0; i < argumentNames.Length; i++)
			{
				FArgumentNames.Enqueue(new JavaScriptVariableObject(argumentNames[i]));
			}
		}

		public JQuery PJQuery
		{
			set { FJQuery = value; }
		}

		protected override string GetObjectScript(int indentSteps, bool breakInternalLines, bool breakAfter)
		{
			string text = "function(";
			int queueLength = FArgumentNames.Count;
			bool doBreakInternalLines = breakInternalLines && !FJQuery.PIsEmpty;
			int count = 1;
			foreach (JavaScriptVariableObject argument in FArgumentNames)
			{
				text += argument.PScript(indentSteps, breakInternalLines, breakAfter);
				if (count != queueLength)
				{
					text += ", ";
				}
				count++;
			}

			text += ") {";
			if (doBreakInternalLines)
			{
				text += "\n";
			}
			text += FJQuery.PScript(doBreakInternalLines ? indentSteps + 1 : 0, breakInternalLines, breakInternalLines);
			if (doBreakInternalLines)
			{
				for (int i = 0; i < indentSteps; i++)
				{
					text += "\t";
				}
			}
			text += "}";
			return text;
		}
	}
}
