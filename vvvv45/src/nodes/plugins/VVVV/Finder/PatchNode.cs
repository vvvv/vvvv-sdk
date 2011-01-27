using System;
using System.Drawing;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

using VVVV.Core;
using VVVV.Core.View;
using VVVV.Core.Collections;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V2.Graph;
using System.Diagnostics;

namespace VVVV.Nodes.Finder
{
    public class PatchNode: IParent, INamed, IDescripted, ISelectable, IDecoratable, ILinkable, IDisposable
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
        
        public IEnumerable Childs { get; private set; }
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
        private string FDecoratedName;
        private IPin2 FChannelPin;
        private IPin2 FLabelPin;
        private IPin2 FCommentPin;
        private INodeInfo FNodeInfo;
        private Filter FFilter;
        private readonly bool FRecursively;
        
        public PatchNode(INode2 self, Filter filter, bool includeChildren, bool recursively)
        {
            FNode = self;
			Debug.Assert(Node != null);
			
            FFilter = filter;
            FRecursively = recursively;
			
			Name = Node.Name;
            Childs = FChildNodes.AsViewableList();
			
			Node.Added += HandleNodeAdded;
			Node.Removed += HandleNodeRemoved;
            
            //init static properties via INode
            ID = FNode.ID;
            FNodeInfo = FNode.NodeInfo;
            NodeType = FNodeInfo.Type;
            
            FLabelPin = FindPin(Node, "Descriptive Name");
			FLabelPin.Changed += HandlePinChanged;
            
            if (FNodeInfo.Name == "S")
            {
                FChannelPin = FindPin(Node, "Send String");
                FIsSource = true;
            }
            else if (FNodeInfo.Name == "R")
                FChannelPin = FindPin(Node, "Receive String");
            
            if (FChannelPin != null)
            {
				FChannelPin.Changed += HandlePinChanged;
                Icon = NodeIcon.SRNode;
            }
            
            //init dynamic properties via INode
            UpdateProperties();
            UpdateName();
            
            if ((NodeType == NodeType.Patch || (NodeType == NodeType.Module && FFilter.Scope == SearchScope.Local)) && includeChildren)
                InitChildren(recursively);
        }
        
        public PatchNode(INode2 self) 
			: this(self, new Filter(), true, true)
        {
        }
        
        public void Dispose()
        {
            //remove pinlisteners
            if (FLabelPin != null)
            {
				FLabelPin.Changed -= HandlePinChanged;;
                FLabelPin = null;
            }
            
            if (FCommentPin != null)
            {
				FCommentPin.Changed -= HandlePinChanged;;
                FCommentPin = null;
            }
            
            if (FChannelPin != null)
            {
				FChannelPin.Changed -= HandlePinChanged;
                FChannelPin = null;
            }
            
            //remove nodelistener
            if (FNode != null)
            {
                FNode.Added -= HandleNodeAdded;
				FNode.Removed -= HandleNodeRemoved;
            }
            
            //free children
            foreach (var child in FChildNodes)
            {
                child.Renamed -= child_Renamed;
                child.Dispose();
            }

            FChildNodes.Dispose();
        }
		
		public override string ToString ()
		{
			return string.Format ("[PatchNode: Childs={0}, ChildNodes={1}, ID={2}, SRChannel={3}, Comment={4}, DescriptiveName={5}, IsIONode={6}, IsBoygrouped={7}, IsMissing={8}, NodeType={9}, IsActiveWindow={10}, Node={11}, Selected={12}, Name={13}, Description={14}, TextColor={15}, TextHoverColor={16}, BackColor={17}, BackHoverColor={18}, OutlineColor={19}, Text={20}, Icon={21}, IsSource={22}, Channel={23}]", Childs, ChildNodes, ID, SRChannel, Comment, DescriptiveName, IsIONode, IsBoygrouped, IsMissing, NodeType, IsActiveWindow, Node, Selected, Name, Description, TextColor, TextHoverColor, BackColor, BackHoverColor, OutlineColor, Text, Icon, IsSource, Channel);
		}
        
        private readonly INode2 FNode;
        public INode2 Node
        {
            get
            {
                return FNode;
            }
        }
		
		IPin2 FindPin(INode2 node, string name)
		{
			var query =
				from pin in node.Pins
				where pin.Name == name
				select pin;
            return query.FirstOrDefault();
		}

        void HandlePinChanged (object sender, EventArgs e)
        {
        	//may be called via FCommentPin, FLabelPin, FChannelPin
            //in all cases do:
            UpdateName();
            
            OnRenamed(Name);
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
        
        private bool CheckForInclusion(PatchNode node)
        {
            if (FFilter.Tags == null)
                return true;
            
            bool include = false;
            
            if (FFilter.Tags.Count == 0)
            {
                if (FFilter.QuickTagsUsed())
                {
                    include = FFilter.SendReceive && !string.IsNullOrEmpty(node.SRChannel);
                    include |= FFilter.Comments && !string.IsNullOrEmpty(node.Comment);
                    include |= FFilter.Labels && !string.IsNullOrEmpty(node.DescriptiveName);
                    include |= FFilter.IONodes && node.IsIONode;
                    include |= FFilter.Natives && node.NodeType == NodeType.Native;
                    include |= FFilter.Modules && node.NodeType == NodeType.Module;
                    include |= FFilter.Effects && node.NodeType == NodeType.Effect;
                    include |= FFilter.Freeframes && node.NodeType == NodeType.Freeframe;
                    include |= FFilter.VSTs && node.NodeType == NodeType.VST;
                    include |= FFilter.Plugins && (node.NodeType == NodeType.Plugin || node.NodeType == NodeType.Dynamic);
                    include |= FFilter.Patches && node.NodeType == NodeType.Patch;
                    include |= FFilter.Unknowns && node.IsMissing;
                    include |= FFilter.Boygrouped && node.IsBoygrouped;
                    include |= FFilter.Addons && (node.NodeType != NodeType.Native && node.NodeType != NodeType.Text && node.NodeType != NodeType.Patch);
                    include |= FFilter.Windows && (node.Node.HasGUI || (node.Node.HasPatch && node.NodeType != NodeType.Module));
                }
                else
                    include = true;
            }
            else
            {
                if (FFilter.SendReceive && !string.IsNullOrEmpty(node.SRChannel))
                {
                    var inc = true;
                    var channel = node.SRChannel.ToLower();
                    
                    foreach (string tag in FFilter.Tags)
                        inc = inc && channel.Contains(tag);
                    include |= inc;
                }
                if (FFilter.IDs)
                {
                    var inc = true;
                    var id = node.ID;
                    
                    foreach (string tag in FFilter.Tags)
                        inc = inc && int.Parse(tag) == id;
                    include |= inc;
                }
                if (FFilter.Comments && !string.IsNullOrEmpty(node.Comment))
                {
                    var inc = true;
                    var comment = node.Comment.ToLower();
                    
                    foreach (string tag in FFilter.Tags)
                        inc = inc && comment.Contains(tag);
                    include |= inc;
                }
                if (FFilter.IONodes && node.IsIONode)
                {
                    var inc = true;
                    var dname = node.DescriptiveName.ToLower();
                    
                    foreach (string tag in FFilter.Tags)
                        inc = inc && dname.Contains(tag);
                    include |= inc;
                }
                if (FFilter.Labels && !string.IsNullOrEmpty(node.DescriptiveName))
                {
                    var inc = true;
                    var dname = node.DescriptiveName.ToLower();
                    
                    foreach (string tag in FFilter.Tags)
                        inc = inc && dname.Contains(tag);
                    include |= inc;
                }
                if ((FFilter.Effects && node.NodeType == NodeType.Effect)
                    || (FFilter.Modules && node.NodeType == NodeType.Module)
                    || (FFilter.Plugins && (node.NodeType == NodeType.Plugin || node.NodeType == NodeType.Dynamic))
                    || (FFilter.Freeframes && node.NodeType == NodeType.Freeframe)
                    || (FFilter.Natives && node.NodeType == NodeType.Native)
                    || (FFilter.VSTs && node.NodeType == NodeType.VST)
                    || (FFilter.Patches && node.NodeType == NodeType.Patch)
                    || (FFilter.Unknowns && node.IsMissing)
                    || (FFilter.Boygrouped && node.IsBoygrouped)
                    || (FFilter.Addons && (node.NodeType != NodeType.Native && node.NodeType != NodeType.Text && node.NodeType != NodeType.Patch))
                    || (FFilter.Windows && (node.Node.HasGUI || (node.Node.HasPatch && node.NodeType != NodeType.Module))))
                {
                    var inc = true;
                    var name = node.Name.ToLower();
                    
                    foreach (string tag in FFilter.Tags)
                        inc = inc && name.Contains(tag);
                    include |= inc;
                }
                
                //if non of the one-character tags is chosen
                if (!FFilter.QuickTagsUsed())
                {
                    var inc = true;
                    var name = node.Name.ToLower();
                    
                    foreach (string tag in FFilter.Tags)
                        inc = inc && name.Contains(tag);
                    include |= inc;
                }
            }
            
            if (include)
                node.SetTags(FFilter.Tags);
            
            return include;
        }
        
        public PatchNode FindNode(INode2 node)
        {
            if (FNode == node)
                return this;
            else
                foreach(var child in FChildNodes)
	            {
	                var n = child.FindNode(node);
	                if (n != null)
	                    return n;
	            }
            
            return null;
        }
        
        public void SetActiveWindow(IWindow2 window)
        {
            var windowState = IsActiveWindow;
            
            //hand window downtree until someone finds itself in the window
            if (FNode.Window == window)
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
                
                if (FNode.HasPatch)
                {    if (FNode.ContainsMissingNodes)
                        return SDarkRed;
                    else if (FNode.ContainsBoygroupedNodes)
                        return SDarkBlue;
                    else
                        return SDarkGray;
                }
                else if (FNode.IsMissing)
                    return SLightRed;
                else if (FNode.IsBoygrouped)
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
                
                if (FNode.HasPatch)
                {   if (FNode.ContainsMissingNodes)
                        return SHoverRed;
                    else if (FNode.ContainsBoygroupedNodes)
                        return SHoverBlue;
                    else
                        return SHoverGray;
                }
                else if (FNode.IsMissing)
                    return SHoverRed;
                else if (FNode.IsBoygrouped)
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
                    return SRChannel + " - " + FNode.NodeInfo.Category;
                else
                    return null;
            }
        }
        #endregion ILinkable
		
		void HandleNodeAdded (IViewableCollection collection, object item)
        {
			var childNode = item as INode2;
			
			bool found = false;
            foreach(var child in FChildNodes)
			{
                if (child.Node == childNode)
	            {
	                found = true;
	                break;
	            }
			}
            
            if (!found)
            {
                FChildNodes.BeginUpdate();
                try
                {
                    var pn = new PatchNode(childNode, FFilter, FRecursively, FRecursively);
                    if (CheckForInclusion(pn))
                    {
                        FChildNodes.Add(pn);
                        pn.Renamed += child_Renamed;
                        SortChildren();
                    }
					else
						pn.Dispose();
                }
                finally
                {
                    FChildNodes.EndUpdate();
                }
            }
        }
		
		void HandleNodeRemoved (IViewableCollection collection, object item)
        {
        	var childNode = item as INode2;
			
			foreach(var child in FChildNodes)
			{
                if (child.Node == childNode)
	            {
	                FChildNodes.Remove(child);
	                child.Renamed -= child_Renamed;
	                child.Dispose();
	                break;
	            }
			}
        }
		
		void child_Renamed(INamed sender, string newName)
        {
            SortChildren();
        }
        
        private void InitChildren(bool recursively)
        {
            if (FNode.Count > 0)
            {
                FChildNodes.BeginUpdate();
                try
                {
                    foreach(var child in FNode)
                    {
                        var pn = new PatchNode(child, FFilter, recursively, recursively);
                        if (pn.ChildNodes.Count > 0 || CheckForInclusion(pn))
                            FChildNodes.Add(pn);
						else
							pn.Dispose();
                    }
                    
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
                                 
                                 if ((w1 != 0) || (w2 != 0))
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
                pos = -100;
            
            return pos;
        }
        
        private void UpdateProperties()
        {
            IsBoygrouped = FNode.IsBoygrouped;
            IsMissing = FNode.IsMissing;
            
            if (Node.HasGUI)
            {
                if (Node.HasCode)
                    Icon = NodeIcon.GUICode;
                else if (Node.HasPatch)
                    Icon = NodeIcon.GUIPatch;
                else
                    Icon = NodeIcon.GUI;
            }
            else if (Node.HasCode)
                Icon = NodeIcon.Code;
            else if (Node.HasPatch)
                Icon = NodeIcon.Patch;
        }
        
        private void UpdateName()
        {
            DescriptiveName = FLabelPin[0];
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
                        FCommentPin.Changed -= HandlePinChanged;
                        FCommentPin = null;
                    }
                }
                //string ioboxes may be comments if they have no connection
                else if (FNodeInfo.Category == "String" && !FindPin(Node, "Input String").IsConnected && !FindPin(Node, "Output String").IsConnected)
                {
                    if (FCommentPin == null)
                    {
                        FCommentPin = FindPin(Node, "Input String");
                        FCommentPin.Changed += HandlePinChanged;
                    }
                    
                    Comment = FCommentPin[0];
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
                        FCommentPin.Changed -= HandlePinChanged;
                        FCommentPin = null;
                    }
                }
            }
            else if (FNodeInfo.Name == "S" || FNodeInfo.Name == "R")
            {
                SRChannel = FChannelPin[0];
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
