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
    public abstract class Resource<TDevice, TResource, TMetadata> : IDisposable 
        where TResource : class, IDisposable
    {
        private readonly Func<TMetadata, TDevice, TResource> FCreateResourceFunc;
        private readonly Action<TMetadata, TResource> FUpdateResourceFunc;
        private readonly Action<TMetadata, TResource, DestroyReason> FDestroyResourceAction;
        
        public Resource(
            TMetadata metadata, 
            Func<TMetadata, TDevice, TResource> createResourceFunc, 
            Action<TMetadata, TResource> updateResourceFunc = null, 
            Action<TMetadata, TResource, DestroyReason> destroyResourceAction = null)
        {
            Metadata = metadata;
            FCreateResourceFunc = createResourceFunc;
            FUpdateResourceFunc = updateResourceFunc;
            FDestroyResourceAction = destroyResourceAction ?? DestroyResource;
            NeedsUpdate = true;
        }

        public event EventHandler<TDevice> DeviceAdded;
        public event EventHandler<TDevice> DeviceRemoved;

        /// <summary>
        /// Some arbitrary data associated with this resource.
        /// </summary>
        public TMetadata Metadata { get; set; }

        /// <summary>
        /// The resources per device.
        /// </summary>
        public Dictionary<TDevice, TResource> Resources { get; set; } = new Dictionary<TDevice, TResource>();

        /// <summary>
        /// Whether or not the Update method has to be called for this resource.
        /// By default this flag is true.
        /// Note: The Update method will always get called for new resources.
        /// </summary>
        public bool NeedsUpdate { get; set; }
        public TResource this[TDevice device]
        {
            get
            {
                TResource result;
                if (!Resources.TryGetValue(device, out result))
                {
                    result = CreateResoure(Metadata, device);
                    NeedsUpdate = true;
                    Resources[device] = result;
                }
                return result;
            }
        }

        public IEnumerable<TResource> DeviceResources { get { return Resources.Values; } }

        protected virtual TResource CreateResoure(TMetadata metadata, TDevice device)
        {
            return FCreateResourceFunc?.Invoke(metadata, device);
        }
        
        public void UpdateResource(TDevice device)
        {
            TResource resource;
            if (Resources.TryGetValue(device, out resource))
            {
                if (NeedsUpdate)
                {
                    FUpdateResourceFunc?.Invoke(Metadata, resource);
                }
            }
            else
            {
                FUpdateResourceFunc?.Invoke(Metadata, this[device]);
                DeviceAdded?.Invoke(this, device);
            }
        }
        
        public void DestroyResource(TDevice device)
        {
            TResource resource;
            if (Resources.TryGetValue(device, out resource))
            {
                Resources.Remove(device);
                FDestroyResourceAction(Metadata, resource, DestroyReason.DeviceLost);
                DeviceRemoved?.Invoke(this, device);
            }
        }
        
        public void Dispose()
        {
            foreach (var resource in Resources.Values)
            {
                FDestroyResourceAction(Metadata, resource, DestroyReason.Dispose);
            }
            
            Resources.Clear();
            DeviceAdded = null;
            DeviceRemoved = null;
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
