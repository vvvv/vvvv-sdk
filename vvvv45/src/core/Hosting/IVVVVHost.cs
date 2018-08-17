using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using VVVV.Hosting.Interfaces;
using VVVV.Hosting.Interfaces.EX9;
using VVVV.PluginInterfaces.InteropServices.EX9;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Hosting
{
    [Guid("B2F7BF7A-C77B-498E-8C18-273519A9C406"),
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    /// <summary>
    /// vvvv as seen by HDE host.
    /// </summary>
    public interface IVVVVHost
    {
        void AddNodeSelectionListener(INodeSelectionListener listener);
        void RemoveNodeSelectionListener(INodeSelectionListener listener);
        void AddMouseClickListener(IMouseClickListener listener);
        void RemoveMouseClickListener(IMouseClickListener listener);
        void AddWindowSelectionListener(IWindowSelectionListener listener);
        void RemoveWindowSelectionListener(IWindowSelectionListener listener);
        void AddWindowListener(IWindowListener listener);
        void RemoveWindowListener(IWindowListener listener);
        void AddComponentModeListener(IComponentModeListener listener);
        void RemoveComponentModeListener(IComponentModeListener listener);
        void AddEnumListener(IEnumListener listener);
        void RemoveEnumListener(IEnumListener listener);

        /// <summary>
        /// The graphs root node
        /// </summary>
        INode Root
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
        /// <param name="EntryCount">Number of entries in the Enum.</param>
        void GetEnumEntryCount(string EnumName, out int EntryCount);
        
        /// <summary>
        /// Returns the name of a given EnumEntry of a given Enum.
        /// </summary>
        /// <param name="EnumName">The name of the Enum to get the EntryName of.</param>
        /// <param name="Index">Index of the EnumEntry.</param>
        /// <param name="EntryName">String representation of the EnumEntry.</param>
        void GetEnumEntry(string EnumName, int Index, out string EntryName);
        
        /// <summary>
        /// Allows the InternalHDEHost to write messages to a console on the vvvv host.
        /// </summary>
        /// <param name="Type">The type of message. Depending on the setting of this parameter the console can handle messages differently.</param>
        /// <param name="Message">The message to be logged.</param>
        void Log(TLogType Type, string Message);
        
        /// <summary>
        /// Returns the current time which the plugin should use if it does timebased calculations.
        /// </summary>
        /// <param name="CurrentTime">The hosts current time.</param>
        void GetCurrentTime(out double CurrentTime);
        
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
        /// The <see cref="IInternalNodeInfoFactory">node info factory</see> used to create <see cref="INodeInfo">node infos</see>.
        /// </summary>
        IInternalNodeInfoFactory NodeInfoFactory
        {
            get;
        }
        
        /// <summary>
        /// The currently selected patch window.
        /// </summary>
        IWindow ActivePatchWindow
        {
            get;
        }
        
        /// <summary>
        /// Gets the Direct3D9 device service.
        /// </summary>
        IInternalDXDeviceService DeviceService
        {
            get;
        }
        
        /// <summary>
        /// Gets the main loop.
        /// </summary>
        IInternalMainLoop MainLoop
        {
            get;
        }
        
        /// <summary>
        /// Gets the ExposedNode service.
        /// </summary>
        IInternalExposedNodeService ExposedNodeService
        {
        	get;
        }
        bool IsBoygroupClient 
        {
			get;
		}
    	
		string BoygroupServerIP 
		{
			get;
		}

        /// <summary>
        /// Whether or not vvvv is running in background.
        /// </summary>
        bool IsInBackground
        {
            get;
        }
        
        bool IsBlackBoxMode 
        {
			get;
		}

        /// <summary>
        /// Disables the short cuts of vvvv. Each disable call needs to followed by an enable call.
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
        double GetOriginalFrameTime();

        /// <summary>
        /// The given provider gets called by vvvv when it needs to pin down an official frame time for the current frame. 
        /// By using this you can potenitally reduce latency.
        /// </summary>
        void SetTimeProvider(ITimeProvider timeProvider);

        void ShowVLEditor();
    }

    #region Listeners

    /// <summary>
    /// Listener interface to be informed of a changed node-selection.
    /// </summary>
    [Guid("C9ACADDA-1D3F-410D-B23C-E8D576F4F361"),
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface INodeSelectionListener
    {
        void NodeSelectionChangedCB(INode[] nodes);
    }
    
    /// <summary>
    /// Listener interface to be informed of a mouseclicks in a patch.
    /// </summary>
    [Guid("2E1F9CF2-9D98-43DC-B3D9-F67FCA4ACED4"),
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMouseClickListener
    {
        void MouseDownCB(INode node, Mouse_Buttons button, Modifier_Keys keys);
        void MouseUpCB(INode node, Mouse_Buttons button, Modifier_Keys keys);
    }
    
    /// <summary>
    /// Listener interface to be informed of added/removed windows.
    /// </summary>
    [Guid("804F060E-5770-4D5E-82F0-A0655321EBE3"),
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IWindowListener
    {
        void WindowAddedCB(IWindow window);
        void WindowRemovedCB(IWindow window);
    }
    
    /// <summary>
    /// Listener interface to be informed of changed componentmodes of windows.
    /// </summary>
    [Guid("F14A619F-9378-42CE-9F18-D96BAE3EEC16"),
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IComponentModeListener
    {
        void BeforeComponentModeChangedCB(IWindow window, ComponentMode componentMode);
        void AfterComponentModeChangedCB(IWindow window, ComponentMode componentMode);
    }
    
    /// <summary>
    /// Listener interface to be informed of the active window.
    /// </summary>
    [Guid("9FB8F749-E2FF-4E6A-A0A6-A9BE74F989A1"),
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IWindowSelectionListener
    {
        void WindowSelectionChangeCB(IWindow window);
    }

    /// <summary>
    /// Listener interface to be informed of a changed Enum.
    /// </summary>
    [Guid("D5248C93-C357-4378-A638-A322D14FAFCC"),
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IEnumListener
    {
        void EnumChangeCB(string enumName);
    }

    #endregion Listeners
}
