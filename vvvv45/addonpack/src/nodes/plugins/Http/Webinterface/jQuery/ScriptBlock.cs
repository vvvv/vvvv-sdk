using System;
using System.Collections.Generic;
using System.Text;

namespace VVVV.Nodes.jQuery
{
	class ScriptBlock
	{
		private bool FDoBreakInternalLines;
		
		public ScriptBlock()
		{
			FDoBreakInternalLines = true;
		}

		public ScriptBlock(bool doBreakInternalLines)
		{
			FDoBreakInternalLines = doBreakInternalLines;
		}

		public bool PDoBreakInternalLines
		{
			get { return FDoBreakInternalLines; }
			set { FDoBreakInternalLines = value; }
		}
	}
}
