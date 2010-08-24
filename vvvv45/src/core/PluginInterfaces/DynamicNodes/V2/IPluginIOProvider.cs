using System;
using VVVV.PluginInterfaces.V1;

namespace VVVV.PluginInterfaces.V2
{
	public interface IPluginIOProvider
	{
		IPluginIO PluginIO { get; }	
	}
}
