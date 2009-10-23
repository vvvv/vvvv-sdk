using System;
using System.Collections.Generic;
using System.Text;

namespace VVVV.Nodes.jQuery
{
	public class JQueryObject : IScriptGenerator
	{
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

        #region IScriptGenerator Members

        public string PScript(int indentSteps, bool breakInternalLines, bool breakAfter)
		{
			string text = "$";
			if (FFunctionParameters != null)
			{
				text += "(" + FFunctionParameters.PScript(indentSteps, breakInternalLines, breakAfter) + ")" ;
			}
			return text;
		}

		#endregion
	}
}
