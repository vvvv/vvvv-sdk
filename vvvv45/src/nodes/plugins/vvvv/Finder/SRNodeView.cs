using System;
using VVVV.Core.View;
using VVVV.Core.View.GraphicalEditor;
using VVVV.PluginInterfaces.V2.Graph;

namespace VVVV.Nodes.Finder
{
    abstract class SRNodeView : NodeView
    {
        private readonly IPin2 FChannelPin;
        protected string FSRChannel;
        
        public SRNodeView(NodeView parentNodeView, INode2 node, NodeFilter filter, int depth)
            : base(parentNodeView, node, filter, depth, false)
        {
            FChannelPin = FNode.FindPin(ChannelPinName);
            FChannelPin.Changed += HandleChannelPinChanged;
            
            FSRChannel = FChannelPin[0];
        }
        
        public override void Dispose()
        {
            FChannelPin.Changed -= HandleChannelPinChanged;
            
            base.Dispose();
        }
        
        protected abstract string ChannelPinName
        {
            get;
        }
        
        void HandleChannelPinChanged(object sender, EventArgs e)
        {
            FSRChannel = FChannelPin[0];
            OnRenamed(Name);
        }
        
        public override string Name 
        {
            get 
            { 
                return FNode.NodeInfo.Username + ": " + FSRChannel;
            }
        }
        
        public override string Description 
        {
            get 
            { 
                return FNode.NodeInfo.Username + " [id " + FNode.ID.ToString() + "]\nChannel: " + FSRChannel;
            }
        }
        
        public override NodeIcon Icon 
        {
            get 
            { 
                return NodeIcon.SRNode;
            }
        }
    }
    
    class SNodeView : SRNodeView, ILinkSource
    {
        public SNodeView(NodeView parentNodeView, INode2 node, NodeFilter filter, int depth)
            : base(parentNodeView, node, filter, depth)
        {
            
        }
        
        public override FilterFlags Flags 
        {
            get 
            { 
                return base.Flags | FilterFlags.Send;
            }
        }
        
        protected override string ChannelPinName 
        {
            get 
            {
                return "Send String";
            }
        }
        
        public string Channel
        {
            get
            {
                return FSRChannel;
            }
        }
    }
    
    class RNodeView : SRNodeView, ILinkSink
    {
        public RNodeView(NodeView parentNodeView, INode2 node, NodeFilter filter, int depth)
            : base(parentNodeView, node, filter, depth)
        {
            
        }
        
        public override FilterFlags Flags 
        {
            get 
            { 
                return base.Flags | FilterFlags.Receive;
            }
        }
        
        protected override string ChannelPinName 
        {
            get 
            {
                return "Receive String";
            }
        }
        
        public bool Accepts(ILinkSource source)
        {
            return source.Channel == FSRChannel;
        }
    }
}
