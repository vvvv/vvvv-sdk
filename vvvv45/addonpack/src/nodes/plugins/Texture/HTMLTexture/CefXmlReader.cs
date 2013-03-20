using System;
using System.Collections.Generic;
using System.Xml;
using CefGlue;
using System.IO;

namespace VVVV.Nodes.Texture.HTML
{
    class CefXmlReader : XmlReader
    {
        private CefDomDocument document;
        private CefDomNode currentNode;
        private XmlReader xmlReader; // Used to parse text nodes
        private TextReader textReader;

        public CefXmlReader(CefDomDocument cefDomDocument)
        {
            this.document = cefDomDocument;
            this.nameTable = new NameTable();
            this.namespaceManager = new XmlNamespaceManager(nameTable);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.currentNode != null)
                {
                    this.currentNode.Dispose();
                    this.currentNode = null;
                }
                foreach (var node in this.nodeStack)
                    node.Dispose();
                this.nodeStack.Clear();
                this.document = null;
                //System.GC.Collect();
                //System.GC.WaitForPendingFinalizers();
            }
            base.Dispose(disposing);
        }

        public override int AttributeCount
        {
            get 
            {
                if (xmlReader != null)
                {
                    return xmlReader.AttributeCount;
                }
                if (attributeMap != null)
                {
                    return attributeMap.Count;
                }
                return 0;
            }
        }

        public override string BaseURI
        {
            get { return string.Empty; } 
        }

        public override void Close()
        {
            readState = System.Xml.ReadState.Closed;
        }

        public override int Depth
        {
            get 
            {
                if (xmlReader != null)
                {
                    return xmlReader.Depth + nodeStack.Count - 1;
                }
                return nodeStack.Count - 1; 
            }
        }

        public override bool EOF
        {
            get { return readState == System.Xml.ReadState.EndOfFile; }
        }

        private const string I_IS_OUT_OF_RANGE = "i is out of range. It must be non-negative and less than the size of the attribute collection.";
        public override string GetAttribute(int i)
        {
            if (xmlReader != null)
            {
                return xmlReader.GetAttribute(i);
            }
            if (attributeMap == null)
            {
                throw new ArgumentOutOfRangeException(I_IS_OUT_OF_RANGE);
            }
            if (i < 0 || i >= attributeMap.Count)
            {
                throw new ArgumentOutOfRangeException(I_IS_OUT_OF_RANGE);
            }
            string value;
            if (!attributeMap.TryGetValue(i, out value))
            {
                return null;
            }
            return value;
        }

        public override string GetAttribute(string name, string namespaceURI)
        {
            throw new NotImplementedException();
        }

        public override string GetAttribute(string name)
        {
            if (xmlReader != null)
            {
                return xmlReader.GetAttribute(name);
            }
            if (name == null)
            {
                throw new ArgumentNullException("name is a null reference");
            }
            if (attributeMap == null)
            {
                return null;
            }
            string value;
            if (!attributeMap.TryGetValue(name, out value))
            {
                return null;
            }
            if (value == string.Empty)
            {
                return null;
            }
            return value;
        }

        public override bool IsEmptyElement
        {
            get 
            {
                if (xmlReader != null)
                {
                    return xmlReader.IsEmptyElement;
                }
                return !currentNode.HasChildren; 
            }
        }

        public override string LocalName
        {
            get 
            {
                if (xmlReader != null)
                {
                    return xmlReader.LocalName;
                }
                switch (NodeType)
                {
                    case XmlNodeType.Attribute:
                        string key;
                        attributeMap.TryGetKey(currentAttributeIndex, out key);
                        return GetLocalName(key);
                    default:
                        return GetLocalName(currentNode.GetName());
                }
            }
        }

        public override string LookupNamespace(string prefix)
        {
            throw new NotImplementedException();
        }

        public override bool MoveToAttribute(string name, string ns)
        {
            throw new NotImplementedException();
        }

        public override bool MoveToAttribute(string name)
        {
            throw new NotImplementedException();
        }

        public override bool MoveToElement()
        {
            if (xmlReader != null)
            {
                return xmlReader.MoveToElement();
            }
            if (currentAttributeIndex >= 0)
            {
                currentAttributeIndex = -1;
                return true;
            }
            else
            {
                return false;
            }
        }

        private int currentAttributeIndex = -1;
        public override bool MoveToFirstAttribute()
        {
            if (xmlReader != null)
            {
                return xmlReader.MoveToFirstAttribute();
            }
            if (attributeMap != null)
            {
                currentAttributeIndex = attributeMap.Count - 1;
                return true;
            }
            else
            {
                currentAttributeIndex = -1;
                return false;
            }
        }

        public override bool MoveToNextAttribute()
        {
            if (xmlReader != null)
            {
                return xmlReader.MoveToNextAttribute();
            }
            if (!currentNode.IsElement)
            {
                return false;
            }
            if (!currentNode.HasElementAttributes())
            {
                return false;
            }
            currentAttributeIndex--;
            return currentAttributeIndex >= 0;
        }

        private XmlNameTable nameTable;
        private XmlNamespaceManager namespaceManager;
        public override XmlNameTable NameTable
        {
            get { return nameTable; }
        }

        public override string NamespaceURI
        {
            get 
            {
                return namespaceManager.LookupNamespace(Prefix); 
            }
        }

        public override XmlNodeType NodeType
        {
            get 
            {
                if (xmlReader != null)
                {
                    return xmlReader.NodeType;
                }
                if (currentNode == null)
                {
                    return XmlNodeType.None;
                }
                if (currentAttributeIndex >= 0)
                {
                    return XmlNodeType.Attribute;
                }
                switch (currentNode.Type)
                {
                    case CefDomNodeType.Attribute:
                        return XmlNodeType.Attribute;
                    case CefDomNodeType.CDataSection:
                        return XmlNodeType.CDATA;
                    case CefDomNodeType.Comment:
                        return XmlNodeType.Comment;
                    case CefDomNodeType.Document:
                        return XmlNodeType.Document;
                    case CefDomNodeType.DocumentFragment:
                        return XmlNodeType.DocumentFragment;
                    case CefDomNodeType.DocumentType:
                        return XmlNodeType.DocumentType;
                    case CefDomNodeType.Element:
                        return traverseDirection == TraverseDirection.Down
                            ? XmlNodeType.Element
                            : XmlNodeType.EndElement;
                    case CefDomNodeType.Entity:
                        return traverseDirection == TraverseDirection.Down
                            ? XmlNodeType.Entity
                            : XmlNodeType.EndEntity;
                    case CefDomNodeType.EntityReference:
                        return XmlNodeType.EntityReference;
                    case CefDomNodeType.Notation:
                        return XmlNodeType.Notation;
                    case CefDomNodeType.ProcessingInstruction:
                        return XmlNodeType.ProcessingInstruction;
                    case CefDomNodeType.Text:
                        // Try to parse the text as it is valid xml in some cases (CDATA)
                        var value = "<span>" + currentNode.GetValue() + "</span>";
                        if (IsXml(value))
                        {
                            textReader = new StringReader(value);
                            xmlReader = XmlReader.Create(textReader, new XmlReaderSettings() { NameTable = nameTable });
                            xmlReader.Read(); // span
                            xmlReader.Read(); // content
                            return xmlReader.NodeType;
                        }
                        else
                        {
                            return XmlNodeType.Text;
                        }
                    case CefDomNodeType.Unsupported:
                    case CefDomNodeType.XPathNamespace:
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        private bool IsXml(string text)
        {
            try
            {
                using (var textReader = new StringReader(text))
                {
                    using (var xmlReader = XmlReader.Create(textReader))
                    {
                        while (!xmlReader.EOF)
                        {
                            xmlReader.Read();
                        }
                    }
                }
                return true;
            }
            catch (XmlException)
            {
                return false;
            }
        }

        public override string Prefix
        {
            get 
            {
                if (xmlReader != null)
                {
                    return xmlReader.Prefix;
                }
                if (currentAttributeIndex >= 0)
                {
                    string key;
                    attributeMap.TryGetKey(currentAttributeIndex, out key);
                    return GetPrefix(key);
                }
                return GetPrefix(currentNode.GetName());
            }
        }

        private static string GetPrefix(string name)
        {
            var n = name.Split(':');
            if (n.Length > 1)
            {
                return n[0];
            }
            return string.Empty;
        }

        private static string GetNamespacePrefix(string name)
        {
            // Example: xmlns:m="http://www.w3.org/1998/Math/MathML"
            var n = name.Split(':');
            if (n.Length > 1)
            {
                return n[n.Length - 1];
            }
            return string.Empty;
        }

        private static string GetLocalName(string name)
        {
            var n = name.ToLowerInvariant().Split(':');
            if (n.Length > 1)
            {
                return XmlConvert.EncodeLocalName(n[1]);
            }
            return XmlConvert.EncodeLocalName(n[0]);
        }

        enum TraverseDirection { Down, Up }

        private Stack<CefDomNode> nodeStack = new Stack<CefDomNode>();
        private TraverseDirection traverseDirection;
        private CefStringMap attributeMap;
        public override bool Read()
        {
            // First call to Read
            if (ReadState == System.Xml.ReadState.Initial)
            {
                var rootNode = this.document.GetDocument();
                this.nodeStack.Push(rootNode);
                currentNode = rootNode.GetFirstChild();
                return CheckReadStateAndReturn();
            }

            if (xmlReader != null)
            {
                var result = xmlReader.Read();
                if (result && xmlReader.Depth > 0)
                {
                    return result;
                }
                else
                {
                    xmlReader.Close();
                    textReader.Dispose();
                    xmlReader = null;
                    textReader = null;
                }
            }

            switch (traverseDirection)
            {
                case TraverseDirection.Down:
                    // As long as there're children go down (depth first)
                    if (currentNode.HasChildren)
                    {
                        nodeStack.Push(currentNode);
                        currentNode = currentNode.GetFirstChild();
                    }
                    else
                    {
                        var parent = nodeStack.Peek();
                        using (var lastChild = parent.GetLastChild())
                        {
                            if (currentNode.IsSame(lastChild))
                            {
                                // Time to move back up
                                nodeStack.Pop();
                                traverseDirection = TraverseDirection.Up;
                                currentNode.Dispose();
                                currentNode = parent;
                            }
                            else
                            {
                                // There're still siblings left to visit
                                var nextSibling = currentNode.GetNextSibling();
                                currentNode.Dispose();
                                currentNode = nextSibling;
                            }
                        }
                    }
                    break;
                case TraverseDirection.Up:
                    // See if we can find siblings (need to have a parent)
                    if (nodeStack.Count > 1)
                    {
                        var parent = nodeStack.Peek();
                        using (var lastChild = parent.GetLastChild())
                        {
                            if (currentNode.IsSame(lastChild))
                            {
                                // No sibling left, go further up
                                nodeStack.Pop();
                                currentNode.Dispose();
                                currentNode = parent;
                            }
                            else
                            {
                                // There's a sibling, try to traverse down again (depth first)
                                traverseDirection = TraverseDirection.Down;
                                var nextSibling = currentNode.GetNextSibling();
                                currentNode.Dispose();
                                currentNode = nextSibling;
                            }
                        }
                    }
                    else
                    {
                        // No parent left, we have to be the root -> EOF
                        currentNode.Dispose();
                        currentNode = null;
                        readState = System.Xml.ReadState.EndOfFile;
                        return false;
                    }
                    break;
            }

            if (currentNode != null)
            {
                // See if this node defines a new namespace
                if (currentNode.IsElement && currentNode.HasElementAttributes())
                {
                    attributeMap = currentNode.GetElementAttributes();
                    switch (traverseDirection)
                    {
                        case TraverseDirection.Down:
                            for (int i = 0; i < attributeMap.Count; i++)
                            {
                                string key, value;
                                if (attributeMap.TryGetKey(i, out key) && attributeMap.TryGetValue(i, out value))
                                {
                                    if (key.StartsWith("xmlns"))
                                    {
                                        var prefix = GetNamespacePrefix(key);
                                        namespaceManager.AddNamespace(prefix, value);
                                    }
                                }
                            }
                            namespaceManager.PushScope();
                            break;
                        case TraverseDirection.Up:
                            namespaceManager.PopScope();
                            break;
                    }
                }
                else
                {
                    attributeMap = null;
                }
            }

            return CheckReadStateAndReturn();
        }

        private bool CheckReadStateAndReturn()
        {
            if (currentNode == null)
            {
                readState = System.Xml.ReadState.EndOfFile;
                return false;
            }
            readState = System.Xml.ReadState.Interactive;
            return true;
        }

        public override bool ReadAttributeValue()
        {
            throw new NotImplementedException();
        }

        private ReadState readState = ReadState.Initial;
        public override ReadState ReadState { get { return readState; } }

        public override void ResolveEntity()
        {
            throw new NotImplementedException();
        }

        public override string Value
        {
            get 
            {
                if (xmlReader != null)
                {
                    return xmlReader.Value;
                }

                string value;
                switch (NodeType)
                {
                    case XmlNodeType.Attribute:
                        attributeMap.TryGetValue(currentAttributeIndex, out value);
                        break;
                    case XmlNodeType.CDATA:
                    case XmlNodeType.Comment:
                    case XmlNodeType.DocumentType:
                    case XmlNodeType.ProcessingInstruction:
                    case XmlNodeType.SignificantWhitespace:
                    case XmlNodeType.Text:
                    case XmlNodeType.Whitespace:
                    case XmlNodeType.XmlDeclaration:
                        value = currentNode.GetValue();
                        break;
                    default:
                        value = string.Empty;
                        break;
                }
                return value;
            }
        }
    }
}
