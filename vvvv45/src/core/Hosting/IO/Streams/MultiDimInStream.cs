using System;
using System.Diagnostics;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils;
using VVVV.Utils.Streams;

namespace VVVV.Hosting.IO.Streams
{
    class MultiDimInStream<T> : IInStream<IInStream<T>>, IDisposable
    {
        class MultiDimReader : IStreamReader<IInStream<T>>
        {
            private readonly MultiDimInStream<T> FStream;
            private readonly IStreamReader<int> FBinSizeReader;
            private int FPosition;
            private int FCurrentOffset;
            
            public MultiDimReader(MultiDimInStream<T> stream)
            {
                FStream = stream;
                FBinSizeReader = stream.FNormBinSizeStream.GetCyclicReader();
                Length = stream.Length;
            }
            
            public bool Eos
            {
                get
                {
                    return FPosition >= Length;
                }
            }
            
            public int Position
            {
                get
                {
                    return FPosition;
                }
                set
                {
                    // Compute offset
                    // TODO: Find faster solution.
                    FBinSizeReader.Position = FPosition;
                    for (int i = FPosition; i < value; i++)
                    {
                        FCurrentOffset += FBinSizeReader.Read();
                    }
                    for (int i = FPosition; i > value; i--)
                    {
                        FCurrentOffset -= FBinSizeReader.Read();
                    }
                    FBinSizeReader.Position = value;
                    
                    FPosition = value;
                }
            }
            
            public int Length
            {
                get;
                private set;
            }
            
            public IInStream<T> Current
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
            
            public IInStream<T> Read(int stride = 1)
            {
                if (Eos) throw new InvalidOperationException("Stream is EOS.");
                
                int offset = FCurrentOffset;
                int length = FBinSizeReader.Read(0);
                Position += stride;
                
                return new InnerStream(FStream.FDataStream, offset, length);
            }
            
            public int Read(IInStream<T>[] buffer, int index, int length, int stride)
            {
                // TODO: Is there a faster solution?
                return StreamUtils.Read(this, buffer, index, length, stride);
            }

            public void Dispose()
            {
                FBinSizeReader.Dispose();
            }
            
            public void Reset()
            {
                Position = 0;
            }
        }
        
        class InnerStream : IInStream<T>
        {
            class InnerStreamReader : IStreamReader<T>
            {
                private readonly InnerStream FStream;
                private readonly CyclicStreamReader<T> FDataReader;
                
                public InnerStreamReader(InnerStream stream)
                {
                    FStream = stream;
                    FDataReader = stream.FDataStream.GetCyclicReader();
                    Reset();
                }
                
                public bool Eos
                {
                    get
                    {
                        return Position >= Length;
                    }
                }

                private int FPosition;
                public int Position
                {
                    get { return FPosition; }
                    set
                    {
                        FPosition = value;
                        FDataReader.Position = FStream.FOffset + value;
                    }
                }
                
                public int Length
                {
                    get { return FStream.Length; }
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
                
                public T Read(int stride = 1)
                {
                    var value = FDataReader.Length > 0 ? FDataReader.Read(stride) : default(T);
                    FPosition += stride;
                    return value;
                }
                
                public int Read(T[] buffer, int index, int length, int stride)
                {
                    int numSlicesToRead = StreamUtils.GetNumSlicesAhead(this, index, length, stride);
                    if (FDataReader.Length > 0)
                        FDataReader.Read(buffer, index, numSlicesToRead, stride);
                    else
                    {
                        buffer.Fill(index, numSlicesToRead, default(T));
                    }
                    FPosition += numSlicesToRead * stride;
                    return numSlicesToRead;
                }
                
                public void Dispose()
                {
                    FDataReader.Dispose();
                }
                
                public void Reset()
                {
                    Position = 0;
                }
            }
            
            private readonly IInStream<T> FDataStream;
            private readonly int FOffset;
            
            public InnerStream(IInStream<T> dataStream, int offset, int length)
            {
                FDataStream = dataStream;
                FOffset = offset;
                Length = length;
            }
            
            public int Length
            {
                get;
                private set;
            }
            
            public bool Sync()
            {
                return IsChanged;
            }
            
            public bool IsChanged
            {
                get
                {
                    return FDataStream.IsChanged;
                }
            }
            
            public object Clone()
            {
                throw new NotImplementedException();
            }
            
            public IStreamReader<T> GetReader()
            {
                return new InnerStreamReader(this);
            }
            
            public System.Collections.Generic.IEnumerator<T> GetEnumerator()
            {
                return GetReader();
            }
            
            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
        
        private readonly IIOContainer<IInStream<T>> FDataContainer;
        private readonly IIOContainer<IInStream<int>> FBinSizeContainer;
        private readonly IInStream<T> FDataStream;
        private readonly IInStream<int> FBinSizeStream;
        private readonly BufferedIOStream<int> FNormBinSizeStream;
        private readonly int[] FBinSizeBuffer;
        
        public MultiDimInStream(IIOFactory factory, InputAttribute attribute)
        {
            FDataContainer = factory.CreateIOContainer<IInStream<T>>(attribute, false);
            FBinSizeContainer = factory.CreateIOContainer<IInStream<int>>(attribute.GetBinSizeInputAttribute(), false);
            FDataStream = FDataContainer.IOObject;
            FBinSizeStream = FBinSizeContainer.IOObject;
            FNormBinSizeStream = new BufferedIOStream<int>(FBinSizeStream.Length);
            FBinSizeBuffer = new int[16]; // 64 byte
        }
        
        public void Dispose()
        {
            FDataContainer.Dispose();
            FBinSizeContainer.Dispose();
        }
        
        public int Length
        {
            get;
            private set;
        }
        
        public bool Sync()
        {
            // Sync source
            IsChanged = FDataStream.Sync();
            IsChanged |= FBinSizeStream.Sync();
            
            if (IsChanged)
            {
                int dataLength = FDataStream.Length;
                int binSizeLength = FBinSizeStream.Length;
                int binSizeSum = 0;
                
                // Normalize bin size
                using (var binSizeReader = FBinSizeStream.GetReader())
                {
                    FNormBinSizeStream.Length = binSizeLength;
                    using (var normBinSizeWriter = FNormBinSizeStream.GetWriter())
                    {
                        int numSlicesToRead = Math.Min(FBinSizeBuffer.Length, binSizeLength);
                        while (!binSizeReader.Eos)
                        {
                            int numSlicesRead = binSizeReader.Read(FBinSizeBuffer, 0, numSlicesToRead);
                            for (int i = 0; i < numSlicesRead; i++)
                            {
                                FBinSizeBuffer[i] = SpreadUtils.NormalizeBinSize(dataLength, FBinSizeBuffer[i]);
                                binSizeSum += FBinSizeBuffer[i];
                            }
                            normBinSizeWriter.Write(FBinSizeBuffer, 0, numSlicesRead);
                        }
                    }
                }
                
                int binTimes = SpreadUtils.DivByBinSize(dataLength, binSizeSum);
                binTimes = binTimes > 0 ? binTimes : 1;
                Length = binTimes * binSizeLength;
            }
            
            return IsChanged;
        }
        
        public bool IsChanged
        {
            get;
            private set;
        }
        
        public object Clone()
        {
            throw new NotImplementedException();
        }
        
        public IStreamReader<IInStream<T>> GetReader()
        {
            return new MultiDimReader(this);
        }
        
        public System.Collections.Generic.IEnumerator<IInStream<T>> GetEnumerator()
        {
            return GetReader();
        }
        
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
