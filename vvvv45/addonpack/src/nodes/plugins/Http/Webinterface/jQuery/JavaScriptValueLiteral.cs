using System;
using System.Collections.Generic;
using System.Text;

namespace VVVV.Webinterface.jQuery
{
	public class JavaScriptValueLiteral<T> : JavaScriptObject
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
	

		protected override string GetObjectScript(int indentSteps, bool breakInternalLines, bool breakAfter)
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
