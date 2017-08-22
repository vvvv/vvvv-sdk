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
        public XDocumentFrameDelayNode() : base(XDocumentCopier.Default) { }
    }

    class XDocumentCopier : Copier<XDocument>
    {
        public static readonly XDocumentCopier Default = new XDocumentCopier();

        public override XDocument Copy(XDocument value)
        {
            if (value == null) return null;
            return new XDocument(value);
        }
    }
}

