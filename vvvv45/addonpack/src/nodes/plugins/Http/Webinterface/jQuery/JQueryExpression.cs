using System;
using System.Collections.Generic;
using System.Text;

namespace VVVV.Nodes.jQuery
{
	class JQueryExpression : IScriptGenerator
	{
		protected Selector FSelector;
		protected Queue<MethodCall> FMethodCalls;

		public JQueryExpression() : this(Selector.AllSelector)
		{
		}

		public JQueryExpression(Selector selector)
		{
			FSelector = selector;
			FMethodCalls = new Queue<MethodCall>();
		}

		public JQueryExpression ApplyMethodCall(String methodName, params object[] arguments)
		{
			FMethodCalls.Enqueue(new MethodCall(methodName, arguments));
			return this;
		}

		#region IScriptGenerator Members

		public string PScript(int indentSteps, bool breakInternalLines)
		{
			string text = "$(" + FSelector.PScript(indentSteps, breakInternalLines) + ")";
			foreach (MethodCall methodCall in FMethodCalls)
			{
				text += methodCall.PScript(indentSteps, breakInternalLines);
			}
			return text;
		}

		#endregion
	}
}
