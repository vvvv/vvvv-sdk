using System;
using System.Runtime.InteropServices;
using VVVV.PluginInterfaces.V1;

namespace VVVV.PluginInterfaces.V2
{
    [Guid("9A6BD787-6865-44FA-B42A-6E57E4852AF9"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IPluginHost2 : IPluginHost, INode
	{
        IPluginBase Plugin
        {
            get;
            set;
        }
	}
}
