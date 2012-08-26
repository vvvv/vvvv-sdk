using System;
using System.Collections.Concurrent;
using System.Linq;
using SlimDX.Direct3D9;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace VVVV.Nodes.ImagePlayer
{
    public class TexturePool : IDisposable
    {
        private readonly Dictionary<Device, Dictionary<int, ObjectPool<Texture>>> FTexturePools = new Dictionary<Device, Dictionary<int, ObjectPool<Texture>>>();
        private readonly Dictionary<Device, int> FRefCount = new Dictionary<Device, int>();
        
        public Texture GetTexture(
            Device device,
            int width,
            int height,
            int levelCount,
            Usage usage,
            Format format,
            Pool pool)
        {
            Dictionary<int, ObjectPool<Texture>> devicePool;
            if (!FTexturePools.TryGetValue(device, out devicePool))
            {
                devicePool = new Dictionary<int, ObjectPool<Texture>>();
                FTexturePools[device] = devicePool;
                FRefCount[device] = 0;
            }

            int key = GetKey(width, height, levelCount, usage, format, pool);
            
            ObjectPool<Texture> texturePool = null;
            if (!devicePool.TryGetValue(key, out texturePool))
            {
                texturePool = new ObjectPool<Texture>(() => new Texture(device, width, height, levelCount, usage, format, pool));
                devicePool[key] = texturePool;
            }

            FRefCount[device]++;
            return texturePool.GetObject();
        }
        
        public void PutTexture(Texture texture)
        {
            var desc = texture.GetLevelDescription(0);
            int key = GetKey(
                desc.Width,
                desc.Height,
                texture.LevelCount,
                desc.Usage,
                desc.Format,
                desc.Pool);

            var devicePool = FTexturePools[texture.Device];
            var texturePool = devicePool[key];
            FRefCount[texture.Device]--;
            texturePool.PutObject(texture);
        }
        
        private int GetKey(
            int width,
            int height,
            int levelCount,
            Usage usage,
            Format format,
            Pool pool)
        {
            unchecked
            {
                var hash = 17;
                hash = 23 * hash + width.GetHashCode();
                hash = 23 * hash + height.GetHashCode();
                hash = 23 * hash + levelCount.GetHashCode();
                hash = 23 * hash + usage.GetHashCode();
                hash = 23 * hash + format.GetHashCode();
                hash = 23 * hash + pool.GetHashCode();
                return hash;
            }
        }
        
        public int Release(Device device)
        {
            var refCount = FRefCount[device];
            if (refCount == 0)
            {
                var devicePool = FTexturePools[device];
                foreach (var texturePool in devicePool.Values)
                {
                    foreach (var texture in texturePool.ToArrayAndClear())
                    {
                        texture.Dispose();
                    }
                }
                FTexturePools.Remove(device);
                FRefCount.Remove(device);
            }
            return refCount;
        }

        public void Clear()
        {
            foreach (var device in FTexturePools.Keys.ToArray())
            {
                var refCount = Release(device);
                if (refCount > 0)
                {
                    throw new Exception(string.Format("Couldn't clear texture pool. There're still {0} textures in use for device {1}.", refCount, device));
                }
            }
            FTexturePools.Clear();
        }
        
        public void Dispose()
        {
            Clear();
        }
    }
}
