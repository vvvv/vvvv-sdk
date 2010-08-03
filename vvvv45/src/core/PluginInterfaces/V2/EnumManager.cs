using System;
using System.ComponentModel.Composition;
using VVVV.PluginInterfaces.V1;

namespace VVVV.PluginInterfaces.V2
{
	/// <summary>
	/// Represents a Key/Value Pair of an Enum
	/// </summary>
	public class EnumEntry
	{
		public string Name { get; private set; }
		public int Index { get; private set; }

		public EnumEntry(string enumName, int index)
		{
			Index = index;
			Name = EnumManager.GetEnumEntryString(enumName, index);
		}

		//EnumEntry can be used like a string
		public static implicit operator string(EnumEntry e)
		{
			return e.Name;
		}

		//EnumEntry can be used like an int
		public static implicit operator int(EnumEntry e)
		{
			return e.Index;
		}
	}

	/// <summary>
	/// Manages the global Enums of the Host.
	/// </summary>
	public static class EnumManager
	{
		[Import]
		private static IHDEHost FHost;

		/// <summary>
		/// Allows a plugin to create/update an Enum with vvvv
		/// </summary>
		/// <param name="enumName">The Enums name.</param>
		/// <param name="defaultEntry">The Enums default value.</param>
		/// <param name="enumEntries">An array of strings that specify the enums entries.</param>
		public static void UpdateEnum(string enumName, string defaultEntry, string[] enumEntries)
		{
			FHost.UpdateEnum(enumName, defaultEntry, enumEntries);
		}

		/// <summary>
		/// Returns the number of entries for a given Enum.
		/// </summary>
		/// <param name="enumName">The name of the Enum to get the EntryCount of.</param>
		/// <returns>Number of entries in the Enum.</returns>
		public static int GetEnumEntryCount(string enumName)
		{
			int count;
			FHost.GetEnumEntryCount(enumName, out count);
			return count;
		}

		/// <summary>
		/// Returns the name of a given EnumEntry of a given Enum.
		/// </summary>
		/// <param name="enumName">The name of the Enum to get the EntryName of.</param>
		/// <param name="index">Index of the EnumEntry.</param>
		/// <returns>String representation of the EnumEntry.</returns>
		public static string GetEnumEntryString(string enumName, int index)
		{
			string entry;
			FHost.GetEnumEntry(enumName, index, out entry);
			return entry;
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
