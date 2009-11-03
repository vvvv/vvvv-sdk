using System;
using System.Collections.Generic;
using System.Text;

namespace VVVV.Webinterface.jQuery
{
	public class JavaScriptAnonymousFunction : JavaScriptObject
	{
		protected JQuery FJQuery;
		private List<JavaScriptVariableObject> FArguments;

		public List<JavaScriptVariableObject> Arguments
		{
			get { return FArguments; }
		}

		public JavaScriptAnonymousFunction() : this(new JQuery(), null)
		{
		
		}
		
		public JavaScriptAnonymousFunction(JQuery jQuery, params string[] argumentNames)
		{
			FJQuery = jQuery;
			FArguments = new List<JavaScriptVariableObject>();
			if (argumentNames != null)
			{
				for (int i = 0; i < argumentNames.Length; i++)
				{
					FArguments.Add(new JavaScriptVariableObject(argumentNames[i]));
				}
			}
		}

		public JavaScriptAnonymousFunction(params string[] argumentNames) : this(new JQuery(), argumentNames)
		{

		}

		public JQuery PJQuery
		{
			set { FJQuery = value; }
		}

		protected override string GenerateObjectScript(int indentSteps, bool breakInternalLines, bool breakAfter)
		{
			string text = "function(";
			int queueLength = FArguments.Count;
			bool doBreakInternalLines = breakInternalLines && !FJQuery.PIsEmpty;
			int count = 1;
			foreach (JavaScriptVariableObject argument in FArguments)
			{
				text += argument.GenerateScript(indentSteps, breakInternalLines, breakAfter);
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
			text += FJQuery.GenerateScript(doBreakInternalLines ? indentSteps + 1 : 0, breakInternalLines, breakInternalLines);
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
