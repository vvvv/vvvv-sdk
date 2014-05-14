using System;
using System.Runtime.InteropServices;

namespace VVVV.Hosting.Interfaces.EX9
{
    /// <summary>
    /// IDXInternalDeviceService provides access to Direct3D9 devices created by vvvv.
    /// </summary>
    [Guid("60C92F66-6EF7-4017-9D5C-8DAF508CB405"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IInternalDXDeviceService
    {
        IntPtr[] Devices
        {
            get;
        }
        
        void Subscribe(IInternalDXDeviceListener listener);
        void Unsubscribe(IInternalDXDeviceListener listener);
        bool UseDx9Ex { get; }
    }
}
