using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using Microsoft.Practices.Unity;

using VVVV.HDE.Model;

namespace VVVV.PluginInterfaces.V1
{
    #region IHDEHost
    /// <summary>
	/// The interface to be implemented by a program to host IHDEPlugins.
	/// </summary>
	[Guid("2B24AC85-E543-40B3-9090-2828D26978A0"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IHDEHost 
	{
		/// <summary>
		/// Provides access to the ISolution hosted by this IHDEHost.
		/// </summary>
		ISolution Solution { get; }
		
		/// <summary>
		/// Provides access to the IUnityContainer used by this IHDEHost.
		/// </summary>
		IUnityContainer UnityContainer { get; }
		
		/// <summary>
		/// Allows a plugin to register IListeners on the host
		/// </summary>
		/// <param name="listener">The listener to register. Most likely the plugin itself, implementing an IListener.</param>
		void AddListener(IListener listener);
	    
		/// <summary>
		/// Allows a plugin to unregister ILiseners from the host
		/// </summary>
		/// <param name="listener">The listener to unregister. Most likely the plugin itself, implementing an IListener.</param>
	    void RemoveListener(IListener listener);
	}
	#endregion IHDEHost
	
	#region INode
	/// <summary>
	/// Gives access to vvvv nodes
	/// </summary>
	[Guid("98D74C3D-8E8B-4203-A03B-92BDECAF7BDF"), 
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface INode
	{
		/// <summary>
		/// Get the node ID.
		/// </summary>
		/// <returns>Returns this nodes ID.</returns>
		int GetID();
		/// <summary>
		/// Get the nodes info.
		/// </summary>
		/// <returns>Returns this nodes INodeInfo.</returns>
		INodeInfo GetNodeInfo();
		
		//todo: check GetChildren mem leak?!
		int GetChildCount();
		INode GetChild(int index);
		INode[] GetChildren();
		
		//todo: check GetPins mem leak?!
		IPin[] GetPins();
		IPin GetPin(string Name);
		
		/// <summary>
		/// Allows a plugin to register an IListener on a specific vvvv node.
		/// </summary>
		/// <param name="listener">The listener to register.</param>
		void AddListener(IListener listener);
		
		/// <summary>
		/// Allows a plugin to unregister an IListener from a specific vvvv node.
		/// </summary>
		/// <param name="listener">The listener to unregister.</param>
		void RemoveListener(IListener listener);
	}	
	
	/// <summary>
	/// Gives access to a vvvv nodes pins
	/// </summary>
	[Guid("2ED56B52-F43C-41C4-9F34-48911048FA13"), 
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IPin
	{
	    string GetName();
	    string GetValue(int index);
	    bool IsConnected();
	    //int GetSliceCount();
	    //enum GetDirection();
	    //Enum GetType();
	}
	
	[Guid("1ABB290D-9A96-4944-80CC-F544C8CDD14B"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface INodeChangedListener: IListener
    {
        void NodeChangedCB();
    }
	#endregion INode
	
	#region IWindow
	/// <summary>
	/// Gives access to vvvv windows
	/// </summary>
	[Guid("1DF0E66D-EDE7-49C4-B0DF-DE789D741480"), 
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IWindow
	{
		/// <summary>
		/// Get the windows caption.
		/// </summary>
		/// <returns>Returns this windows caption.</returns>
		string GetCaption();
		/// <summary>
		/// Get the windows type.
		/// </summary>
		/// <returns>Returns this windows type.</returns>
		TWindowType GetWindowType();
		/// <summary>
		/// Get the windows associated INode
		/// </summary>
		/// <returns>Returns this windows INode</returns>
		INode GetNode();
	}	
	#endregion INode
	
	#region Listener
	/// <summary>
	/// Base interface for all listeners.
	/// </summary>
    [Guid("167FCD7A-CD13-4462-8BD0-CE496236AEE4"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IListener
    {}
    
    /// <summary>
    /// Listener interface to be informed of added/removed NodeInfos.
    /// </summary>
    [Guid("8FF7C831-8E22-4D72-BCE7-E726C326BF24"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface INodeInfoListener: IListener
    {
        void NodeInfoAddedCB(INodeInfo nodeInfo);
        void NodeInfoRemovedCB(INodeInfo nodeInfo);
    }
    
    /// <summary>
    /// Listener interface to be informed of a changed node-selection.
    /// </summary>
    [Guid("C9ACADDA-1D3F-410D-B23C-E8D576F4F361"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface INodeSelectionListener: IListener
    {
        void NodeSelectionChangedCB(INode[] nodes);
    }
    
    /// <summary>
    /// Listener interface to be informed of added/removed windows.
    /// </summary>
    [Guid("804F060E-5770-4D5E-82F0-A0655321EBE3"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IWindowListener: IListener
    {
        void WindowAddedCB(IWindow window);
        void WindowRemovedCB(IWindow window);
    }
    
    /// <summary>
    /// Listener interface to be informed of the active window.
    /// </summary>
    [Guid("9FB8F749-E2FF-4E6A-A0A6-A9BE74F989A1"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IWindowSelectionListener: IListener
    {
        void WindowSelectionChangeCB(IWindow window);
    }
    #endregion Listener
}
