using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.Core;
using System.Xml;
using System.Xml.Linq;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Nodes.XML
{
    public static class XMLNodes
    {
        [Node]
        public static void Split(this XElement element, out string name, out string value, out ISpread<XElement> childs,
            out int childCount, out ISpread<XAttribute> attributes, out int attributeCount, out XElement documentRoot, out XElement parent,
            out XElement next, out XmlNodeType nodeType) //, out bool changed)
        {
            if (element != null)
            {
                name = element.Name.LocalName;
                value = element.Value;
                childs = element.Elements().ToSpread();
                childCount = childs.SliceCount;
                attributes = element.Attributes().ToSpread();
                attributeCount = attributes.SliceCount;
                if (element.Document != null)
                    documentRoot = element.Document.Root;
                else
                    documentRoot = null;
                parent = element.Parent;
                next = element.NextNode as XElement;
                nodeType = element.NodeType;
                //changed = element.ch;
            }
            else
            {
                name = "";
                value = "";
                childs = new Spread<XElement>(); // should access a static empty spread
                childCount = 0;
                attributes = new Spread<XAttribute>();
                attributeCount = 0;
                documentRoot = null;
                parent = null;
                next = null;
                nodeType = default(XmlNodeType);
                //changed = false;
            }
        }

        [Node]
        public static void Split(this XAttribute attribute, out string name, out string value, out XElement parent, out XmlNodeType nodeType)
        {
            if (attribute != null)
            {
                name = attribute.Name.LocalName;
                value = attribute.Value;
                parent = attribute.Parent;
                nodeType = attribute.NodeType;
            }
            else
            {
                name = "";
                value = "";
                parent = null;
                nodeType = default(XmlNodeType);
            }
        }

        [Node]
        public static void AsElement(this string xml, out XElement element)
        {
            try
            {
                XDocument doc = XDocument.Parse(xml);
                element = doc.Root;
            }
            catch
            {
                element = null;
            }
        }
    }
}
