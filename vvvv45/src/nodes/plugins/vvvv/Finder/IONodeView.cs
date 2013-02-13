using System;
using VVVV.Core.View;
using VVVV.PluginInterfaces.V2.Graph;

namespace VVVV.Nodes.Finder
{
    class IONodeView : NodeView
    {
        #region enum IOType
        enum IOTypeCode
        {
            Normal,
            Comment,
            IO
        }
        #endregion
        
        private readonly IPin2 FCommentPin;
        private string FComment;
        private string FLabel;
        
        public IONodeView(NodeView parentNodeView, INode2 node, NodeFilter filter, int depth)
            : base(parentNodeView, node, filter, depth)
        {
            FNode.LabelPin.Changed += HandleLabelPinChanged;
            FLabel = FNode.LabelPin[0];
            FComment = string.Empty;
            
            if (FNode.NodeInfo.Category == "String")
            {
                FCommentPin = FNode.FindPin("Input String");
                FCommentPin.Changed += HandleCommentPinChanged;
                FComment = FCommentPin[0] ?? string.Empty;
            }
        }
        
        public override void Dispose()
        {
            FNode.LabelPin.Changed -= HandleLabelPinChanged;
            
            if (FCommentPin != null)
            {
                FCommentPin.Changed -= HandleCommentPinChanged;
            }
            
            base.Dispose();
        }
        
        private IOTypeCode IOType
        {
            get
            {
                if (!string.IsNullOrEmpty(FLabel))
                {
                    return IOTypeCode.IO;
                }
                else if (FNode.NodeInfo.Category == "String" && !FNode.IsConnected())
                {
                    return IOTypeCode.Comment;
                }
                else
                {
                    return IOTypeCode.Normal;
                }
            }
        }
        
        void HandleLabelPinChanged(object sender, EventArgs e)
        {
            FLabel = FNode.LabelPin[0];
            
            if (IOType == IOTypeCode.IO)
            {
                OnRenamed(Name);
            }
        }
        
        void HandleCommentPinChanged(object sender, EventArgs e)
        {
            FComment = FCommentPin[0] ?? string.Empty;
            
            if (IOType == IOTypeCode.Comment)
            {
                OnRenamed(Name);
            }
        }
        
        public override FilterFlags Flags
        {
            get
            {
            	var flags = base.Flags;
            	if (Node.IsExposed())
            		flags |= FilterFlags.Exposed;
            	
                switch (IOType)
                {
                    case IONodeView.IOTypeCode.Comment:
                        return flags | FilterFlags.Comment;
                    case IONodeView.IOTypeCode.IO:
                        return flags | FilterFlags.IONode;
                }
                	
                return flags;
            }
        }
        
        protected override bool CheckFilterTags()
        {
            if ((FFilter.Flags & FilterFlags.Comment) == FilterFlags.Comment && IOType == IOTypeCode.Comment)
            {
                foreach (var tag in FFilter.Tags)
                {
                    if (FComment.Contains(tag))
                    {
                        return true;
                    }
                }
            }
            
            return base.CheckFilterTags();
        }
        
        public override string Name
        {
            get
            {
                switch (IOType)
                {
                    case IONodeView.IOTypeCode.Comment:
                        var cmt = FCommentPin[0];
                        if (!string.IsNullOrEmpty(cmt))
                        {
                            var maxChar = 30;
                            var linebreak = cmt.IndexOf("\n");
                            if (linebreak > 0 && linebreak < maxChar)
                                return cmt.Substring(0, linebreak) + "...";
                            else if (cmt.Length > maxChar)
                                return cmt.Substring(0, maxChar) + "...";
                            else
                                return cmt;
                        }
                        break;
                    case IONodeView.IOTypeCode.IO:
                        return FLabel;
                }
                
                return base.Name;
            }
        }
        
        public override string Description
        {
            get
            {
                switch (IOType) {
                    case IONodeView.IOTypeCode.Comment:
                        return FComment;
                    case IONodeView.IOTypeCode.IO:
                        return "IO " + FNode.NodeInfo.Category + " [id " + FNode.ID.ToString() + "]\n" + FLabel;
                    default:
                        return base.Description;
                }
            }
        }
        
        public override VVVV.Core.View.NodeIcon Icon
        {
            get
            {
                switch (IOType)
                {
                    case IONodeView.IOTypeCode.Comment:
                        return NodeIcon.Comment;
                    case IONodeView.IOTypeCode.IO:
                        return NodeIcon.IONode;
                    default:
                        return base.Icon;
                }
            }
        }
    }
}
