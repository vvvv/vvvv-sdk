using System;
using System.Collections.Generic;
using System.Text;

namespace VVVV.Nodes.jQuery
{
	class JavaScriptAnonymousFunction : JavaScriptObject
	{
		protected JQuery FJQuery;
		protected Queue<string> FArgumentNames;

		public JavaScriptAnonymousFunction(JQuery jQuery)
		{
			
			FJQuery = jQuery;
			FArgumentNames = new Queue<string>();
		}

		public JQuery PJQuery
		{
			set { FJQuery = value; }
		}

		public override string PScript
		{
			get
			{
				string text = "function(";
				int queueLength = FArgumentNames.Count;
				int count = 1;
				foreach (string argument in FArgumentNames)
				{
					text += argument;
					if (count != queueLength)
					{
						text += ",";
					}
					count++;
				}
				
				text += ") {\n";
				text += FJQuery.PScript;
				text += "}";
				return text;
			}
		}
	}
}
