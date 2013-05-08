using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.Core;
using System.Xml;
using System.Xml.Linq;
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

        class ElementInfo
        {
            public IIOContainer<ISpread<string>> ElementContainer;
            public IIOContainer<ISpread<bool>> ElementExistsContainer;
            public ISpread<string> ElementOutputPin { get { return ElementContainer.IOObject; } }
            public ISpread<bool> ElementExistsOutputPin { get { return ElementExistsContainer.IOObject; } }
            public string ElementName;
        }



#pragma warning disable 0649
        [Config("Base Element Name", DefaultString = "MyElement", IsSingle = true)]
        public IDiffSpread<string> BaseElementNamePin;

        [Config("Attribute Names", DefaultString = "AttribA, AttribB", IsSingle = true)]
        public IDiffSpread<string> AttributeNamesPin;

        [Config("Element Names", DefaultString = "ElementA, ElementB", IsSingle=true)]
        public IDiffSpread<string> ElementNamesPin;

        [Input("Element")]
        public IDiffSpread<XElement> Element;

        [Output("Elements")]
        public ISpread<ISpread<XElement>> Elements;

        [Import()]
        IIOFactory IOFactory; 
#pragma warning restore

        XName BaesElementName;
        string[] AttributeNames;
        string[] ElementNames;
        List<AttributeInfo> AttributeInfos = new List<AttributeInfo>();
        List<ElementInfo> ElementInfos = new List<ElementInfo>();
        private bool ConfigChanged;

        public void OnImportsSatisfied()
        {
            BaseElementNamePin.Changed += BaseElementName_Changed;
            ElementNamesPin.Changed += ElementNamesPin_Changed;
            AttributeNamesPin.Changed += AttributeNamesPin_Changed;
            
        }

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

        void ElementNamesPin_Changed(IDiffSpread<string> spread)
        {
            if (spread.SliceCount == 0) return;

            ElementNames = ElementNamesPin[0].Split(',').ToList().Select(s => s.Trim()).Where(s => s.Length > 0).ToArray();
            int z = 1;
            // add new pins
            foreach (var elementName in ElementNames)
            {
                if (!ElementInfos.Any(info => info.ElementName == elementName)) 
                {
                    var outputInfo = new ElementInfo()
                    {
                        ElementName = elementName,
                        ElementContainer = IOFactory.CreateIOContainer<ISpread<string>>( new OutputAttribute(elementName)),
                        ElementExistsContainer = IOFactory.CreateIOContainer<ISpread<bool>>(
                            new OutputAttribute(elementName + " Available") { Visibility = PinVisibility.Hidden }
                        ),
                    };
                    ElementInfos.Add(outputInfo);
                }
            }
            int a = 1;

            // remove obsolete pins
            foreach (var outputInfo in ElementInfos.ToArray())
            {
                if (!ElementNames.Contains(outputInfo.ElementName)) 
                {
                    ElementInfos.Remove(outputInfo);
                    outputInfo.ElementContainer.Dispose();
                    outputInfo.ElementExistsContainer.Dispose();
                }
            }

            ConfigChanged = true;
        }

        void BaseElementName_Changed(IDiffSpread<string> spread)
        {
            BaesElementName = XName.Get(BaseElementNamePin[0]);

            ConfigChanged = true;
        }

        static ISpread<XElement> GetElementsByName(XElement element, XName name)
        {
            if (element != null)
                return element.Elements(name).ToSpread();
            else
                return XMLNodes.NoElements;
        }

        public void Evaluate(int SpreadMax)
        {
            if (SpreadMax == 0) return;

            if (!Element.IsChanged && !ConfigChanged) return;

            Elements.SliceCount = SpreadMax;

            for (int i = 0; i < SpreadMax; i++)
            {
                var element = Element[i];
                Elements[i] = element != null ? GetElementsByName(element, BaesElementName) : XMLNodes.NoElements;
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

            // process elements
            foreach (var elementInfo in ElementInfos)
            {
                elementInfo.ElementOutputPin.SliceCount = allElements.Length;
                elementInfo.ElementExistsOutputPin.SliceCount = allElements.Length;

                int i = 0;
                foreach (var element in allElements)
                {
                    var elements = element.Elements(elementInfo.ElementName);
                    var el = elements.FirstOrDefault();

                    elementInfo.ElementOutputPin[i] = el != null ? el.Value : "";
                    elementInfo.ElementExistsOutputPin[i] = el != null;

                    i++;
                }
            }


            ConfigChanged = false;
        }
    }
}
