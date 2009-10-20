using System;
using System.Collections.Generic;
using System.Text;

namespace VVVV.Nodes.jQuery
{
	class JavaScriptNumberObject : JavaScriptObject
	{
		protected double FValue;

		public JavaScriptNumberObject(double value)
		{
			FValue = value;
		}

		public JavaScriptNumberObject(int value)
		{
			FValue = (double)value;
		}

		public double PDoubleValue
		{
			set { FValue = value; }
		}

		public int PIntValue
		{
			set { FValue = (double) value; }
		}

		public override string PScript(int indentSteps, bool breakInternalLines)
		{
			return FValue.ToString();
		}
	}
}
