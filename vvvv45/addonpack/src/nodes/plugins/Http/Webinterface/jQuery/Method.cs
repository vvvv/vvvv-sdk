using System;
using System.Collections.Generic;
using System.Text;

namespace VVVV.Nodes.jQuery
{
	public class Method : IScriptGenerator
	{
		protected string FName;

		public Method(string name)
		{
			FName = name;
		}
		
		public string PName
		{
			set { FName = value; }
		}

		#region IScriptGenerator Members

		public string PScript(int indentSteps, bool breakInternalLines)
		{
			return FName;
		}

		#endregion
	}
}
