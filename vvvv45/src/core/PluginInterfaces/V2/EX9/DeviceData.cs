/*
 * Erstellt mit SharpDevelop.
 * Benutzer: TF
 * Datum: 29.08.2010
 * Zeit: 11:09
 * 
 * Sie können diese Vorlage unter Extras > Optionen > Codeerstellung > Standardheader ändern.
 */
using System;

namespace VVVV.PluginInterfaces.V2.EX9
{
	/// <summary>
	/// Base Class for custom data per graphics device.
	/// </summary>
	public class DeviceData
	{
		/// <summary>
		/// Update the device data this frame?
		/// </summary>
		public bool Update { get; set; }
		
		/// <summary>
		/// Recreate the device data this frame?
		/// </summary>
		public bool Recreate { get; set; }
		
		/// <summary>
		/// Create a DeviceData instance with 'Update = true' and 'Recreate = false'.
		/// </summary>
		public DeviceData()
		{
			Update = true;
			Recreate = false;
		}
	}
}
