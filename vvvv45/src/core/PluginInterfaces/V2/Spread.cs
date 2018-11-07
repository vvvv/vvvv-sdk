using System;
using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using VVVV.Utils.Streams;
using VVVV.Utils.VMath;
using VVVV.PluginInterfaces.V2.NonGeneric;

namespace VVVV.PluginInterfaces.V2
{
    /// <summary>
    /// The spread data type
    /// </summary>
    [ComVisible(false)]
    public class Spread<T> : ISpread<T>
    {
        private readonly MemoryIOStream<T> FStream;
        
        public Spread(MemoryIOStream<T> stream)
        {
            FStream = stream;
        }
        
        public Spread(int size)
            : this(new MemoryIOStream<T>(size))
        {
            SliceCount = size;
        }
        
        public Spread()
            : this(0)
        {
        }
        
        public Spread(IList<T> original)
            : this(original.Count)
        {
            var buffer = new T[original.Count];
            original.CopyTo(buffer, 0);
            using (var writer = FStream.GetWriter())
            {
                writer.Write(buffer, 0, buffer.Length);
            }
        }
        
        public virtual bool Sync()
        {
            return FStream.Sync();
        }
        
        public bool IsChanged
        {
            get
            {
                return FStream.IsChanged;
            }
        }

        public virtual void Flush(bool force = false)
        {
            FStream.Flush(force);
        }
        
        public MemoryIOStream<T> Stream
        {
            get
            {
                return FStream;
            }
        }
        
        public T this[int index]
        {
            get
            {
                var length = FStream.Length;
                if (length > 0)
                    return FStream[VMath.Zmod(index, length)];
                else
                    throw new IndexOutOfRangeException("The index was outside of the bounds of the spread.");
            }
            set
            {
                var length = FStream.Length;
                if (length > 0)
                    FStream[VMath.Zmod(index, length)] = value;
                else
                    throw new IndexOutOfRangeException("The index was outside of the bounds of the spread.");
            }
        }
        
        object ISpread.this[int index]
        {
            get
            {
                return this[index];
            }
            set
            {
                this[index] = (T) value;
            }
        }
        
        public int SliceCount
        {
            get
            {
                return FStream.Length;
            }
            set
            {
                if (value != FStream.Length)
                {
                    int oldSliceCount = FStream.Length;
                    int newSliceCount = value >= 0 ? value : 0;
                    
                    if (newSliceCount < oldSliceCount)
                        SliceCountDecreasing(oldSliceCount, newSliceCount);
                    
                    FStream.Length = newSliceCount;
                    
                    if (newSliceCount > oldSliceCount)
                        SliceCountIncreased(oldSliceCount, newSliceCount);
                }
            }
        }
        
        protected virtual void SliceCountIncreased(int oldSliceCount, int newSliceCount)
        {

        }
        
        protected virtual void SliceCountDecreasing(int oldSliceCount, int newSliceCount)
        {

        }
        
        public MemoryIOStream<T>.StreamReader GetEnumerator()
        {
            return FStream.GetEnumerator();
        }

        int IReadOnlyCollection<T>.Count => SliceCount;
        
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return GetEnumerator();
        }
        
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        
        public Spread<T> Clone()
        {
            return new Spread<T>(FStream.Clone() as MemoryIOStream<T>);
        }
        
        ISpread<T> ISpread<T>.Clone()
        {
            return Clone();
        }
        
        object ICloneable.Clone()
        {
            return Clone();
        }
    }
}