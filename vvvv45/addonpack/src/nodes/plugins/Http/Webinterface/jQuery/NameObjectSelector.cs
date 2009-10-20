using System;
using System.Collections.Generic;
using System.Text;

namespace VVVV.Nodes.jQuery
{
	class NameObjectSelector : ObjectSelector
	{
		protected string FObjectName;
		
		public NameObjectSelector(string objectName)
		{
			FObjectName = objectName;
		}

		public string PObjectName
		{
			set { FObjectName = value; }
		}

		protected override string PSelector
		{
			get
			{
				return FObjectName;
			}
		}
	}
}
