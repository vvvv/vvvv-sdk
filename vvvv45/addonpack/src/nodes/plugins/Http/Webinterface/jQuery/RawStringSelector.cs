using System;
using System.Collections.Generic;
using System.Text;

namespace VVVV.Nodes.jQuery
{
	class RawStringSelector : Selector
	{
		protected string FRawString;
		
		public RawStringSelector(string rawString)
		{
			PRawString = rawString;
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
