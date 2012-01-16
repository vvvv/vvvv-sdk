using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

using SlimDX;
using SlimDX.Direct3D9;

namespace VVVV.PluginInterfaces.V2.EX9
{
    [ComVisible(false)]
    public class DXResource<TResource, TMetadata> : Resource<Device, TResource, TMetadata> where TResource : IComObject
    {
        public DXResource(
            TMetadata metadata, 
            Func<TMetadata, Device, TResource> createResourceFunc, 
            Action<TMetadata, TResource> updateResourceFunc, 
            Action<TMetadata, TResource, DestroyReason> destroyResourceAction)
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
    }
}
