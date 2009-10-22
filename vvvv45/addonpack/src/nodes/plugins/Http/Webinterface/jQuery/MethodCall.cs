using System;

using System.Collections.Generic;
using System.Text;

namespace VVVV.Nodes.jQuery
{
	public class MethodCall : IScriptGenerator
	{
		protected Method FMethod;
		protected Queue<JavaScriptObject> FArguments;

		protected MethodCall()
		{
			FArguments = new Queue<JavaScriptObject>();
		}
		
		public MethodCall(Method method) : this()
		{
			FMethod = method;	
		}

		public MethodCall(String method) : this()
		{
			FMethod = new Method(method);
		}

		public MethodCall(String method, params object[] arguments) : this()
		{
			FMethod = new Method(method);
			for (int i = 0; i < arguments.Length; i++)
			{
				if (arguments[i] != null)
				{
					FArguments.Enqueue(JavaScriptObject.ConvertToJavaScriptObject(arguments[i]));
				}
			}

		}

		#region IScriptGenerator Members

		public string PScript(int indentSteps, bool breakInternalLines, bool breakAfter)
		{
			string text = '.' + FMethod.PScript(indentSteps, breakInternalLines, breakAfter) + '(';
			int queueLength = FArguments.Count;
			int count = 1;
			foreach (JavaScriptObject argument in FArguments)
			{
				text += argument.PScript(indentSteps, breakInternalLines, breakAfter);
				if (count != queueLength)
				{
					text += ", ";
				}
				count++;
			}
			text += ')';
			return text;
		}

		#endregion
	}
}
