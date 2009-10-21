using System;
using System.Collections.Generic;
using System.Text;

namespace VVVV.Nodes.jQuery
{
	public class DynamicXML : JQueryFunctionParameters
	{
		protected string FXml;

		public DynamicXML()
		{
			FXml = "";
		}

		public DynamicXML(string xml)
		{
			FXml = xml;
		}

		public string PXml
		{
			set { FXml = value; }
		}
	

		public override string PScript(int indentSteps, bool breakInternalLines)
		{
			return "'" + FXml + "'";
		}
	}
}
