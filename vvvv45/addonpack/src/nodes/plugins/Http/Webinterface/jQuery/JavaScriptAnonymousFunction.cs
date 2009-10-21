using System;
using System.Collections.Generic;
using System.Text;

namespace VVVV.Nodes.jQuery
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

		public override string PScript(int indentSteps, bool breakInternalLines)
		{
			string text = "function(";
			int queueLength = FArgumentNames.Count;
			int count = 1;
			foreach (JavaScriptVariableObject argument in FArgumentNames)
			{
				text += argument.PScript(indentSteps, breakInternalLines);
				if (count != queueLength)
				{
					text += ", ";
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
