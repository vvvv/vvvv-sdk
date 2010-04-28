using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using VVVV.HDE.Model;

namespace VVVV.PluginInterfaces.V1
{
    [Guid("167FCD7A-CD13-4462-8BD0-CE496236AEE4"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IListener
    {}
    
    [Guid("8FF7C831-8E22-4D72-BCE7-E726C326BF24"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface INodeInfoListener: IListener
    {
        void NodeInfoAddedCB(INodeInfo nodeInfo);
        void NodeInfoRemovedCB(INodeInfo nodeInfo);
    }
    
    [Guid("C9ACADDA-1D3F-410D-B23C-E8D576F4F361"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface INodeSelectionListener: IListener
    {
        void NodeSelectionChangedCB(INode[] nodes);
    }
    
	[Guid("2B24AC85-E543-40B3-9090-2828D26978A0"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IHDEHost 
	{
		/// <summary>
		/// Provides access to the Solution hosted by this IHDEHost.
		/// </summary>
		ISolution Solution { get; }
		
		void AddListener(IListener listener);
	    
	    void RemoveListener(IListener listener);
	}
}
