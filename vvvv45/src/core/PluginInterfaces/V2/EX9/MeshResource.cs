using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX.Direct3D9;

namespace VVVV.PluginInterfaces.V2.EX9
{
    public static class MeshResource
    {
        public static MeshResource<TMetadata> Create<TMetadata>(
            TMetadata metadata,
            Func<TMetadata, Device, Mesh> createResourceFunc,
            Action<TMetadata, Mesh> updateResourceFunc = null,
            Action<TMetadata, Mesh> destroyResourceAction = null)
        {
            return new MeshResource<TMetadata>(metadata, createResourceFunc, updateResourceFunc, destroyResourceAction);
        }
    }

    public class MeshResource<TMetadata> : DXResource<Mesh, TMetadata>
    {
        public MeshResource(
            TMetadata metadata, 
            Func<TMetadata, Device, Mesh> createResourceFunc,
            Action<TMetadata, Mesh> updateResourceFunc = null,
            Action<TMetadata, Mesh> destroyResourceAction = null)
            : base(metadata, createResourceFunc, updateResourceFunc, destroyResourceAction)
        {
        }
    }
}
