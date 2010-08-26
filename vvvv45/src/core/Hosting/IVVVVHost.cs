using System;
using System.Runtime.InteropServices;
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
		INodeInfo[] NodeInfos
		{
			get;
		}
	
	    void Add(INodeInfo NodeInfo);
	    
	    void Update(INodeInfo NodeInfo);
	
	    void Remove(INodeInfo NodeInfo);
	    
	    void AddListener(IListener listener);
	    
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
		/// Returns the absolut file path to the plugins host.
		/// </summary>
		/// <param name="Path">Absolut file path to the plugins host (i.e path to the patch the plugin is placed in, in vvvv).</param>
		void GetHostPath(out string Path);
		
		/// <summary>
		/// Returns a slash-separated path of node IDs that uniquely identifies this node in the vvvv graph.
		/// </summary>
		/// <param name="UseDescriptiveNames">If TRUE descriptive node names are used where available instead of the node ID.</param>
		/// <param name="Path">Slash-separated path of node IDs that uniquely identifies this node in the vvvv graph.</param>
		void GetNodePath(bool UseDescriptiveNames, out string Path);
	}
}
