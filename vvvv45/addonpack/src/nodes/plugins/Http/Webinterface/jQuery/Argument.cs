using System;
using System.Collections.Generic;
using System.Text;

namespace VVVV.Nodes.jQuery
{
	public abstract class Argument : IScriptGenerator
	{
		#region IScriptGenerator Members

		public abstract string PScript(int indentSteps, bool breakInternalLines);

		#endregion
	}
}
