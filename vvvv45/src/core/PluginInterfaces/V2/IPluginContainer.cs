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
    [ComVisible(false)]
    public interface IPluginContainer
    {
        IPluginBase PluginBase { get; }
    }
}
