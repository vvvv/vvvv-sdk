using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.Practices.Unity;

using VVVV.Core;
using VVVV.Core.Model;
using VVVV.PluginInterfaces.V1;

namespace VVVV.PluginInterfaces.V2
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
		/// Allows a plugin to register IListeners on the host
		/// </summary>
		/// <param name="listener">The listener to register. Most likely the plugin itself, implementing an IListener.</param>
		void AddListener(IListener listener);
	    
		/// <summary>
		/// Allows a plugin to unregister ILiseners from the host
		/// </summary>
		/// <param name="listener">The listener to unregister. Most likely the plugin itself, implementing an IListener.</param>
	    void RemoveListener(IListener listener);
	    
	    /// <summary>
		/// Allows a plugin to create/update an Enum with vvvv
		/// </summary>
		/// <param name="EnumName">The Enums name.</param>
		/// <param name="Default">The Enums default value.</param>
		/// <param name="EnumEntries">An array of strings that specify the enums entries.</param>
		void UpdateEnum(string EnumName, string Default, string[] EnumEntries);
		
		/// <summary>
		/// Returns the number of entries for a given Enum.
		/// </summary>
		/// <param name="EnumName">The name of the Enum to get the EntryCount of.</param>
		/// <returns>Number of entries in the Enum.</returns>
		int GetEnumEntryCount(string EnumName);
		
		/// <summary>
		/// Returns the name of a given EnumEntry of a given Enum.
		/// </summary>
		/// <param name="EnumName">The name of the Enum to get the EntryName of.</param>
		/// <param name="Index">Index of the EnumEntry.</param>
		/// <returns>String representation of the EnumEntry.</returns>
		string GetEnumEntry(string EnumName, int Index);
		
		/// <summary>
		/// Returns the current time which the plugin should use if it does timebased calculations.
		/// </summary>
		/// <returns>The hosts current time.</returns>
		double GetCurrentTime();
	    
		/// <summary>
		/// Returns all registered node infos.
		/// </summary>
		IEnumerable<INodeInfo> NodeInfos
		{
			get;
		}
		
		/// <summary>
		/// Returns the INodeInfo for given systemname or null if not found.
		/// </summary>
		/// <param name="systemname">The unique systemname of the INodeInfo to look for.</param>
		/// <returns>The INodeInfo with given systemname or null if not found.</returns>
		INodeInfo GetNodeInfo(string systemname);
	}
	#endregion IHDEHost
	
	#region NodeBrowser
	/// <summary>
	/// Allows the NodeBrower to be contacted by the host
	/// </summary>
	[Guid("A0C810DA-E0CC-4A2E-BC3F-8139766945F1"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface INodeBrowser: IPluginHDE
	{
		void Initialize(string text);
		void DragDrop(bool allow);
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
		void CloneNode(INodeInfo nodeInfo, string path, string Name, string Category, string Version);
		void CreateNodeFromFile(string filePath);
		void CreateComment(string comment);
		void ShowHelpPatch(INodeInfo nodeInfo);
		void ShowNodeReference(INodeInfo nodeInfo);
	}	
	#endregion NodeBrowser
	
	#region WindowSwitcher
	/// <summary>
	/// Allows the WindowSwitcher to be contacted by the host
	/// </summary>
	[Guid("41CC97F3-106E-4DC9-AA74-E50C0B5694DD"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IWindowSwitcher: IPluginHDE
	{
		void Initialize(IWindow currentWindow, out int width, out int height);
		void AfterShow();
		void Up();
		void Down();
	}
	
	/// <summary>
	/// Allows the WindowSwitcher to communicate back to the host
	/// </summary>
	[Guid("A14BBFDE-9B91-430B-B098-FD8E2DC7D60B"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IWindowSwitcherHost
	{
		void HideMe(IWindow window);
	}	
	#endregion WindowSwitcher
	
	#region Kommunikator
	/// <summary>
	/// Allows the Kommunikator to be contacted by the host
	/// </summary>
	[Guid("CF40CDDD-55BE-42D5-B6BB-1A05AE8FF9A8"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IKommunikator: IPluginHDE
	{
		void Initialize(string path, string description);
		void SaveCurrentImage(string filename);
	}
	
	/// <summary>
	/// Allows the Kommunikator to communicate back to its host
	/// </summary>
	[Guid("8FCFCF38-14B4-4BB3-9A2A-7D0D71BB98BD"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IKommunikatorHost
	{
		void HideMe();
	}	
	#endregion Kommunikator
	
	#region GraphViewer
	/// <summary>
	/// Allows the GraphViewer to be contacted by the host
	/// </summary>
	[Guid("B226EE83-3C06-45E2-9B03-F4105DFDB27D"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IGraphViewer: IPluginHDE
	{
		void Initialize(INode root);
	}
	
	/// <summary>
	/// Allows the GraphViewer to communicate back to the host
	/// </summary>
	[Guid("CD119190-E089-4AC6-9EC0-FC03DA11B895"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IGraphViewerHost
	{
	    void SelectNodes(INode[] node);
		void ShowPatchOfNode(INode node);
		void ShowHelpPatch(INodeInfo nodeInfo);
		void ShowNodeReference(INodeInfo nodeInfo);
	}	
	#endregion GraphViewer
	
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
		WindowType GetWindowType();
		/// <summary>
		/// Get the windows associated INode
		/// </summary>
		/// <returns>Returns this windows INode</returns>
		INode GetNode();
		/// <summary>
		/// Get the windows visible state
		/// </summary>
		/// <returns>Returns true if this window is visible, false if not.</returns>
		bool IsVisible();
	}	
	#endregion IWindow
	
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
        void NodeInfoUpdatedCB(INodeInfo nodeInfo);
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
    /// Listener interface to be informed of a mouseclicks in a patch.
    /// </summary>
    [Guid("2E1F9CF2-9D98-43DC-B3D9-F67FCA4ACED4"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMouseClickListener: IListener
    {
        void MouseDownCB(INode node, Mouse_Buttons button, Modifier_Keys keys);
        void MouseUpCB(INode node, Mouse_Buttons button, Modifier_Keys keys);
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
