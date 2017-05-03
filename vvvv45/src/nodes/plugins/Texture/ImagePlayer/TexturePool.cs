using System;
using System.Collections.Concurrent;
using System.Linq;
using SlimDX.Direct3D9;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace VVVV.Nodes.ImagePlayer
{
    public class TexturePool : IDisposable
    {
        private readonly Dictionary<Device, Dictionary<int, Stack<Texture>>> FTexturePools = new Dictionary<Device, Dictionary<int, Stack<Texture>>>();
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
            Dictionary<int, Stack<Texture>> devicePool;
            if (!FTexturePools.TryGetValue(device, out devicePool))
            {
                devicePool = new Dictionary<int, Stack<Texture>>();
                FTexturePools[device] = devicePool;
                FRefCount[device] = 0;
            }

            int key = GetKey(width, height, levelCount, usage, format, pool);

            Stack<Texture> texturePool = null;
            if (!devicePool.TryGetValue(key, out texturePool))
            {
                texturePool = new Stack<Texture>(1);
                devicePool[key] = texturePool;
            }

            FRefCount[device]++;
            if (texturePool.Count > 0)
                return texturePool.Pop();
            else
                return new Texture(device, width, height, levelCount, usage, format, pool);
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
            texturePool.Push(texture);
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
                    while (texturePool.Count > 0)
                    {
                        var texture = texturePool.Pop();
                        texture.Dispose();
                    }
                }
                FTexturePools.Remove(device);
                FRefCount.Remove(device);
            }
            return refCount;
        }

        public void ReleaseUnused(int capacity)
        {
            foreach (var deviceTexturePool in FTexturePools.Values.ToArray())
            {
                foreach (var texturePool in deviceTexturePool.Values.ToArray())
                {
                    while (texturePool.Count > capacity)
                    {
                        var texture = texturePool.Pop();
                        Debug.WriteLine("Released texture {0}.", texture);
                        texture.Dispose();
                    }
                }
            }
        }
        
        public void Dispose()
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

        public bool HasTextureForDevice(Device device)
        {
            return FRefCount.ContainsKey(device);
        }

        public override string ToString()
        {
            var sb = new System.Text.StringBuilder();
            foreach (var device in FRefCount.Keys.ToArray())
            {
                sb.AppendLine(string.Format("Device {0}", device));
                sb.AppendLine(string.Format("  Unused: {0}, Used: {1}", FTexturePools[device].Values.Select(p => p.Count).Sum(), FRefCount[device]));
            }
            return sb.ToString();
        }
    }
}
