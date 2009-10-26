using System;
using System.Collections.Generic;
using System.Text;

namespace VVVV.Nodes.jQuery
{
	public class JavaScriptValueLiteral<T> : IJavaScriptObject
	{
		protected T FValue;

		protected JavaScriptValueLiteral()
		{

		}
		
		public JavaScriptValueLiteral(T value)
		{
			FValue = value;
		}

		public T PValue
		{
			set { FValue = value; }
		}
	

		public string PScript(int indentSteps, bool breakInternalLines, bool breakAfter)
		{
			string stringValue = FValue as string;
			if (stringValue != null)
			{
				return "'" + stringValue + "'";
			}
			else
			{
				return FValue.ToString().ToLower();
			}
		}
	}
}
