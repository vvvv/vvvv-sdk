using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

using SlimDX.Direct3D9;

namespace VVVV.PluginInterfaces.InteropServices.EX9
{
    [ComVisible(false)]
    internal class TextureMarshaler : ICustomMarshaler
    {
        private static TextureMarshaler marshaler = null;
        public static ICustomMarshaler GetInstance(string cookie)
        {
            if (marshaler == null)
            {
                marshaler = new TextureMarshaler();
            }
            return marshaler;
        }
        
        public object MarshalNativeToManaged(IntPtr pNativeData)
        {
            if (pNativeData == IntPtr.Zero)
                return null;
            else
                return Texture.FromPointer(pNativeData);
        }
        
        public IntPtr MarshalManagedToNative(object ManagedObj)
        {
            Texture texture = ManagedObj as Texture;
            if (texture != null)
            {
                var ptr = texture.ComPointer;
                if (ptr != IntPtr.Zero)
                {
                    // TODO: We need to do this. Find out why!
                    Marshal.AddRef(ptr);
                    return ptr;
                }
            }
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
