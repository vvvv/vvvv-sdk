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
        public XAttributeFrameDelayNode() : base(XAttributeCopier.Default) { }
    }

    class XAttributeCopier : Copier<XAttribute>
    {
        public static readonly XAttributeCopier Default = new XAttributeCopier();

        public override XAttribute Copy(XAttribute value)
        {
            if (value == null) return null;
            return new XAttribute(value);
        }
    }
}

