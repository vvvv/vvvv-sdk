using System;
using System.Runtime.InteropServices;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Hosting
{
	/// <summary>
    /// Listener interface to be informed of added/removed NodeInfos.
    /// Should only be used internally. Use INodeInfoFactory and its
    /// events.
    /// </summary>
    [Guid("8FF7C831-8E22-4D72-BCE7-E726C326BF24"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface INodeInfoListener
    {
        void NodeInfoAddedCB(INodeInfo nodeInfo);
        void NodeInfoUpdatedCB(INodeInfo nodeInfo);
        void NodeInfoRemovedCB(INodeInfo nodeInfo);
    }
	
	/// <summary>
	/// Factory to create a <see cref="INodeInfo">nodeInfo</see>.
	/// This interface is implemented in vvvv.
	/// </summary>
	[Guid("26606872-475F-4DDF-AB51-9BCD5BE2532B"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IInternalNodeInfoFactory
	{
		void AddListener(INodeInfoListener listener);
	    void RemoveListener(INodeInfoListener listener);
		INodeInfo CreateNodeInfo(string name, string category, string version, string filename, bool beginUpdate);
		void UpdateNodeInfo(INodeInfo nodeInfo, string name, string category, string version, string filename);
		void DestroyNodeInfo(INodeInfo nodeInfo);
		bool ContainsKey(string name, string category, string version, string filename);
		INodeInfo[] NodeInfos
		{
			get;
		}
		uint Timestamp
		{
		    get;
		}
	}
}
