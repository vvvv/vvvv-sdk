using System;
using System.Collections;
using System.Collections.Generic;

using VVVV.PluginInterfaces.V2;
using VVVV.Core;
using VVVV.Core.View;

namespace VVVV.Nodes.NodeBrowser
{
    /// <summary>
    /// Description of CategoryEntry.
    /// </summary>
    public class CategoryEntry: IEnumerable, INamed, IDescripted
    {
        List<NodeInfoEntry> FNodeInfos = new List<NodeInfoEntry>();
        string FDescription;
        
        public event RenamedHandler Renamed;
        
        public List<NodeInfoEntry> NodeInfoEntries
        {
            get{return FNodeInfos;}
        }
        
        public CategoryEntry(string name, string description): base()
        {
            Name = name;
            FDescription = description;
        }
        
        public string Name
        {
            get;set;
        }
        
        public void Add(INodeInfo nodeInfo)
        {
            var entry = new NodeInfoEntry(nodeInfo);
            FNodeInfos.Add(entry);
            FNodeInfos.Sort(delegate(NodeInfoEntry e1, NodeInfoEntry e2) {return e1.Name.CompareTo(e2.Name);});
        }
        
        public void Remove(INodeInfo nodeInfo)
        {
            for (int i=0; i<Count; i++)
                if (FNodeInfos[i].Name == nodeInfo.Username)
            {
                FNodeInfos.RemoveAt(i);
                break;
            }
        }
        
        public event CollectionDelegate Added;
        
        public event CollectionDelegate Removed;
        
        public int Count 
        {
            get {return FNodeInfos.Count;;}
        }
        
        public System.Collections.IEnumerator GetEnumerator()
        {
            return FNodeInfos.GetEnumerator();
        }
        
        public string Description 
        {
            get{return FDescription;}
        }
    }
}
