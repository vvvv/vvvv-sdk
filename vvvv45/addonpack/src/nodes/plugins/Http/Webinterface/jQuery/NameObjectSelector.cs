using System;
using System.Collections.Generic;
using System.Text;

namespace VVVV.Nodes.jQuery
{
	class RawStringSelector : StringSelector
	{
		protected string FRawString;
		
		public RawStringSelector(string rawString)
		{
			FRawString = rawString;
		}

		public string PRawString
		{
			set { FRawString = value; }
		}

		protected override string PSelector
		{
			get
			{
				return FRawString;
			}
		}
	}
}
