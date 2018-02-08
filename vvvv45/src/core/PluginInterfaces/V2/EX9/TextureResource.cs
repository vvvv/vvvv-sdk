using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX.Direct3D9;

namespace VVVV.PluginInterfaces.V2.EX9
{
    /// <summary>
    /// A factory to create <see cref="TextureResource{TMetadata}">texture resources</see>.
    /// </summary>
    public static class TextureResource
    {
        /// <summary>
        /// Creates a new <see cref="TextureResource{TMetadata}">texture resource</see>.
        /// </summary>
        /// <typeparam name="TMetadata">The type of the metadata.</typeparam>
        /// <param name="metadata">The metadata associated with the texture resource.</param>
        /// <param name="createResourceFunc">A function which creates the texture.</param>
        /// <param name="updateResourceFunc">A function which updates the texture.</param>
        /// <param name="destroyResourceAction">A function which destroys the texture.</param>
        /// <returns>The newly created <see cref="TextureResource{TMetadata}">texture resource</see>.</returns>
        public static TextureResource<TMetadata> Create<TMetadata>(
            TMetadata metadata,
            Func<TMetadata, Device, Texture> createResourceFunc,
            Action<TMetadata, Texture> updateResourceFunc = null,
            Action<TMetadata, Texture> destroyResourceAction = null)
        {
            if (destroyResourceAction != null)
                return new TextureResource<TMetadata>(metadata, createResourceFunc, updateResourceFunc, (m, texture, reason) => destroyResourceAction(m, texture));
            else
                return new TextureResource<TMetadata>(metadata, createResourceFunc, updateResourceFunc);
        }

        /// <summary>
        /// Creates a new <see cref="TextureResource{TMetadata}">texture resource</see>.
        /// </summary>
        /// <typeparam name="TMetadata">The type of the metadata.</typeparam>
        /// <param name="metadata">The metadata associated with the texture resource.</param>
        /// <param name="createResourceFunc">A function which creates the texture.</param>
        /// <param name="updateResourceFunc">A function which updates the texture.</param>
        /// <param name="destroyResourceAction">A function which destroys the texture.</param>
        /// <returns>The newly created <see cref="TextureResource{TMetadata}">texture resource</see>.</returns>
        public static TextureResource<TMetadata> Create<TMetadata>(
            TMetadata metadata,
            Func<TMetadata, Device, Texture> createResourceFunc,
            Action<TMetadata, Texture> updateResourceFunc,
            Action<TMetadata, Texture, DestroyReason> destroyResourceAction)
        {
            return new TextureResource<TMetadata>(metadata, createResourceFunc, updateResourceFunc, destroyResourceAction);
        }
    }

    /// <summary>
    /// A resource container for textures.
    /// </summary>
    /// <typeparam name="TMetadata">The type of the metadata.</typeparam>
    public class TextureResource<TMetadata> : DXResource<Texture, TMetadata>
    {
        public TextureResource(
            TMetadata metadata, 
            Func<TMetadata, Device, Texture> createResourceFunc = null,
            Action<TMetadata, Texture> updateResourceFunc = null,
            Action<TMetadata, Texture, DestroyReason> destroyResourceAction = null)
            : base(metadata, createResourceFunc, updateResourceFunc, destroyResourceAction)
        {
        }
    }
}
