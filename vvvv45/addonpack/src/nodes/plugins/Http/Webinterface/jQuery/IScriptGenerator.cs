using System;
using System.Collections.Generic;
using System.Text;

namespace VVVV.Webinterface.jQuery
{
	public interface IScriptGenerator
	{
		string GenerateScript(int indentSteps, bool breakInternalLines, bool breakAfter);
	}
}
