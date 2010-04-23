using System;
using System.Runtime.InteropServices;
using System.IO;
using System.Drawing;

namespace VVVV.PluginInterfaces.V1
{
	/// <summary>
	/// If a plugin needs access to the HDE host this interface needs to be implemented.
	/// </summary>
	[Guid("873E27B0-76E9-4F1E-8E42-D0A48DD8F42F"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IHDEPlugin : IPluginBase
	{
		/// <summary>
		/// Called by the PluginHost to hand over the HDE host which gives direct access to parts of the HDE.
		/// </summary>
		/// <param name="host">Interface to the HDEHost.</param>
		void SetHDEHost(IHDEHost host);
	}
}
