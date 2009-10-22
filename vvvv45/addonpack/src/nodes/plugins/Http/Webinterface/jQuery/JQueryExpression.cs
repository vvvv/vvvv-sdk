using System;
using System.Collections.Generic;
using System.Text;

namespace VVVV.Nodes.jQuery
{
	public class JQueryExpression : JavaScriptObject
	{
		protected JQueryObject FJQueryObject;
		protected Queue<MethodCall> FMethodCalls;

		public JQueryExpression(JQueryObject jQueryObject)
		{
			FJQueryObject = jQueryObject;
			FMethodCalls = new Queue<MethodCall>();
		}
		
		public JQueryExpression() : this(new JQueryObject())
		{
		}

		public JQueryExpression(JavaScriptObject jsObject) : this(new JQueryObject(jsObject))
		{
		}

		public JQueryExpression ApplyMethodCall(String methodName, params object[] arguments)
		{
			FMethodCalls.Enqueue(new MethodCall(methodName, arguments));
			return this;
		}

		public JQueryExpression Append(object jsObject)
		{
			FMethodCalls.Enqueue(new MethodCall("append", jsObject));
			return this;
		}
		
		public void Post(string url, JavaScriptObject data, string type, JavaScriptAnonymousFunction callback)
		{
			FMethodCalls.Enqueue(new MethodCall("post", url, data, type, callback));
		}

		public override string PScript(int indentSteps, bool breakInternalLines, bool breakAfter)
		{
			string text = FJQueryObject.PScript(indentSteps, breakInternalLines, breakAfter);
			foreach (MethodCall methodCall in FMethodCalls)
			{
				text += methodCall.PScript(indentSteps, breakInternalLines, breakAfter);
			}
			return text;
		}
	}
}
