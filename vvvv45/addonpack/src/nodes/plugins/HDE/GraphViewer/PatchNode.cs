using System;
using System.Collections.Generic;
using System.Reflection;
using System.CodeDom.Compiler;

using VVVV.PluginInterfaces.V1;

namespace VVVV.Graph
{
	public class PatchNode: INode
	{
	    List<PatchNode> FChildren = new List<PatchNode>();
	    INode FNode;
	    
	    public PatchNode(INode self, INode[] children)
		{
	        FNode = self;
	        List<INode> cs = new List<INode>();
	        
		    if (children != null)
    		    foreach(INode child in children)
		    {
		        cs.Clear();
		        for (int i = 0; i < child.GetChildCount(); i++)
                    cs.Add(child.GetChild(i));
		        FChildren.Add(new PatchNode(child, cs.ToArray()));
		        //FChildren.Add(new PatchNode(child.GetChildren()));
		    }
    		        
		}
		
		public int GetID()
		{
		    return FNode.GetID();
		}
		
		public INodeInfo GetNodeInfo()
		{
		    if (FNode != null)
    		    return FNode.GetNodeInfo();
		    else 
		        return null;
		}
		
		public INode[] GetChildren()
		{
		    return FChildren.ToArray();
		}
		
		public INode GetChild(int index)
		{
		    return FChildren[index];
		}
		
		public int GetChildCount()
		{
		    return FChildren.Count;
		}
		
		public void Add(PatchNode patchNode)
		{
		    FChildren.Add(patchNode);
		}
	}
}
