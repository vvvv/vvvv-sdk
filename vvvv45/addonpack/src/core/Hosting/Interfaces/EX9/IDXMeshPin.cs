using System;
using System.Runtime.InteropServices;
using SlimDX.Direct3D9;
using VVVV.PluginInterfaces.InteropServices.EX9;

namespace VVVV.Hosting.Interfaces.EX9
{
    [Guid("7DB22BA8-7CB6-4086-BD60-02D5E938C285"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDXMeshPin : IDXResourcePin
    {
        Mesh this[[In, MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(DeviceMarshaler))] Device device, int slice]
        {
            [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(MeshMarshaler))]
            get;
        }
    }
}
