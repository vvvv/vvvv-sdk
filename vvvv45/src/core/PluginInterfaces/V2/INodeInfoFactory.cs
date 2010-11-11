using System;

namespace VVVV.PluginInterfaces.V2
{
	public delegate void NodeInfoEventHandler(object sender, INodeInfo nodeInfo);
	
	/// <summary>
	/// Factory to create a <see cref="INodeInfo">nodeInfo</see>.
	/// </summary>
	public interface INodeInfoFactory
	{
		/// <summary>
		/// Deserializes a textual representation of a <see cref="INodeInfo">node info</see> 
		/// given by <paramref name="val"/>.
		/// </summary>
		/// <param name="val">
		/// 	Is one of:
		///     	- Name (Category Version)|relative/path/to/file.ext
		///         - relative/path/to/file.dll|ClassName
		///         - relative/path/to/file.v4p (.fx, .dll, ...)
		///         - Name (Category Version)
		/// </param>
		/// <param name="path">
		/// Absolute path of the patch which is used to resolve the possible relative
		/// paths in <paramref name="val"/>.
		/// </param>
		/// <returns>The deserialized <see cref="INodeInfo">node info</see>.</returns>
		//INodeInfo StringToNodeInfo(string val, string path);
		
		/// <summary>
		/// Creates a new <see cref="INodeInfo">node info</see>.
		/// </summary>
		/// <param name="name">The name of the node.</param>
		/// <param name="category">The category of the node.</param>
		/// <param name="version">The version of the node.</param>
		/// <param name="filename">The absolute path to source file which contains this <see cref="INodeInfo">node info</see>.</param>
		/// <returns>The newly created <see cref="INodeInfo">node info</see>.</returns>
		INodeInfo CreateNodeInfo(string name, string category, string version, string filename);
		
		/// <summary>
		/// Array of all registered <see cref="INodeInfo">node infos</see>.
		/// </summary>
		INodeInfo[] NodeInfos
		{
			get;
		}
		
		event NodeInfoEventHandler NodeInfoAdded;
		event NodeInfoEventHandler NodeInfoUpdated;
		event NodeInfoEventHandler NodeInfoRemoved;
	}
}
