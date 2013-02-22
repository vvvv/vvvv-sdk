using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using SlimDX.Direct3D9;

namespace VVVV.PluginInterfaces.V2.EX9
{
    /// <summary>
    /// IDXDeviceService provides access to Direct3D9 devices created by vvvv.
    /// </summary>
    [ComVisible(false)]
    public interface IDXDeviceService
    {
        IEnumerable<Device> Devices
        {
            get;
        }
        
        event EventHandler<DeviceEventArgs> DeviceAdded;
        event EventHandler<DeviceEventArgs> DeviceRemoved;
        event EventHandler<DeviceEventArgs> DeviceEnabled;
        event EventHandler<DeviceEventArgs> DeviceDisabled;

        bool UseDx9Ex { get; }
    }
    
    [ComVisible(false)]
    public class DeviceEventArgs : EventArgs
    {
        public DeviceEventArgs(Device device)
        {
            Device = device;
        }
        
        public Device Device
        {
            get;
            private set;
        }
    }
}
