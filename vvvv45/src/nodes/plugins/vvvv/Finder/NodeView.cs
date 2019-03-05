using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

using VVVV.Core;
using VVVV.Core.Collections;
using VVVV.Core.Collections.Sync;
using VVVV.Core.View;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V2.Graph;

namespace VVVV.Nodes.Finder
{
    public class NodeView: IParent, INamed, IDescripted, IDecoratable, ISelectable, IDisposable
    {
        #region SolidBrush definitions
        static SolidBrush SDarkGray = new SolidBrush(Color.FromArgb(154, 154, 154));
        static SolidBrush SLightGray = new SolidBrush(Color.FromArgb(192, 192, 192));
        static SolidBrush SHoverGray = new SolidBrush(Color.FromArgb(216, 216, 216));
        
        static SolidBrush SDarkGreen = new SolidBrush(Color.FromArgb(81, 163, 96));
        static SolidBrush SLightGreen = new SolidBrush(Color.FromArgb(157, 232, 177));
        static SolidBrush SHoverGreen = new SolidBrush(Color.FromArgb(161, 226, 185));
        
        static SolidBrush SDarkRed = new SolidBrush(Color.FromArgb(168, 82, 82));
        static SolidBrush SLightRed = new SolidBrush(Color.FromArgb(229, 162, 162));
        static SolidBrush SHoverRed = new SolidBrush(Color.FromArgb(233, 158, 158));
        
        static SolidBrush SDarkBlue = new SolidBrush(Color.FromArgb(82, 112, 168));
        static SolidBrush SLightBlue = new SolidBrush(Color.FromArgb(162, 174, 229));
        static SolidBrush SHoverBlue = new SolidBrush(Color.FromArgb(158, 193, 233));
        #endregion
        
        protected readonly INode2 FNode;
        protected readonly NodeFilter FFilter;
        readonly EditableList<NodeView> FChildNodes = new EditableList<NodeView>();
        readonly int FDepth;
        readonly NodeView FParentNodeView;
        readonly bool FOpenModules;
        
        public NodeView(NodeView parentNodeView, INode2 node, NodeFilter filter, int depth, bool openModules)
        {
            FParentNodeView = parentNodeView;
            FNode = node;
            FFilter = filter;
            FDepth = depth;
            FOpenModules = openModules;
            
            FNode.Added += HandleNodeAdded;
            FNode.Removed += HandleNodeRemoved;
            FNode.StatusChanged += HandleNodeStatusChanged;
            FNode.InnerStatusChanged += HandleNodeInnerStatusChanged;
            FNode.Renamed += HandleNodeRenamed;
            FChildNodes.Updated += HandleChildNodesUpdated;
            
            // Set flags
            Flags = FilterFlags.None;
            var nodeInfo = FNode.NodeInfo;
            if (nodeInfo != null)
            {
                switch (nodeInfo.Type)
                {
                    case NodeType.Native:
                        Flags |= FilterFlags.Native;
                        break;
                    case NodeType.Patch:
                        Flags |= FilterFlags.Patch;
                        break;
                    case NodeType.Module:
                        Flags |= FilterFlags.Module;
                        break;
                    case NodeType.Freeframe:
                        Flags |= FilterFlags.Freeframe;
                        break;
                    case NodeType.VST:
                        Flags |= FilterFlags.VST;
                        break;
                    case NodeType.Effect:
                        Flags |= FilterFlags.Effect;
                        break;
                    case NodeType.Plugin:
                        Flags |= FilterFlags.Plugin;
                        break;
                    case NodeType.Dynamic:
                        Flags |= FilterFlags.Dynamic;
                        break;
                    case NodeType.VL:
                        Flags |= FilterFlags.VL;
                        break;
                    case NodeType.Text:
                        Flags |= FilterFlags.Text;
                        break;
                    case NodeType.Unknown:
                        Flags |= FilterFlags.Unknown;
                        break;
                    default:
                        throw new Exception("Invalid value for NodeType");
                }
            }
            
            if (FNode.HasGUI)
            {
                Flags |= FilterFlags.Window;
            }
            
            ReloadChildren();
        }
        
        public virtual void Dispose()
        {
            FNode.Added -= HandleNodeAdded;
            FNode.Removed -= HandleNodeRemoved;
            FNode.StatusChanged -= HandleNodeStatusChanged;
            FNode.InnerStatusChanged -= HandleNodeInnerStatusChanged;
            FNode.Renamed -= HandleNodeRenamed;
            FChildNodes.Updated -= HandleChildNodesUpdated;
            
            foreach (var nodeView in FChildNodes)
                nodeView.Dispose();
            FChildNodes.Dispose();
        }
        
        public INode2 Node
        {
            get
            {
                return FNode;
            }
        }
        
        public EditableList<NodeView> Children
        {
            get
            {
                return FChildNodes;
            }
        }
        
        private IWindow2 FLastActiveWindow;
        public void SetActiveWindow(IWindow2 window)
        {
            FLastActiveWindow = window;
            
            if (window.Equals(FNode.Window))
            {
                IsActive = true;
            }
            else
            {
                IsActive = false;
            }
            
            foreach (var nodeView in FChildNodes)
            {
                nodeView.SetActiveWindow(window);
            }
        }
        
        private bool FIsActive;
        protected virtual bool IsActive
        {
            get
            {
                return FIsActive;
            }
            set
            {
                if (value != FIsActive)
                {
                    FIsActive = value;
                    OnDecorationChanged();
                }
            }
        }
        
        public NodeView Parent
        {
            get
            {
                return FParentNodeView;
            }
        }
        
        public virtual FilterFlags Flags
        {
            get;
            private set;
        }
        
        private NodeView CreateNodeView(INode2 node)
        {
            NodeView nodeView = null;
            
            var nodeInfo = node.NodeInfo;
            switch (nodeInfo?.Name)
            {
                case "IOBox":
                    nodeView = new IONodeView(this, node, FFilter, FDepth + 1);
                    break;
                case "S":
                    nodeView = new SNodeView(this, node, FFilter, FDepth + 1);
                    break;
                case "R":
                    nodeView = new RNodeView(this, node, FFilter, FDepth + 1);
                    break;
                default:
                    if (FOpenModules)
                        nodeView = new NodeView(this, node, FFilter, FDepth + 1, true);
                    else
                    {
                        switch (nodeInfo?.Type)
                        {
                            case NodeType.Module:
                                nodeView = new ModuleNodeView(this, node, FFilter, FDepth + 1);
                                break;
                            default:
                                nodeView = new NodeView(this, node, FFilter, FDepth + 1, false);
                                break;
                        }
                    }

                    break;
            }
            
            if (FLastActiveWindow != null)
                nodeView.SetActiveWindow(FLastActiveWindow);
            
            return nodeView;
        }
        
        private void AddNodeView(NodeView nodeView)
        {
            nodeView.Renamed += HandleNodeViewRenamed;
            FChildNodes.Add(nodeView);
        }
        
        private void RemoveNodeView(NodeView nodeView)
        {
            nodeView.Renamed -= HandleNodeViewRenamed;
            FChildNodes.Remove(nodeView);
            nodeView.Dispose();
        }
        
        void HandleNodeAdded (IViewableCollection<INode2> collection, INode2 node)
        {
            if (!FChildNodes.Any((n) => n.FNode == node))
            {
                var nodeView = CreateNodeView(node);
                if (nodeView.IsIncludedInFilter())
                {
                    FChildNodes.BeginUpdate();
                    AddNodeView(nodeView);
                    FChildNodes.EndUpdate();
                }
                else
                {
                    nodeView.Dispose();
                }
            }
        }
        
        void HandleNodeRemoved (IViewableCollection<INode2> collection, INode2 node)
        {
            var nodeView = FChildNodes.FirstOrDefault((n) => n.FNode == node);
            if (nodeView != null)
            {
                RemoveNodeView(nodeView);
            }
        }
        
        void HandleChildNodesUpdated(IViewableCollection collection)
        {
            SortChildren();
        }
        
        void HandleNodeRenamed(INamed sender, string newName)
        {
            OnRenamed(newName);
        }
        
        void HandleNodeViewRenamed(INamed sender, string newName)
        {
            SortChildren();
        }
        
        void HandleNodeStatusChanged(object sender, EventArgs e)
        {
            OnDecorationChanged();
        }
        
        void HandleNodeInnerStatusChanged(object sender, EventArgs e)
        {
            OnDecorationChanged();
        }
        
        protected virtual void ReloadChildren()
        {
            if (FNode.Count > 0 && IsInFilterScope())
            {
                FChildNodes.BeginUpdate();
                
                try
                {
                    // Add all nodes we've not yet included
                    foreach (var node in FNode)
                    {
                        HandleNodeAdded(FNode, node);
                    }
                    
                    // Remove all nodes which don't match the filter
                    foreach (var nodeView in FChildNodes.ToList())
                    {
                        if (!nodeView.IsIncludedInFilter())
                        {
                            RemoveNodeView(nodeView);
                        }
                    }
                }
                finally
                {
                    FChildNodes.EndUpdate();
                }
            }
        }
        
        private void SortChildren()
        {
            FChildNodes.Sort(delegate(NodeView nv1, NodeView nv2)
                             {
                                 //order:
                                 //subpatches
                                 //inlets
                                 //outlets
                                 //Sends
                                 //Receives
                                 //comments
                                 //other nodes
                                 
                                 int w1 = nv1.SortPos;
                                 int w2 = nv2.SortPos;
                                 
                                 if ((w1 != 0) || (w2 != 0))
                                 {
                                     if (w1 > w2)
                                         return -1;
                                     else if (w1 < w2)
                                         return 1;
                                     else
                                         return nv1.Name.CompareTo(nv2.Name);
                                 }
                                 else
                                     return nv1.Name.CompareTo(nv2.Name);
                             });
        }
        
        protected int SortPos
        {
            get
            {
                if ((Flags & FilterFlags.Patch) == FilterFlags.Patch)
                    return 100;
                else if ((Flags & FilterFlags.IONode) == FilterFlags.IONode)
                    return 90;
                else if ((Flags & FilterFlags.Send) == FilterFlags.Send)
                    return 81;
                else if ((Flags & FilterFlags.Receive) == FilterFlags.Receive)
                    return 80;
                else if ((Flags & FilterFlags.Comment) == FilterFlags.Comment)
                    return -100;
                else
                    return 0;
            }
        }
        
        private bool IsIncludedInFilter()
        {
            // Check filter scope
            if (!IsInFilterScope())
            {
                return false;
            }
            
            // If we have children (which already checked if they're included) we're done here.
            if (FChildNodes.Count > 0)
            {
                return true;
            }
            
            // Check filter flags
            if ((FFilter.Flags & Flags) == 0)
            {
                return false;
            }
            
            // Check filter tags
            if (FFilter.Tags.Count > 0)
            {
                // See if our id matches one of the tags
                if ((FFilter.Flags & FilterFlags.ID) == FilterFlags.ID)
                {
                    foreach (var tag in FFilter.Tags)
                    {
                        try
                        {
                            if (int.Parse(tag) == FNode.ID)
                            {
                                return true;
                            }
                        }
                        catch (Exception)
                        {
                            // Ignore
                        }
                    }
                }
                
                // See if our label contains one of the tags
                if ((FFilter.Flags & FilterFlags.Label) == FilterFlags.Label)
                {
                    var label = FNode.LabelPin[0];
                    if (!string.IsNullOrEmpty(label))
                    {
                        foreach (var tag in FFilter.Tags)
                        {
                            if (label.Contains(tag))
                            {
                                return true;
                            }
                        }
                    }
                }
                
                // See if our name contains one of the tags
                if ((FFilter.Flags & FilterFlags.Name) == FilterFlags.Name)
                {
                    var name = Name.ToLower();
                    foreach (var tag in FFilter.Tags)
                    {
                        if (name.Contains(tag))
                        {
                            return true;
                        }
                    }
                }
                
                // Give subclasses a chance
                return CheckFilterTags();
            }
            else
                return true;
        }
        
        protected virtual bool CheckFilterTags()
        {
            return false;
        }
        
        private bool IsInFilterScope()
        {
            return FFilter.MinLevel <= FDepth && FDepth <= FFilter.MaxLevel;
        }
        
        #region IParent
        public IEnumerable Childs
        {
            get
            {
                return FChildNodes;
            }
        }
        #endregion
        
        #region INamed
        public event RenamedHandler Renamed;
        
        protected virtual void OnRenamed(string newName)
        {
            if (Renamed != null) {
                Renamed(this, newName);
            }
        }
        
        public virtual string Name
        {
            get
            {
                return FNode.Name;
            }
        }
        #endregion
        
        #region IDescription
        public virtual string Description
        {
            get
            {
                var nodeInfo = FNode.NodeInfo;
                if (nodeInfo.Type == NodeType.Native)
                    return nodeInfo.Username + " [id " + FNode.ID.ToString() + "]";
                else
                    return nodeInfo.Username + " [id " + FNode.ID.ToString() + "]\n" + nodeInfo.Filename;
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
                if (IsActive)
                    return Brushes.White;

                if (FNode.ContainsProblem())
                    return SLightRed;
                else if (FNode.ContainsBoygroupedNodes())
                    return SLightBlue;
                else if (FNode.HasProblem())
                    return SDarkRed;
                else if (FNode.IsExposed())
                    return SDarkGreen;
                else if (FNode.IsBoygrouped())
                    return SDarkBlue;
                else if (FNode.HasPatch)
                    return SDarkGray;
                else
                    return SLightGray;
            }
        }
        
        public System.Drawing.Brush BackHoverColor
        {
            get
            {
                if (FNode.IsMissing() || FNode.ContainsMissingNodes())
                    return SHoverRed;
                else if (FNode.IsBoygrouped() || FNode.ContainsBoygroupedNodes())
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
        
        public string Text
        {
            get
            {
                return BoldenString(Name, FFilter.Tags, "@@", "@@");
            }
        }
        
        public virtual NodeIcon Icon
        {
            get
            {
                if (Node.HasGUI)
                {
                    if (Node.HasCode)
                        return NodeIcon.GUICode;
                    else if (Node.HasPatch)
                        return NodeIcon.GUIPatch;
                    else
                        return NodeIcon.GUI;
                }
                else if (Node.HasCode)
                    return NodeIcon.Code;
                else if (Node.HasPatch)
                    return NodeIcon.Patch;
                
                return NodeIcon.None;
            }
        }
        
        public event DecorationChangedHandler DecorationChanged;
        protected virtual void OnDecorationChanged()
        {
            if (DecorationChanged != null) {
                DecorationChanged();
            }
        }
        #endregion IDecoratable
        
        #region ISelectable
        public event SelectionChangedHandler SelectionChanged;
        
        protected virtual void OnSelectionChanged()
        {
            if (SelectionChanged != null) {
                SelectionChanged(this, EventArgs.Empty);
            }
        }
        
        private bool FSelected;
        public bool Selected
        {
            get
            {
                return FSelected;
            }
            set
            {
                FSelected = value;
                OnSelectionChanged();
            }
        }
        #endregion
        
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
    }
}
