using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

using SlimDX;
using SlimDX.Direct3D9;

namespace VVVV.PluginInterfaces.V2.EX9
{
    public class DXResource<TResource, TMetadata> : Resource<Device, IntPtr, TResource, TMetadata> where TResource : IComObject
    {
        public DXResource(TMetadata metadata, Func<TMetadata, Device, TResource> createResourceFunc, Action<TMetadata, TResource> updateResourceFunc, Action<TMetadata, TResource> destroyResourceAction)
            : base(metadata, createResourceFunc, updateResourceFunc, destroyResourceAction)
        {
        }
        
        public DXResource(TMetadata metadata, Func<TMetadata, Device, TResource> createResourceFunc, Action<TMetadata, TResource> updateResourceFunc)
            : base(metadata, createResourceFunc, updateResourceFunc)
        {
        }
        
        public DXResource(TMetadata metadata, Func<TMetadata, Device, TResource> createResourceFunc)
            : base(metadata, createResourceFunc)
        {
        }
        
        protected override IntPtr GetDeviceKey(Device device)
        {
            return device.ComPointer;
        }
    }
}
