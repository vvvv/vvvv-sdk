using System;
using VVVV.PluginInterfaces.V2;
using VVVV.Nodes.Generic;
using System.Xml;
using System.Xml.Linq;

namespace VVVV.Nodes
{

    [PluginInfo(Name = "FrameDelay",
	            Category = "XElement",
                Version = "Document",
                Help = "Delays the input value one calculation frame.",
	            Tags = "generic"
	           )]
    public class XDocumentFrameDelayNode : FrameDelayNode<XDocument>
    {
        protected override XDocument CloneSlice(XDocument slice)
        {
            if (slice == null) return null;
            return new XDocument(slice);
        }
    }

}

