using System;
using System.Runtime.InteropServices;

namespace VVVV.PluginInterfaces.V1
{
	/// <summary>
	/// Allows the NodeBrower to communicate back to the host
	/// </summary>
	[Guid("A0C810DA-E0CC-4A2E-BC3F-8139766945F1"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface INodeBrowser
	{
		/// <summary>
		/// Called by the NodeBrowserHost to hand itself over to the NodeBrowser.
		/// </summary>
		/// <param name="Host">Interface to the NodeBrowserHost.</param>
		void SetNodeBrowserHost(INodeBrowserHost host);
		void Initialize(string path, string text, out int width);
		void AfterShow();
		void BeforeHide();
	}
	
	/// <summary>
	/// Allows the NodeBrower to communicate back to the host
	/// </summary>
	[Guid("5567811E-D2D3-4654-A3E3-2E8324C9F022"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface INodeBrowserHost
	{
		void CreateNode(INodeInfo nodeInfo);
		void CreateNodeFromFile(string filePath);
		void CreateComment(string comment);
		void CreateDynamicNode(string nodeName);
		void ShowHelpPatch(INodeInfo nodeInfo);
		void ShowNodeReference(INodeInfo nodeInfo);
	}	
}
