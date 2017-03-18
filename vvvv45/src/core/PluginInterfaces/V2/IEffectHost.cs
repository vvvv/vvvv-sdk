using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using VVVV.PluginInterfaces.V1;

namespace VVVV.PluginInterfaces.V2
{
	[Guid("5A7B81D4-3548-4E4C-B3F5-44A50E3C8E1B"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IEffectHost : IAddonHost, INode
	{
        void SetEffect(string filename, IStream code);
	    string GetParameterDescription();
		string GetErrors();	    
	}
}
