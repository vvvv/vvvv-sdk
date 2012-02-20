using System;

namespace VVVV.Utils.Streams
{
    // TODO: This should not be public
    public class ManagedIOStream<T> : IIOStream<T>
    {
        public class StreamReader : IStreamReader<T>
        {
            private readonly ManagedIOStream<T> FStream;
            private readonly T[] FBuffer;
            private readonly int FLength;
            
            internal StreamReader(ManagedIOStream<T> stream)
            {
                FStream = stream;
                FBuffer = stream.FBuffer;
                FLength = stream.FLength;
            }
            
            public void Dispose()
            {
                
            }
            
            public bool Eos
            {
                get
                {
                    return Position >= FLength;
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
                    return FLength;
                }
            }
            
            public void Reset()
            {
                Position = 0;
            }
            
            public T Read(int stride = 1)
            {
                var result = FBuffer[Position];
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
                            buffer.Init(FBuffer[Position]); // Slightly faster
                        else
                            buffer.Fill(index, slicesToRead, FBuffer[Position]);
                        break;
                    case 1:
                        Array.Copy(FBuffer, Position, buffer, index, slicesToRead);
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
        
        public class StreamWriter : IStreamWriter<T>
        {
            private readonly ManagedIOStream<T> FStream;
            private readonly T[] FBuffer;
            private readonly int FLength;
            
            internal StreamWriter(ManagedIOStream<T> stream)
            {
                FStream = stream;
                FBuffer = stream.FBuffer;
                FLength = stream.FLength;
            }
            
            public void Dispose()
            {
                
            }
            
            public bool Eos
            {
                get
                {
                    return Position >= FLength;
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
                    return FLength;
                }
            }
            
            public void Reset()
            {
                Position = 0;
            }
            
            public void Write(T value, int stride = 1)
            {
                FStream.FChanged = true;
                FBuffer[Position] = value;
                Position += stride;
            }
            
            public int Write(T[] buffer, int index, int length, int stride = 1)
            {
                FStream.FChanged = true;
                
                int slicesToWrite = StreamUtils.GetNumSlicesAhead(this, index, length, stride);
                
                switch (stride)
                {
                    case 0:
                        FBuffer[Position] = buffer[index + slicesToWrite - 1];
                        break;
                    case 1:
                        Array.Copy(buffer, index, FBuffer, Position, slicesToWrite);
                        Position += slicesToWrite;
                        break;
                    default:
                        for (int i = index; i < index + slicesToWrite; i++)
                        {
                            FBuffer[Position] = buffer[i];
                            Position += stride;
                        }
                        break;
                }
                
                return slicesToWrite;
            }
        }
        
        private T[] FBuffer;
        private int FLength;
        private int FLowerThreshold;
        private int FUpperThreshold;
        protected bool FChanged;
        
        public ManagedIOStream()
        {
            FBuffer = new T[0];
            FChanged = true;
        }
        
        public virtual bool Sync()
        {
            // Nothing to do
            return true;
        }
        
        public virtual void Flush()
        {
            FChanged = false;
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
                    FChanged = true;
                }
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
            var stream = new ManagedIOStream<T>();
            stream.Length = Length;
            Array.Copy(FBuffer, stream.FBuffer, stream.FBuffer.Length);
            return stream;
        }
        
        private void ResizeInternalBuffer()
        {
            if (FLength > FUpperThreshold)
            {
                FUpperThreshold = StreamUtils.NextHigher(FLength);
                FLowerThreshold = FUpperThreshold / 2;

                var oldBuffer = FBuffer;
                FBuffer = new T[FUpperThreshold];

                BufferIncreased(oldBuffer, FBuffer);
            }
            else if (FLength < FLowerThreshold)
            {
                FUpperThreshold = FUpperThreshold / 2;
                FLowerThreshold = FUpperThreshold / 2;

                var oldBuffer = FBuffer;
                FBuffer = new T[FUpperThreshold];

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
