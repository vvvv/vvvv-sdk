using System;
using System.Collections.Generic;
using System.Linq;

using VVVV.Core;
using VVVV.Core.Collections;
using VVVV.PluginInterfaces.V2;

namespace VVVV.HDE.ProjectExplorer.NodeModel
{
	public class Node : ViewableList<Node>, INamed
	{
		#region Nested class InternalNodeListener
		class InternalNodeListener : INodeListener, IDisposable
		{
			private readonly Node FObservedNode;
			
			public InternalNodeListener(Node nodeToObserve)
			{
				FObservedNode = nodeToObserve;
				FObservedNode.FInternalNode.AddListener(this);
			}
			
			public void AddedCB(INode internalChildNode)
			{
				var childNode = new Node(internalChildNode);
				
				FObservedNode.Add(childNode);
			}
			
			public void RemovedCB(INode internalChildNode)
			{
				var query =
					from node in FObservedNode
					where node.FInternalNode == internalChildNode
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
				FObservedNode.FInternalNode.RemoveListener(this);
			}
		}
		#endregion
		
		private readonly INode FInternalNode;
		private readonly INodeInfo FNodeInfo;
		private readonly InternalNodeListener FInternalNodeListener;
		
		internal Node(INode internalNode)
		{
			FInternalNode = internalNode;
			// TODO: Use ProxyNodeInfoFactory to deal with node infos.
			FNodeInfo = internalNode.GetNodeInfo();
			FName = FNodeInfo.Username;

			var children = internalNode.GetChildren();
			if (children != null)
			{
				foreach (var internalChildNode in internalNode.GetChildren())
				{
					var childNode = new Node(internalChildNode);
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
	}
}
