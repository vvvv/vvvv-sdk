using System;
using System.Collections.Generic;
using System.Text;

namespace VVVV.Nodes.jQuery
{
	class JavaScriptStringObject : JavaScriptObject
	{
		private string FValue; 
		
		public JavaScriptStringObject (string value)
		{
			FValue = value;
		}

		public string PValue
		{
			set { FValue = value;}
		}
	
			
		public override string PScript
		{
			get { return "'" + FValue + "'"; }
		}
	}
}
