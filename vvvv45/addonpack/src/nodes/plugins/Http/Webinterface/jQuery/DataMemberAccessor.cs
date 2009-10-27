using System;
using System.Collections.Generic;
using System.Text;

namespace VVVV.Nodes.jQuery
{
	class DataMemberAccessor : ChainableOperation
	{
		protected string FMemberName;

		public DataMemberAccessor(string memberName)
		{
			FMemberName = memberName;
		}

		public string PMemberName
		{
			set { FMemberName = value; }
		}
	
		public override string PScript(int indentSteps, bool breakInternalLines, bool breakAfter)
		{
			return ('.' + FMemberName);
		}
	}
}
