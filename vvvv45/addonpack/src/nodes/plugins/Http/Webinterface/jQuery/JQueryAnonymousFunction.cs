using System;
using System.Collections.Generic;
using System.Text;

namespace VVVV.Nodes.jQuery
{
	class JavaScriptAnonymousFunction : JavaScriptObject
	{
		protected JQuery FJQuery;

		public JavaScriptAnonymousFunction()
		{
			FJQuery = new JQuery();
		}

		public JavaScriptAnonymousFunction(JQuery jQuery)
		{
			PJQuery = jQuery;
		}

		public JQuery PJQuery
		{
			set
			{
				FJQuery = value;
				FJQuery.Indent();
			}
		}

		public override string PScript
		{
			get
			{
				string text = "function() {\n";
				text += FJQuery.PScript;
				text += "}";
				return text;
			}
		}
	}
}
