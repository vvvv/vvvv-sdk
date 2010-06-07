using System;
using System.Collections.Generic;

using VVVV.PluginInterfaces.V1;
using VVVV.Utils.Notify;

namespace VVVV.Nodes.NodeBrowser
{
    /// <summary>
    /// Description of NodeInfoEntry.
    /// </summary>
    public class NodeInfoEntry: Notifier
    {
        public INodeInfo NodeInfo; 
        
        public NodeInfoEntry(INodeInfo nodeInfo): base()
        {
            NodeInfo = nodeInfo;
        }
        
        public string Username
        {
            get {return NodeInfo.Username;}
        }
        
        public string Category
        {
            get {return NodeInfo.Category;}
        }
    }
}
