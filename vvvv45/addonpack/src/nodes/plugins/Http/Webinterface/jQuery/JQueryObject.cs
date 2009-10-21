using System;
using System.Collections.Generic;
using System.Text;

namespace VVVV.Nodes.jQuery
{
	public class JQueryObject : IScriptGenerator
	{
		#region IScriptGenerator Members

		protected JavaScriptObject FFunctionParameters;

		public JQueryObject ()
		{
			FFunctionParameters = null;
		}

		public JQueryObject(JavaScriptObject jsObject)
		{
			FFunctionParameters = jsObject;
		}

		public void Clear()
		{
			FFunctionParameters = null;
		}

		public string PScript(int indentSteps, bool breakInternalLines)
		{
			string text = "$";
			if (FFunctionParameters != null)
			{
				text += "(" + FFunctionParameters.PScript(indentSteps, breakInternalLines) + ")" ;
			}
			return text;
		}

		#endregion
	}
}
