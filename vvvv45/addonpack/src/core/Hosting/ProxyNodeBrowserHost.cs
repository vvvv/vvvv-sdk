using System;
using System.Drawing;
using System.Runtime.InteropServices;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Hosting
{
	/// <summary>
	/// Ensures that internal INodeInfo is used.
	/// </summary>
	[ComVisible(false)]
	class ProxyNodeBrowserHost : INodeBrowserHost
	{
		private INodeBrowserHost FNodeBrowserHost;
		private ProxyNodeInfoFactory FNodeInfoFactory;
		
		public ProxyNodeBrowserHost(INodeBrowserHost nodeBrowserHost, ProxyNodeInfoFactory nodeInfoFactory)
		{
			FNodeBrowserHost = nodeBrowserHost;
			FNodeInfoFactory = nodeInfoFactory;
		}
		
		public void CreateNode(INodeInfo nodeInfo)
		{
			if (nodeInfo != null)
			{
				nodeInfo = FNodeInfoFactory.ToInternal(nodeInfo);
			}
			FNodeBrowserHost.CreateNode(nodeInfo);
		}
		
		public int CreateNode(INodeInfo nodeInfo, Point point)
		{
			if (nodeInfo != null)
			{
				nodeInfo = FNodeInfoFactory.ToInternal(nodeInfo);
			}
			return FNodeBrowserHost.CreateNode(nodeInfo, point);
		}
		
		public void CloneNode(INodeInfo nodeInfo, string path, string Name, string Category, string Version)
		{
			nodeInfo = FNodeInfoFactory.ToInternal(nodeInfo);
			FNodeBrowserHost.CloneNode(nodeInfo, path, Name, Category, Version);
		}
		
		public void CreateComment(string comment)
		{
			FNodeBrowserHost.CreateComment(comment);
		}
	}
}
