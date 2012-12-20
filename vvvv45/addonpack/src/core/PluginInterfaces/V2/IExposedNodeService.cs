using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using VVVV.PluginInterfaces.V2.Graph;

namespace VVVV.PluginInterfaces.V2
{
    [ComVisible(false)]
	public delegate void NodeEventHandler(INode2 node);
	
	/// <summary>
	/// Provides access to nodes that have been exposed for being remotely controlled
	/// </summary>
	[ComVisible(false)]
	public interface IExposedNodeService
	{
		IEnumerable<INode2> Nodes
		{
			get;
		}
		
		event NodeEventHandler NodeAdded;
		event NodeEventHandler NodeRemoved;
	}
}