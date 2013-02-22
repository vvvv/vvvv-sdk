﻿using System;
using System.Runtime.InteropServices;

namespace VVVV.Utils.Streams
{
    /// <summary>
    /// Implemention of <see cref="IIOStream{T}"/> using an array as storage.
    /// Useful as wrapper if a stream is accessed randomly.
    /// </summary>
    [ComVisible(false)]
    public class BufferedIOStream<T> : IIOStream<T>
    {
        [ComVisible(false)]
        public class StreamReader : IStreamReader<T>
        {
            private readonly BufferedIOStream<T> FStream;
            
            internal StreamReader(BufferedIOStream<T> stream)
            {
                FStream = stream;
            }
            
            public void Dispose()
            {
                
            }
            
            public bool Eos
            {
                get
                {
                    return Position >= Length;
                }
            }
            
            public int Position
            {
                get;
                set;
            }
            
            public int Length
            {
                get
                {
                    return FStream.FLength;
                }
            }
            
            public void Reset()
            {
                Position = 0;
            }
            
            public T Read(int stride = 1)
            {
                var result = FStream.FBuffer[Position];
                Position += stride;
                return result;
            }
            
            public int Read(T[] buffer, int index, int length, int stride = 1)
            {
                int slicesToRead = StreamUtils.GetNumSlicesAhead(this, index, length, stride);
                
                switch (stride)
                {
                    case 0:
                        if (index == 0 && slicesToRead == buffer.Length)
                            buffer.Init(FStream.FBuffer[Position]); // Slightly faster
                        else
                            buffer.Fill(index, slicesToRead, FStream.FBuffer[Position]);
                        break;
                    case 1:
                        Array.Copy(FStream.FBuffer, Position, buffer, index, slicesToRead);
                        Position += slicesToRead * stride;
                        break;
                    default:
                        for (int i = index; i < index + slicesToRead; i++)
                        {
                            buffer[i] = Read(stride);
                        }
                        break;
                }
                
                return slicesToRead;
            }
            
            public T Current
            {
                get;
                private set;
            }
            
            object System.Collections.IEnumerator.Current
            {
                get
                {
                    return Current;
                }
            }
            
            public bool MoveNext()
            {
                var result = !Eos;
                if (result)
                {
                    Current = Read();
                }
                return result;
            }
        }
        
        [ComVisible(false)]
        public class StreamWriter : IStreamWriter<T>
        {
            private readonly BufferedIOStream<T> FStream;
            
            internal StreamWriter(BufferedIOStream<T> stream)
            {
                FStream = stream;
            }
            
            public void Dispose()
            {
                
            }
            
            public bool Eos
            {
                get
                {
                    return Position >= Length;
                }
            }
            
            public int Position
            {
                get;
                set;
            }
            
            public int Length
            {
                get
                {
                    return FStream.FLength;
                }
            }
            
            public void Reset()
            {
                Position = 0;
            }
            
            public void Write(T value, int stride = 1)
            {
                // Allow the stream to expand.
                if (Eos) 
                    FStream.Length = Position + 1;
                FStream.IsChanged = true;
                FStream.FBuffer[Position] = value;
                Position += stride;
            }
            
            public int Write(T[] buffer, int index, int length, int stride = 1)
            {
                FStream.IsChanged = true;

                int slicesToWrite = StreamUtils.GetNumSlicesAhead(this, index, length, stride);
                
                switch (stride)
                {
                    case 0:
                        FStream.FBuffer[Position] = buffer[index + slicesToWrite - 1];
                        break;
                    case 1:
                        Array.Copy(buffer, index, FStream.FBuffer, Position, slicesToWrite);
                        Position += slicesToWrite;
                        break;
                    default:
                        for (int i = index; i < index + slicesToWrite; i++)
                        {
                            FStream.FBuffer[Position] = buffer[i];
                            Position += stride;
                        }
                        break;
                }
                
                return slicesToWrite;
            }
        }
        
        private T[] FBuffer;
        private int FLength;
        private int FCapacity;
        protected int FChangeCount;
        
        public BufferedIOStream(int initialCapacity = 0)
        {
            FCapacity = initialCapacity;
            FBuffer = new T[initialCapacity];
            IsChanged = true;
        }

        public BufferedIOStream(T[] buffer)
        {
            FCapacity = buffer.Length;
            FBuffer = buffer;
            FLength = buffer.Length;
            IsChanged = true;
        }
        
        public virtual bool Sync()
        {
            return IsChanged;
        }

        public virtual void Flush(bool force = false)
        {
            FChangeCount = 0;
        }

        public T[] Buffer { get { return FBuffer; } }
        
        public bool IsChanged
        {
            get { return FChangeCount > 0; }
            set { if (value) FChangeCount++; else FChangeCount = 0; }
        }
        
        public int Length
        {
            get
            {
                return FLength;
            }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException();
                }
                
                if (value != FLength)
                {
                    FLength = value;
                    ResizeInternalBuffer();
                    IsChanged = true;
                }
            }
        }
        
        public T this[int index]
        {
            get
            {
                return FBuffer[index];
            }
            set
            {
                IsChanged = true;
                FBuffer[index] = value;
            }
        }
        
        public StreamReader GetReader()
        {
            return new StreamReader(this);
        }
        
        public StreamWriter GetWriter()
        {
            return new StreamWriter(this);
        }
        
        public object Clone()
        {
            var stream = new BufferedIOStream<T>(FCapacity);
            stream.Length = Length;
            Array.Copy(FBuffer, stream.FBuffer, stream.FBuffer.Length);
            return stream;
        }
        
        private void ResizeInternalBuffer()
        {
            if (FLength > FCapacity)
            {
                FCapacity = StreamUtils.NextHigher(FLength);

                var oldBuffer = FBuffer;
                FBuffer = new T[FCapacity];

                BufferIncreased(oldBuffer, FBuffer);
            }
            else if (FLength < FCapacity / 2)
            {
                FCapacity = FCapacity / 2;

                var oldBuffer = FBuffer;
                FBuffer = new T[FCapacity];

                BufferDecreased(oldBuffer, FBuffer);
            }
        }
        
        protected virtual void BufferIncreased(T[] oldBuffer, T[] newBuffer)
        {
            Array.Copy(oldBuffer, newBuffer, oldBuffer.Length);
            if (oldBuffer.Length > 0)
            {
                for (int i = oldBuffer.Length; i < newBuffer.Length; i++)
                    newBuffer[i] = oldBuffer[i % oldBuffer.Length];
            }
        }
        
        protected virtual void BufferDecreased(T[] oldBuffer, T[] newBuffer)
        {
            Array.Copy(oldBuffer, newBuffer, newBuffer.Length);
        }
        
        IStreamReader<T> IInStream<T>.GetReader()
        {
            return GetReader();
        }
        
        IStreamWriter<T> IOutStream<T>.GetWriter()
        {
            return GetWriter();
        }
        
        public StreamReader GetEnumerator()
        {
            return GetReader();
        }
        
        System.Collections.Generic.IEnumerator<T> System.Collections.Generic.IEnumerable<T>.GetEnumerator()
        {
            return GetEnumerator();
        }
        
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
