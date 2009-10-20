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
			FSelector = Selector.AllSelector;
			FMethodCalls = new Queue<MethodCall>();
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
