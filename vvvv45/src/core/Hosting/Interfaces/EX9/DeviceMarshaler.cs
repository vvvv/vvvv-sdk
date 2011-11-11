using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

using SlimDX.Direct3D9;

namespace VVVV.Hosting.Interfaces.EX9
{
    [ComVisible(false)]
    public class DeviceMarshaler : ICustomMarshaler
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
            var device = ManagedObj as Device;
            device.Dispose();
        }
        
        public int GetNativeDataSize()
        {
            return Marshal.SizeOf(typeof(IntPtr));
        }
    }
}
