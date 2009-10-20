using System;
using System.Collections.Generic;
using System.Text;

namespace VVVV.Nodes.jQuery
{
	interface IScriptLayout
	{
		bool DoBreakInternalLines
		{
			set;
		}

		
		void Indent();
		void Indent(int steps);
	}
}
