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
            FRenderer.OnVisitDom(document);
        }
    }
}
