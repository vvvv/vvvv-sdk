using System;
using System.Collections.Generic;
using System.Text;

namespace VVVV.Nodes.jQuery
{
	public class IDSelector : StringSelector
	{
		protected string FID;

		public IDSelector(string ID)
		{
			FID = ID;
		}

		public string PID
		{
			set { FID = value; }
		}

		protected override string PSelector
		{
			get
			{
				return "#" + FID;
			}
		}
	}
}
