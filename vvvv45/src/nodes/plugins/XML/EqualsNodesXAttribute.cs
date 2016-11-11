using System;
using VVVV.PluginInterfaces.V2;
using VVVV.Nodes.Generic;
using System.Xml;
using System.Xml.Linq;

namespace VVVV.Nodes
{

	#region SingleValue
	
	[PluginInfo(Name = "Equals", 
	            Category = "XElement",
                Version = "Attribute",
	            Help = "returns true if the values at the inputs are equal",
	            Tags = "compare, same"
	           )]
	public class XAttributeEqualsNode: Equals<XAttribute> {}
	
	#endregion SingleValue
	
	#region SpreadOps
	
	[PluginInfo(Name = "Occurrence", 
	            Category = "XElement",
	            Help = "Counts the occurrence of equal slices.",
	            Tags = "count, spectral, generic",
	           	Author = "woei"
	           )]
	public class XAttributeOccurrenceNode: Occurrence<XAttribute> 
	{
		//uncomment this method to override the equals directly if you can't or dont want to override it for the class
//		public override bool Equals(XAttribute a, XAttribute b)
//		{
//			return a.Equals(b);
//		}
	}
	
	#endregion SpreadOps

}

