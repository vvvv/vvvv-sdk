using System;
using System.Runtime.InteropServices;
using SlimDX.Direct3D9;
using VVVV.PluginInterfaces.InteropServices.EX9;

namespace VVVV.Hosting.Interfaces.EX9
{
    [Guid("D6363A89-7B3F-4D5A-8114-991615F31853"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDXResourcePin
    {
        void UpdateResources([In, MarshalAs(UnmanagedType.CustomMarshaler,  MarshalTypeRef = typeof(DeviceMarshaler))] Device device);
        void DestroyResources([In, MarshalAs(UnmanagedType.CustomMarshaler,  MarshalTypeRef = typeof(DeviceMarshaler))] Device device, bool onlyUnmanaged);
    }
}
