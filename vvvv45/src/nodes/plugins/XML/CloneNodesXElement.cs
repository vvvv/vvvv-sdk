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
        public XElementFrameDelayNode() : base(XElementCopier.Default) { }
    }

    class XElementCopier : Copier<XElement>
    {
        public static readonly XElementCopier Default = new XElementCopier();

        public override XElement Copy(XElement value)
        {
            if (value == null) return null;
            return new XElement(value);
        }
    }
}

