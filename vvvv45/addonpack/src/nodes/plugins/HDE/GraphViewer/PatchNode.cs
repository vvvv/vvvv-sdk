using System;
using System.Collections.Generic;
using System.Reflection;
using System.CodeDom.Compiler;

using VVVV.Utils.Notify;
using VVVV.PluginInterfaces.V1;

namespace VVVV.Nodes.GraphViewer
{
    public class PatchNode: Notifier, INodeChangedListener
    {
        List<PatchNode> FChildren = new List<PatchNode>();
        
        private PatchNode FUpNode;
        public INode Node{get;set;}
        public bool Selected{get;set;}
        public bool MarkedForDelete{get;set;}
        public string Text
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
                    
                    
                    return Node.GetNodeInfo().Username + " " + descriptiveName + srChannel + comment + "[id " + Node.GetID().ToString() + "]";
                }
                else
                    return "..";
            }
        }
        
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
                foreach(var child in FChildren)
                    child.MarkedForDelete = true;
                
                //unmark existing children from being deleted and add new ones
                if (FUpNode != null)
                    FUpNode.MarkedForDelete = false;
                foreach(INode child in children)
                {
                    bool found = false;
                    foreach(var c in FChildren)
                        if (c.Node == child)
                    {
                        c.MarkedForDelete = false;
                        found = true;
                        break;                        
                    }

                    if (!found)
                        FChildren.Add(new PatchNode(child));
                }
                
                //remove all children still marked for delete
                for (int i=FChildren.Count-1; i>=0; i--) 
                {
                    if (FChildren[i].MarkedForDelete)
                        FChildren.RemoveAt(i);
                }
                
                FChildren.Sort(delegate(PatchNode p1, PatchNode p2) {return p1.Text.CompareTo(p2.Text);});
                
                //insert an .. (up) node as first element
                if (FUpNode == null)
                {
                    FUpNode = new PatchNode(null);
                    FChildren.Insert(0, FUpNode);
                }                
            }
        }
        
        public void UnSubscribe()
        {
            Node.RemoveListener(this);
        }
        
        public PatchNode[] GetChildren()
        {
            return FChildren.ToArray();
        }
        
        public void SelectNodes(INode[] nodes)
        {
            //deselect all
            foreach(PatchNode pn in FChildren)
                pn.Selected = false;
            
            if (nodes != null)
                //reselect selected
                foreach(INode node in nodes)
            {
                PatchNode pn = FChildren.Find(delegate (PatchNode p) {return p.Node == node;});
                if (pn != null)
                    pn.Selected = true;
            }
            
            OnPropertyChanged("Selection");
        }
        
        public void Add(PatchNode patchNode)
        {
            FChildren.Add(patchNode);
        }
        
        public void NodeChangedCB()
        {
            UpdateChildren();
            OnPropertyChanged("Children");
        }
    }
}
