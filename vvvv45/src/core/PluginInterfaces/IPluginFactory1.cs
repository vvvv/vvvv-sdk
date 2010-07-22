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
	[Guid("36D732BD-7F2C-4ECF-91E1-5961B213BBF3"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IAddonFactory
	{
	    event NodeInfoEventHandler NodeInfoAdded;
	    event NodeInfoEventHandler NodeInfoRemoved;
	    bool ExtractNodeInfos(string filename, out INodeInfo[] nodeInfos);
	    void StartWatching();
	}
	
	/// <summary>
	/// A factory which handles plugin nodes
	/// </summary>
	[Guid("445266A3-02A6-41F3-9055-50083FCE1CBC"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IPluginFactory : IAddonFactory
	{		
	    bool Create(INodeInfo nodeInfo, IPluginHost host, out IPluginBase plugin);
	
	    bool Register(IExecutable executable);
	
	    bool UnRegister(IExecutable executable);
	}
}
