using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

using SlimDX.Direct3D9;
using VVVV.PluginInterfaces.V2.EX9;

namespace VVVV.PluginInterfaces.InteropServices.EX9
{
    [ComVisible(false)]
    internal class DeviceMarshaler : ICustomMarshaler
    {
        private static DeviceMarshaler marshaler = null;
        public static ICustomMarshaler GetInstance(string cookie)
        {
            if (marshaler == null)
            {
                marshaler = new DeviceMarshaler();
            }
            return marshaler;
        }
        
        public object MarshalNativeToManaged(IntPtr pNativeData)
        {
            if (pNativeData == IntPtr.Zero)
                return null;
            else
                return Device.FromPointer(pNativeData);
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
    }
}
