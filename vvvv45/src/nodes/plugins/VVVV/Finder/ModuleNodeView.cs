using System;
using VVVV.PluginInterfaces.V2.Graph;

namespace VVVV.Nodes.Finder
{
    class ModuleNodeView : NodeView
    {
        public ModuleNodeView(NodeView parentNodeView, INode2 node, NodeFilter filter, int depth)
            : base(parentNodeView, node, filter, depth)
        {
        }
        
        protected override bool IsActive 
        {
            get 
            {
                return base.IsActive;
            }
            set 
            { 
                if (value != IsActive)
                {
                    if (value)
                        ReloadChildren();
                    else
                        Children.Clear();
                }
                
                base.IsActive = value;
            }
        }
    }
}
