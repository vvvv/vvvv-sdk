using System;
using VVVV.PluginInterfaces.V2;
using VVVV.Nodes.Generic;

namespace VVVV.Nodes
{
	//1.) do a 'replace all' for REPLACEME_CLASS with the name of your own type
	
	//2.) do a 'replace all' for NODECATEGORY to set the version and the class name prefix for all node (e.g. Value, Color...)

    //3.) you have to override the CloneSlice or the CloneSpread method for your type. overriding CloneSlice is easier, CloneSpread might allow some performance optimizations

    [PluginInfo(Name = "FrameDelay",
	            Category = "NODECATEGORY",
                Help = "Delays the input value one calculation frame.",
	            Tags = "generic"
	           )]
    public class NODECATEGORYFrameDelayNode : FrameDelayNode<REPLACEME_CLASS>
    {
        protected override REPLACEME_CLASS CloneSlice(REPLACEME_CLASS slice)
        {
            throw new NotImplementedException("You need to implement the Clone method");
        }
    }

}

