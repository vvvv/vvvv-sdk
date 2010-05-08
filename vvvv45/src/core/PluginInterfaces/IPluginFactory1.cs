using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using VVVV.HDE.Model;

namespace VVVV.PluginInterfaces.V1
{
	public delegate void NodeInfoEventHandler(IPluginFactory factory, INodeInfo nodeInfo);
	
	[Guid("445266A3-02A6-41F3-9055-50083FCE1CBC"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IPluginFactory 
	{		
	    bool Create(INodeInfo nodeInfo, out IPluginBase plugin);
	
	    bool Register(IExecutable executable);
	
	    bool UnRegister(IExecutable executable);
	    
	    bool ExtractNodeInfos(string filename, out INodeInfo[] nodeInfos);
	    
	    event NodeInfoEventHandler OnNodeInfoAdded;
	
	    event NodeInfoEventHandler OnNodeInfoRemoved;
	}
}
