using System;
using System.Collections.Generic;

using VVVV.PluginInterfaces.V1;
using VVVV.Core;
using VVVV.Core.View;

namespace VVVV.Nodes.NodeBrowser
{
    /// <summary>
    /// Description of NodeInfoEntry.
    /// </summary>
    public class NodeInfoEntry: INamed, IDescripted, IDraggable
    {
        public INodeInfo NodeInfo;
        
        public event RenamedHandler Renamed;
        
        public NodeInfoEntry(INodeInfo nodeInfo): base()
        {
            NodeInfo = nodeInfo;
        }
        
        public string Category
        {
            get {return NodeInfo.Category;}
        }
        
        public string Name
        {
            get {return NodeInfo.Username;}
        }
        
        public string Description
        {
            get
            {
                string tip = "";
                switch (NodeInfo.Type)
                {
                        case TNodeType.Native: {tip = ""; break;}
                        case TNodeType.Plugin: {tip = "P  "; break;}
                        case TNodeType.Patch: {tip = "M  "; break;}
                        case TNodeType.Effect: {tip = "FX  "; break;}
                        case TNodeType.Freeframe: {tip = "FF  "; break;}
                        case TNodeType.VST: {tip = "V  "; break;}
                }
                
                if (!string.IsNullOrEmpty(NodeInfo.ShortCut))
                    tip += "(" + NodeInfo.ShortCut + ") " ;
                if (!string.IsNullOrEmpty(NodeInfo.Help))
                    tip += NodeInfo.Help;
                if (!string.IsNullOrEmpty(NodeInfo.Warnings))
                    tip += "\n WARNINGS: " + NodeInfo.Warnings;
                if (!string.IsNullOrEmpty(NodeInfo.Bugs))
                    tip += "\n BUGS: " + NodeInfo.Bugs;
                if ((!string.IsNullOrEmpty(NodeInfo.Author)) && (NodeInfo.Author != "vvvv group"))
                    tip += "\n AUTHOR: " + NodeInfo.Author;
                if (!string.IsNullOrEmpty(NodeInfo.Credits))
                    tip += "\n CREDITS: " + NodeInfo.Credits;
                return tip;
            }
        }
        
        public bool AllowDrag()
        {
            return true;
        }
        
        public object ItemToDrag()
        {
            return NodeInfo.Systemname;
        }
    }
}
