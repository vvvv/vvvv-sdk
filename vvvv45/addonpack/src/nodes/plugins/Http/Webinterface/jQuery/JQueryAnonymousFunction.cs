using System;
using System.Collections.Generic;
using System.Text;

namespace VVVV.Nodes.jQuery
{
	class JQueryAnonymousFunction : JavaScriptObject
	{
		protected JQuery FJQuery;

		public JQueryAnonymousFunction()
		{
			FJQuery = new JQuery();
		}

		public override string PScript
		{
			get
			{
				string text = "function() {";
				text += FJQuery.PScript;
				text += "}";
				return text;
			}
		}
	}
}
