using System;

using System.Collections.Generic;
using System.Text;

namespace VVVV.Webinterface.jQuery
{
	public class MethodCall : ChainableOperation
	{
		protected Method FMethod;
		protected List<IJavaScriptObject> FArguments;

		protected MethodCall()
		{
			FArguments = new List<IJavaScriptObject>();
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
					FArguments.Add(JavaScriptObjectFactory.Create(arguments[i]));
				}
			}

		}

		public void SetParameters(params object[] arguments)
		{
			FArguments.Clear();
			for (int i = 0; i < arguments.Length; i++)
			{
				if (arguments[i] != null)
				{
					FArguments.Add(JavaScriptObjectFactory.Create(arguments[i]));
				}
			}
		}

		#region IScriptGenerator Members

		public override string GenerateScript(int indentSteps, bool breakInternalLines, bool breakAfter)
		{
			string text = '.' + FMethod.GenerateScript(indentSteps, breakInternalLines, breakAfter) + '(';
			int queueLength = FArguments.Count;
			int count = 1;
			foreach (IJavaScriptObject argument in FArguments)
			{
				text += argument.GenerateScript(indentSteps, breakInternalLines, breakAfter);
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
