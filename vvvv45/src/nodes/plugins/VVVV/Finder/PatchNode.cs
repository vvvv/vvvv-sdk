using System;
using System.Drawing;
using System.Collections.Generic;

using VVVV.Core;
using VVVV.Core.View;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Nodes.Finder
{
    public class PatchNode: IViewableCollection, INamed, IDescripted, INodeListener, ISelectable, IDecoratable, ILinkable
    {
        List<PatchNode> FChildNodes = new List<PatchNode>();
        
        static SolidBrush SDarkGray = new SolidBrush(Color.FromArgb(154, 154, 154));
        static SolidBrush SLightGray = new SolidBrush(Color.FromArgb(192, 192, 192));
        static SolidBrush SHoverGray = new SolidBrush(Color.FromArgb(216, 216, 216));
        
        static SolidBrush SDarkRed = new SolidBrush(Color.FromArgb(168, 82, 82));
        static SolidBrush SLightRed = new SolidBrush(Color.FromArgb(229, 162, 162));
        static SolidBrush SHoverRed = new SolidBrush(Color.FromArgb(233, 158, 158));
        
        static SolidBrush SDarkBlue = new SolidBrush(Color.FromArgb(82, 112, 168));
        static SolidBrush SLightBlue = new SolidBrush(Color.FromArgb(162, 174, 229));
        static SolidBrush SHoverBlue = new SolidBrush(Color.FromArgb(158, 193, 233));
        
        public bool MarkedForDelete{get;set;}

        public string SRChannel {get; private set;}
        public string Comment {get; private set;}
        public string DescriptiveName {get; private set;}
        
        public bool IsIONode {get; private set;}
        public bool IsBoygrouped {get; private set;}
        public bool IsMissing {get; private set;}
        
        public bool IsActiveWindow {get; private set;}
        public void SetActiveWindow(IWindow window)
        {
            //hand this downtree until someone finds itself in the window
            if (FWindow == window)
                IsActiveWindow = true;
            else
                foreach(var child in FChildNodes)
                    child.SetActiveWindow(window);
        }
        
        private IWindow FWindow;
        private string FDecoratedName;
        
        public NodeType NodeType {get; private set;}
        
        public PatchNode()
        {}
        
        public PatchNode(INode self)
        {
            Node = self;
            
            if (NodeType != NodeType.Module)
                InitChildren();
        }
        
        private INode FNode;
        public INode Node
        {
            get
            {
                return FNode;
            }
            set
            {
                FNode = value;
                if (FNode != null)
                {
                    IsBoygrouped = FNode.IsBoygrouped();
                    IsMissing = FNode.IsMissing();
                    
                    Name = "????";
                    if (Node.HasGUI())
                    {
                        if (Node.HasCode())
                            Icon = NodeIcon.GUICode;
                        else if (Node.HasPatch())
                            Icon = NodeIcon.GUIPatch;
                        else
                            Icon = NodeIcon.GUI;
                    }
                    else if (Node.HasCode())
                        Icon = NodeIcon.Code;
                    else if (Node.HasPatch())
                        Icon = NodeIcon.Patch;
                    
                    FWindow = FNode.Window;
                    
                    UpdateName();
                    
                    Node.AddListener(this);
                }
            }
        }
        private bool FSelected;
        public bool Selected
        {
            get {return FSelected;}
            set
            {
                FSelected = value;
                OnSelectionChanged(null);
            }
        }
        
        #region IViewableCollection
        public int Count
        {
            get{return FChildNodes.Count;}
        }
        
        public bool Contains(object item)
        {
            throw new NotImplementedException();
        }
        
        public event CollectionDelegate Added;
        
        protected virtual void OnAdded(object item)
        {
            if (Added != null) {
                Added(this, item);
            }
        }
        
        public event CollectionDelegate Removed;
        
        protected virtual void OnRemoved(object item)
        {
            if (Removed != null) {
                Removed(this, item);
            }
        }
        
        public System.Collections.IEnumerator GetEnumerator()
        {
            return FChildNodes.GetEnumerator();
        }
        
        public event CollectionUpdateDelegate UpdateBegun;
        
        protected virtual void OnUpdateBegun(IViewableCollection collection)
        {
            if (UpdateBegun != null) {
                UpdateBegun(collection);
            }
        }
        
        public event CollectionUpdateDelegate Updated;
        
        protected virtual void OnUpdated(IViewableCollection collection)
        {
            if (Updated != null) {
                Updated(collection);
            }
        }
        #endregion IViewableCollection
        
        #region INamed
        public string Name{get; private set;}
        
        public event RenamedHandler Renamed;
        
        protected virtual void OnRenamed(string newName)
        {
            if (Renamed != null) {
                Renamed(this, newName);
            }
        }
        #endregion INamed
        
        #region IDescription
        public string Description
        {
            get
            {
                if (Node == null)
                    return "";
                else
                {
                    var ni = Node.GetNodeInfo();
                    if (!string.IsNullOrEmpty(SRChannel))
                        return ni.Username + " [id " + Node.GetID().ToString() + "]\nChannel: " + SRChannel;
                    else if (!string.IsNullOrEmpty(Comment))
                        return Comment;
                    else if (IsIONode)
                        return "IO " + Node.GetNodeInfo().Category + "\n" + DescriptiveName;
                    else if (ni.Type == NodeType.Native)
                        return ni.Username + " [id " + Node.GetID().ToString() + "]";
                    else
                        return ni.Username + " [id " + Node.GetID().ToString() + "]\n" + ni.Filename;
                }
            }
        }
        #endregion IDescription
        
        #region INodeListener
        public void AddedCB(INode childNode)
        {
            bool found = false;
            foreach(var child in FChildNodes)
                if (child.Node == childNode)
            {
                found = true;
                break;
            }
            
            if (!found)
                Add(new PatchNode(childNode));
        }
        
        public void RemovedCB(INode childNode)
        {
            foreach(var child in FChildNodes)
                if (child.Node == childNode)
            {
                Remove(child);
                break;
            }
        }
        
        public void LabelChangedCB()
        {
            UpdateName();
            OnRenamed(Name);
        }
        #endregion INodeListener
        
        private void UpdateName()
        {
            DescriptiveName = Node.GetPin("Descriptive Name").GetValue(0);
            string hyphen = "";
            if (!string.IsNullOrEmpty(DescriptiveName))
                hyphen = " -- ";
            
            var ni = FNode.GetNodeInfo();
            if (ni != null)
            {
                NodeType = ni.Type;
                
                //subpatches
                if (string.IsNullOrEmpty(ni.Name))
                {
                    string file = System.IO.Path.GetFileNameWithoutExtension(ni.Filename);
                    
                    //unsaved patch
                    if (string.IsNullOrEmpty(file))
                        Name = ni.Filename + hyphen + DescriptiveName;
                    //patch with valid filename
                    else
                        Name = file + hyphen + DescriptiveName;
                }
                else if ((ni.Username == "IOBox (Value Advanced)") || (ni.Username == "IOBox (Color)") || (ni.Username == "IOBox (Enumerations)") || (ni.Username == "IOBox (Node)"))
                {
                    if (string.IsNullOrEmpty(DescriptiveName))
                        Name = ni.Username + hyphen + DescriptiveName;
                    else
                        IsIONode = true;
                }
                else if (ni.Username == "IOBox (String)")
                {
                    if (string.IsNullOrEmpty(DescriptiveName))
                        if ((!Node.GetPin("Input String").IsConnected()) && (!Node.GetPin("Output String").IsConnected()))
                    {
                        Comment = Node.GetPin("Input String").GetValue(0);
                        var cmt = Comment;
                        var maxChar = 30;
                        var linebreak = cmt.IndexOf("\n");
                        if (linebreak > 0 && linebreak < maxChar)
                            cmt = cmt.Substring(0, linebreak) + "...";
                        else if (cmt.Length > maxChar)
                            cmt = cmt.Substring(0, maxChar) + "...";
                        Name = cmt;
                        Icon = NodeIcon.Comment;
                    }
                    else
                        Name = ni.Username + hyphen + DescriptiveName;
                    else
                        IsIONode = true;
                }
                else if (ni.Name == "S")
                {
                    SRChannel = FNode.GetPin("SendString").GetValue(0);
                    FIsSource = true;
                    Name = ni.Username + ": " + SRChannel;
                    Icon = NodeIcon.SRNode;
                }
                else if (ni.Name == "R")
                {
                    SRChannel = FNode.GetPin("ReceiveString").GetValue(0);
                    Name = ni.Username + ": " + SRChannel;
                    Icon = NodeIcon.SRNode;
                }
                else
                    Name = ni.Username + hyphen + DescriptiveName;
                
                if (IsIONode)
                {
                    Name = DescriptiveName;
                    Icon = NodeIcon.IONode;
                }
            }
        }
        
        private void InitChildren()
        {
            if (Node == null)
                return;
            
            INode[] children = Node.GetChildren();
            if (children != null)
            {
                foreach(INode child in children)
                    Add(new PatchNode(child));
            }
            
            SortChildren();
        }
        
        private void SortChildren()
        {
            FChildNodes.Sort(delegate(PatchNode p1, PatchNode p2)
                             {
                                 //order:
                                 //subpatches
                                 //inlets
                                 //outlets
                                 //Sends
                                 //Receives
                                 //comments
                                 //other nodes
                                 
                                 int w1 = 0, w2 = 0;
                                 if (p1.NodeType == NodeType.Patch)
                                     w1 = 100;
                                 else if (p1.IsIONode)
                                     w1 = 90;
                                 else if (p1.Name.StartsWith("S "))
                                     w1 = 81;
                                 else if (p1.Name.StartsWith("R "))
                                     w1 = 80;
                                 else if (!string.IsNullOrEmpty(p1.Comment))
                                     w1 = 200;
                                 
                                 if (p2.NodeType == NodeType.Patch)
                                     w2 = 100;
                                 else if (p1.IsIONode)
                                     w2 = 90;
                                 else if (p2.Name.StartsWith("S "))
                                     w2 = 81;
                                 else if (p2.Name.StartsWith("R "))
                                     w2 = 80;
                                 else if (!string.IsNullOrEmpty(p1.Comment))
                                     w2 = 200;
                                 
                                 if ((w1 > 0) || (w2 > 0))
                                 {
                                     if (w1 > w2)
                                         return -1;
                                     else if (w1 < w2)
                                         return 1;
                                     else
                                         return p1.Name.CompareTo(p2.Name);
                                 }
                                 else
                                     return p1.Name.CompareTo(p2.Name);
                             });
        }
        
        public void SetTags(List<string> tags)
        {
            FDecoratedName = BoldenString(Name, tags, "@@", "@@");
        }
        
        private string BoldenString(string input, List<string> tags, string startTag, string endTag)
        {
            var loweredInput = input.ToLower();
            var tagged = loweredInput.ToCharArray();
            foreach (string tag in tags)
            {
                //in the tagged char[] mark all matching characters as ° for later being surrounded by start and endTags.
                int start = 0;
                while (loweredInput.IndexOf(tag, start) >= 0)
                {
                    int pos = loweredInput.IndexOf(tag, start);
                    for (int i=pos; i<pos + tag.Length; i++)
                        tagged[i] = '°';
                    start = pos+1;
                }
            }
            
            //now create the outputstring based on the tagged char[]
            string output = "";
            var inTag = false;
            int j = 0;
            foreach (var c in tagged)
            {
                if (c == '°')
                {
                    if (!inTag)
                    {
                        inTag = true;
                        output += startTag + input[j];
                    }
                    else
                        output += input[j];
                }
                else
                {
                    if (inTag)
                    {
                        inTag = false;
                        output += endTag + input[j];
                    }
                    else
                        output += input[j];
                }
                j++;
            }
            
            return output;
        }
        
        public void TearDown()
        {
            if (Node != null)
                Node.RemoveListener(this);
            
            for (int i = FChildNodes.Count - 1; i >= 0; i--)
                Remove(FChildNodes[i]);
        }
        /*
        public void SelectNodes(INode[] nodes)
        {
            //deselect all
            foreach(PatchNode pn in FChildNodes)
                pn.Selected = false;
            
            if (nodes != null)
                //reselect selected
                foreach(INode node in nodes)
            {
                PatchNode pn = FChildNodes.Find(delegate (PatchNode p) {return p.Node == node;});
                if (pn != null)
                    pn.Selected = true;
            }
        }
         */
        public void Add(PatchNode childNode)
        {
            OnUpdateBegun(this);
            try
            {
                FChildNodes.Add(childNode);
                SortChildren();
                
                //sync with viewer
                OnAdded(childNode);
            } catch (Exception)
            {
                OnUpdated(this);
            }
        }

        private void Remove(PatchNode childNode)
        {
            OnUpdateBegun(this);
            try
            {
                childNode.TearDown();
                FChildNodes.Remove(childNode);
                
                //sync with viewer
                OnRemoved(childNode);
            } catch (Exception)
            {
                OnUpdated(this);
            }
        }
        
        public event SelectionChangedHandler SelectionChanged;
        
        protected virtual void OnSelectionChanged(EventArgs args)
        {
            if (SelectionChanged != null) {
                SelectionChanged(this, args);
            }
        }
        
        #region IDecoratable
        public System.Drawing.Pen TextColor
        {
            get
            {
                return Pens.Black;
            }
        }
        
        public System.Drawing.Pen TextHoverColor
        {
            get
            {
                return Pens.Black;
            }
        }

        public System.Drawing.Brush BackColor
        {
            get
            {
                if (FNode == null)
                    return Brushes.AliceBlue;
                
                if (IsActiveWindow)
                    return Brushes.White;
                
                if (FNode.HasPatch())
                {    if (FNode.ContainsMissingNodes())
                        return SDarkRed;
                    else if (FNode.ContainsBoygroupedNodes())
                        return SDarkBlue;
                    else
                        return SDarkGray;
                }
                else if (FNode.IsMissing())
                    return SLightRed;
                else if (FNode.IsBoygrouped())
                    return SLightBlue;
                else
                    return SLightGray;
            }
        }
        
        public System.Drawing.Brush BackHoverColor
        {
            get
            {
                if (FNode == null)
                    return Brushes.AliceBlue;
                
                if (FNode.HasPatch())
                {   if (FNode.ContainsMissingNodes())
                        return SHoverRed;
                    else if (FNode.ContainsBoygroupedNodes())
                        return SHoverBlue;
                    else
                        return SHoverGray;
                }
                else if (FNode.IsMissing())
                    return SHoverRed;
                else if (FNode.IsBoygrouped())
                    return SHoverBlue;
                else
                    return SHoverGray;
            }
        }
        
        public System.Drawing.Pen OutlineColor
        {
            get
            {
                return Pens.Black;
            }
        }
        
        public string Text {
            get
            {
                return FDecoratedName;
            }
        }
        
        public NodeIcon Icon {get; private set;}
        #endregion IDecoratable
        
        #region ILinkable
        private bool FIsSource;
        public bool IsSource {
            get
            {
                return FIsSource;
            }
        }
        
        public string Channel {
            get
            {
                if (FNode != null)
                    return SRChannel + " - " + FNode.GetNodeInfo().Category;
                else
                    return null;
            }
        }
        #endregion ILinkable
    }
}
