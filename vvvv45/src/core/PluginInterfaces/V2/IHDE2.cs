using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using Microsoft.Practices.Unity;
using VVVV.Core;
using VVVV.Core.Model;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2.Graph;

namespace VVVV.PluginInterfaces.V2
{
    #region IHDEHost
    public class NodeEventArgs : EventArgs
	{
		public INode Node
		{
			get;
			private set;
		}
		
		public NodeEventArgs(INode node)
		{
			Node = node;
		}
	}
    
    public class NodeSelectionEventArgs : EventArgs
	{
    	public INode[] Nodes
		{
			get;
			private set;
		}
		
    	public NodeSelectionEventArgs(INode[] nodes)
		{
			Nodes = nodes;
		}
	}
    
    public class WindowEventArgs : EventArgs
    {
    	public IWindow Window
    	{
    		get;
    		private set;
    	}
    	
    	public WindowEventArgs(IWindow window)
    	{
    		Window = window;
    	}
    }
    
    public class MouseEventArgs : EventArgs
    {
    	public INode Node
    	{
    		get;
    		private set;
    	}
    	
    	public Mouse_Buttons Button
    	{
    		get;
    		private set;
    	}
    	
    	public Modifier_Keys ModifierKey
    	{
    		get;
    		private set;
    	}
    	
    	public MouseEventArgs(INode node, Mouse_Buttons button, Modifier_Keys key)
    	{
    		Node = node;
    		Button = button;
    		ModifierKey = key;
    	}
    }
	
	public delegate void NodeEventHandler(object sender, NodeEventArgs args);
	public delegate void MouseEventHandler(object sender, MouseEventArgs args);
	public delegate void NodeSelectionEventHandler(object sender, NodeSelectionEventArgs args);
	public delegate void WindowEventHandler(object sender, WindowEventArgs args);
    
    /// <summary>
	/// The interface to be implemented by a program to host IHDEPlugins.
	/// </summary>
	[Guid("2B24AC85-E543-40B3-9090-2828D26978A0"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IHDEHost
	{
	    /// <summary>
	    /// Returns an interface to the graphs root node
	    /// </summary>
	    /// <returns>The graphs root node.</returns>
	    INode Root
	    {
	        get;
	    }
	    
	    INode2 RootNode
	    {
	        get;
	    }
	    
	    event NodeSelectionEventHandler NodeSelectionChanged;
	    event MouseEventHandler MouseUp;
	    event MouseEventHandler MouseDown;
	    event WindowEventHandler WindowSelectionChanged;
	    event WindowEventHandler WindowAdded;
	    event WindowEventHandler WindowRemoved;
	    
	    /// <summary>
	    /// The currently selected patch window.
	    /// </summary>
	    IWindow ActivePatchWindow
	    {
	    	get;
	    }
	    
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
		/// Opens the given file.
		/// </summary>
		/// <param name="file">The file to open by vvvv.</param>
		/// <param name="inActivePatch">Whether it should be openend in the active patch or in the root patch.</param>
		/// <param name="window">If the created node has a GUI it will tabbed with this window.</param>
		void Open(string file, bool inActivePatch, IWindow window);
		
		/// <summary>
		/// Sets the component mode of the given nodes associated GUI.
		/// </summary>
		/// <param name="node">The node whose GUIs ComponentMode is to be changed.</param>
		/// <param name="componentMode">The new ComponentMode.</param>
		void SetComponentMode(INode node, ComponentMode componentMode);
		
		/// <summary>
		/// Selects the given nodes in their patch.
		/// </summary>
		/// <param name="nodes">The nodes to be selected.</param>
		void SelectNodes(INode[] nodes);
		
		/// <summary>
		/// Opens the editor of the given node.
		/// </summary>
		/// <param name="node">The node whose editor to open.</param>
		void ShowEditor(INode node);
		
		/// <summary>
		/// Opens the GUI of the given node.
		/// </summary>
		/// <param name="node">The node whose GUI to open.</param>
		void ShowGUI(INode node);
		
		/// <summary>
		/// Opens the help-patch of the given nodeinfo.
		/// </summary>
		/// <param name="nodeInfo">The nodeinfo to open the help-patch for.</param>
		void ShowHelpPatch(INodeInfo nodeInfo);
		
		/// <summary>
		/// Opens the online-reference page on vvvv.org for the given nodeinfo.
		/// </summary>
		/// <param name="nodeInfo">The nodeinfo to show the online-reference for.</param>
		void ShowNodeReference(INodeInfo nodeInfo);
		
		/// <summary>
		/// The addon factories used to collect node infos and create nodes.
		/// </summary>
		List<IAddonFactory> AddonFactories
		{
			get;
		}
		
		/// <summary>
		/// Get the full path to the vvvv.exe
		/// </summary>
		string ExePath
		{
			get;
		}
		
		/// <summary>
		/// Raised if a node was created.
		/// </summary>
		event NodeEventHandler NodeAdded;
		
		/// <summary>
		/// Raised if a node was destroyed.
		/// </summary>
	    event NodeEventHandler NodeRemoved;
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
		void CreateComment(string comment);
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
	    void Initialize();
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
		void HideMe();		
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
		/// <summary>
		/// Check if the node can offer a GUI window
		/// </summary>
		/// <returns>Returns true if this node can offer a GUI window.</returns>
		bool HasGUI();
		bool HasPatch();
		bool HasCode();
		
		bool IsBoygrouped();
        bool ContainsBoygroupedNodes();
        bool IsMissing();
        bool ContainsMissingNodes();
		
		//todo: check GetChildren mem leak?!
		int GetChildCount();
		INode GetChild(int index);
		INode[] GetChildren();
		
		//todo: check GetPins mem leak?!
		IPin[] GetPins();
		IPin GetPin(string Name);
		
		/// <summary>
		/// Allows a plugin to register an INodeListener on a specific vvvv node.
		/// </summary>
		/// <param name="listener">The listener to register.</param>
		void AddListener(INodeListener listener);
		
		/// <summary>
		/// Allows a plugin to unregister an INodeListener from a specific vvvv node.
		/// </summary>
		/// <param name="listener">The listener to unregister.</param>
		void RemoveListener(INodeListener listener);
		
		/// <summary>
		/// Gets the last runtime error that occured or null if there were no errors.
		/// </summary>
		string LastRuntimeError
		{
			get;
		}
		
		/// <summary>
		/// Gets the <see cref="IWindow">window</see> of this node. Or null if
		/// this node doesn't have a window.
		/// </summary>
		IWindow Window
		{
			get;
		}
	}	
	
	[Guid("1ABB290D-9A96-4944-80CC-F544C8CDD14B"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface INodeListener
    {
        void AddedCB(INode childNode);
        void RemovedCB(INode childNode);
        void LabelChangedCB();
    }
	#endregion INode
	
	#region IPin
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
	    
	    /// <summary>
		/// Allows a plugin to register an IPinListener on a specific pin.
		/// </summary>
		/// <param name="listener">The listener to register.</param>
		void AddListener(IPinListener listener);
		
		/// <summary>
		/// Allows a plugin to unregister an IPinListener from a specific pin.
		/// </summary>
		/// <param name="listener">The listener to unregister.</param>
		void RemoveListener(IPinListener listener);
	    
	    //int GetSliceCount();
	    //enum GetDirection();
	    //Enum GetType();*/
	}

	[Guid("F8D09D3D-D988-434D-9AD4-8AD4C94001E7"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IPinListener
    {
        void ChangedCB();
    }
	#endregion IPin
	
	#region IWindow
	/// <summary>
	/// Gives access to vvvv windows
	/// </summary>
	[Guid("1DF0E66D-EDE7-49C4-B0DF-DE789D741480"), 
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IWindow
	{
		/// <summary>
		/// Get/set the windows caption.
		/// </summary>
		string Caption
		{
			get;
			set;
		}
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
		
		int Left
		{
			get;
		}
		int Top
		{
			get;
		}
		int Width
		{
			get;
		}
		int Height
		{
			get;
		}
	}	
	#endregion IWindow
	
    #region IEditor
    /// <summary>
    /// Interface for all document editors. Use in combination with the
    /// <see cref="EditorInfoAttribute">EditorInfoAttribute</see> 
    /// to define with which file extensions this editor works with.
    /// </summary>
    [Guid("ECC649C2-01B7-454E-9E22-E848D4AABAEC"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IEditor : IPluginBase
    {
    	/// <summary>
    	/// Informs the editor to open a file located at filename.
    	/// </summary>
    	/// <param name="filename">The path to the file to open.</param>
    	void Open(string filename);
    	
    	/// <summary>
    	/// Informs the editor to move to the line number lineNumber.
    	/// <param name="lineNumber">The line number to move to.</param>
    	/// <param name="column">The column number to move to.</param>
    	/// </summary>
    	void MoveTo(int lineNumber, int column);
    	
    	/// <summary>
    	/// Informs the editor to close the currently opened file.
    	/// </summary>
    	void Close();
    	
    	/// <summary>
    	/// Tells the editor to save the currently opened file.
    	/// </summary>
    	void Save();
    	
    	/// <summary>
    	/// Tells the editor to save the currently opened file under 
    	/// the new filename.
    	/// </summary>
    	/// <param name="filename">The new path to save the currently opened file to.</param>
    	void SaveAs(string filename);
    	
    	/// <summary>
    	/// The node this editor is attached to. Shows runtime errors of this node.
    	/// </summary>
    	INode AttachedNode
    	{
    		get;
    		set;
    	}
    	
    	/// <summary>
    	/// The absolute path to file currently opened.
    	/// </summary>
    	string OpenedFile
    	{
    		get;
    	}
    }
    #endregion
}
