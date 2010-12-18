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
        private IPin FLabelPin;
        private IPin FCommentPin;
        private INodeInfo FNodeInfo;
        
        public PatchNode()
        {
            Name = "????";
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
            //remove pinlisteners
            if (FCommentPin != null)
            {
                FLabelPin.RemoveListener(this);
                FLabelPin = null;
            }
            
            if (FCommentPin != null)
            {
                FCommentPin.RemoveListener(this);
                FCommentPin = null;
            }
            
            if (FChannelPin != null)
            {
                FChannelPin.RemoveListener(this);
                FChannelPin = null;
            }
            
            //remove nodelistener
            if (Node != null)
            {
                Node.RemoveListener(this);
                Node = null;
            }
            
            //free window
            FWindow = null;
            
            //free children
            foreach (var child in FChildNodes)
            {
                child.Renamed -= child_Renamed;
                child.Dispose();
            }
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
                    Node.AddListener(this);
                    
                    //init static properties via INode
                    ID = FNode.GetID();
                    FNodeInfo = FNode.GetNodeInfo();
                    NodeType = FNodeInfo.Type;
                    
                    FLabelPin = Node.GetPin("Descriptive Name");
                    FLabelPin.AddListener(this);
                    
                    if (FNodeInfo.Name == "S")
                    {
                        FChannelPin = FNode.GetPin("SendString");
                        FIsSource = true;
                    }
                    else if (FNodeInfo.Name == "R")
                        FChannelPin = FNode.GetPin("ReceiveString");
                    
                    if (FChannelPin != null)
                    {
                        FChannelPin.AddListener(this);
                        Icon = NodeIcon.SRNode;
                    }
                    
                    //init dynamic properties via INode
                    UpdateProperties();
                    UpdateName();
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
                    if (!string.IsNullOrEmpty(SRChannel))
                        return FNodeInfo.Username + " [id " + ID.ToString() + "]\nChannel: " + SRChannel;
                    else if (!string.IsNullOrEmpty(Comment))
                        return Comment;
                    else if (IsIONode)
                        return "IO " + FNodeInfo.Category + " [id " + ID.ToString() + "]\n" + DescriptiveName;
                    else if (FNodeInfo.Type == NodeType.Native)
                        return FNodeInfo.Username + " [id " + ID.ToString() + "]";
                    else
                        return FNodeInfo.Username + " [id " + ID.ToString() + "]\n" + FNodeInfo.Filename;
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
        
        public event DecorationChangedHandler DecorationChanged;
        protected virtual void OnDecorationChanged()
        {
            if (DecorationChanged != null) {
                DecorationChanged();
            }
        }
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
        
        void child_Renamed(INamed sender, string newName)
        {
            SortChildren();
        }
        
        public void LabelChangedCB()
        {
            //now via ordinary pinlistener
           // UpdateName();
           // OnRenamed(Name);
        }
        #endregion INodeListener
        
        #region IPinListener
        public void ChangedCB()
        {
            //may be called via FCommentPin, FLabelPin, FChannelPin
            //in all cases do:
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
        
        private void UpdateProperties()
        {
            IsBoygrouped = FNode.IsBoygrouped();
            IsMissing = FNode.IsMissing();
            
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
        }
        
        private void UpdateName()
        {
            DescriptiveName = FLabelPin.GetValue(0);
            string hyphen = "";
            if (!string.IsNullOrEmpty(DescriptiveName))
                hyphen = " -- ";
            
            //subpatches
            if (string.IsNullOrEmpty(FNodeInfo.Name))
            {
                string file = System.IO.Path.GetFileNameWithoutExtension(FNodeInfo.Filename);
                
                //unsaved patch
                if (string.IsNullOrEmpty(file))
                    Name = FNodeInfo.Filename + hyphen + DescriptiveName;
                //patch with valid filename
                else
                    Name = file + hyphen + DescriptiveName;
            }
            else if (FNodeInfo.Name == "IOBox")
            {
                //ioboxes with descriptive names are IOs
                if (!string.IsNullOrEmpty(DescriptiveName))
                {
                    IsIONode = true;
                    Name = DescriptiveName;
                    Icon = NodeIcon.IONode;
                    
                    //this is no longer a comment iobox
                    if (FCommentPin != null)
                    {
                        FCommentPin.RemoveListener(this);
                        FCommentPin = null;
                    }
                }
                //string ioboxes may be comments if they have no connection
                else if (FNodeInfo.Category == "String" && !Node.GetPin("Input String").IsConnected() && !Node.GetPin("Output String").IsConnected())
                {
                    if (FCommentPin == null)
                    {
                        FCommentPin = Node.GetPin("Input String");
                        FCommentPin.AddListener(this);
                    }
                    
                    Comment = FCommentPin.GetValue(0);
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
                //ordinary display IOBox
                else
                {
                    IsIONode = false;
                    Name = FNodeInfo.Username + hyphen + DescriptiveName;
                    
                    //this is no longer a comment iobox
                    if (FCommentPin != null)
                    {
                        FCommentPin.RemoveListener(this);
                        FCommentPin = null;
                    }
                }
            }
            else if (FNodeInfo.Name == "S" || FNodeInfo.Name == "R")
            {
                SRChannel = FChannelPin.GetValue(0);
                Name = FNodeInfo.Username + ": " + SRChannel;
            }
            else
                Name = FNodeInfo.Username + hyphen + DescriptiveName;
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
    }
}
