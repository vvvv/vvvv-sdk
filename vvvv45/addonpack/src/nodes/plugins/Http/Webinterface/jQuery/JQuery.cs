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
			MethodCall readyMethod = new MethodCall("ready");
			readyMethod.AddArgument(new JavaScriptObjectArgument(new JavaScriptAnonymousFunction(handler)));
			documentReadyExpression.AddMethodCall(readyMethod);

			JQuery documentReady = new JQuery();
			documentReady.AddStatement(documentReadyExpression);
			return documentReady;
		}

		public JQuery()
		{
			FStatements = new Queue<JQueryExpression>();
			FScriptBlock = new ScriptBlock();
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
					for (int i = 0; i < FScriptBlock.PIndentSteps; i++)
					{
						text += "\t";
					}
					
					text += statement.PScript + ";";
					if (FScriptBlock.PDoBreakInternalLines)
					{
						text += "\n";
					}
				}
				return text;
			}
		}

		public void AddStatement(JQueryExpression statement)
		{
			FStatements.Enqueue(statement);
		}
	}
}
