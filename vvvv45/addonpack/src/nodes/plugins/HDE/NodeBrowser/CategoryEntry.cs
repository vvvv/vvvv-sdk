using System;
using System.Collections.Generic;

using VVVV.PluginInterfaces.V1;
using VVVV.Utils.Notify;

namespace VVVV.Nodes.NodeBrowser
{
    /// <summary>
    /// Description of CategoryEntry.
    /// </summary>
    public class CategoryEntry: Notifier
    {
        List<NodeInfoEntry> FNodeInfos = new List<NodeInfoEntry>();
        public List<NodeInfoEntry> NodeInfoEntries
        {
            get{return FNodeInfos;}
        }
        
        public CategoryEntry(string name): base()
        {
            Name = name;
        }
        
        public string Name
        {
            get;set;
        }
        
        public void Add(NodeInfoEntry entry)
        {
            FNodeInfos.Add(entry);
            FNodeInfos.Sort(delegate(NodeInfoEntry e1, NodeInfoEntry e2) {return e1.Username.CompareTo(e2.Username);});
            //FireOnNotifyChanged();
        }
        
        public void Remove(NodeInfoEntry entry)
        {
            FNodeInfos.Remove(entry);
            //FireOnNotifyChanged();
        }
    }
}
