using System;
using System.Runtime.InteropServices;
using VVVV.PluginInterfaces.V1;

namespace VVVV.PluginInterfaces.V2
{
    [Guid("21230B31-1929-44F8-B8C0-03E5C2AA42EF"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IPluginHost2 : IPluginHost
	{
		IPluginBase Plugin
		{
			get;
			set;
		}
	}
}
