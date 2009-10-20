using System;
using System.Collections.Generic;
using System.Text;

namespace VVVV.Nodes.jQuery
{
	class ScriptBlock
	{
		private int FNumIndentSteps;
		private bool FDoBreakInternalLines;
		
		public ScriptBlock()
		{
			FNumIndentSteps = 1;
			FDoBreakInternalLines = true;
		}

		public ScriptBlock(int numIndentSteps, bool doBreakInternalLines)
		{
			FNumIndentSteps = numIndentSteps;
			FDoBreakInternalLines = doBreakInternalLines;
		}

		public void Indent()
		{
			IndentBy(1);
		}

		public void IndentBy(int steps)
		{
			FNumIndentSteps += steps;
		}
		
		public int PIndentSteps
		{
			get { return FNumIndentSteps; }
			set { FNumIndentSteps = value; }
		}
	
		public bool PDoBreakInternalLines
		{
			get { return FDoBreakInternalLines; }
			set { FDoBreakInternalLines = value; }
		}
	}
}
