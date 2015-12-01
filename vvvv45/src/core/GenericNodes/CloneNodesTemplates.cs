using System;
using VVVV.PluginInterfaces.V2;
using VVVV.Nodes.Generic;

namespace VVVV.Nodes
{
	//1.) do a 'replace all' for REPLACEME_CLASS with the name of your own type
	
	//2.) do a 'replace all' for NODECATEGORY to set the version and the class name prefix for all node (e.g. Value, Color...)

    //3.) you have to override the Copy or the CopySpread method for your type. overriding Copy is easier, CopySpread might allow some performance optimizations

    [PluginInfo(Name = "FrameDelay",
	            Category = "NODECATEGORY",
                Help = "Delays the input value one calculation frame.",
	            Tags = "generic"
	           )]
    public class REPLACEME_CLASSFrameDelayNode : FrameDelayNode<REPLACEME_CLASS>
    {
        public REPLACEME_CLASSFrameDelayNode() : base(REPLACEME_CLASSCopier.Default) { }
    }

    class REPLACEME_CLASSCopier : Copier<REPLACEME_CLASS>
    {
        public static readonly REPLACEME_CLASSCopier Default = new REPLACEME_CLASSCopier();

        public override REPLACEME_CLASS Copy(REPLACEME_CLASS value)
        {
            throw new NotImplementedException("You need to implement the Copy method");
        }
    }
}

