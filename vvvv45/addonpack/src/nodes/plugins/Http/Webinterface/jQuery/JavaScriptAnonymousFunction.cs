using System;
using System.Collections.Generic;
using System.Text;

namespace VVVV.Nodes.jQuery
{
	class JavaScriptAnonymousFunction : JavaScriptObject
	{
		protected JQuery FJQuery;
		protected Queue<string> FArgumentNames;

		public JavaScriptAnonymousFunction(JQuery jQuery)
		{
			
			FJQuery = jQuery;
			FArgumentNames = new Queue<string>();
		}

		public JQuery PJQuery
		{
			set { FJQuery = value; }
		}

		public override string PScript(int indentSteps, bool breakInternalLines)
		{
			string text = "function(";
			int queueLength = FArgumentNames.Count;
			int count = 1;
			foreach (string argument in FArgumentNames)
			{
				text += argument;
				if (count != queueLength)
				{
					text += ",";
				}
				count++;
			}

			text += ") {";
			if (breakInternalLines)
			{
				text += "\n";
			}
			text += FJQuery.PScript(breakInternalLines ? indentSteps + 1 : 0, breakInternalLines);
			if (breakInternalLines)
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
