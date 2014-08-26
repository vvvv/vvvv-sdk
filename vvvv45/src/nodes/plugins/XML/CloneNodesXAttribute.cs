using System;
using VVVV.PluginInterfaces.V2;
using VVVV.Nodes.Generic;
using System.Xml;
using System.Xml.Linq;

namespace VVVV.Nodes
{

    [PluginInfo(Name = "FrameDelay",
	            Category = "XElement",
                Version = "Attribute",
                Help = "Delays the input value one calculation frame.",
	            Tags = "generic"
	           )]
    public class XAttributeFrameDelayNode : FrameDelayNode<XAttribute>
    {
        protected override XAttribute CloneSlice(XAttribute slice)
        {
            if (slice == null) return null;
            return new XAttribute(slice);
        }
    }

}

