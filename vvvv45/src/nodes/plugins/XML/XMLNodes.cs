using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.Core;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using VVVV.PluginInterfaces.V2;
using System.Diagnostics;
using System.Collections;
using System.IO;
using Commons.Xml.Relaxng;
using Commons.Xml.Relaxng.Rnc;

namespace VVVV.Nodes.XML
{
    public enum Validation
    {
        None,
        Dtd,
        Schema
    }

    public static class XMLNodes
    {
        public static readonly ISpread<XElement> NoElements = new Spread<XElement>();
        public static readonly ISpread<XAttribute> NoAttributes = new Spread<XAttribute>();

        [Node]
        public static void Split(this XElement element, out string name, out string value, out string deepvalue, out ISpread<XElement> childs,
            out ISpread<XAttribute> attributes, out XElement documentRoot, out XElement parent,
            out XElement next, out XmlNodeType nodeType) //, out bool changed)
        {
            if (element != null)
            {
                name = element.Name.LocalName;

                //deepvalue now gets you the legacy value out implementation: all the text contents of all text nodes of this element and its childs get concatenated
                //it is a strange default implementation for value, since it blurrs every NESTED text value into one big string, which is not related to how the value is set.
                deepvalue = element.Value;
                // anyway: we output this thing as deepvalue to get legacy patches working.

                //now here comes the new implementation:                
                    //http://stackoverflow.com/questions/4251215/how-to-get-xelements-value-and-not-value-of-all-child-nodes
                    //value = string.Concat(element.Nodes().OfType<XText>().Select(t => t.Value));
                
                    //still not satisfied. trim spaces in front and at the back.
                    //value = string.Concat(element.Nodes().OfType<XText>().Select(t => t.Value)).Trim();

                //still not satisfied. actually we don't want to trim blindly. 
                //we just want to pick that single text node (if existant), that has more than just whitespace in it and output it without modification.
                //this covers the standard case in which text value is NOT spread all over the element, separated by child nodes.
                //when found it is returned without modification. we shouldn't delete information for no reason.
                var textNode = element.Nodes().OfType<XText>().FirstOrDefault(t => !string.IsNullOrWhiteSpace(t.Value));
                // if there is just none, let's output the first text nodes' value in all its glory.
                textNode = textNode ?? element.Nodes().OfType<XText>().FirstOrDefault();
                //in summary: out value now just supports the std. case, where only ONE text value is placed into the xelement. 
                //(join nodes shouldn't construct anything else anyway)
                value = textNode != null ? textNode.Value : "";
                                
                childs = element.Elements().ToSpread();
                attributes = element.Attributes().ToSpread();
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
                deepvalue = "";
                childs = NoElements;
                attributes = NoAttributes;
                documentRoot = null;
                parent = null;
                next = null;
                nodeType = default(XmlNodeType);
                //changed = false;
            }
        }

        [Node]
        public static ISpread<XElement> GetElementsByName(this XElement element, string name)
        {
            if (element != null)
            {
                string prefix = null;
                string localName = name;
                var s = name.Split(':');
                if (s.Length > 1)
                {
                    prefix = s[0];
                    localName = s[1];
                }
                XNamespace ns = null;
                if (prefix != null)
                    ns = element.GetNamespaceOfPrefix(prefix);
                if (ns == null)
                    ns = element.GetDefaultNamespace();
                return element.Elements(ns + localName).ToSpread();
            }
            else
                return NoElements;
        }

        [Node]
        public static ISpread<XAttribute> GetAttributesByName(this XElement element, string name)
        {
            if (element != null)
            {
                var n = XName.Get(name);
                return element.Attributes(n).ToSpread();
            }
            else
                return NoAttributes;
        }

        [Node]
        public static ISpread<T> GetElementsByXPath<T>(this XElement element, string xPath, string nodesName, IXmlNamespaceResolver nameSpaceResolver, out string errorMessage) where T : XObject
        {
            if (element != null)
            {
                try
                {
                    var obj = (IEnumerable)element.XPathEvaluate(xPath, nameSpaceResolver);
                    var whatINeed = obj.Cast<T>();
                    errorMessage = "";
                    return whatINeed.ToSpread();
                }
                catch
                {
                    errorMessage = string.Format("couldn't map xpath '{0}' to xml {1}", xPath, nodesName);
                    return null;
                }
            }
            else
            {
                errorMessage = "no element to run xpath on";
                return null;
            }
        }

        //public static ISpread<T> GetElementsByXPath<T>(this XElement element, string xPath, string nodesName, out string errorMessage) where T : XObject
        //{
        //    return GetElementsByXPath<T>(element, xPath, nodesName, null, out errorMessage);
        //}

        [Node]
        public static ISpread<XElement> GetElementsByXPath(this XElement element, string xPath, IXmlNamespaceResolver nameSpaceResolver, out string errorMessage)
        {
            var result = element.GetElementsByXPath<XElement>(xPath, "elements", nameSpaceResolver, out errorMessage);
            return result != null ? result : NoElements;
        }

        [Node]
        public static ISpread<XAttribute> GetAttributesByXPath(this XElement element, string xPath, IXmlNamespaceResolver nameSpaceResolver, out string errorMessage)
        {
            var result = element.GetElementsByXPath<XAttribute>(xPath, "attributes", nameSpaceResolver, out errorMessage);
            return result != null ? result : NoAttributes;
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
        public static XDocument AsDocument(this string xml, Validation validation = Validation.None)
        {
            var settings = new XmlReaderSettings();
            switch (validation)
            {
                case Validation.Dtd:
                    settings.DtdProcessing = DtdProcessing.Parse;
                    settings.ValidationType = ValidationType.DTD;
                    break;
                case Validation.Schema:
                    settings.ValidationType = ValidationType.Schema;
                    break;
                default:
                    settings.DtdProcessing = DtdProcessing.Ignore;
                    break;
            }
            using (var textReader = new StringReader(xml))
            using (var reader = XmlReader.Create(textReader, settings))
            {
                return XDocument.Load(reader);
            }
        }

        [Node]
        public static IXmlNamespaceResolver CreateNamespaceResolver(this XNode node, IEnumerable<Tuple<string, string>> namespaces)
        {
            //Grab the reader
            var reader = node.CreateReader();
            //Use the reader NameTable
            var namespaceManager = new XmlNamespaceManager(reader.NameTable);
            foreach (var t in namespaces)
                namespaceManager.AddNamespace(t.Item1, t.Item2);
            return namespaceManager;
        }

        [Node]
        public static string AsString(this XElement element)
        {
            return element != null ? element.AsString() : "";
        }

        [Node]
        public static string RelaxNGValidate(string xml, string rngFile)
        {
            string r = "\r\n";

            // Files must exist.
            if (!File.Exists(rngFile))
                return "Schema file not found.";

            // Grammar.
            RelaxngPattern p = null;
            
            if (Path.GetExtension(rngFile).ToUpper() == ".RNG")
            {
                XmlTextReader xtrRng = new XmlTextReader(rngFile);
                try
                {
                    p = RelaxngPattern.Read(xtrRng);
                    p.Compile();
                }
                catch (Exception ex1)
                {
                    return "Schema file has invalid grammar:" + r
                           + rngFile + r + ex1.Message;
                }
                finally
                {
                    xtrRng.Close();
                }
            }
            else
            if (Path.GetExtension(rngFile).ToUpper() == ".RNC")
            {
                var trRnc = new StreamReader(rngFile);
                try
                {
                    p = RncParser.ParseRnc(trRnc);
                    p.Compile();
                }
                catch (Exception ex1)
                {
                    return "Schema file has invalid grammar:" + r
                           + rngFile + r + ex1.Message;
                }
                finally
                {
                    trRnc.Close();
                }
            }
            else
                return "Unknown schema file extension: " + Path.GetExtension(rngFile);

            byte[] byteArray = Encoding.Default.GetBytes(xml);
            MemoryStream stream = new MemoryStream(byteArray);

            // Validate instance.
            XmlTextReader xtrXml = new XmlTextReader(stream);
            RelaxngValidatingReader vr = new RelaxngValidatingReader(xtrXml, p);
            try
            {
                while (!vr.EOF) vr.Read();
                // XML file is valid.
                return "";
            }
            catch (RelaxngException ex2)
            {
                // XML file not valid.
                return ex2.Message;
            }
            catch (Exception ex3)
            {
                // XML file not well-formed.
                return ex3.Message;
            }
            finally
            {
                vr.Close();
                xtrXml.Close();
            }
        }
    }
}
