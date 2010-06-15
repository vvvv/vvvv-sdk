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
        
        public INode Node{get;set;}
        public bool Selected{get;set;}
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
                FChildren.Clear();
                foreach(INode child in children)
                {
                    //for (int i = 0; i < FNode.GetChildCount(); i++)
                    //    FChildren.Add(new PatchNode(FNode.GetChild(i)));
                    FChildren.Add(new PatchNode(child));
                }
                
                FChildren.Sort(delegate(PatchNode p1, PatchNode p2) {return p1.Text.CompareTo(p2.Text);});
                
                //insert an .. (up) node as first element
                FChildren.Insert(0, new PatchNode(null));
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
