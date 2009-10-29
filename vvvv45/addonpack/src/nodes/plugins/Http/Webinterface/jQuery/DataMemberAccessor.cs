using System;
using System.Collections.Generic;
using System.Text;

namespace VVVV.Webinterface.jQuery
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
	
		public override string GenerateScript(int indentSteps, bool breakInternalLines, bool breakAfter)
		{
			return ('.' + FMemberName);
		}
	}
}
