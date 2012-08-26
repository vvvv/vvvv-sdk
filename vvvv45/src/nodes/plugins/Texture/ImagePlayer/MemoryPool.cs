using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;
using System.Linq;

namespace VVVV.Nodes.ImagePlayer
{
    class MemoryPool<T> : IDisposable
    {
        private readonly Dictionary<int, ObjectPool<T>> FMemoryPools = new Dictionary<int, ObjectPool<T>>();
        private readonly Func<int, T> FConstructor;
        private Action<T> FDestructor;
        private int FHandedOutBytes;

        public MemoryPool(Func<int, T> constructor, Action<T> destructor)
        {
            FConstructor = constructor;
            FDestructor = destructor;
        }
        
        public T GetMemory(int length)
        {
            lock (this)
            {
                ObjectPool<T> memoryPool = null;
                if (!FMemoryPools.TryGetValue(length, out memoryPool))
                {
                    memoryPool = new ObjectPool<T>(() => FConstructor(length));
                    FMemoryPools[length] = memoryPool;
                }
                FHandedOutBytes += length;
                return memoryPool.GetObject();
            }
        }
        
        public void PutMemory(T mem, int length)
        {
            lock (this)
            {
                var memoryPool = FMemoryPools[length];
                FHandedOutBytes -= length;
                memoryPool.PutObject(mem);
            }
        }

        public void Clear()
        {
            lock (this)
            {
                foreach (var memoryPool in FMemoryPools.Values)
                {
                    foreach (var mem in memoryPool.ToArrayAndClear())
                    {
                        FDestructor(mem);
                    }
                }
            }
        }
        
        public void Dispose()
        {
            lock (this)
            {
                Clear();
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

        public void Clear()
        {
            ManagedPool.Clear();
            UnmanagedPool.Clear();
        }

        public void Dispose()
        {
            ManagedPool.Dispose();
            UnmanagedPool.Dispose();
        }
    }

    class ManagedMemoryPool : MemoryPool<byte[]>
    {
        public ManagedMemoryPool()
            : base((length) => new byte[length], (m) => { })
        {

        }

        public void PutMemory(byte[] array)
        {
            base.PutMemory(array, array.Length);
        }
    }

    class UnmanagedMemoryPool : MemoryPool<SharpDX.DataPointer>
    {
        public UnmanagedMemoryPool()
            : base((length) => new SharpDX.DataPointer(SharpDX.Utilities.AllocateMemory(length), length), (m) => SharpDX.Utilities.FreeMemory(m.Pointer))
        {

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
