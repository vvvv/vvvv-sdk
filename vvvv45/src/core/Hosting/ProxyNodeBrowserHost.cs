using System;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Hosting
{
	/// <summary>
	/// Ensures that internal INodeInfo is used.
	/// </summary>
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
			nodeInfo = FNodeInfoFactory.ToInternal(nodeInfo);
			FNodeBrowserHost.CreateNode(nodeInfo);
		}
		
		public void CloneNode(INodeInfo nodeInfo, string path, string Name, string Category, string Version)
		{
			nodeInfo = FNodeInfoFactory.ToInternal(nodeInfo);
			FNodeBrowserHost.CloneNode(nodeInfo, path, Name, Category, Version);
		}
		
		public void CreateNodeFromFile(string filePath)
		{
			FNodeBrowserHost.CreateNodeFromFile(filePath);
		}
		
		public void CreateComment(string comment)
		{
			FNodeBrowserHost.CreateComment(comment);
		}
	}
}
