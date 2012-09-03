using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using SlimDX.Direct3D9;
using VVVV.Hosting.Interfaces.EX9;
using VVVV.PluginInterfaces.V2.EX9;
using VVVV.PluginInterfaces.InteropServices.EX9;

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
            	var device = (Device) DeviceMarshaler.GetInstance().MarshalNativeToManaged(devicePtr);
                FDeviceService.OnDeviceAdded(new DeviceEventArgs(device));
            }
            
            public void DeviceRemovedCB(IntPtr devicePtr)
            {
                var device = (Device) DeviceMarshaler.GetInstance().MarshalNativeToManaged(devicePtr);
                FDeviceService.OnDeviceRemoved(new DeviceEventArgs(device));
                device.Dispose();
            }
            
            public void DeviceEnabledCB(IntPtr devicePtr)
            {
                var device = (Device) DeviceMarshaler.GetInstance().MarshalNativeToManaged(devicePtr);
                FDeviceService.OnDeviceEnabled(new DeviceEventArgs(device));
            }
            
            public void DeviceDisabledCB(IntPtr devicePtr)
            {
                var device = (Device) DeviceMarshaler.GetInstance().MarshalNativeToManaged(devicePtr);
                FDeviceService.OnDeviceDisabled(new DeviceEventArgs(device));
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
        
        public event EventHandler<DeviceEventArgs> DeviceEnabled;
        
        protected virtual void OnDeviceEnabled(DeviceEventArgs e)
        {
            if (DeviceEnabled != null) 
            {
                DeviceEnabled(this, e);
            }
        }
        
        public event EventHandler<DeviceEventArgs> DeviceDisabled;
        
        protected virtual void OnDeviceDisabled(DeviceEventArgs e)
        {
            if (DeviceDisabled != null) 
            {
                DeviceDisabled(this, e);
            }
        }
        
        public IEnumerable<Device> Devices 
        {
            get 
            {
                foreach (var devicePtr in FDeviceService.Devices)
                {
                	yield return (Device) DeviceMarshaler.GetInstance().MarshalNativeToManaged(devicePtr);
                }
            }
        }
    }
}
