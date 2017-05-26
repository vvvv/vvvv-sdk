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
        private static Guid FDX9EX = new Guid("{B18B10CE-2649-405a-870F-95F777D4313A}");
        public static ICustomMarshaler GetInstance(string cookie = "")
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
            {
            	IntPtr pD3ex;
            	Marshal.QueryInterface(pNativeData, ref FDX9EX, out pD3ex);
                if (pD3ex == IntPtr.Zero)
                    return Device.FromPointer(pNativeData);
                else
                {
                    // QueryInterface increased the reference count -> need to decrease it again
                    Marshal.Release(pD3ex);
                    return DeviceEx.FromPointer(pD3ex);
                }
            }
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
