using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

using SlimDX.Direct3D9;

namespace VVVV.PluginInterfaces.InteropServices.EX9
{
    [ComVisible(false)]
    internal class MeshMarshaler : ICustomMarshaler
    {
        private static MeshMarshaler marshaler = null;
        public static ICustomMarshaler GetInstance(string cookie)
        {
            if (marshaler == null)
            {
                marshaler = new MeshMarshaler();
            }
            return marshaler;
        }
        
        public object MarshalNativeToManaged(IntPtr pNativeData)
        {
            if (pNativeData == IntPtr.Zero)
                return null;
            else
                return Mesh.FromPointer(pNativeData);
        }
        
        public IntPtr MarshalManagedToNative(object ManagedObj)
        {
            Mesh mesh = ManagedObj as Mesh;
            if (mesh != null)
            {
                // TODO: We need to do this. Find out why!
                Marshal.AddRef(mesh.ComPointer);
                return mesh.ComPointer;
            }
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
