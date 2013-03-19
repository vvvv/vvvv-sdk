using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CefGlue;
using System.Xml.Linq;

namespace VVVV.Nodes.Texture.HTML
{
    internal class DomVisitor : CefDomVisitor
    {
        private readonly HTMLTextureRenderer FRenderer;

        public DomVisitor(HTMLTextureRenderer renderer)
        {
            FRenderer = renderer;
        }

        protected override void Visit(CefDomDocument document)
        {
            using (var xmlReader = new CefXmlReader(document))
            {
                try
                {
                    Document = XDocument.Load(xmlReader);
                }
                catch (Exception e)
                {
                    Exception = e;
                }
            }
        }

        public XDocument Document { get; private set; }
        public Exception Exception { get; private set; }
    }
}
