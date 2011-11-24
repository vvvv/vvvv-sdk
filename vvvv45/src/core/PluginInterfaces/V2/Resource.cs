using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace VVVV.PluginInterfaces.V2
{
    [ComVisible(false)]
    public abstract class Resource<TDevice, TDeviceKey, TResource, TMetadata> : IDisposable where TResource : IDisposable
    {
        private readonly Func<TMetadata, TDevice, TResource> FCreateResourceFunc;
        private readonly Action<TMetadata, TResource> FUpdateResourceFunc;
        private readonly Action<TMetadata, TResource> FDestroyResourceAction;
        private readonly Dictionary<TDeviceKey, TResource> FResources = new Dictionary<TDeviceKey, TResource>();
		private readonly TMetadata FMetadata;
        
        public Resource(TMetadata metadata, Func<TMetadata, TDevice, TResource> createResourceFunc, Action<TMetadata, TResource> updateResourceFunc, Action<TMetadata, TResource> destroyResourceAction)
        {
            FMetadata = metadata;
            FCreateResourceFunc = createResourceFunc;
            FUpdateResourceFunc = updateResourceFunc;
            FDestroyResourceAction = destroyResourceAction;
        }
        
        public Resource(TMetadata metadata, Func<TMetadata, TDevice, TResource> createResourceFunc, Action<TMetadata, TResource> updateResourceFunc)
            : this(metadata, createResourceFunc, updateResourceFunc, DestroyResource)
        {
        }
        
        public Resource(TMetadata metadata, Func<TMetadata, TDevice, TResource> createResourceFunc)
            : this(metadata, createResourceFunc, UpdateResource)
        {
        }
        
        protected abstract TDeviceKey GetDeviceKey(TDevice device);
        
        public TResource this[TDevice device]
        {
            get
            {
                TResource result = default(TResource);
                TDeviceKey deviceKey = GetDeviceKey(device);
                if (!FResources.TryGetValue(deviceKey, out result))
                {
                    result = FCreateResourceFunc(FMetadata, device);
                    FResources[deviceKey] = result;
                }
                return result;
            }
        }
        
        public void UpdateResource(TDevice device)
        {
            TDeviceKey deviceKey = GetDeviceKey(device);
            if (FResources.ContainsKey(deviceKey))
            {
                FUpdateResourceFunc(FMetadata, FResources[deviceKey]);
            }
            else
            {
                FUpdateResourceFunc(FMetadata, this[device]);
            }
        }
        
        public void DestroyResource(TDevice device)
        {
            TDeviceKey deviceKey = GetDeviceKey(device);
            if (FResources.ContainsKey(deviceKey))
            {
                FDestroyResourceAction(FMetadata, FResources[deviceKey]);
                FResources.Remove(deviceKey);
            }
        }
        
        public void Dispose()
        {
            foreach (var resource in FResources.Values)
            {
                FDestroyResourceAction(FMetadata, resource);
            }
            
            FResources.Clear();
        }
        
        private static void UpdateResource(TMetadata metadata, TResource resource)
        {
            // Do nothing
        }
        
        private static void DestroyResource(TMetadata metadata, TResource resource)
        {
            resource.Dispose();
        }
    }
}
