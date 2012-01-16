using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace VVVV.PluginInterfaces.V2
{
    public enum DestroyReason
    {
        DeviceLost,
        Dispose
    }
    
    [ComVisible(false)]
    public abstract class Resource<TDevice, TResource, TMetadata> : IDisposable where TResource : IDisposable
    {
        private readonly Func<TMetadata, TDevice, TResource> FCreateResourceFunc;
        private readonly Action<TMetadata, TResource> FUpdateResourceFunc;
        private readonly Action<TMetadata, TResource, DestroyReason> FDestroyResourceAction;
        private readonly Dictionary<TDevice, TResource> FResources = new Dictionary<TDevice, TResource>();
		private readonly TMetadata FMetadata;
        
        public Resource(
		    TMetadata metadata, 
		    Func<TMetadata, TDevice, TResource> createResourceFunc, 
		    Action<TMetadata, TResource> updateResourceFunc, 
		    Action<TMetadata, TResource, DestroyReason> destroyResourceAction)
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
        
        public TResource this[TDevice device]
        {
            get
            {
                TResource result = default(TResource);
                if (!FResources.TryGetValue(device, out result))
                {
                    result = FCreateResourceFunc(FMetadata, device);
                    FResources[device] = result;
                }
                return result;
            }
        }
        
        public TMetadata Metadata
        {
            get
            {
                return FMetadata;
            }
        }
        
        public void UpdateResource(TDevice device)
        {
            if (FResources.ContainsKey(device))
            {
                FUpdateResourceFunc(FMetadata, FResources[device]);
            }
            else
            {
                FUpdateResourceFunc(FMetadata, this[device]);
            }
        }
        
        public void DestroyResource(TDevice device, bool onlyUnmanaged)
        {
            if (FResources.ContainsKey(device))
            {
                FDestroyResourceAction(FMetadata, FResources[device], DestroyReason.DeviceLost);
                FResources.Remove(device);
            }
        }
        
        public void Dispose()
        {
            foreach (var resource in FResources.Values)
            {
                FDestroyResourceAction(FMetadata, resource, DestroyReason.Dispose);
            }
            
            FResources.Clear();
        }
        
        private static void UpdateResource(TMetadata metadata, TResource resource)
        {
            // Do nothing
        }
        
        private static void DestroyResource(TMetadata metadata, TResource resource, DestroyReason reason)
        {
            if (resource != null)
            {
                resource.Dispose();
            }
        }
    }
}
