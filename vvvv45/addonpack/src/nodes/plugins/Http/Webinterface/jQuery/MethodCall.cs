using System;

using System.Collections.Generic;
using System.Text;

namespace VVVV.Nodes.jQuery
{
	class MethodCall : IScriptGenerator
	{
		protected Method FMethod;
		protected Queue<Argument> FArguments;

		public MethodCall(Method method)
		{
			FMethod = method;
			FArguments = new Queue<Argument>();
		}

		public void AddArgument(Argument argument)
		{
			FArguments.Enqueue(argument);
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
