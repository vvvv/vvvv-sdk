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
        
        protected override void ReloadChildren()
        {
            if (IsActive)
            {
               base.ReloadChildren();
            }
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
                    base.IsActive = value;
                    
                    if (value)
                        ReloadChildren();
                    else
                        Children.Clear();
                }
            }
        }
    }
}
