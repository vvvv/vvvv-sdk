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
    [PluginInfo(Name = "Element", Category = "XML", Version = "Split")]
    public class ElementSplitNode : IPluginEvaluate
    {
        [Input("Element")]
        public IDiffSpread<XElement> Element;
        [Output("Name")]
        public ISpread<string> Name;
        [Output("Value")]
        public ISpread<string> Value;
        [Output("Childs")]
        public ISpread<ISpread<XElement>> Childs;
        [Output("Attributes")]
        public ISpread<ISpread<XAttribute>> Attributes;
        [Output("Document Root")]
        public ISpread<XElement> DocumentRoot;
        [Output("Parent")]
        public ISpread<XElement> Parent;
        [Output("Next")]
        public ISpread<XElement> Next;
        [Output("Node Type")]
        public ISpread<XmlNodeType> NodeType;

        public void Evaluate(int spreadMax)
        {
            if (!Element.IsChanged) return;

            Name.SliceCount = spreadMax;
            Value.SliceCount = spreadMax;
            Childs.SliceCount = spreadMax;
            Attributes.SliceCount = spreadMax;
            DocumentRoot.SliceCount = spreadMax;
            Parent.SliceCount = spreadMax;
            Next.SliceCount = spreadMax;
            NodeType.SliceCount = spreadMax;

            for (int i = 0; i < spreadMax; i++)
            {
                string name;
                string value;
                ISpread<XElement> childs;
                ISpread<XAttribute> attributes;
                XElement documentRoot;
                XElement parent;
                XElement next;
                XmlNodeType nodeType;

                XMLNodes.Split(Element[i], out name, out value, out childs, out attributes,
                    out documentRoot, out parent, out next, out nodeType);

                Name[i] = name;
                Value[i] = value;
                Childs[i] = childs;
                Attributes[i] = attributes;
                DocumentRoot[i] = documentRoot;
                Parent[i] = parent;
                Next[i] = next;
                NodeType[i] = nodeType;
            }
        }
    }

    [PluginInfo(Name = "Attribute", Category = "XML", Version = "Split")]
    public class AttributeSplitNode : IPluginEvaluate
    {
        [Input("Attribute")]
        public IDiffSpread<XAttribute> Attribute;
        [Output("Name")]
        public ISpread<string> Name;
        [Output("Value")]
        public ISpread<string> Value;
        [Output("Parent")]
        public ISpread<XElement> Parent;
        [Output("Node Type")]
        public ISpread<XmlNodeType> NodeType;

        public void Evaluate(int spreadMax)
        {
            if (!Attribute.IsChanged) return;

            Name.SliceCount = spreadMax;
            Value.SliceCount = spreadMax;
            Parent.SliceCount = spreadMax;
            NodeType.SliceCount = spreadMax;

            for (int i = 0; i < spreadMax; i++)
            {
                string name;
                string value;
                XElement parent;
                XmlNodeType nodeType;

                XMLNodes.Split(Attribute[i], out name, out value, out parent, out nodeType);

                Name[i] = name;
                Value[i] = value;
                Parent[i] = parent;
                NodeType[i] = nodeType;
            }
        }
    }

    [PluginInfo(Name = "GetElements", Category = "XML", Version = "ByName")]
    public class GetElementsNode : IPluginEvaluate
    {
        [Input("Element")]
        public IDiffSpread<XElement> Element;

        [Input("Name", DefaultString = "MyTag")]
        public IDiffSpread<string> Name;

        [Output("Elements")]
        public ISpread<ISpread<XElement>> Elements;

        public void Evaluate(int spreadMax)
        {
            if (!Element.IsChanged && !Name.IsChanged) return;

            Elements.SliceCount = spreadMax;

            for (int i = 0; i < spreadMax; i++)
            {
                Elements[i] = Element[i].GetElementsByName(Name[i]);
            }
        }
    }

    [PluginInfo(Name = "GetAttributes", Category = "XML", Version = "ByName")]
    public class GetAttributesNode : IPluginEvaluate
    {
        [Input("Element")]
        public IDiffSpread<XElement> Element;

        [Input("Name", DefaultString = "MyAttribute")]
        public IDiffSpread<string> Name;

        [Output("Attributes")]
        public ISpread<ISpread<XAttribute>> Attributes;

        public void Evaluate(int spreadMax)
        {
            if (!Element.IsChanged && !Name.IsChanged) return;

            Attributes.SliceCount = spreadMax;

            for (int i = 0; i < spreadMax; i++)
            {
                Attributes[i] = Element[i].GetAttributesByName(Name[i]);
            }
        }
    }

    [PluginInfo(Name = "GetElements", Category = "XML", Version = "ByXPath")]
    public class GetElementsByXPathNode : IPluginEvaluate
    {
        [Input("Element")]
        public IDiffSpread<XElement> Element;

        [Input("XPath", DefaultString = "/MyTag")]
        public IDiffSpread<string> XPath;

        [Output("Elements")]
        public ISpread<ISpread<XElement>> Elements;

        [Output("Error Message")]
        public ISpread<string> ErrorMessage;

        public void Evaluate(int spreadMax)
        {
            if (!Element.IsChanged && !XPath.IsChanged) return;

            Elements.SliceCount = spreadMax;
            ErrorMessage.SliceCount = spreadMax;

            for (int i = 0; i < spreadMax; i++)
            {
                string error; 
                Elements[i] = Element[i].GetElementsByXPath(XPath[i], out error);
                ErrorMessage[i] = error;
            }
        }
    }

    [PluginInfo(Name = "GetAttributes", Category = "XML", Version = "ByXPath")]
    public class GetAttributesByXPathNode : IPluginEvaluate
    {
        [Input("Element")]
        public IDiffSpread<XElement> Element;

        [Input("XPath", DefaultString = "/MyTag/@MyAttribute")]
        public IDiffSpread<string> XPath;

        [Output("Attributes")]
        public ISpread<ISpread<XAttribute>> Attributes;

        [Output("Error Message")]
        public ISpread<string> ErrorMessage;

        public void Evaluate(int spreadMax)
        {
            if (!Element.IsChanged && !XPath.IsChanged) return;

            Attributes.SliceCount = spreadMax;
            ErrorMessage.SliceCount = spreadMax;

            for (int i = 0; i < spreadMax; i++)
            {
                string error;
                Attributes[i] = Element[i].GetAttributesByXPath(XPath[i], out error);
                ErrorMessage[i] = error;
            }
        }
    }

    [PluginInfo(Name = "AsElement", Category = "XML")]
    public class XMLAsXElementNode : IPluginEvaluate
    {
        [Input("XML")]
        public IDiffSpread<string> XML;

        [Output("Element")]
        public ISpread<XElement> Element;

        public void Evaluate(int spreadMax)
        {
            if (!XML.IsChanged) return;

            Element.SliceCount = spreadMax;

            for (int i = 0; i < spreadMax; i++)
            {
                XElement element;

                XMLNodes.AsElement(XML[i], out element);

                Element[i] = element;
            }
        }
    }
}
