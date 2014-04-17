using System;
using VVVV.PluginInterfaces.V2;
using VVVV.Nodes.Generic;

namespace VVVV.Nodes
{
	//1.) do a 'replace all' for REPLACEME_CLASS with the name of your own type
	
	//2.) do a 'replace all' for NODECATEGORY to set the version and the class name prefix for all node (e.g. Value, Color...)
	
	//3.) you have to override the Distance method for your type

	[PluginInfo(Name = "NearestNeighbour",
	            Category = "Spreads",
	            Version = "NODECATEGORY",
	            Author = "vux"
	           )]
    public class NODECATEGORYNearestNeighbourNode : NearestNeighbour<REPLACEME_CLASS>
    {
		protected override double Distance(REPLACEME_CLASS a, REPLACEME_CLASS b)
		{
			return 0; //calculate distance between a and b
		}
    }
	

}

