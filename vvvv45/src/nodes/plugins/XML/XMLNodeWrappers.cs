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
    public class XMLElementSplitNode : IPluginEvaluate
    {
        [Input("Element")]
        public IDiffSpread<XElement> Element;
        [Output("Name")]
        public ISpread<string> Name;
        [Output("Value")]
        public ISpread<string> Value;
        [Output("Childs")]
        public ISpread<ISpread<XElement>> Childs;
        [Output("Child Count")]
        public ISpread<int> ChildCount;
        [Output("Attributes")]
        public ISpread<ISpread<XAttribute>> Attributes;
        [Output("Attribute Count")]
        public ISpread<int> AttributeCount;
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
            ChildCount.SliceCount = spreadMax;
            Attributes.SliceCount = spreadMax;
            AttributeCount.SliceCount = spreadMax;
            DocumentRoot.SliceCount = spreadMax;
            Parent.SliceCount = spreadMax;
            Next.SliceCount = spreadMax;
            NodeType.SliceCount = spreadMax;

            for (int i = 0; i < spreadMax; i++)
            {
                string name;
                string value;
                ISpread<XElement> childs;
                int childCount;
                ISpread<XAttribute> attributes;
                int attributeCount;
                XElement documentRoot;
                XElement parent;
                XElement next;
                XmlNodeType nodeType;

                XMLNodes.Split(Element[i], out name, out value, out childs, out childCount, out attributes, out attributeCount,
                    out documentRoot, out parent, out next, out nodeType);

                Name[i] = name;
                Value[i] = value;
                Childs[i] = childs;
                ChildCount[i] = childCount;
                Attributes[i] = attributes;
                AttributeCount[i] = attributeCount;
                DocumentRoot[i] = documentRoot;
                Parent[i] = parent;
                Next[i] = next;
                NodeType[i] = nodeType;
            }
        }
    }

    [PluginInfo(Name = "Attribute", Category = "XML", Version = "Split")]
    public class XMLAttributeSplitNode : IPluginEvaluate
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
