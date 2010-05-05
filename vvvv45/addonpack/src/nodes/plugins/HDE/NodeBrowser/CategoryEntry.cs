using System;
using System.Collections.Generic;

using VVVV.PluginInterfaces.V1;
using VVVV.Utils.Notifier;

namespace VVVV.Nodes.NodeBrowser
{
    /// <summary>
    /// Description of CategoryEntry.
    /// </summary>
    public class CategoryEntry: Notifier
    {
        List<NodeInfoDummy> FNodeInfos = new List<NodeInfoDummy>();
        
        public CategoryEntry(string name)
        {
            Name = name;
        }
        
        public string Name
        {
            get;set;
        }
        
        public void Add(INodeInfo nodeInfo)
        {
            FNodeInfos.Add(new NodeInfoDummy(nodeInfo));
            FNodeInfos.Sort(delegate(NodeInfoDummy e1, NodeInfoDummy e2) {return e1.Username.CompareTo(e2.Username);});
            //FireOnNotifyChanged();
        }
        
        public void Remove(INodeInfo nodeInfo)
        {
           // FNodeInfos.Remove(nodeInfo);
            //FireOnNotifyChanged();
        }
        
        public object[] GetNodeInfos()
        {
            return FNodeInfos.ToArray();
        }
    }
}
