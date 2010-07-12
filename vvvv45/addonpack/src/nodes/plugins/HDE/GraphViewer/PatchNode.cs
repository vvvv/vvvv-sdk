using System;
using System.Collections.Generic;

using VVVV.Core;
using VVVV.Core.View;
using VVVV.PluginInterfaces.V1;

namespace VVVV.Nodes.GraphViewer
{
    public class PatchNode: IViewableCollection, INamed, IDescripted, INodeChangedListener
    {
        List<PatchNode> FChildNodes = new List<PatchNode>();
        
        private PatchNode FUpNode;
        public INode Node{get;set;}
        public bool Selected{get;set;}
        public bool MarkedForDelete{get;set;}
        
        public PatchNode(INode self)
        {
            Node = self;
            
            if (Node != null)
            {
                UpdateChildren();
                Node.AddListener(this);
            }
        }
        
        private void UpdateChildren()
        {
            INode[] children = Node.GetChildren();
            if (children != null)
            {
                //mark all children for being deleted
                foreach(var child in FChildNodes)
                    child.MarkedForDelete = true;
                
                //unmark existing children from being deleted and add new ones
                if (FUpNode != null)
                    FUpNode.MarkedForDelete = false;
                foreach(INode child in children)
                {
                    bool found = false;
                    foreach(var c in FChildNodes)
                        if (c.Node == child)
                    {
                        c.MarkedForDelete = false;
                        found = true;
                        break;                        
                    }

                    if (!found)
                        Add(new PatchNode(child));
                }
                
                //remove all children still marked for delete
                for (int i=FChildNodes.Count-1; i>=0; i--) 
                {
                    if (FChildNodes[i].MarkedForDelete)
                        Remove(FChildNodes[i]);
                }
                
                FChildNodes.Sort(delegate(PatchNode p1, PatchNode p2) {return p1.Name.CompareTo(p2.Name);});
                
                //insert an .. (up) node as first element
                if (FUpNode == null)
                {
                    FUpNode = new PatchNode(null);
                    FChildNodes.Insert(0, FUpNode);
                }                
            }
        }
        
        public void UnSubscribe()
        {
            Node.RemoveListener(this);
        }
        
        public void SelectNodes(INode[] nodes)
        {
            //deselect all
            foreach(PatchNode pn in FChildNodes)
                pn.Selected = false;
            
            if (nodes != null)
                //reselect selected
                foreach(INode node in nodes)
            {
                PatchNode pn = FChildNodes.Find(delegate (PatchNode p) {return p.Node == node;});
                if (pn != null)
                    pn.Selected = true;
            }
        }
        
        public void Add(PatchNode patchNode)
        {
            FChildNodes.Add(patchNode);
            OnAdded(patchNode);
        }
        
        public void Remove(PatchNode patchNode)
        {
            FChildNodes.Remove(patchNode);
            OnRemoved(patchNode);
        }
        
        public event CollectionDelegate Added;
        
        protected virtual void OnAdded(object item)
        {
            if (Added != null) {
                Added(this, item);
            }
        }
        
        public event CollectionDelegate Removed;
        
        protected virtual void OnRemoved(object item)
        {
            if (Removed != null) {
                Removed(this, item);
            }
        }
        
        public int Count 
        {
            get{return FChildNodes.Count;}
        }
        
        public System.Collections.IEnumerator GetEnumerator()
        {
            return FChildNodes.GetEnumerator();
        }
        
        public string Name 
        {
            get 
            {
                string descriptiveName = "";
                string srChannel = "";
                string comment = "";
                if ((Node != null) && (Node.GetNodeInfo() != null))
                {
                    descriptiveName = Node.GetPin("Descriptive Name").GetValue(0) + " ";
                                        
                    if (Node.GetNodeInfo().Username == "IOBox (String)")
                    {
                        if ((!Node.GetPin("Input String").IsConnected()) && (!Node.GetPin("Output String").IsConnected()))
                            comment = Node.GetPin("Input String").GetValue(0) + " ";
                    }
                    else if (Node.GetNodeInfo().Name == "S")
                        srChannel = Node.GetPin("SendString").GetValue(0) + " "; 
                    else if (Node.GetNodeInfo().Name == "R")
                        srChannel = Node.GetPin("ReceiveString").GetValue(0) + " "; 
                    
                    
                    return Node.GetNodeInfo().Username + " " + descriptiveName + srChannel + comment;
                }
                else
                    return "..";
            }
        }
        
        public string Description 
        {
            get 
            {
                var ni = Node.GetNodeInfo();
                if (ni.Type == TNodeType.Native)
                    return "[id " + Node.GetID().ToString() + "]"; 
                else
                    return ni.Filename + " [id " + Node.GetID().ToString() + "]";
            }
        }
        
        public void NodeChangedCB()
        {
            UpdateChildren();
        }
        
        public event RenamedHandler Renamed;
        
        public bool Contains(object item)
        {
            throw new NotImplementedException();
        }
    }
}
