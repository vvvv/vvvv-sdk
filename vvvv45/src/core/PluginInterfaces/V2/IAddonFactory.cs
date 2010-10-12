using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;

using VVVV.Core.Model;
using VVVV.PluginInterfaces.V1;

namespace VVVV.PluginInterfaces.V2
{
	public delegate void NodeInfoEventHandler(IAddonFactory factory, INodeInfo nodeInfo);
	
	/// <summary>
	/// General addon factory
	/// </summary>
	public interface IAddonFactory
	{
	    event NodeInfoEventHandler NodeInfoAdded;
	    event NodeInfoEventHandler NodeInfoRemoved;
	    IEnumerable<INodeInfo> ExtractNodeInfos(string filename);
	    void StartWatching();
	    bool Create(INodeInfo nodeInfo, IAddonHost host);
	    bool Delete(INodeInfo nodeInfo, IAddonHost host);
	    bool Clone(INodeInfo nodeInfo, string path, string name, string category, string version);
	}
}
