using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace VVVV.PluginInterfaces.V2
{
    /// <summary>
    /// Implement this interface on your user control in order to get notified
    /// from vvvv when the control's parent window gets activated or deactivated.
    /// Useful in combination with IHDEHost.EnableShortCuts and IHDEHost.DisableShortCuts.
    /// </summary>
    [Guid("8f584960-8900-11e3-baa8-0800200c9a66"),
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IActivatableControl
    {
        void OnActivated();
        void OnDeactivated();
    }
}
