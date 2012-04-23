using System;
using System.Runtime.InteropServices;

namespace VVVV.Hosting.Interfaces.EX9
{
    /// <summary>
    /// IDXInternalDeviceListener is the callback interface used by IDXInternalDeviceService.
    /// </summary>
    [Guid("98B82B7E-487B-476A-A174-37FA07C1D408"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IInternalDXDeviceListener
    {
        void DeviceAddedCB(IntPtr devicePtr);
        void DeviceRemovedCB(IntPtr devicePtr);
        void DeviceEnabledCB(IntPtr devicePtr);
        void DeviceDisabledCB(IntPtr devicePtr);
    }
}
