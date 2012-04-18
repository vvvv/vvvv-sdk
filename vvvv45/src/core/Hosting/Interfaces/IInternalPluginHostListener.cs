using System;
using System.Runtime.InteropServices;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Hosting.Interfaces
{
    [Guid("3CDBD4E2-32C8-4FF5-9F08-120156AA57C0"),
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IInternalPluginHostListener
    {
        void ConnectCB(IPluginIO sender, IPin otherPin);
        void DisconnectCB(IPluginIO sender, IPin otherPin);
    }
}
