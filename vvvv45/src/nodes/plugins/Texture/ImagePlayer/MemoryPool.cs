using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;
using System.Linq;
using System.Diagnostics;

namespace VVVV.Nodes.ImagePlayer
{
    abstract class MemoryPool<T> : IDisposable
    {
        private readonly Dictionary<int, Stack<T>> FMemoryPools = new Dictionary<int, Stack<T>>();
        private long FHandedOutBytes;

        protected abstract T Allocate(int length);
        protected abstract void Free(T mem);
        
        public T GetMemory(int length)
        {
            lock (this)
            {
                Stack<T> memoryPool = null;
                if (!FMemoryPools.TryGetValue(length, out memoryPool))
                {
                    memoryPool = new Stack<T>(1);
                    FMemoryPools[length] = memoryPool;
                }
                FHandedOutBytes += length;
                if (memoryPool.Count > 0)
                    return memoryPool.Pop();
                else
                    return Allocate(length);
            }
        }
        
        public void PutMemory(T mem, int length)
        {
            lock (this)
            {
                var memoryPool = FMemoryPools[length];
                FHandedOutBytes -= length;
                memoryPool.Push(mem);
            }
        }

        public void ReleaseUnused(int capacity)
        {
            lock (this)
            {
                foreach (var memoryPool in FMemoryPools.Values)
                {
                    while (memoryPool.Count > capacity)
                    {
                        var mem = memoryPool.Pop();
                        Free(mem);
                    }
                }
            }
        }
        
        public void Dispose()
        {
            lock (this)
            {
                ReleaseUnused(0);
                FMemoryPools.Clear();
            }
        }

        public override string ToString()
        {
            return string.Format("Unused: {0}, Used: {1}", FMemoryPools.Keys.Select(length => FMemoryPools[length].Count * length).Sum(), FHandedOutBytes);
        }
    }

    class MemoryPool : IDisposable
    {
        public MemoryPool()
        {
            ManagedPool = new ManagedMemoryPool();
            UnmanagedPool = new UnmanagedMemoryPool();
            StreamPool = new UnmanagedStreamPool(UnmanagedPool);
        }

        public ManagedMemoryPool ManagedPool
        {
            get;
            private set;
        }

        public UnmanagedMemoryPool UnmanagedPool
        {
            get;
            private set;
        }

        public UnmanagedStreamPool StreamPool
        {
            get;
            private set;
        }

        public void ReleaseUnused(int capacity)
        {
            ManagedPool.ReleaseUnused(capacity);
            UnmanagedPool.ReleaseUnused(capacity);
        }

        public void Dispose()
        {
            ManagedPool.Dispose();
            UnmanagedPool.Dispose();
        }
    }

    class ManagedMemoryPool : MemoryPool<byte[]>
    {
        protected override byte[] Allocate(int length)
        {
            return new byte[length];
        }

        protected override void Free(byte[] mem)
        {
            Debug.WriteLine("ManagedMemoryPool: released {0} bytes.", mem.Length);
        }

        public void PutMemory(byte[] array)
        {
            base.PutMemory(array, array.Length);
        }
    }

    class UnmanagedMemoryPool : MemoryPool<SharpDX.DataPointer>
    {
        protected override SharpDX.DataPointer Allocate(int length)
        {
            return new SharpDX.DataPointer(Marshal.AllocHGlobal(length), length);
        }

        protected override void Free(SharpDX.DataPointer mem)
        {
            Marshal.FreeHGlobal(mem.Pointer);
            Debug.WriteLine("UnmanagedMemoryPool: released {0} bytes.", mem.Size);
        }

        public void PutMemory(SharpDX.DataPointer dp)
        {
            base.PutMemory(dp, dp.Size);
        }
    }

    class UnmanagedStreamPool
    {
        private readonly UnmanagedMemoryPool FUnmanagedMemoryPool;

        public UnmanagedStreamPool(UnmanagedMemoryPool unmanagedMemoryPool)
        {
            FUnmanagedMemoryPool = unmanagedMemoryPool;
        }

        public unsafe Stream GetStream(int length)
        {
            var capacity = VVVV.Utils.Streams.StreamUtils.NextHigher(length);
            var memory = FUnmanagedMemoryPool.GetMemory(capacity);
            var stream = new SharpDX.DataStream(memory.Pointer, length, true, true);
            return stream;
        }

        public unsafe void PutStream(Stream stream)
        {
            var unmanagedStream = stream as SharpDX.DataStream;
            var memory = new SharpDX.DataPointer(unmanagedStream.DataPointer, VVVV.Utils.Streams.StreamUtils.NextHigher((int)unmanagedStream.Length));
            unmanagedStream.Dispose();
            FUnmanagedMemoryPool.PutMemory(memory);
        }
    }

    class ManagedStreamPool
    {
        private readonly ManagedMemoryPool FMemoryPool;

        public ManagedStreamPool(ManagedMemoryPool memoryPool)
        {
            FMemoryPool = memoryPool;
        }

        public unsafe Stream GetStream(int length)
        {
            var capacity = VVVV.Utils.Streams.StreamUtils.NextHigher(length);
            var memory = FMemoryPool.GetMemory(capacity);
            var stream = new MemoryStream(memory, 0, length, true, true);
            return stream;
        }

        public unsafe void PutStream(Stream stream)
        {
            var memoryStream = stream as MemoryStream;
            FMemoryPool.PutMemory(memoryStream.GetBuffer());
            memoryStream.Dispose();
        }
    }
}
