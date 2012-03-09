using System;
using System.Runtime.InteropServices;
using SlimDX.Direct3D9;

namespace VVVV.PluginInterfaces.InteropServices.EX9
{
    /// <summary>
    /// IDXDeviceService provides access to Direct3D9 devices created by vvvv.
    /// </summary>
    [Guid("60C92F66-6EF7-4017-9D5C-8DAF508CB405"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDXDeviceService
    {
        IntPtr[] Devices
        {
            get;
        }
        
        void Subscribe(IDXDeviceListener listener);
        void Unsubscribe(IDXDeviceListener listener);
    }
}
