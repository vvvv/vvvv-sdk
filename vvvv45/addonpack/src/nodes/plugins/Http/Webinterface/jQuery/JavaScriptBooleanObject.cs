using System;
using System.Collections.Generic;
using System.Text;

namespace VVVV.Nodes.jQuery
{
	class JavaScriptBooleanObject : JavaScriptObject
	{
		protected bool FValue;

		public JavaScriptBooleanObject(bool value)
		{
			FValue = value;
		}

		public bool PValue
		{
			set { FValue = value; }
		}

		public override string PScript(int indentSteps, bool breakInternalLines)
		{
			return FValue.ToString().ToLower();
		}
	}
}
