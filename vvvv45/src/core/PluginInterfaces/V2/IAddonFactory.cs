using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;

using VVVV.Core.Model;
using VVVV.PluginInterfaces.V1;

namespace VVVV.PluginInterfaces.V2
{
	/// <summary>
	/// General addon factory
	/// </summary>
	[Guid("F9E49FFD-E1E3-4F00-8AB6-1B85858BE314"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IAddonFactory
	{
	    INodeInfo[] ExtractNodeInfos(string filename, string arguments);
	    bool Create(INodeInfo nodeInfo, INode host);
	    bool Delete(INodeInfo nodeInfo, INode host);
	    bool Clone(INodeInfo nodeInfo, string path, string name, string category, string version, out INodeInfo newNodeInfo);
	    string JobStdSubPath { get; }
	    // Used in nodelist.xml
	    string Name { get; }
	    bool AllowCaching { get; }
	    void AddDir(string dir, bool recursive);
	    void RemoveDir(string dir);
	}
}
