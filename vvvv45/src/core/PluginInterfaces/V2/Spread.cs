﻿using System;
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
        private readonly BufferedIOStream<T> FStream;
        
        public Spread(BufferedIOStream<T> stream)
        {
            FStream = stream;
        }
        
        public Spread(int size)
            : this(new BufferedIOStream<T>(size))
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
        
        public BufferedIOStream<T> Stream
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
                return FStream[VMath.Zmod(index, FStream.Length)];
            }
            set
            {
                FStream[VMath.Zmod(index, FStream.Length)] = value;
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
        
        public BufferedIOStream<T>.StreamReader GetEnumerator()
        {
            return FStream.GetEnumerator();
        }
        
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
            return new Spread<T>(FStream.Clone() as BufferedIOStream<T>);
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