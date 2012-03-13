using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace VVVV.Nodes.ImagePlayer
{
    class MemoryPool : IDisposable
    {
        private readonly Dictionary<int, ObjectPool<byte[]>> FMemoryPools = new Dictionary<int, ObjectPool<byte[]>>();
        
        public byte[] GetMemory(int length)
        {
            lock (FMemoryPools)
            {
                ObjectPool<byte[]> memoryPool = null;
                if (!FMemoryPools.TryGetValue(length, out memoryPool))
                {
                    memoryPool = new ObjectPool<byte[]>(() => new byte[length]);
                    FMemoryPools[length] = memoryPool;
                }
                return memoryPool.GetObject();
            }
        }
        
        public void PutMemory(byte[] array)
        {
            lock (FMemoryPools)
            {
                var memoryPool = FMemoryPools[array.Length];
                memoryPool.PutObject(array);
            }
        }
        
        public void Dispose()
        {
            lock (FMemoryPools)
            {
                foreach (var memoryPool in FMemoryPools.Values)
                {
                    memoryPool.ToArrayAndClear();
                }
                FMemoryPools.Clear();
            }
        }
    }
}
