using System;
using System.Collections.Concurrent;

namespace VVVV.Nodes.ImagePlayer
{
	static class MemoryPool
	{
	    private static readonly ConcurrentDictionary<int, ObjectPool<byte[]>> FMemoryPools = new ConcurrentDictionary<int, ObjectPool<byte[]>>();
		
	    public static byte[] GetMemory(int length)
		{
	        ObjectPool<byte[]> memoryPool = null;
			if (!FMemoryPools.TryGetValue(length, out memoryPool))
			{
			    memoryPool = new ObjectPool<byte[]>(() => new byte[length]);
				FMemoryPools[length] = memoryPool;
			}
			
			return memoryPool.GetObject();
		}
		
	    public static void PutMemory(byte[] array)
		{
			var memoryPool = FMemoryPools[array.Length];
			memoryPool.PutObject(array);
		}
	}
}
