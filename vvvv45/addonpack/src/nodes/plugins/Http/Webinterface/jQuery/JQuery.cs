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
	

		public string PScript
		{
			get
			{
				string text = "";
				foreach (JQueryExpression statement in FStatements)
				{
					/*for (int i = 0; i < FScriptBlock.PIndentSteps; i++)
					{
						text += "\t";
					}*/
					
					text += statement.PScript + ";";
					if (FScriptBlock.PDoBreakInternalLines)
					{
						text += "\n";
					}
				}
				return text;
			}
		}
	}
}
