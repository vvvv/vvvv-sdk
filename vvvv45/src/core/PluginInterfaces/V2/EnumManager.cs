using System;
using System.Runtime.InteropServices;
using System.ComponentModel.Composition;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;

namespace VVVV.PluginInterfaces.V2
{
	/// <summary>
	/// Represents a Key/Value Pair of an enum.
	/// </summary>
	[ComVisible(false)]
	public sealed class EnumEntry
	{
		/// <summary>
		/// The string representation of this entry.
		/// </summary>
		public string Name { get; private set; }
		
		/// <summary>
		/// The index of this entry in the enum.
		/// </summary>
		public int Index { get; private set; }
		
		/// <summary>
		/// The name of the enum to which this entry belongs.
		/// </summary>
		public string EnumName { get; private set; }
		
		/// <summary>
		/// Creates an EnumEntry from a specific enum.
		/// </summary>
		/// <param name="enumName">The enum name</param>
		/// <param name="index">Position in enum</param>
		public EnumEntry(string enumName, int index)
		{
			Index = index;
			Name = EnumManager.GetEnumEntryString(enumName, index);
			EnumName = enumName;
		}

        public EnumEntry(string enumName, int index, string entryName)
        {
            Index = index;
            Name = entryName;
            EnumName = enumName;
        }

        /// <summary>
        /// EnumEntry can be used like a string
        /// </summary>
        public static implicit operator string(EnumEntry e)
		{
			return e.Name;
		}

		/// <summary>
		/// EnumEntry can be used like an int
		/// </summary>
		public static implicit operator int(EnumEntry e)
		{
			return e.Index;
		}
	}

	/// <summary>
	/// Manages the global Enums of the HDEHost.
	/// </summary>
	[ComVisible(false)]
	public static class EnumManager
	{
		//the host
		private static IHDEHost FHost;
		public static void SetHDEHost(IHDEHost host)
		{
			FHost = host;
		}
		
		/// <summary>
		/// Allows a plugin to create/update an Enum with vvvv
		/// </summary>
		/// <param name="enumName">The enums name.</param>
		/// <param name="defaultEntry">The enums default value.</param>
		/// <param name="enumEntries">An array of strings that specify the enums entries.</param>
		public static void UpdateEnum(string enumName, string defaultEntry, string[] enumEntries)
		{
			FHost.UpdateEnum(enumName, defaultEntry, enumEntries);
		}
		
		/// <summary>
		/// Adds an enum entry to the end of an enum. This method is quite slow,
		/// it copies all old entries, adds the new one and commits it back to the host.
		/// </summary>
		/// <param name="enumName">The enums name.</param>
		/// <param name="entryName">The new enum entry.</param>
		public static void AddEntry(string enumName, string entryName)
		{
			var count = GetEnumEntryCount(enumName);
			var entries = new string[count + 1];
			
			for (int i = 0; i<count; i++) 
			{
				entries[i] = GetEnumEntryString(enumName, i);
			}
			
			entries[count] = entryName;
			
			UpdateEnum(enumName, entries[0], entries);
		}
		
		/// <summary>
		/// Adds an enum entry at a specific position of an enum.
		/// </summary>
		/// <param name="enumName">The enums name.</param>
		/// <param name="entryName">The new enum entry.</param>
		/// <param name="index">Position of the new entry.</param>
		public static void AddEntry(string enumName, string entryName, int index)
		{
			var count = GetEnumEntryCount(enumName);
			
			index = VMath.Clamp(index, 0, count);
			
			var entries = new string[count + 1];
			
			for (int i = 0; i < count; i++) 
			{
				int offset = (i < index) ? 0 : 1;
				entries[i + offset] = GetEnumEntryString(enumName, i);
			}
			
			entries[index] = entryName;
			
			UpdateEnum(enumName, entries[0], entries);
		}

		/// <summary>
		/// Returns the number of entries for a given Enum.
		/// </summary>
		/// <param name="enumName">The name of the Enum to get the EntryCount of.</param>
		/// <returns>Number of entries in the Enum.</returns>
		public static int GetEnumEntryCount(string enumName)
		{
			return FHost.GetEnumEntryCount(enumName);
		}

		/// <summary>
		/// Returns the name of a given EnumEntry of a given Enum.
		/// </summary>
		/// <param name="enumName">The name of the Enum to get the EntryName of.</param>
		/// <param name="index">Index of the EnumEntry.</param>
		/// <returns>String representation of the EnumEntry.</returns>
		public static string GetEnumEntryString(string enumName, int index)
		{
			return FHost.GetEnumEntry(enumName, index);
		}

		/// <summary>
		/// Returns an EnumEntry instance of a enum entry of a given Enum.
		/// </summary>
		/// <param name="enumName">The name of the Enum.</param>
		/// <param name="index">Index of the EnumEntry.</param>
		/// <returns>EnumEntry instance of the enum entry.</returns>
		public static EnumEntry GetEnumEntry(string enumName, int index)
		{
			return new EnumEntry(enumName, index);
		}

	}
}
