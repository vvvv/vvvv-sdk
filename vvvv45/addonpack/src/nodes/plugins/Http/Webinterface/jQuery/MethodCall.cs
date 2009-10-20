using System;

using System.Collections.Generic;
using System.Text;

namespace VVVV.Nodes.jQuery
{
	class MethodCall : IScriptGenerator
	{
		protected Method FMethod;
		protected Queue<Argument> FArguments;

		protected MethodCall()
		{
			FArguments = new Queue<Argument>();
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
				Argument argumentObject;
				if (arguments[i] is Argument)
				{
					argumentObject = (Argument)arguments[i];
				}
				else
				{
					argumentObject = new JavaScriptObjectArgument(JavaScriptObject.ConvertToJavaScriptObject(arguments[i]));
				}
				FArguments.Enqueue(argumentObject);
			}

		}

		public string PScript
		{
			get
			{
				string text = '.' + FMethod.PScript + '(';
				int queueLength = FArguments.Count;
				int count = 1;
				foreach (Argument argument in FArguments)
				{
					text += argument.PScript;
					if (count != queueLength)
					{
						text += ",";
					}
					count++;
				}
				text += ')';
				return text;
			}
		}
	}
}
