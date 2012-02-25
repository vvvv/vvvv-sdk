using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using SlimDX.Direct3D9;

namespace VVVV.PluginInterfaces.InteropServices.EX9
{
    [ComVisible(false)]
    internal class DeviceMarshaler : ICustomMarshaler, IDXDeviceListener
    {
        private static DeviceMarshaler marshaler = null;
        public static ICustomMarshaler GetInstance(string cookie)
        {
            if (marshaler == null)
            {
                throw new InvalidOperationException("DeviceMarshaler has not yet been initialized.");
            }
            return marshaler;
        }
        
        internal static void Initialize(IDXDeviceService deviceService)
        {
            if (marshaler == null)
            {
                marshaler = new DeviceMarshaler(deviceService);
            }
            else
            {
                throw new InvalidOperationException("DeviceMarshaler has already been initialized.");
            }
        }
        
        private readonly Dictionary<IntPtr, Device> FDevices = new Dictionary<IntPtr, Device>();
        private readonly IDXDeviceService FDeviceService;
        
        private DeviceMarshaler(IDXDeviceService deviceService)
        {
            FDeviceService = deviceService;
            foreach (var devicePtr in FDeviceService.Devices)
            {
                FDevices[devicePtr] = Device.FromPointer(devicePtr);
            }
            
            FDeviceService.Subscribe(this);
        }
        
        public object MarshalNativeToManaged(IntPtr pNativeData)
        {
            Device device = null;
            FDevices.TryGetValue(pNativeData, out device);
            return device;
        }
        
        public IntPtr MarshalManagedToNative(object ManagedObj)
        {
            Device device = ManagedObj as Device;
            if (device != null)
                return device.ComPointer;
            else
                return IntPtr.Zero;
        }
        
        public void CleanUpNativeData(IntPtr pNativeData)
        {
            // Do nothing
        }
        
        public void CleanUpManagedData(object ManagedObj)
        {
            // Do nothing
        }
        
        public int GetNativeDataSize()
        {
            return Marshal.SizeOf(typeof(IntPtr));
        }
        
        void IDXDeviceListener.DeviceAddedCB(IntPtr devicePtr)
        {
            Device device = null;
            if (!FDevices.TryGetValue(devicePtr, out device))
            {
                device = Device.FromPointer(devicePtr);
                FDevices.Add(devicePtr, device);
            }
        }
        
        void IDXDeviceListener.DeviceRemovedCB(IntPtr devicePtr)
        {
            Device device = null;
            if (FDevices.TryGetValue(devicePtr, out device))
            {
                device.Dispose();
                FDevices.Remove(devicePtr);
            }
        }
    }
}
