using System;
using System.Collections.Concurrent;

namespace VVVV.Nodes.ImagePlayer
{
    class MemoryPool : IDisposable
	{
	    private readonly ConcurrentDictionary<int, ObjectPool<byte[]>> FMemoryPools = new ConcurrentDictionary<int, ObjectPool<byte[]>>();
		
	    public byte[] GetMemory(int length)
		{
	        ObjectPool<byte[]> memoryPool = null;
			if (!FMemoryPools.TryGetValue(length, out memoryPool))
			{
			    memoryPool = new ObjectPool<byte[]>(() => new byte[length]);
				FMemoryPools[length] = memoryPool;
			}
			
			return memoryPool.GetObject();
		}
		
	    public void PutMemory(byte[] array)
		{
			var memoryPool = FMemoryPools[array.Length];
			memoryPool.PutObject(array);
		}
        
        public void Dispose()
        {
            foreach (var memoryPool in FMemoryPools.Values)
            {
                memoryPool.ToArrayAndClear();
            }
            FMemoryPools.Clear();
        }
	}
}
