using System;
using System.Collections.Generic;
using System.Text;

namespace VVVV.Webinterface.jQuery
{
	public class JQuery : IScriptGenerator
	{
		protected Queue<Expression> FStatements;

		public static JQuery GenerateDocumentReady(JQuery handler)
		{
			return JQueryExpression.Document().ApplyMethodCall("ready", new JavaScriptAnonymousFunction(handler));
		}

		public JQuery()
		{
			FStatements = new Queue<Expression>();
		}

		public JQuery(params JQueryExpression[] statements) : this()
		{
			for (int i = 0; i < statements.Length; i++)
			{
				FStatements.Enqueue(statements[i]);
			}
		}

		public bool PIsEmpty
		{
			get { return FStatements.Count == 0; }
		}
	

		#region IScriptGenerator Members

		public virtual string PScript(int indentSteps, bool breakInternalLines, bool breakAfter)
		{
			string text = "";
			int numStatements = FStatements.Count;
			int count = 1;
			foreach (Expression statement in FStatements)
			{
				for (int i = 0; i < indentSteps; i++)
				{
					text += "\t";
				}

				text += statement.PScript(indentSteps, breakInternalLines, breakAfter) + ";";
				if (breakInternalLines && count != numStatements)
				{
					text += "\n";
				}
				count++;
			}
			if (breakAfter && numStatements > 0)
			{
				text += "\n";
			}
			return text;
		}

		#endregion
	}
}
