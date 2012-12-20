using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

using SlimDX;
using SlimDX.Direct3D9;

namespace VVVV.PluginInterfaces.V2.EX9
{
    [ComVisible(false)]
    public class DXResource<TResource, TMetadata> : Resource<Device, TResource, TMetadata> 
        where TResource : ComObject
    {
        public DXResource(
            TMetadata metadata, 
            Func<TMetadata, Device, TResource> createResourceFunc, 
            Action<TMetadata, TResource> updateResourceFunc = null, 
            Action<TMetadata, TResource, DestroyReason> destroyResourceAction = null)
            : base(metadata, createResourceFunc, updateResourceFunc, destroyResourceAction)
        {
        }

        public bool IsDefaultPool
        {
            get;
            private set;
        }

        protected override TResource CreateResoure(TMetadata metadata, Device device)
        {
            var resource = base.CreateResoure(metadata, device);
            if (resource != null)
            {
                IsDefaultPool = resource.IsDefaultPool;
            }
            return resource;
        }
    }
}
