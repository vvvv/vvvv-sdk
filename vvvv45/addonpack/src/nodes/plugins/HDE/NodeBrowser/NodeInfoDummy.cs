using System;
using System.Collections.Generic;

using VVVV.PluginInterfaces.V1;
using VVVV.Utils.Notifier;

namespace VVVV.Nodes.NodeBrowser
{
    /// <summary>
    /// Description of NodeInfoDummy.
    /// </summary>
    public class NodeInfoDummy
    {
        INodeInfo FNodeInfo; 
        
        public NodeInfoDummy(INodeInfo nodeInfo)
        {
            FNodeInfo = nodeInfo;
        }
        
        public string Username
        {
            get {return FNodeInfo.Username;}
        }
    }
}
