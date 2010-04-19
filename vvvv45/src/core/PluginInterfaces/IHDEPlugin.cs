using System;
using System.Runtime.InteropServices;
using System.IO;
using System.Drawing;

/// <summary>
/// Version 1 of the VVVV PluginInterface.
/// DirectX/SlimDX related parts are in a separate file: PluginDXInterface1.cs
///
/// To convert this to a typelib make sure AssemblyInfo.cs states: ComVisible(true).
/// Then on a commandline type:
/// <c>regasm _PluginInterfaces.dll /tlb</c>
/// This generates and registers the typelib which can then be imported e.g. via Delphi:Components:Import Component:Import Typelib
/// </summary>
namespace VVVV.PluginInterfaces.V1
{
	/// <summary>
	/// Optional interface to be implemented on a plugin that needs to know when one of its pins is connected or disconnected
	/// </summary>
	[Guid("873E27B0-76E9-4F1E-8E42-D0A48DD8F42F"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IHDEPlugin
	{
		/// <summary>
		/// Called by the PluginHost to hand over the HDE host which gives direct access to parts of the HDE.
		/// </summary>
		/// <param name="host">Interface to the HDEHost.</param>
		void SetHDEHost(IHDEHost host);
	}
}
