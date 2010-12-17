using System;
using System.Drawing;
using System.Collections.Generic;

using VVVV.Core;
using VVVV.Core.View;
using VVVV.Core.Collections;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Nodes.Finder
{
    public class PatchNode: IParent, INamed, IDescripted, INodeListener, ISelectable, IDecoratable, ILinkable, IDisposable, IPinListener
    {
        static SolidBrush SDarkGray = new SolidBrush(Color.FromArgb(154, 154, 154));
        static SolidBrush SLightGray = new SolidBrush(Color.FromArgb(192, 192, 192));
        static SolidBrush SHoverGray = new SolidBrush(Color.FromArgb(216, 216, 216));
        
        static SolidBrush SDarkRed = new SolidBrush(Color.FromArgb(168, 82, 82));
        static SolidBrush SLightRed = new SolidBrush(Color.FromArgb(229, 162, 162));
        static SolidBrush SHoverRed = new SolidBrush(Color.FromArgb(233, 158, 158));
        
        static SolidBrush SDarkBlue = new SolidBrush(Color.FromArgb(82, 112, 168));
        static SolidBrush SLightBlue = new SolidBrush(Color.FromArgb(162, 174, 229));
        static SolidBrush SHoverBlue = new SolidBrush(Color.FromArgb(158, 193, 233));
        
        public IViewableCollection Childs { get; private set; }
        EditableList<PatchNode> FChildNodes = new EditableList<PatchNode>();
        public IEditableList<PatchNode> ChildNodes { get {return FChildNodes;} }
        
        public int ID {get; private set;}
        public string SRChannel {get; private set;}
        public string Comment {get; private set;}
        public string DescriptiveName {get; private set;}
        
        public bool IsIONode {get; private set;}
        public bool IsBoygrouped {get; private set;}
        public bool IsMissing {get; private set;}
        public NodeType NodeType {get; private set;}
        public bool IsActiveWindow {get; private set;}
        private IWindow FWindow;
        private string FDecoratedName;
        private IPin FChannelPin;
        
        public PatchNode()
        {
            Childs = FChildNodes.AsViewableList();
        }
        
        public PatchNode(INode self) : this()
        {
            Node = self;
            
            if (NodeType != NodeType.Module)
                InitChildren();
        }
        
        public void Dispose()
        {
            if (FChannelPin != null)
                FChannelPin.RemoveListener(this);
            
            if (Node != null)
                Node.RemoveListener(this);
            
            Node = null;
            foreach (var child in FChildNodes)
                child.Dispose();
            FChildNodes.Clear();
            FChildNodes.Dispose();
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
                    ID = FNode.GetID();
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
        
        public void SetActiveWindow(IWindow window)
        {
            var windowState = IsActiveWindow;
            
            //hand window downtree until someone finds itself in the window
            if (FWindow == window)
                IsActiveWindow = true;
            else
            {
                IsActiveWindow = false;
                foreach(var child in FChildNodes)
                    child.SetActiveWindow(window);
            }
            
            if (windowState != IsActiveWindow)
                OnDecorationChanged();
        }
        
        public event SelectionChangedHandler SelectionChanged;
        protected virtual void OnSelectionChanged(EventArgs args)
        {
            if (SelectionChanged != null) {
                SelectionChanged(this, args);
            }
        }
        
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
                        return ni.Username + " [id " + ID.ToString() + "]\nChannel: " + SRChannel;
                    else if (!string.IsNullOrEmpty(Comment))
                        return Comment;
                    else if (IsIONode)
                        return "IO " + Node.GetNodeInfo().Category + " [id " + ID.ToString() + "]\n" + DescriptiveName;
                    else if (ni.Type == NodeType.Native)
                        return ni.Username + " [id " + ID.ToString() + "]";
                    else
                        return ni.Username + " [id " + ID.ToString() + "]\n" + ni.Filename;
                }
            }
        }
        #endregion IDescription
        
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
            {
                FChildNodes.BeginUpdate();
                try
                {
                    var pn = new PatchNode(childNode);
                    FChildNodes.Add(pn);
                    pn.Renamed += child_Renamed;
                    SortChildren();
                }
                finally
                {
                    FChildNodes.EndUpdate();
                }
            }
        }
        
        void child_Renamed(INamed sender, string newName)
        {
            SortChildren();
        }
        
        public void RemovedCB(INode childNode)
        {
            foreach(var child in FChildNodes)
                if (child.Node == childNode)
            {
                FChildNodes.Remove(child);
                child.Renamed -= child_Renamed;
                break;
            }
        }
        
        public void LabelChangedCB()
        {
            UpdateName();
            OnRenamed(Name);
        }
        #endregion INodeListener
        
        #region IPinListener
        public void ChangedCB()
        {
            UpdateName();
            OnRenamed(Name);
        }
        #endregion IPinListener
        
        private void InitChildren()
        {
            if (Node == null)
                return;
            
            INode[] children = Node.GetChildren();
            
            if (children != null)
            {
                FChildNodes.BeginUpdate();
                try
                {
                    foreach(INode child in children)
                        FChildNodes.Add(new PatchNode(child));
                    
                    SortChildren();
                }
                finally
                {
                    FChildNodes.EndUpdate();
                }
            }
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
                                 
                                 int w1 = SortPos(p1);
                                 int w2 = SortPos(p2);
                                 
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
        
        private static int SortPos(PatchNode n)
        {
            int pos = 0;
            if (n.NodeType == NodeType.Patch)
                pos = 100;
            else if (n.IsIONode)
                pos = 90;
            else if (n.Name.StartsWith("S "))
                pos = 81;
            else if (n.Name.StartsWith("R "))
                pos = 80;
            else if (!string.IsNullOrEmpty(n.Comment))
                pos = 200;
            
            return pos;
        }
        
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
                    {
                        IsIONode = false;
                        Name = ni.Username + hyphen + DescriptiveName;
                    }
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
                        if (!string.IsNullOrEmpty(cmt))
                        {
                            var maxChar = 30;
                            var linebreak = cmt.IndexOf("\n");
                            if (linebreak > 0 && linebreak < maxChar)
                                cmt = cmt.Substring(0, linebreak) + "...";
                            else if (cmt.Length > maxChar)
                                cmt = cmt.Substring(0, maxChar) + "...";
                            Name = cmt;
                        }
                        Icon = NodeIcon.Comment;
                    }
                    else
                        Name = ni.Username + hyphen + DescriptiveName;
                    else
                        IsIONode = true;
                }
                else if (ni.Name == "S")
                {
                    if (FChannelPin == null)
                    {
                        FChannelPin = FNode.GetPin("SendString");
                        FChannelPin.AddListener(this);
                    }
                    SRChannel = FChannelPin.GetValue(0);
                    
                    FIsSource = true;
                    Name = ni.Username + ": " + SRChannel;
                    Icon = NodeIcon.SRNode;
                }
                else if (ni.Name == "R")
                {
                    if (FChannelPin == null)
                    {
                        FChannelPin = FNode.GetPin("ReceiveString");
                        FChannelPin.AddListener(this);
                    }
                    SRChannel = FChannelPin.GetValue(0);

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
        
        public event DecorationChangedHandler DecorationChanged;
        
        protected virtual void OnDecorationChanged()
        {
            if (DecorationChanged != null) {
                DecorationChanged();
            }
        }
    }
}
