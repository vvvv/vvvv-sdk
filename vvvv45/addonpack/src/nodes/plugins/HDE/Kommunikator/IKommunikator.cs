using System;
using System.Runtime.InteropServices;

namespace VVVV.PluginInterfaces.V1
{
	/// <summary>
	/// Allows the Kommunikator to communicate back to the host
	/// </summary>
	[Guid("CF40CDDD-55BE-42D5-B6BB-1A05AE8FF9A8"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IKommunikator
	{
		/// <summary>
		/// Called by the KommunikatorHost to hand itself over to the Kommunikator.
		/// </summary>
		/// <param name="Host">Interface to the KommunikatorHost.</param>
		void SetKommunikatorHost(IKommunikatorHost host);
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
}
