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

		public JQuery(params Expression[] statements) : this()
		{
			for (int i = 0; i < statements.Length; i++)
			{
				FStatements.Add(statements[i]);
			}
		}

		public bool PIsEmpty
		{
			get
			{
				for (int i = 0; i < FStatements.Count; i++)
				{
					if (FStatements[i].DoInclude) return false;
				}

				return true;
			}
		}

		public Expression Expression
		{
			set {
				FStatements.Clear();
				FStatements.Add(value);
			}
		}

		public void AddExpression(Expression expression)
		{
			FStatements.Add(expression);
		}

		#region IScriptGenerator Members

		public virtual string GenerateScript(int indentSteps, bool breakInternalLines, bool breakAfter)
		{
			string text = "";
			int count = 0;
			for (int i = 0; i < FStatements.Count; i++)
			{
				if (FStatements[i].DoInclude)
				{
					if (breakInternalLines && count > 0)
					{
						text += "\n";
					}
					for (int j = 0; j < indentSteps; j++)
					{
						text += "\t";
					}
					text += FStatements[i].GenerateScript(indentSteps, breakInternalLines, breakAfter) + ";";
					count++;
				}
			}
			if (breakAfter && count > 0)
			{
				text += "\n";
			}
			return text;
		}

		#endregion
	}
}
