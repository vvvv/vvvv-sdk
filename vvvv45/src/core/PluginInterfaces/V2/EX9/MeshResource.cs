using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX.Direct3D9;

namespace VVVV.PluginInterfaces.V2.EX9
{
    public static class MeshResource
    {
        /// <summary>
        /// Creates a new <see cref="MeshResource{TMetadata}">mesh resource</see>.
        /// </summary>
        /// <typeparam name="TMetadata">The type of the metadata.</typeparam>
        /// <param name="metadata">The metadata associated with the mesh resource.</param>
        /// <param name="createResourceFunc">A function which creates the mesh.</param>
        /// <param name="updateResourceFunc">A function which updates the mesh.</param>
        /// <param name="destroyResourceAction">A function which destroys the mesh.</param>
        /// <returns>The newly created <see cref="MeshResource{TMetadata}">mesh resource</see>.</returns>
        public static MeshResource<TMetadata> Create<TMetadata>(
            TMetadata metadata,
            Func<TMetadata, Device, Mesh> createResourceFunc,
            Action<TMetadata, Mesh> updateResourceFunc = null,
            Action<TMetadata, Mesh> destroyResourceAction = null)
        {
            return new MeshResource<TMetadata>(metadata, createResourceFunc, updateResourceFunc, destroyResourceAction);
        }
    }

    /// <summary>
    /// A resource container for meshes.
    /// </summary>
    /// <typeparam name="TMetadata">The type of the metadata.</typeparam>
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
