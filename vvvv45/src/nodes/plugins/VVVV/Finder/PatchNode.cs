using System;
using System.Collections.Generic;

using VVVV.Core;
using VVVV.Core.View;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Nodes.Finder
{
    public class PatchNode: IViewableCollection, INamed, IDescripted, INodeChangedListener
    {
        List<PatchNode> FChildNodes = new List<PatchNode>();
        
        private FinderPluginNode FFinder;
        public INode Node{get;set;}
        public bool Selected{get;set;}
        public bool MarkedForDelete{get;set;}
        
        public PatchNode(INode self, FinderPluginNode finder)
        {
            Node = self;
            FFinder = finder;
            
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
              //  if (FUpNode != null)
                //    FUpNode.MarkedForDelete = false;
                
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

                    if (!found && (child.GetChildCount() > 0) && (child.GetNodeInfo().Type != NodeType.Module))
                        Add(new PatchNode(child, FFinder));
                }
                
                //remove all children still marked for delete
                for (int i=FChildNodes.Count-1; i>=0; i--)
                {
                    if (FChildNodes[i].MarkedForDelete)
                        Remove(FChildNodes[i]);
                }
                
                FChildNodes.Sort(delegate(PatchNode p1, PatchNode p2)
                                 {
                                     //order:
                                     //subpatches
                                     //inlets
                                     //outlets
                                     //Sends
                                     //Receives
                                     //comments
                                     //other nodes
                                     
                                /*     int w1 = 0, w2 = 0;
                                     if (p1.Count > 0)
                                         w1 = 100;
                                     else if (p1.Name.StartsWith("I: "))
                                         w1 = 91;
                                     else if (p1.Name.StartsWith("O: "))
                                         w1 = 90;
                                     else if (p1.Name.StartsWith("S "))
                                         w1 = 81;
                                     else if (p1.Name.StartsWith("R "))
                                         w1 = 80;
                                     else if (p1.Name.StartsWith("// "))
                                         w1 = 70;
                                     
                                     if (p2.Count > 0)
                                         w2 = 100;
                                     else if (p2.Name.StartsWith("I: "))
                                         w2 = 91;
                                     else if (p2.Name.StartsWith("O: "))
                                         w2 = 90;
                                     else if (p2.Name.StartsWith("S "))
                                         w2 = 81;
                                     else if (p2.Name.StartsWith("R "))
                                         w2 = 80;
                                     else if (p2.Name.StartsWith("// "))
                                         w2 = 70;
                                     
                                     if ((w1 > 0) || (w2 > 0))
                                     {
                                         if (w1 > w2)
                                             return -1;
                                         else if (w1 < w2)
                                             return 1;
                                         else
                                             return 0;
                                     }
                                     else */
                                        return p1.Name.CompareTo(p2.Name);
                                 });
                
                //insert an .. (up) node as first element
          /*      if (FUpNode == null)
                {
                    FUpNode = new PatchNode(null);
                    FChildNodes.Insert(0, FUpNode);
                }*/
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
                if ((Node != null) && (Node.GetNodeInfo() != null))
                {
                    string descriptiveName = Node.GetPin("Descriptive Name").GetValue(0);
                    string hyphen = "";
                    if (!string.IsNullOrEmpty(descriptiveName))
                       hyphen = " -- ";
                    
                    var ni = Node.GetNodeInfo();
                    
                    //subpatches
                    if (string.IsNullOrEmpty(ni.Name))
                    {
                        string file = System.IO.Path.GetFileNameWithoutExtension(ni.Filename);
                        
                        //unsaved patch
                        if (string.IsNullOrEmpty(file))
                            return ni.Filename + hyphen + descriptiveName;
                        //patch with valid filename
                        else
                            return file + hyphen + descriptiveName;
                    }
                    else if (ni.Username == "IOBox (Value Advanced)")
                    {
                        //inlets
                        if ((!Node.GetPin("Y Input Value").IsConnected()) && (Node.GetPin("Y Output Value").IsConnected()) && (!string.IsNullOrEmpty(descriptiveName)))
                            return "I: " + descriptiveName;
                        //outlets
                        else if ((Node.GetPin("Y Input Value").IsConnected()) && (!Node.GetPin("Y Output Value").IsConnected()) && (!string.IsNullOrEmpty(descriptiveName)))
                            return "O: " + descriptiveName;
                        else
                            return ni.Username + hyphen + descriptiveName;
                    }
                    else if (ni.Username == "IOBox (String)")
                    {
                        //inlets
                        if ((!Node.GetPin("Input String").IsConnected()) && (Node.GetPin("Output String").IsConnected()) && (!string.IsNullOrEmpty(descriptiveName)))
                            return "I: " + descriptiveName;
                        //outlets
                        else if ((Node.GetPin("Input String").IsConnected()) && (!Node.GetPin("Output String").IsConnected()) && (!string.IsNullOrEmpty(descriptiveName)))
                            return "O: " + descriptiveName;
                        //comments
                        else if ((!Node.GetPin("Input String").IsConnected()) && (!Node.GetPin("Output String").IsConnected()))
                            return "// " + Node.GetPin("Input String").GetValue(0);
                        else
                            return ni.Username + hyphen + descriptiveName;
                    }
                    else if (ni.Username == "IOBox (Color)")
                    {
                        //inlets
                        if ((!Node.GetPin("Input Color").IsConnected()) && (Node.GetPin("Output Color").IsConnected()) && (!string.IsNullOrEmpty(descriptiveName)))
                            return "I: " + descriptiveName;
                        //outlets
                        else if ((Node.GetPin("Input Color").IsConnected()) && (!Node.GetPin("Output Color").IsConnected()) && (!string.IsNullOrEmpty(descriptiveName)))
                            return "O: " + descriptiveName;
                        else
                            return ni.Username + hyphen + descriptiveName;
                    }
                    else if (ni.Username == "IOBox (Enumerations)")
                    {
                        //inlets
                        if ((!Node.GetPin("Input Enum").IsConnected()) && (Node.GetPin("Output Enum").IsConnected()) && (!string.IsNullOrEmpty(descriptiveName)))
                            return "I: " + descriptiveName;
                        //outlets
                        else if ((Node.GetPin("Input Enum").IsConnected()) && (!Node.GetPin("Output Enum").IsConnected()) && (!string.IsNullOrEmpty(descriptiveName)))
                            return "O: " + descriptiveName;
                        else
                            return ni.Username + hyphen + descriptiveName;
                    }
                    else if (ni.Username == "IOBox (Node)")
                    {
                        //inlets
                        if ((!Node.GetPin("Input Node").IsConnected()) && (Node.GetPin("Output Node").IsConnected()) && (!string.IsNullOrEmpty(descriptiveName)))
                            return "I: " + descriptiveName;
                        //outlets
                        else if ((Node.GetPin("Input Node").IsConnected()) && (!Node.GetPin("Output Node").IsConnected()) && (!string.IsNullOrEmpty(descriptiveName)))
                            return "O: " + descriptiveName;
                        else
                            return ni.Username + hyphen + descriptiveName;
                    }
                    else if (ni.Name == "S")
                        return ni.Username + ": " + Node.GetPin("SendString").GetValue(0);
                    else if (ni.Name == "R")
                        return ni.Username + ": " + Node.GetPin("ReceiveString").GetValue(0);
                    else
                        return ni.Username + hyphen + descriptiveName;
                }
                else
                    return "..";
            }
        }
        
        public string Description
        {
            get
            {
                if (Node == null)
                    return "";
                else
                {
                    var ni = Node.GetNodeInfo();
                    if (string.IsNullOrEmpty(ni.Name))
                        return "[id " + Node.GetID().ToString() + "] " + System.IO.Path.GetDirectoryName(ni.Filename);
                    else if (ni.Type == NodeType.Native)
                        return "[id " + Node.GetID().ToString() + "]";
                    else
                        return ni.Filename + " [id " + Node.GetID().ToString() + "]";
                }
            }
        }
        
        public void NodeChangedCB()
        {
            UpdateChildren();
            FFinder.UpdateView();
            
        }
        
        public event RenamedHandler Renamed;
        
		protected virtual void OnRenamed(string newName)
		{
			if (Renamed != null) {
				Renamed(this, newName);
			}
		}
        
        public bool Contains(object item)
        {
            throw new NotImplementedException();
        }
    }
}
