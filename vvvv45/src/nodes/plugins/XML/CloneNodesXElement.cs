using System;
using VVVV.PluginInterfaces.V2;
using VVVV.Nodes.Generic;
using System.Xml;
using System.Xml.Linq;

namespace VVVV.Nodes
{

    [PluginInfo(Name = "FrameDelay",
	            Category = "XElement",
                Help = "Delays the input value one calculation frame.",
	            Tags = "generic"
	           )]
    public class XElementFrameDelayNode : FrameDelayNode<XElement>
    {
        protected override XElement CloneSlice(XElement slice)
        {
            if (slice == null) return null;
            return new XElement(slice);
        }
    }

}

