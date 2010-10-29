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
		/// Opens the given file.
		/// </summary>
		/// <param name="file">The file to open by vvvv.</param>
		/// <param name="inActivePatch">Whether it should be openend in the active patch or in the root patch.</param>
		void Open(string file, bool inActivePatch);
		
		/// <summary>
		/// The <see cref="INodeInfoFactory">node info factory</see> used to create <see cref="INodeInfo">node infos</see>.
		/// </summary>
		INodeInfoFactory NodeInfoFactory
		{
			get;
		}
	}
}
