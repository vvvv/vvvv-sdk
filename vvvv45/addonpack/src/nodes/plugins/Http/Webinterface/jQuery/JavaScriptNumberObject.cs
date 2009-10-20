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

		public double PValue
		{
			set { FValue = value; }
		}
	

		public override string PScript
		{
			get { return FValue.ToString(); }
		}
	}
}
