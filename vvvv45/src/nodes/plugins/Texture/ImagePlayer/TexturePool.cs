using System;
using System.Collections.Concurrent;
using System.Linq;
using SlimDX.Direct3D9;

namespace VVVV.Nodes.ImagePlayer
{
    public class TexturePool : IDisposable
    {
        private readonly ConcurrentDictionary<int, ObjectPool<Texture>> FTexturePools = new ConcurrentDictionary<int, ObjectPool<Texture>>();
        private int FTextureCount;
        
        public Texture GetTexture(
            Device device,
            int width,
            int height,
            int levelCount,
            Usage usage,
            Format format,
            Pool pool)
        {
            int key = GetKey(device, width, height, levelCount, usage, format, pool);
            
            ObjectPool<Texture> texturePool = null;
            if (!FTexturePools.TryGetValue(key, out texturePool))
            {
                texturePool = new ObjectPool<Texture>(() => new Texture(device, width, height, levelCount, usage, format, pool));
                FTexturePools[key] = texturePool;
            }
            
            FTextureCount++;
            
            return texturePool.GetObject();
        }
        
        public void PutTexture(Texture texture)
        {
            var desc = texture.GetLevelDescription(0);
            int key = GetKey(
                texture.Device,
                desc.Width,
                desc.Height,
                texture.LevelCount,
                desc.Usage,
                desc.Format,
                desc.Pool);
            
            FTextureCount--;
            
            var texturePool = FTexturePools[key];
            texturePool.PutObject(texture);
        }
        
        private int GetKey(
            Device device,
            int width,
            int height,
            int levelCount,
            Usage usage,
            Format format,
            Pool pool)
        {
            return
                device.GetHashCode() ^
                width.GetHashCode() ^
                height.GetHashCode() ^
                levelCount.GetHashCode() ^
                usage.GetHashCode() ^
                format.GetHashCode() ^
                pool.GetHashCode();
        }
        
        public void TryToCleanup()
        {
            if (FTextureCount == 0)
            {
                foreach (var key in FTexturePools.Keys.ToArray())
                {
                    ObjectPool<Texture> texturePool = null;
                    if (FTexturePools.TryRemove(key, out texturePool))
                    {
                        foreach (var texture in texturePool.ToArrayAndClear())
                        {
                            texture.Dispose();
                        }
                    }
                }
            }
        }
        
        public void Dispose()
        {
            TryToCleanup();
        }
    }
}
