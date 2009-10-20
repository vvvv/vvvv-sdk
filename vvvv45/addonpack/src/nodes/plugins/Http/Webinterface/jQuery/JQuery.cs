using System;
using System.Collections.Generic;
using System.Text;

namespace VVVV.Nodes.jQuery
{
	class JQuery : IScriptGenerator
	{
		protected Queue<JQueryExpression> FStatements;
		protected ScriptBlock FScriptBlock;

		public static JQuery GenerateDocumentReady(JQuery handler)
		{
			JQueryExpression documentReadyExpression = new JQueryExpression(Selector.DocumentSelector);
			documentReadyExpression.ApplyMethodCall("ready", new JavaScriptAnonymousFunction(handler));
			JQuery documentReady = new JQuery(documentReadyExpression);
			return documentReady;
		}

		public JQuery()
		{
			FStatements = new Queue<JQueryExpression>();
			FScriptBlock = new ScriptBlock();
		}

		public JQuery(params JQueryExpression[] statements) : this()
		{
			
			for (int i = 0; i < statements.Length; i++)
			{
				FStatements.Enqueue(statements[i]);
			}
		}

		public ScriptBlock PScriptBlock
		{
			get { return FScriptBlock; }
		}

		#region IScriptGenerator Members

		public string PScript(int indentSteps, bool breakInternalLines)
		{
			string text = "";
			foreach (JQueryExpression statement in FStatements)
			{
				for (int i = 0; i < indentSteps; i++)
				{
					text += "\t";
				}

				text += statement.PScript(indentSteps, breakInternalLines) + ";";
			}
			return text;
		}

		#endregion
	}
}
