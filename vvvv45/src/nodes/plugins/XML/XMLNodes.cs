﻿using System;
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
    public static class XMLNodes
    {
        public static readonly ISpread<XElement> NoElements = new Spread<XElement>();
        public static readonly ISpread<XAttribute> NoAttributes = new Spread<XAttribute>();

        [Node]
        public static void Split(this XElement element, out string name, out string value, out ISpread<XElement> childs,
            out ISpread<XAttribute> attributes, out XElement documentRoot, out XElement parent,
            out XElement next, out XmlNodeType nodeType) //, out bool changed)
        {
            if (element != null)
            {
                name = element.Name.LocalName;
                value = element.Value;
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
                var n = XName.Get(name);
                return element.Elements(n).ToSpread();
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
        public static ISpread<T> GetElementsByXPath<T>(this XElement element, string xPath, string nodesName, out string errorMessage) where T : XObject
        {
            if (element != null)
            {
                try
                {
                    var obj = (IEnumerable)element.XPathEvaluate(xPath);
                    var whatINeed = obj.Cast<T>();
                    errorMessage = "";
                    return whatINeed.ToSpread();
                }
                catch
                {
                    errorMessage = String.Format("couldn't map xpath '{0}' to xml {1}", xPath, nodesName);
                    return null;
                }
            }
            else
            {
                errorMessage = "no element to run xpath on";
                return null;
            }
        }

        [Node]
        public static ISpread<XElement> GetElementsByXPath(this XElement element, string xPath, out string errorMessage)
        {
            var result = element.GetElementsByXPath<XElement>(xPath, "elements", out errorMessage);
            return result != null ? result : NoElements;
        }

        [Node]
        public static ISpread<XAttribute> GetAttributesByXPath(this XElement element, string xPath, out string errorMessage)
        {
            var result = element.GetElementsByXPath<XAttribute>(xPath, "attributes", out errorMessage);
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
        public static void AsElement(this string xml, out XDocument doc, out XElement element)
        {
            try
            {
                doc = XDocument.Parse(xml);
                element = doc.Root;
            }
            catch
            {
                doc = null;
                element = null;
            }
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
