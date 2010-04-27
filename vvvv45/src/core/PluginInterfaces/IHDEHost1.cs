using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using VVVV.HDE.Model;

namespace VVVV.PluginInterfaces.V1
{
	[Guid("2B24AC85-E543-40B3-9090-2828D26978A0"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IHDEHost 
	{
		/// <summary>
		/// Provides access to the Solution hosted by this IHDEHost.
		/// </summary>
		ISolution Solution { get; }
	}
}
