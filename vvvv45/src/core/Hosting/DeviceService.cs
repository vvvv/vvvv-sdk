using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using SlimDX.Direct3D9;
using VVVV.Hosting.Interfaces.EX9;
using VVVV.PluginInterfaces.V2.EX9;

namespace VVVV.Hosting
{
    [ComVisible(false)]
    class DeviceService : IDXDeviceService, IDisposable
    {
        class DeviceListener : IInternalDXDeviceListener
        {
            private readonly DeviceService FDeviceService;
            
            public DeviceListener(DeviceService deviceService)
            {
                FDeviceService = deviceService;
            }
            
            public void DeviceAddedCB(IntPtr devicePtr)
            {
                var device = Device.FromPointer(devicePtr);
                FDeviceService.OnDeviceAdded(new DeviceEventArgs(device));
            }
            
            public void DeviceRemovedCB(IntPtr devicePtr)
            {
                var device = Device.FromPointer(devicePtr);
                FDeviceService.OnDeviceRemoved(new DeviceEventArgs(device));
                device.Dispose();
            }
        }
        
        private readonly IInternalDXDeviceService FDeviceService;
        private readonly DeviceListener FDeviceListener;
        
        public DeviceService(IInternalDXDeviceService deviceService)
        {
            FDeviceService = deviceService;
            FDeviceListener = new DeviceListener(this);
            FDeviceService.Subscribe(FDeviceListener);
        }
        
        public void Dispose()
        {
            FDeviceService.Unsubscribe(FDeviceListener);
        }
        
        public event EventHandler<DeviceEventArgs> DeviceAdded;
        
        protected virtual void OnDeviceAdded(DeviceEventArgs e)
        {
            if (DeviceAdded != null) 
            {
                DeviceAdded(this, e);
            }
        }
        
        public event EventHandler<DeviceEventArgs> DeviceRemoved;
        
        protected virtual void OnDeviceRemoved(DeviceEventArgs e)
        {
            if (DeviceRemoved != null) 
            {
                DeviceRemoved(this, e);
            }
        }
        
        public IEnumerable<Device> Devices 
        {
            get 
            {
                foreach (var devicePtr in FDeviceService.Devices)
                {
                    yield return Device.FromPointer(devicePtr);
                }
            }
        }
    }
}
