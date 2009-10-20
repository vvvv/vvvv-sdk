using System;
using System.Collections.Generic;
using System.Text;

namespace VVVV.Nodes.jQuery
{
	public abstract class JavaScriptObject : IScriptGenerator
	{

		public static JavaScriptObject ConvertToJavaScriptObject(object o)
		{
			JavaScriptObject jsObject;
			
			if (o is JavaScriptObject)
			{
				jsObject = (JavaScriptObject)o;
			}
			else if (o is int)
			{
				jsObject = new JavaScriptNumberObject((int)o);
			}
			else if (o is double)
			{
				jsObject = new JavaScriptNumberObject((double)o);
			}
			else if (o is bool)
			{
				jsObject = new JavaScriptBooleanObject((bool)o);
			}
			else if (o is string)
			{
				jsObject = new JavaScriptStringObject((string)o);
			}
			else
			{
				jsObject = new JavaScriptStringObject(o.ToString());
			}

			return jsObject;
		}

		#region IScriptGenerator Members

		public abstract string PScript
		{
			get;
		}

		#endregion
	}
}
