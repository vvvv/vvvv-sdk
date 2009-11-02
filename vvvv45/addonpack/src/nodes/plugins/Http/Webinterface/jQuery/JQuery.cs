using System;
using System.Collections.Generic;
using System.Text;

namespace VVVV.Webinterface.jQuery
{
	public class JQuery : IScriptGenerator
	{
		protected List<Expression> FStatements;

		public static JQuery GenerateDocumentReady(JQuery handler)
		{
			return JQueryExpression.Document().ApplyMethodCall("ready", new JavaScriptAnonymousFunction(handler));
		}

		public JQuery()
		{
			FStatements = new List<Expression>();
		}

		public JQuery(params JQueryExpression[] statements) : this()
		{
			for (int i = 0; i < statements.Length; i++)
			{
				FStatements.Add(statements[i]);
			}
		}

		public bool PIsEmpty
		{
			get { return FStatements.Count == 0; }
		}

		public JQueryExpression Expression
		{
			set {
				FStatements.Clear();
				FStatements.Add(value);
			}
		}
	

		#region IScriptGenerator Members

		public virtual string GenerateScript(int indentSteps, bool breakInternalLines, bool breakAfter)
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

				text += statement.GenerateScript(indentSteps, breakInternalLines, breakAfter) + ";";
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
