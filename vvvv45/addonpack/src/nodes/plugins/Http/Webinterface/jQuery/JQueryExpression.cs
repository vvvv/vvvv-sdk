using System;
using System.Collections.Generic;
using System.Text;

namespace VVVV.Nodes.jQuery
{
	class JQueryExpression : IScriptGenerator
	{
		protected Selector FSelector;
		protected Queue<MethodCall> FMethodCalls;

		public JQueryExpression()
		{
			FSelector = Selector.SelectorAll;
			FMethodCalls = new Queue<MethodCall>();
		}

		public string PScript
		{
			get
			{
				string text = "$(" + FSelector.PScript + ")";
				foreach (MethodCall methodCall in FMethodCalls)
				{
					text += methodCall.PScript;
				}
				return text;
			}
		}
	}
}
