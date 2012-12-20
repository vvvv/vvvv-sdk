using System;
using System.Runtime.InteropServices;

namespace VVVV.PluginInterfaces.V2
{
    [ComVisible(false)]
	public delegate void NodeInfoEventHandler(object sender, INodeInfo nodeInfo);
	
	/// <summary>
	/// Factory to create a <see cref="INodeInfo">nodeInfo</see>.
	/// </summary>
	[ComVisible(false)]
	public interface INodeInfoFactory
	{
		/// <summary>
		/// Creates a new <see cref="INodeInfo">node info</see>.
		/// </summary>
		/// <param name="name">The name of the node.</param>
		/// <param name="category">The category of the node.</param>
		/// <param name="version">The version of the node.</param>
		/// <param name="filename">The absolute path to source file which contains this <see cref="INodeInfo">node info</see>.</param>
		/// <param name="beginUpdate">Whether the NodeInfoAdded event should be supressed till CommitUpdate on node info is called.</param>
		/// <returns>The newly created <see cref="INodeInfo">node info</see>.</returns>
		INodeInfo CreateNodeInfo(string name, string category, string version, string filename, bool beginUpdate);
		
		/// <summary>
		/// Updates the key of an existing <see cref="INodeInfo">node info</see>.
		/// </summary>
		/// <param name="nodeInfo">The node info to update.</param>
		/// <param name="name">The new name.</param>
		/// <param name="category">The new category.</param>
		/// <param name="version">The new version.</param>
		/// <param name="filename">The new absolute path to source file which contains this <see cref="INodeInfo">node info</see>.</param>
		void UpdateNodeInfo(INodeInfo nodeInfo, string name, string category, string version, string filename);
		
		/// <summary>
		/// Destroy given node info.
		/// </summary>
		/// <param name="nodeInfo">The node info to destroy.</param>
		void DestroyNodeInfo(INodeInfo nodeInfo);
		
		/// <summary>
		/// Determines whether a node info with given key already exists.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="category">The category.</param>
		/// <param name="version">The version.</param>
		/// <param name="filename">The filename.</param>
		/// <returns>True if node info with given key already exists, otherwise false.</returns>
		bool ContainsKey(string name, string category, string version, string filename);
		
		/// <summary>
		/// Array of all registered <see cref="INodeInfo">node infos</see>.
		/// </summary>
		INodeInfo[] NodeInfos
		{
			get;
		}
		
		/// <summary>
		/// Gets the current timestamp. The timestamp increases everytime a nodeinfo gets added, removed or updated.
		/// </summary>
		uint Timestamp
		{
		    get;
		}
		
		event NodeInfoEventHandler NodeInfoAdded;
		event NodeInfoEventHandler NodeInfoUpdated;
		event NodeInfoEventHandler NodeInfoRemoved;
	}
}
