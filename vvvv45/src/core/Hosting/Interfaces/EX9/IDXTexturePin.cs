using System;
using System.Runtime.InteropServices;
using SlimDX.Direct3D9;
using VVVV.PluginInterfaces.InteropServices.EX9;

namespace VVVV.Hosting.Interfaces.EX9
{
    [Guid("A1533F1B-4894-4111-84EA-294538129939"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDXTexturePin : IDXResourcePin
    {
        Texture this[[In, MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(DeviceMarshaler))] Device device, int slice]
        {
            [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(TextureMarshaler))]
            get;
        }
    }
}
