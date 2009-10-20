using System;
using System.Collections.Generic;
using System.Text;

namespace VVVV.Nodes.jQuery
{
	class JQuery : IScriptGenerator, IScriptLayout
	{
		protected Queue<JQueryExpression> FStatements;
		protected int FNumIndentSteps;

		public JQuery()
		{
			FStatements = new Queue<JQueryExpression>();
			FNumIndentSteps = 0;
		}
		
		public string PScript
		{
			get
			{
				string text = "";
				foreach (JQueryExpression statement in FStatements)
				{
					for (int i = 0; i < FNumIndentSteps; i++)
					{
						text += "\t";
					}
					text += statement.PScript + ";\n";
				}
				return text;
			}
		}

		public void AddStatement(JQueryExpression statement)
		{
			FStatements.Enqueue(statement);
		}

		#region IScriptLayout Members

		public void Indent()
		{
			Indent(1);
		}

		public void Indent(int steps)
		{
			FNumIndentSteps += steps;
		}

		#endregion
	}
}
