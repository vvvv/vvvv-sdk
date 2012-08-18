using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using CefGlue;

namespace VVVV.Nodes.HTML
{
    class CefXmlReader : XmlReader
    {
        private readonly CefDomDocument document;
        private readonly CefDomNode rootNode;
        private CefDomNode currentNode;

        public CefXmlReader(CefDomDocument cefDomDocument)
        {
            this.document = cefDomDocument;
            this.rootNode = document.GetDocument();
            this.nameTable = new NameTable();
            this.namespaceManager = new XmlNamespaceManager(nameTable);
        }

        public override int AttributeCount
        {
            get 
            {
                if (currentNode.HasElementAttributes())
                {
                    var attributeMap = currentNode.GetElementAttributes();
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
            get { return nodeStack.Count - 1; }
        }

        public override bool EOF
        {
            get { return readState == System.Xml.ReadState.EndOfFile; }
        }

        private const string I_IS_OUT_OF_RANGE = "i is out of range. It must be non-negative and less than the size of the attribute collection.";
        public override string GetAttribute(int i)
        {
            if (!currentNode.HasElementAttributes())
            {
                throw new ArgumentOutOfRangeException(I_IS_OUT_OF_RANGE);
            }
            var attributeMap = currentNode.GetElementAttributes();
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
            if (name == null)
            {
                throw new ArgumentNullException("name is a null reference");
            }
            if (!currentNode.HasElementAttributes())
            {
                return null;
            }
            var attributeMap = currentNode.GetElementAttributes();
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
            get { return !currentNode.HasChildren; }
        }

        public override string LocalName
        {
            get 
            {
                switch (NodeType)
                {
                    case XmlNodeType.Attribute:
                        var attributeMap = currentNode.GetElementAttributes();
                        string key;
                        attributeMap.TryGetKey(currentAttributeIndex, out key);
                        key = XmlConvert.EncodeLocalName(key);
                        return key;
                    default:
                        return currentNode.GetName();
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
            if (currentNode.HasElementAttributes())
            {
                currentAttributeIndex = 0;
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
            if (!currentNode.HasElementAttributes())
            {
                return false;
            }
            currentAttributeIndex++;
            var attributeMap = currentNode.GetElementAttributes();
            return currentAttributeIndex < attributeMap.Count;
        }

        private XmlNameTable nameTable;
        private XmlNamespaceManager namespaceManager;
        public override XmlNameTable NameTable
        {
            get { return nameTable; }
        }

        public override string NamespaceURI
        {
            get { return namespaceManager.DefaultNamespace; }
        }

        public override XmlNodeType NodeType
        {
            get 
            {
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
                        return XmlNodeType.Text;
                    case CefDomNodeType.Unsupported:
                    case CefDomNodeType.XPathNamespace:
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        public override string Prefix
        {
            get { return namespaceManager.LookupPrefix(NamespaceURI); }
        }

        enum TraverseDirection { Down, Up }

        private Stack<CefDomNode> nodeStack = new Stack<CefDomNode>();
        private TraverseDirection traverseDirection;
        public override bool Read()
        {
            // First call to Read
            if (ReadState == System.Xml.ReadState.Initial)
            {
                nodeStack.Push(rootNode);
                currentNode = rootNode.GetFirstChild();
                return CheckReadStateAndReturn();
            }

            switch (traverseDirection)
            {
                case TraverseDirection.Down:
                    // As long as there children go down (depth first)
                    if (currentNode.HasChildren)
                    {
                        nodeStack.Push(currentNode);
                        currentNode = currentNode.GetFirstChild();
                    }
                    else
                    {
                        var parent = nodeStack.Peek();
                        var lastChild = parent.GetLastChild();
                        if (currentNode.IsSame(lastChild))
                        {
                            // Time to move back up
                            nodeStack.Pop();
                            traverseDirection = TraverseDirection.Up;
                            currentNode = parent;
                        }
                        else
                        {
                            // There're still siblings left to visit
                            currentNode = currentNode.GetNextSibling();
                        }
                    }
                    break;
                case TraverseDirection.Up:
                    // See if we can find siblings (need to have a parent)
                    if (nodeStack.Count > 1)
                    {
                        var parent = nodeStack.Peek();
                        var lastChild = parent.GetLastChild();
                        if (currentNode.IsSame(lastChild))
                        {
                            // No sibling left, go further up
                            nodeStack.Pop();
                            currentNode = parent;
                        }
                        else
                        {
                            // There's a sibling, try to traverse down again (depth first)
                            traverseDirection = TraverseDirection.Down;
                            currentNode = currentNode.GetNextSibling();
                        }
                    }
                    else
                    {
                        // No parent left, we have to be the root -> EOF
                        currentNode = null;
                        readState = System.Xml.ReadState.EndOfFile;
                        return false;
                    }
                    break;
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
                string value;
                switch (NodeType)
                {
                    case XmlNodeType.Attribute:
                        var attributeMap = currentNode.GetElementAttributes();
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
