using System;
using System.Collections.Generic;

using VVVV.PluginInterfaces.V2;
using VVVV.Core;
using VVVV.Core.View;

namespace VVVV.Nodes.NodeBrowser
{
    public class NodeInfoEntry: INamed, IDescripted, IDraggable
    {
        public INodeInfo NodeInfo;
        
        public event RenamedHandler Renamed;
        
		protected virtual void OnRenamed(string newName)
		{
			if (Renamed != null) {
				Renamed(this, newName);
			}
		}
        string FCategory;
        string FUsername;
        string FSystemname;
        string FTooltip;
        
        public NodeInfoEntry(INodeInfo nodeInfo): base()
        {
            NodeInfo = nodeInfo;
            FCategory = NodeInfo.Category;
            FUsername = NodeInfo.Username;
            FSystemname = NodeInfo.Systemname;
            
            FTooltip = "";
            switch (NodeInfo.Type)
            {
                    case NodeType.Native: {FTooltip = ""; break;}
                    case NodeType.Plugin: {FTooltip = "p  "; break;}
                    case NodeType.Module: {FTooltip = "m  "; break;}
                    case NodeType.Dynamic: {FTooltip = "d  "; break;}
                    case NodeType.Patch: {FTooltip = "v4p "; break;}
                    case NodeType.Effect: {FTooltip = "x  "; break;}
                    case NodeType.Freeframe: {FTooltip = "f  "; break;}
                    case NodeType.VST: {FTooltip = "a "; break;}
                    case NodeType.VL: {FTooltip = "VL"; break;}
            }
            
            if (!string.IsNullOrEmpty(NodeInfo.Shortcut))
                FTooltip += "(" + NodeInfo.Shortcut + ") " ;
            if (!string.IsNullOrEmpty(NodeInfo.Help))
                FTooltip += NodeInfo.Help;
            if (!string.IsNullOrEmpty(NodeInfo.Warnings))
                FTooltip += "\n WARNINGS: " + NodeInfo.Warnings;
            if (!string.IsNullOrEmpty(NodeInfo.Bugs))
                FTooltip += "\n BUGS: " + NodeInfo.Bugs;
            if ((!string.IsNullOrEmpty(NodeInfo.Author)) && (NodeInfo.Author != "vvvv group"))
                FTooltip += "\n AUTHOR: " + NodeInfo.Author;
            if (!string.IsNullOrEmpty(NodeInfo.Credits))
                FTooltip += "\n CREDITS: " + NodeInfo.Credits;
        }
        
        public string Category
        {
            get 
            {
                return FCategory;
            }
        }
        
        public string Name
        {
            get 
            {
                return FUsername;
            }
        }
        
        public string Description
        {
            get
            {
                return FTooltip;
            }
        }
        
        public bool AllowDrag()
        {
            return true;
        }
        
        public object ItemToDrag()
        {
            return FSystemname;
        }
    }
}
