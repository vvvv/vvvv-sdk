using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX.Direct3D9;

namespace VVVV.PluginInterfaces.V2.EX9
{
    public static class TextureResource
    {
        public static TextureResource<TMetadata> Create<TMetadata>(
            TMetadata metadata,
            Func<TMetadata, Device, Texture> createResourceFunc,
            Action<TMetadata, Texture> updateResourceFunc = null,
            Action<TMetadata, Texture> destroyResourceAction = null)
        {
            return new TextureResource<TMetadata>(metadata, createResourceFunc, updateResourceFunc, destroyResourceAction);
        }
    }

    public class TextureResource<TMetadata> : DXResource<Texture, TMetadata>
    {
        public TextureResource(
            TMetadata metadata, 
            Func<TMetadata, Device, Texture> createResourceFunc,
            Action<TMetadata, Texture> updateResourceFunc = null,
            Action<TMetadata, Texture> destroyResourceAction = null)
            : base(metadata, createResourceFunc, updateResourceFunc, destroyResourceAction)
        {
        }
    }
}
