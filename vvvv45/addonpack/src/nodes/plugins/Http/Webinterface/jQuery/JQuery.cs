using System;
using System.Collections.Generic;
using System.Text;

namespace VVVV.Nodes.jQuery
{
	class JQuery : IScriptGenerator
	{
		protected Queue<JQueryExpression> FStatements;

		public JQuery()
		{
			FStatements = new Queue<JQueryExpression>();
		}
		
		public string PScript
		{
			get
			{
				string text = "";
				foreach (JQueryExpression statement in FStatements)
				{
					text += statement.PScript + ';';
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
