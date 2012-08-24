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

    [PluginInfo(Name = "Element", Category = "XML", Version = "Join")]
    public class ElementJoinNode : IPluginEvaluate
    {
        [Input("Name", DefaultString = "MyTag")]
        public IDiffSpread<string> Name;
        [Input("Value")]
        public IDiffSpread<string> Value;
        [Input("Childs")]
        public IDiffSpread<ISpread<XElement>> Childs;
        [Input("Attributes")]
        public IDiffSpread<ISpread<XAttribute>> Attributes;

        [Output("Element")]
        public ISpread<XElement> Element;

        public void Evaluate(int spreadMax)
        {
            if (!SpreadUtils.AnyChanged(Name, Value, Childs, Attributes)) return;

            spreadMax = Name
                .CombineWith(Value)
                .CombineWith(Childs)
                .CombineWith(Attributes);

            Element.SliceCount = spreadMax;

            for (int i = 0; i < spreadMax; i++)
            {
                var element = new XElement(Name[i], Value[i]);
                
                // clone attributes on the fly if they are already rooted somewhere
                var attribs = Attributes[i].Select(a => a.Parent != null ? new XAttribute(a) : a).ToArray();
                element.Add(attribs);
                
                // clone elements on the fly if they are already rooted somewhere
                element.Add(Childs[i].Select(e => e.Parent != null ? new XElement(e) : e).ToArray());

                Element[i] = element;
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

    [PluginInfo(Name = "Attribute", Category = "XML", Version = "Join")]
    public class AttributeJoinNode : IPluginEvaluate
    {
        [Input("Name")]
        public IDiffSpread<string> Name;
        [Input("Value", DefaultString = "MyAttribute")]
        public IDiffSpread<string> Value;
        [Output("Attribute")]
        public ISpread<XAttribute> Attribute;

        public void Evaluate(int spreadMax)
        {
            if (!SpreadUtils.AnyChanged(Name, Value)) return;

            Attribute.SliceCount = spreadMax;

            for (int i = 0; i < spreadMax; i++)
            {
                Attribute[i] = new XAttribute(Name[i], Value[i]);
            }
        }
    }

    [PluginInfo(Name = "GetElements", Category = "XML", Version = "ByName")]
    public class GetElementsNode : IPluginEvaluate
    {
        [Input("Element")]
        public IDiffSpread<XElement> Element;

        [Input("Name", DefaultString = "MyChildTag")]
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

        [Input("XPath", DefaultString = "MyChildTag/MyChildsChildTag")]
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

        [Input("XPath", DefaultString = "MyChildTag/@OneOfItsAttributes")]
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
    public class XMLAsElementNode : IPluginEvaluate
    {
        [Input("XML")]
        public IDiffSpread<string> XML;

        [Output("Element")]
        public ISpread<XElement> RootElement;

        [Output("Document")]
        public ISpread<XDocument> Document;

        public void Evaluate(int spreadMax)
        {
            if (!XML.IsChanged) return;

            RootElement.SliceCount = spreadMax;
            Document.SliceCount = spreadMax;

            for (int i = 0; i < spreadMax; i++)
            {
                XElement element;
                XDocument dom;
                XMLNodes.AsElement(XML[i], out dom, out element);
                Document[i] = dom;
                RootElement[i] = element;
            }
        }
    }

    //[PluginInfo(Name = "AsString", Category = "XML Element")]
    //public class ElementAsXMLNode : IPluginEvaluate
    //{
    //    [Input("Element")]
    //    public IDiffSpread<XElement> Element;

    //    [Output("XML")]
    //    public ISpread<string> XML;

    //    public void Evaluate(int spreadMax)
    //    {
    //        if (!Element.IsChanged) return;

    //        XML.SliceCount = spreadMax;

    //        for (int i = 0; i < spreadMax; i++)
    //        {
    //            XML[i] = XMLNodes.AsString(Element[i]);
    //        }
    //    }
    //}

    [PluginInfo(Name = "AsString", Category = "Object")]
    public class AsStringNode : IPluginEvaluate
    {
        [Input("Object")]
        public IDiffSpread<Object> Object;

        [Output("String")]
        public ISpread<string> _String;

        public void Evaluate(int spreadMax)
        {
            if (!Object.IsChanged) return;

            _String.SliceCount = spreadMax;

            for (int i = 0; i < spreadMax; i++)
            {
                var o = Object[i];
                _String[i] = o != null ? o.ToString() : "";
            }
        }
    }


    [PluginInfo(Name = "Validate", Category = "XML RelaxNG")]
    public class RelaxNGValidateNode : IPluginEvaluate
    {
        [Input("Xml File", StringType = StringType.Filename, FileMask = "XML (*.xml)|*.xml")]
        public IDiffSpread<string> XmlFile;

        [Input("Rng File", StringType = StringType.Filename, FileMask = "RNC (*.rnc)|*.rnc|RNG (*.rng)|*rng")]
        public IDiffSpread<string> RngFile;

        [Output("Is Valid")]
        public ISpread<bool> Valid;

        [Output("Message")]
        public ISpread<string> Message;

        public void Evaluate(int spreadMax)
        {
            if (!XmlFile.IsChanged && !RngFile.IsChanged) return;

            Message.SliceCount = spreadMax;
            Valid.SliceCount = spreadMax;

            for (int i = 0; i < spreadMax; i++)
            {
                var message = XMLNodes.RelaxNGValidate(XmlFile[i], RngFile[i]);
                Message[i] = message;
                Valid[i] = message == "";
            }
        }
    }


}
