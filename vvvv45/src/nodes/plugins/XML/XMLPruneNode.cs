using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.Core;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using VVVV.PluginInterfaces.V2;
using System.ComponentModel.Composition;


namespace VVVV.Nodes.XML
{
    [PluginInfo(Name = "Prune", Category = "XML")]
    public class XMLPruneNode : IPluginEvaluate, IPartImportsSatisfiedNotification
    {
        class AttributeInfo
        {
            public IIOContainer<ISpread<string>> AttributeContainer;
            public IIOContainer<ISpread<bool>> AttributeExistsContainer;
            public ISpread<string> AttributeOutputPin { get { return AttributeContainer.IOObject; } }
            public ISpread<bool> AttributeExistsOutputPin { get { return AttributeExistsContainer.IOObject; } }
            public string AttributeName;
        }

        class ContentElementInfo
        {
            public IIOContainer<ISpread<string>> ContentElementContainer;
            public IIOContainer<ISpread<bool>> ContentElementExistsContainer;
            public ISpread<string> ContentElementOutputPin { get { return ContentElementContainer.IOObject; } }
            public ISpread<bool> ContentElementExistsOutputPin { get { return ContentElementExistsContainer.IOObject; } }
            public string ContentElementName;
        }

        class ChildElementInfo
        {
            public IIOContainer<ISpread<XElement>> ChildElementContainer;
            public IIOContainer<ISpread<bool>> ChildElementExistsContainer;
            public ISpread<XElement> ChildElementOutputPin { get { return ChildElementContainer.IOObject; } }
            public ISpread<bool> ChildElementExistsOutputPin { get { return ChildElementExistsContainer.IOObject; } }
            public string ChildElementName;
        }

        // XElementContainer = IOFactory.CreateIOContainer<ISpread<XElement>>(
        //new OutputAttribute(elementName + " (XElement)")),

#pragma warning disable 0649
        [Config("Base Element Name", DefaultString = "MyElement", IsSingle = true)]
        public IDiffSpread<string> BaseElementNamePin;

        [Config("Attribute Names", DefaultString = "AttribA, AttribB", IsSingle = true)]
        public IDiffSpread<string> AttributeNamesPin;

        [Config("ContentElement Names", DefaultString = "ElementA, ElementB", IsSingle=true)]
        public IDiffSpread<string> ContentElementNamesPin;

        [Config("ChildElement Names", DefaultString = "ChildA, ChildB", IsSingle = true)]
        public IDiffSpread<string> ChildElementNamesPin;


        [Input("Element")]
        public IDiffSpread<XElement> Element;

        [Input("NoRootTag", IsToggle=true, DefaultBoolean=false, IsSingle=true)]
        public IDiffSpread<bool> FInNoRootTag;


        [Output("Elements")]
        public ISpread<ISpread<XElement>> Elements;

        [Import()]
        IIOFactory IOFactory; 
#pragma warning restore

        XName BaseElementName;
        string[] AttributeNames;
        string[] ContentElementNames;
        string[] ChildElementNames;
        List<AttributeInfo> AttributeInfos = new List<AttributeInfo>();
        List<ContentElementInfo> ContentElementInfos = new List<ContentElementInfo>();
        List<ChildElementInfo> ChildElementInfos = new List<ChildElementInfo>();

        private bool ConfigChanged;
        private bool NoRootTag = false;

        public void OnImportsSatisfied()
        {
            BaseElementNamePin.Changed += BaseElementName_Changed;
            AttributeNamesPin.Changed += AttributeNamesPin_Changed;
            ContentElementNamesPin.Changed += ContentElementNamesPin_Changed;
            ChildElementNamesPin.Changed += ChildElementNamesPin_Changed;
        }

        // on AttributeNames changed
        void AttributeNamesPin_Changed(IDiffSpread<string> spread)
        {
            if (spread.SliceCount == 0) return;

            AttributeNames = AttributeNamesPin[0].Split(',').ToList().Select(s => s.Trim()).Where(s => s.Length > 0).ToArray();

            //add new pins
            foreach (var attributeName in AttributeNames)
            {
                if (!AttributeInfos.Any(info => info.AttributeName == attributeName))
                {
                    var outputInfo = new AttributeInfo()
                    {
                        AttributeName = attributeName,
                        AttributeContainer = IOFactory.CreateIOContainer<ISpread<string>>(new OutputAttribute(attributeName)),
                        AttributeExistsContainer = IOFactory.CreateIOContainer<ISpread<bool>>(
                            new OutputAttribute(attributeName + " Available") { Visibility = PinVisibility.Hidden }
                        ),
                    };
                    AttributeInfos.Add(outputInfo);
                }
            }

            //remove obsolete pins
            foreach (var outputInfo in AttributeInfos.ToArray())
                if (!AttributeNames.Contains(outputInfo.AttributeName))
                {
                    AttributeInfos.Remove(outputInfo);
                    outputInfo.AttributeContainer.Dispose();
                    outputInfo.AttributeExistsContainer.Dispose();
                }

            ConfigChanged = true;
        }
        
        // on ContentElementNames changed 
        void ContentElementNamesPin_Changed(IDiffSpread<string> spread)
        {
            if (spread.SliceCount == 0) return;

            ContentElementNames = ContentElementNamesPin[0].Split(',').ToList().Select(s => s.Trim()).Where(s => s.Length > 0).ToArray();

            // add new pins
            foreach (var elementName in ContentElementNames)
            {
                if (!ContentElementInfos.Any(info => info.ContentElementName == elementName)) 
                {
                    var outputInfo = new ContentElementInfo()
                    {
                        ContentElementName = elementName,
                        ContentElementContainer = IOFactory.CreateIOContainer<ISpread<string>>(new OutputAttribute(elementName)),
                        ContentElementExistsContainer = IOFactory.CreateIOContainer<ISpread<bool>>(
                            new OutputAttribute(elementName + " Available") { Visibility = PinVisibility.Hidden }
                        ),
                    };
                    ContentElementInfos.Add(outputInfo);
                }
            }

            // remove obsolete pins
            foreach (var outputInfo in ContentElementInfos.ToArray())
            {
                if (!ContentElementNames.Contains(outputInfo.ContentElementName)) 
                {
                    ContentElementInfos.Remove(outputInfo);
                    outputInfo.ContentElementContainer.Dispose();
                    outputInfo.ContentElementExistsContainer.Dispose();
                }
            }

            ConfigChanged = true;
        }

        // on ChildElementNames changed
        void ChildElementNamesPin_Changed(IDiffSpread<string> spread)
        {
            if (spread.SliceCount == 0) return;

            ChildElementNames = ChildElementNamesPin[0].Split(',').ToList().Select(s => s.Trim()).Where(s => s.Length > 0).ToArray();

            // add new pins
            foreach (var elementName in ChildElementNames)
            {
                if (!ChildElementInfos.Any(info => info.ChildElementName == elementName))
                {
                    var outputInfo = new ChildElementInfo()
                    {
                        ChildElementName = elementName,
                        ChildElementContainer = IOFactory.CreateIOContainer<ISpread<XElement>>(new OutputAttribute(elementName)),
                        ChildElementExistsContainer = IOFactory.CreateIOContainer<ISpread<bool>>(
                            new OutputAttribute(elementName + " Available") { Visibility = PinVisibility.Hidden }
                        ),
                    };
                    ChildElementInfos.Add(outputInfo);
                }
            }

            // remove obsolete pins
            foreach (var outputInfo in ChildElementInfos.ToArray())
            {
                if (!ChildElementNames.Contains(outputInfo.ChildElementName))
                {
                    ChildElementInfos.Remove(outputInfo);
                    outputInfo.ChildElementContainer.Dispose();
                    outputInfo.ChildElementExistsContainer.Dispose();
                }
            }
            ConfigChanged = true;
        }

        // on BaseElementName changed
        void BaseElementName_Changed(IDiffSpread<string> spread)
        {
            try
            {
                BaseElementName = XName.Get(BaseElementNamePin[0]);
                ConfigChanged = true;
            }
            catch (ArgumentException ae)
            {
                BaseElementName = null;
                ConfigChanged = true;
            }
        }

        // currently not in use
        static ISpread<XElement> GetElementsByName(XElement element, XName name)
        {
            if (element != null)
                return element.Elements(name).ToSpread();
            else
                return XMLNodes.NoElements;
        }

        // using xPath expression for selecting elements
        static ISpread<XElement> GetElementsByXPathQuery(XElement xElement, string xPath, bool noRootTag)
        {
            if (xElement != null)
            {
                if (noRootTag)
                    return xElement.XPathSelectElements("self::*").ToSpread();
                else
                    return xElement.XPathSelectElements(xPath).ToSpread();
            }
            else
                return XMLNodes.NoElements;
        }

        // using xPath expression for selecting single element
        static XElement GetElementByXPathQuery(XElement xElement, string xPath) 
        {
            if (xElement != null)
                return xElement.XPathSelectElements(xPath).FirstOrDefault();
            else
                return XMLNodes.NoElements.FirstOrDefault();
        }

        // method called each frame in vvvv
        public void Evaluate(int SpreadMax)
        {
            if (FInNoRootTag.IsChanged)
            {
                NoRootTag = FInNoRootTag[0];
                ConfigChanged = true;
            }

            if (SpreadMax == 0) return;

            if (!Element.IsChanged && !ConfigChanged) return;

            Elements.SliceCount = SpreadMax;

            for (int i = 0; i < SpreadMax; i++)
            {
                var element = Element[i];
                if (BaseElementName != null)
                    Elements[i] = element != null ? GetElementsByXPathQuery(element, BaseElementName.LocalName, NoRootTag) : XMLNodes.NoElements;
            }

            var allElements = Elements.SelectMany(spread => spread).ToArray();

            // process attributes
            foreach (var attributeInfo in AttributeInfos)
            {
                attributeInfo.AttributeOutputPin.SliceCount = allElements.Length;
                attributeInfo.AttributeExistsOutputPin.SliceCount = allElements.Length;

                int i = 0;
                foreach (var element in allElements)
                {
                    var attributes = element.Attributes(attributeInfo.AttributeName);
                    var attribute = attributes.FirstOrDefault();

                    attributeInfo.AttributeOutputPin[i] = attribute != null ? attribute.Value : "";
                    attributeInfo.AttributeExistsOutputPin[i] = attribute != null;
                    i++;
                }
            }

            // process content elements
            foreach (var contentElementInfo in ContentElementInfos)
            {
                contentElementInfo.ContentElementOutputPin.SliceCount = allElements.Length;
                contentElementInfo.ContentElementExistsOutputPin.SliceCount = allElements.Length;

                int i = 0;
                foreach (var element in allElements)
                {
                    var elements = element.Elements(contentElementInfo.ContentElementName);
                    var el = elements.FirstOrDefault();

                    contentElementInfo.ContentElementOutputPin[i] = el != null ? el.Value : "";
                    contentElementInfo.ContentElementExistsOutputPin[i] = el != null;
                    i++;
                }
            }

            // process child elements
            foreach (var childElementInfo in ChildElementInfos)
            {
                childElementInfo.ChildElementOutputPin.SliceCount = allElements.Length;
                childElementInfo.ChildElementExistsOutputPin.SliceCount = allElements.Length;

                int i = 0;
                foreach (var element in allElements)
                {
                    var elements = element.Elements(childElementInfo.ChildElementName);
                    var el = elements.FirstOrDefault();
                    childElementInfo.ChildElementOutputPin[i] = el != null ? GetElementByXPathQuery(element, childElementInfo.ChildElementName) : XMLNodes.NoElements.FirstOrDefault();
                    childElementInfo.ChildElementExistsOutputPin[i] = el != null;
                    i++;
                }
            }
            ConfigChanged = false;
        }
    }
}
