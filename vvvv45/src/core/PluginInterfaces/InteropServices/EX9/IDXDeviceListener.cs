using System;
using System.Runtime.InteropServices;

namespace VVVV.PluginInterfaces.InteropServices.EX9
{
    /// <summary>
    /// IDXDeviceListener is the callback interface used by IDXDeviceService.
    /// </summary>
    [Guid("98B82B7E-487B-476A-A174-37FA07C1D408"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDXDeviceListener
    {
        void DeviceAddedCB(IntPtr devicePtr);
        void DeviceRemovedCB(IntPtr devicePtr);
    }
}
