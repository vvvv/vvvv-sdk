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

        [Config("Element Name", DefaultString = "MyElement", IsSingle = true)]
        public IDiffSpread<string> ElementNamePin;

        [Config("Attribute Names", DefaultString = "AttribA, AttribB", IsSingle = true)]
        public IDiffSpread<string> AttributeNamesPin;

        [Input("Element")]
        public IDiffSpread<XElement> Element;

        [Output("Elements")]
        public ISpread<ISpread<XElement>> Elements;

        [Import()]
        IIOFactory IOFactory;

        XName ElementName;
        string[] AttributeNames;
        List<AttributeInfo> AttributeInfos = new List<AttributeInfo>();
        private bool ConfigChanged;

        public void OnImportsSatisfied()
        {
            ElementNamePin.Changed += ElementName_Changed;
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
                        AttributeContainer = IOFactory.CreateIOContainer<ISpread<string>>(new OutputAttribute(attributeName + " Value")),
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

        void ElementName_Changed(IDiffSpread<string> spread)
        {
            ElementName = XName.Get(ElementNamePin[0]);

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
                Elements[i] = element != null ? GetElementsByName(element, ElementName) : XMLNodes.NoElements;
            }

            var allElements = Elements.SelectMany(spread => spread).ToArray();

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

            ConfigChanged = false;
        }
    }
}
