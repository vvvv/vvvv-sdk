using System;
using System.Collections.Generic;
using System.Text;

namespace VVVV.Webinterface.jQuery
{
	public abstract class ChainableOperation : IScriptGenerator
	{
		protected bool FDoInclude;

		public bool DoInclude
		{
			get { return FDoInclude; }
			set { FDoInclude = value; }
		}

		protected ChainableOperation()
		{
			FDoInclude = true;
		}

		#region IScriptGenerator Members

		public abstract string GenerateScript(int indentSteps, bool breakInternalLines, bool breakAfter);

		#endregion
	}
}
