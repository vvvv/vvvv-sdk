using System;
using VVVV.PluginInterfaces.V2;
using VVVV.Nodes.Generic;

namespace VVVV.Nodes
{
	//1.) do a 'replace all' for REPLACEME_CLASS with the name of your own type
	
	//2.) do a 'replace all' for NODECATEGORY to set the version and the class name prefix for all node (e.g. Value, Color...)
	
	//3.) you might also override the Equals method of your type which is used to check whether two instances are the same

	#region SingleValue
	
	[PluginInfo(Name = "Equals", 
	            Category = "NODECATEGORY",
	            Help = "returns true if the values at the inputs are equal",
	            Tags = "compare, same"
	           )]
	public class REPLACEME_CLASSEqualsNode: Equals<REPLACEME_CLASS> {}
	
	#endregion SingleValue
	
	#region SpreadOps
	
	[PluginInfo(Name = "Occurrence", 
	            Category = "NODECATEGORY",
	            Help = "Counts the occurrence of equal slices.",
	            Tags = "count, spectral, generic",
	           	Author = "woei"
	           )]
	public class REPLACEME_CLASSOccurrenceNode: Occurrence<REPLACEME_CLASS> 
	{
		//uncomment this method to override the equals directly if you can't or dont want to override it for the class
//		public override bool Equals(REPLACEME_CLASS a, REPLACEME_CLASS b)
//		{
//			return a.Equals(b);
//		}
	}
	
	#endregion SpreadOps

}

