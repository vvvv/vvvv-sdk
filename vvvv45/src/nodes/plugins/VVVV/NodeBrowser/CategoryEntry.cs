using System;
using System.Collections;
using System.Collections.Generic;

using VVVV.PluginInterfaces.V2;
using VVVV.Core;
using VVVV.Core.View;

namespace VVVV.Nodes.NodeBrowser
{
    public class CategoryEntry: IEnumerable, INamed, IDescripted
    {
        private readonly List<NodeInfoEntry> FNodeInfos = new List<NodeInfoEntry>();
        private readonly string FDescription;
        
        public event RenamedHandler Renamed;
        
        protected virtual void OnRenamed(string newName)
        {
            if (Renamed != null) {
                Renamed(this, newName);
            }
        }
        
        public List<NodeInfoEntry> NodeInfoEntries
        {
            get
            {
                return FNodeInfos;
            }
        }
        
        public CategoryEntry(string name, string description): base()
        {
            Name = name;
            FDescription = description;
        }
        
        public string Name
        {
            get;
            private set;
        }
        
        public void Add(INodeInfo nodeInfo)
        {
            var entry = new NodeInfoEntry(nodeInfo);
            FNodeInfos.Add(entry);
            FNodeInfos.Sort((e1, e2) => e1.Name.CompareTo(e2.Name));
        }
        
        public void Remove(INodeInfo nodeInfo)
        {
            for (int i = 0; i < Count; i++)
            {
                if (FNodeInfos[i].Name == nodeInfo.Username)
                {
                    FNodeInfos.RemoveAt(i);
                    break;
                }
            }
        }
        
        public int Count
        {
            get
            {
                return FNodeInfos.Count;
            }
        }
        
        public System.Collections.IEnumerator GetEnumerator()
        {
            return FNodeInfos.GetEnumerator();
        }
        
        public string Description
        {
            get
            {
                return FDescription;
            }
        }
    }
}
