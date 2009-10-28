using System;
using System.Collections.Generic;
using System.Text;

namespace VVVV.Webinterface.jQuery
{
	public interface IScriptGenerator
	{
		string PScript(int indentSteps, bool breakInternalLines, bool breakAfter);
	}
}
