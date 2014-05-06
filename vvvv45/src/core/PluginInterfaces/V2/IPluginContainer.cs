using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using VVVV.PluginInterfaces.V1;

namespace VVVV.PluginInterfaces.V2
{
    /// <summary>
    /// Used when a plugin use a small holder to another one
    /// </summary>
    [Guid("49504454-962E-4698-8F26-D8B37A932E66"),
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IPluginContainer
    {
        IPluginBase PluginBase { get; }
    }
}
