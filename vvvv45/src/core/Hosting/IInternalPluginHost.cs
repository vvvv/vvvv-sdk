using System;
using System.Runtime.InteropServices;
using VVVV.Hosting.Interfaces.EX9;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Hosting
{
	[Guid("21230B31-1929-44F8-B8C0-03E5C2AA42EF"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IInternalPluginHost : IPluginHost2
	{
		IPluginBase Plugin
		{
			get;
			set;
		}
		
		IDXTextureOut CreateTextureOutput2(IDXTexturePin texturePin, string name, TSliceMode sliceMode, TPinVisibility visibility);
	}
}
