using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using VVVV.Core;
using VVVV.Core.Model;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2.EX9;
using VVVV.PluginInterfaces.V2.Graph;

namespace VVVV.PluginInterfaces.V2
{
    #region IHDEHost
    [ComVisible(false)]
    public class NodeSelectionEventArgs : EventArgs
	{
    	public INode2[] Nodes
		{
			get;
			private set;
		}
		
    	public NodeSelectionEventArgs(INode2[] nodes)
		{
			Nodes = nodes;
		}
	}
    
    [ComVisible(false)]
    public class WindowEventArgs : EventArgs
    {
    	public IWindow2 Window
    	{
    		get;
    		private set;
    	}
    	
    	public WindowEventArgs(IWindow2 window)
    	{
    		Window = window;
    	}
    }

    [ComVisible(false)]
    public class EnumEventArgs : EventArgs
    {
        string FEnumName;
        public string EnumName => FEnumName;

        public EnumEventArgs(string enumName)
        {
            FEnumName = enumName;
        }
    }

    [ComVisible(false)]
    public class ComponentModeEventArgs : EventArgs
    {
    	public IWindow2 Window
    	{
    		get;
    		private set;
    	}
    	
    	public ComponentMode ComponentMode
    	{
    		get;
    		private set;
    	}
    	
    	public ComponentModeEventArgs(IWindow2 window, ComponentMode componentMode)
    	{
    		ComponentMode = componentMode;
    		Window = window;
    	}
    }
    
    [ComVisible(false)]
    public class MouseEventArgs : EventArgs
    {
    	public INode2 Node
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
    	
    	public MouseEventArgs(INode2 node, Mouse_Buttons button, Modifier_Keys key)
    	{
    		Node = node;
    		Button = button;
    		ModifierKey = key;
    	}
    }
	
    [ComVisible(false)]
	public delegate void MouseEventHandler(object sender, MouseEventArgs args);
	
	[ComVisible(false)]
	public delegate void NodeSelectionEventHandler(object sender, NodeSelectionEventArgs args);
	
	[ComVisible(false)]
	public delegate void WindowEventHandler(object sender, WindowEventArgs args);
	
	[ComVisible(false)]
	public delegate void ComponentModeEventHandler(object sender, ComponentModeEventArgs args);

    [ComVisible(false)]
    public delegate void EnumEventHandler(object sender, EnumEventArgs args);

    /// <summary>
	/// The interface to be implemented by a program to host IHDEPlugins.
	/// </summary>
	[ComVisible(false)]
	public interface IHDEHost
	{
	    /// <summary>
	    /// Returns an interface to the graphs root node.
	    /// </summary>
	    /// <remarks>Deprecated: Use RootNode instead.</remarks>
	    /// <returns>The graphs root node.</returns>
	    INode Root
	    {
	        get;
	    }
	    
		/// <summary>
	    /// Returns an interface to the graphs root node.
	    /// </summary>
	    /// <returns>The graphs root node.</returns>
	    INode2 RootNode
	    {
	        get;
	    }
	    
	    /// <summary>
	    /// Returns an INode2 given a slash-separated string of node IDs that uniquely identifies that node. 
	    /// </summary>
	    /// <param name="nodePath">A slash-separated string of node IDs.</param>
	    /// <returns></returns>
	    INode2 GetNodeFromPath(string nodePath);
	    
	    event NodeSelectionEventHandler NodeSelectionChanged;
	    event MouseEventHandler MouseUp;
	    event MouseEventHandler MouseDown;
	    event WindowEventHandler WindowSelectionChanged;
	    event WindowEventHandler WindowAdded;
	    event WindowEventHandler WindowRemoved;
	    event ComponentModeEventHandler BeforeComponentModeChange;
	    event ComponentModeEventHandler AfterComponentModeChange;
        event EnumEventHandler EnumChanged;

        /// <summary>
        /// The currently selected patch window.
        /// </summary>
        IWindow2 ActivePatchWindow
	    {
	    	get;
	    }
	    
	    /// <summary>
		/// Allows a plugin to create/update an Enum with vvvv.
		/// </summary>
		/// <param name="enumName">The Enums name.</param>
		/// <param name="defaultEntry">The Enums default value.</param>
		/// <param name="enumEntries">An array of strings that specify the enums entries.</param>
		void UpdateEnum(string enumName, string defaultEntry, string[] enumEntries);
		
		/// <summary>
		/// Returns the number of entries for a given Enum.
		/// </summary>
		/// <param name="enumName">The name of the Enum to get the EntryCount of.</param>
		/// <returns>Number of entries in the Enum.</returns>
		int GetEnumEntryCount(string enumName);
		
		/// <summary>
		/// Returns the name of a given EnumEntry of a given Enum.
		/// </summary>
		/// <param name="enumName">The name of the Enum to get the EntryName of.</param>
		/// <param name="index">Index of the EnumEntry.</param>
		/// <returns>String representation of the EnumEntry.</returns>
		string GetEnumEntry(string enumName, int index);
		
		/// <summary>
		/// Returns the current frame time which the plugin should use if it does timebased calculations.
		/// </summary>
		/// <returns>The hosts current frame time.</returns>
		[Obsolete("Use new property FrameTime now")]
		double GetCurrentTime();
		

		/// <summary>
		/// The current host frame time which the plugin should use if it does timebased calculations.
		/// </summary>
		double FrameTime 
		{
			get;
		}
		
		/// <summary>
		/// The realtime in seconds since the IHDEHost was created. On boygroup clients this ist the time 
		/// since the server IHDEHost was created, synced over network. This time is not frame based,
		/// each call will return a new time.
		/// </summary>
		double RealTime 
		{
			get;
		}		

		/// <summary>
		/// Initialize the internal realtime clock to a specific value
		/// </summary>
		/// <param name="time">New time value</param>
		void SetRealTime(double time = 0);
	    
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
		void SetComponentMode(INode2 node, ComponentMode componentMode);

		/// <summary>
		/// Gives access to the XML-snippet describing the current selection in the active patch. 
		/// </summary>
		/// <returns>An XML-message snippet describing the currently selected nodes in the active patch.</returns>
		string GetXMLSnippetFromSelection();
		
		/// <summary>
        /// Allows sending of XML-message snippets to patches. 
        /// </summary>
        /// <param name="fileName">Filename of the patch to send the message to.</param>
        /// <param name="message">The XML-message snippet.</param>
        /// <param name="undoable">If TRUE the operation performed by this message can be undone by the user using the UNDO command.</param>
        void SendXMLSnippet(string fileName, string message, bool undoable);
		
		/// <summary>
		/// Selects the given nodes in their patch.
		/// </summary>
		/// <param name="nodes">The nodes to be selected.</param>
		void SelectNodes(INode2[] nodes);
		
		/// <summary>
		/// Opens the editor of the given node.
		/// </summary>
		/// <param name="node">The node whose editor to open.</param>
		void ShowEditor(INode2 node);
		
		/// <summary>
		/// Opens the GUI of the given node.
		/// </summary>
		/// <param name="node">The node whose GUI to open.</param>
		void ShowGUI(INode2 node);
		
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
		/// Gets the full path to the vvvv.exe.
		/// </summary>
		string ExePath
		{
			get;
		}
		
		/// <summary>
		/// Provides access to Direct3D9 devices created by vvvv.
		/// </summary>
		IDXDeviceService DeviceService
		{
		    get;
		}
		
		/// <summary>
		/// Gets the main loop, which exposes events from the main loop.
		/// </summary>
		IMainLoop MainLoop
		{
		    get;
		}
		
		/// <summary>
		/// Provides access to nodes that have been exposed for being remotely controlled
		/// </summary>
		IExposedNodeService ExposedNodeService
		{
			get;
		}
		
		/// <summary>
		/// Indicates whether this IHDEHost is a client in a boygroup.
		/// </summary>
		bool IsBoygroupClient
		{
			get;
		}
		
		/// <summary>
		/// The IP adress of the boygroup server.
		/// </summary>
		string BoygroupServerIP
		{
			get;
		}

        /// <summary>
        /// Whether or not vvvv is running in background.
        /// </summary>
        bool IsRunningInBackground
        {
            get;
        }
        
        		
		/// <summary>
		/// Indicates whether this IHDEHost is running in encypted mode.
		/// </summary>
		bool IsBlackBoxMode
		{
			get;
		}

        /// <summary>
        /// Disables the short cuts of vvvv. Each disable call needs to be followed by an enable call.
        /// </summary>
        void DisableShortCuts();

        /// <summary>
        /// Enables the short cuts of vvvv.
        /// </summary>
        void EnableShortCuts();
        
        /// <summary>
        /// Reference to the 50 Editor
        /// </summary>
        IQueryDelete FiftyEditor
        {
            set;
        }

        /// <summary>
        /// This is the untweaked frametime. Tweaking frame time is possible via clock nodes or via SetFrameTime / SetFrameTimeProvider
        /// </summary>
	    double OriginalFrameTime
        {
            get;
        }

        /// <summary>
        /// The given provider gets called by vvvv when it needs to pin down an official frame time for the current frame. 
        /// By using this you can potenitally reduce latency.
        /// </summary>
        void SetFrameTimeProvider(Func<double, double> timeProvider);

        /// <summary>
        /// The given provider gets called by vvvv when it needs to pin down an official frame time for the current frame. 
        /// By using this you can potenitally reduce latency.
        /// </summary>
        void SetFrameTimeProvider(ITimeProvider timeProvider);

        /// <summary>
        /// The version of vvvv. Will for example return "35.5.0.0" for the officially called beta 35.5
        /// </summary>
        Version Version { get; }

        void ShowVLEditor();
    }
    #endregion IHDEHost

    /// <summary>
    /// Implement this small interface and set it on the IHDEHost. 
    /// You'll get called back at the beginning of the next frame when vvvv needs to pin down an official frame for all animation nodes.
    /// </summary>
    [Guid("74E506D4-84B5-4263-8B32-C3A615F4869C"),
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface ITimeProvider
    {
        /// <summary>
        /// provide the time
        /// </summary>
        double GetTime(double originalNewFrameTime);
    }

    #region NodeBrowser
    /// <summary>
    /// Allows the NodeBrower to be contacted by the host
    /// </summary>
    [Guid("A0C810DA-E0CC-4A2E-BC3F-8139766945F1"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface INodeBrowser: IPluginBase
	{
		void Initialize(string text);
		void DragDrop(bool allow);
		void AfterShow();
		void BeforeHide(out string comment);
		bool IsStandalone
		{
		    get;
		    set;
		}
	}
	
	/// <summary>
	/// Allows the NodeBrower to communicate back to the host
	/// </summary>
	[Guid("5567811E-D2D3-4654-A3E3-2E8324C9F022"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface INodeBrowserHost
	{
		void CreateNode(INodeInfo nodeInfo);
		int CreateNode(INodeInfo nodeInfo, Point point);
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
	public interface IWindowSwitcher: IPluginBase
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
	public interface IKommunikator: IPluginBase
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
	/// <remarks>Deprecated: Use <see cref="INode2"/> instead.</remarks>
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
		/// Returns a slash-separated path of node IDs that uniquely identifies this node in the vvvv graph.
		/// </summary>
		/// <param name="useDescriptiveNames">If TRUE descriptive node names are used where available instead of the node ID.</param>
		/// <returns>A slash-separated path of node IDs that uniquely identifies this node in the vvvv graph.</returns>
		string GetNodePath(bool useDescriptiveNames);
		
		/// <summary>
		/// Get the nodes info.
		/// </summary>
		/// <returns>Returns this nodes INodeInfo.</returns>
		INodeInfo GetNodeInfo();
		/// <summary>
		/// Check if the node can offer a GUI window
		/// </summary>
		/// <returns>Returns true if this node can offer a GUI window.</returns>
		bool HasGUI
		{
		    get;
		}
		
		bool HasPatch
		{
		    get;
		}
		
		bool HasCode
		{
		    get;
		}
		
		StatusCode Status
		{
			get;
			set;
		}
		
		StatusCode InnerStatus
		{
			get;
		}
		
		//todo: check GetChildren mem leak?!
		int GetChildCount();
		INode GetChild(int index);
		INode[] GetChildren();
		INode ParentNode
		{
			get;
		}
		
		//todo: check GetPins mem leak?!
		IPin[] GetPins();
		IPin GetPin(string name);
		
		Rectangle GetBounds(BoundsType boundsType);
		
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
			set;
		}
		
		/// <summary>
		/// Gets the <see cref="IWindow">window</see> of this node. Or null if
		/// this node doesn't have a window.
		/// </summary>
		IWindow Window
		{
			get;
		}
		
		/// <summary>
		/// For internal use only.
		/// </summary>
		object Tag { [return: MarshalAs(UnmanagedType.IUnknown)] get; [param: MarshalAs(UnmanagedType.IUnknown)] set; }
	}	
	
	[Guid("1ABB290D-9A96-4944-80CC-F544C8CDD14B"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface INodeListener
    {
        void AddedCB(INode childNode);
        void RemovedCB(INode childNode);
        void PinAddedCB(IPin pin);
        void PinRemovedCB(IPin pin);
        void LabelChangedCB();
		void StatusChangedCB();
		void InnerStatusChangedCB();
		void BoundsChangedCB(BoundsType boundsType);
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
	    string Name
	    {
	    	get;
	    }
	    
	    PinDirection Direction
	    {
	    	get;
	    }
	    
	    PinVisibility Visibility
	    {
	        get; set;
	    }
	    
	    string Type
	    {
	    	get;
	    }
	    
	    Type CLRType 
	    { 
	    	get;
	    }
	    
	    string SubType
	    {
	    	get;
	    }
	    
	    INode ParentNode
		{
			get;
		}
	    
	    StatusCode Status
		{
			get;
		}
	    
		int SliceCount
		{
			get;
		}
		
		string GetNameByParent(INode parent);
		INode GetParentNodeByPatch(INode patch);
		
	    string GetSlice(int sliceIndex);
		void SetSlice(int sliceIndex, string slice);
		
		string GetSpread();
		void SetSpread(string spread);
		
		IPin[] GetConnectedPins();
		
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
		
		/// <summary>
		/// For internal use only.
		/// </summary>
		object Tag { [return: MarshalAs(UnmanagedType.IUnknown)] get; [param: MarshalAs(UnmanagedType.IUnknown)] set; }
	}

	[Guid("F8D09D3D-D988-434D-9AD4-8AD4C94001E7"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IPinListener
    {
        void ChangedCB();
		void StatusChangedCB();
		void SubtypeChangedCB();
		void ConnectedCB(IPin otherPin);
		void DisconnectedCB(IPin otherPin);
    }
	#endregion IPin
	
	#region IWindow
	/// <summary>
	/// Gives access to vvvv windows
	/// </summary>
	/// /// <remarks>Deprecated: Use <see cref="IWindow2"/> instead.</remarks>
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
		
		IntPtr Handle
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
    [Guid("26E07785-59FC-30F9-BBA1-5A3161B75D8B"),
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
    	/// The absolute path to file currently opened.
    	/// </summary>
    	string OpenedFile
    	{
    		get;
    	}
    }
    #endregion
}
