using System;
using System.Collections.Generic;
using System.Linq;

using VVVV.Core;
using VVVV.Core.Collections;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V2.Graph;

namespace VVVV.Hosting.Graph
{
	internal class Node : ViewableList<INode2>, INode2
	{
		#region Nested class InternalNodeListener
		class InternalNodeListener : INodeListener, IDisposable
		{
			private readonly Node FObservedNode;
			
			public InternalNodeListener(Node nodeToObserve)
			{
				FObservedNode = nodeToObserve;
				FObservedNode.InternalCOMInterf.AddListener(this);
			}
			
			public void AddedCB(INode internalChildNode)
			{
				var childNode = new Node(FObservedNode, internalChildNode, FObservedNode.FNodeInfoFactory);
				
				FObservedNode.Add(childNode);
			}
			
			public void RemovedCB(INode internalChildNode)
			{
				var query =
					from node in FObservedNode
					where node.InternalCOMInterf == internalChildNode
					select node;
				
				var childNode = query.First();
				FObservedNode.Remove(childNode);
				childNode.Dispose();
			}
			
			public void LabelChangedCB()
			{
				// TODO: Handle this case properly. See for e.g. Finder/PatchNode or even implement it in delphi code.
				//FObservedNode.Name = FObservedNode.FInternalNode.GetPin("Descriptive Name").GetValue(0);
			}
			
			public void Dispose()
			{
				FObservedNode.FInternalCOMInterf.RemoveListener(this);
			}
		}
		#endregion
		
		private readonly INode FInternalCOMInterf;
		private readonly INodeInfo FNodeInfo;
		private readonly InternalNodeListener FInternalNodeListener;
		private readonly ProxyNodeInfoFactory FNodeInfoFactory;
		private readonly ViewableCollection<IPin2> FPins;
		private readonly IWindow2 FWindow;
		
		internal Node(INode2 parent, INode internalCOMInterf, ProxyNodeInfoFactory nodeInfoFactory)
		{
			Parent = parent;
			FInternalCOMInterf = internalCOMInterf;
			FNodeInfoFactory = nodeInfoFactory;
			
			FNodeInfo = nodeInfoFactory.ToProxy(internalCOMInterf.GetNodeInfo());
			FName = FNodeInfo.Username;
			
			var internalWindow = internalCOMInterf.Window;
			if (internalWindow != null)
			    FWindow = new Window(this, internalWindow);
			
			FPins = new ViewableCollection<IPin2>();
			foreach (var internalPin in internalCOMInterf.GetPins())
			    FPins.Add(new Pin(internalPin));

			var children = internalCOMInterf.GetChildren();
			if (children != null)
			{
				foreach (var internalChildNode in children)
				{
					var childNode = new Node(this, internalChildNode, nodeInfoFactory);
					Add(childNode);
				}
			}
			
			FInternalNodeListener = new InternalNodeListener(this);
		}
		
		public override void Dispose()
		{
			FInternalNodeListener.Dispose();
			
			foreach (var childNode in this)
				childNode.Dispose();
			
			base.Dispose();
		}
		
        public override string ToString()
        {
            return FName;
        }
		
		public INode InternalCOMInterf
		{
            get 
            {
                return FInternalCOMInterf;
            }
        }
		
		public INodeInfo NodeInfo
		{
			get
			{
				return FNodeInfo;
			}
		}
		
		#region INamed
		public event RenamedHandler Renamed;
		
		protected virtual void OnRenamed(string newName)
		{
			if (Renamed != null) {
				Renamed(this, newName);
			}
		}
		
		private string FName;
		public string Name
		{
			get
			{
				return FName;
			}
			private set
			{
				if (value != FName)
				{
					OnRenamed(value);
					FName = value;
				}
			}
		}
		#endregion
	    
        public int ID 
        {
            get 
            {
                return FInternalCOMInterf.GetID();
            }
        }
	    
        public IViewableCollection<IPin2> Pins 
        {
            get
            {
                return FPins;
            }
        }
	    
        public IWindow2 Window 
        {
            get
            {
                return FWindow;
            }
        }
	    
        public string LastRuntimeError 
        {
            get 
            {
                return FInternalCOMInterf.LastRuntimeError;
            }
            set
            {
            	FInternalCOMInterf.LastRuntimeError = value;
            }
        }
		
		public INode2 Parent {
			get;
			private set;
		}
	}
}
