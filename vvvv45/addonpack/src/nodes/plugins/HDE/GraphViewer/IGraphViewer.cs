using System;
using System.Runtime.InteropServices;

namespace VVVV.PluginInterfaces.V1
{
	/// <summary>
	/// Allows the GraphViewer to communicate back to the host
	/// </summary>
	[Guid("B226EE83-3C06-45E2-9B03-F4105DFDB27D"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IGraphViewer
	{
		/// <summary>
		/// Called by the GraphViewerHost to hand itself over to the GraphViewer.
		/// </summary>
		/// <param name="Host">Interface to the NodeBrowserHost.</param>
		void SetGraphViewerHost(IGraphViewerHost host);
		void Initialize(INode root);
	}
	
	/// <summary>
	/// Allows the GraphViewer to communicate back to the host
	/// </summary>
	[Guid("CD119190-E089-4AC6-9EC0-FC03DA11B895"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IGraphViewerHost
	{
		void SelectNode(INode node);
		void ShowPatchOfNode(INode node);
		void ShowHelpPatch(INodeInfo nodeInfo);
		void ShowNodeReference(INodeInfo nodeInfo);
	}	
}
