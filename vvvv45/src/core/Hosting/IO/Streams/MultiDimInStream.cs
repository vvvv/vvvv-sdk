using System;
using System.Diagnostics;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils;
using VVVV.Utils.Streams;

namespace VVVV.Hosting.IO.Streams
{
    class MultiDimInStream<T> : MemoryIOStream<IInStream<T>>, IIOMultiPin, IDisposable
    {
        class InnerStream : IInStream<T>
        {
            class InnerStreamReader : IStreamReader<T>
            {
                private readonly InnerStream FStream;
                private readonly IStreamReader<T> FDataReader;
                
                public InnerStreamReader(InnerStream stream)
                {
                    FStream = stream;
                    var dataStream = stream.FDataStream;
                    if (stream.Offset + stream.Length > dataStream.Length)
                        FDataReader = dataStream.GetCyclicReader();
                    else
                        FDataReader = dataStream.GetReader();
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
                        FDataReader.Position = FStream.Offset + value;
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

            public InnerStream(IInStream<T> dataStream)
            {
                FDataStream = dataStream;
            }

            public int Offset
            {
                get;
                internal set;
            }

            public int Length
            {
                get;
                internal set;
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
                if (Offset == 0 && Length == FDataStream.Length)
                    return FDataStream.GetReader();
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
        private readonly IntInStream FBinSizeStream;

        public IIOContainer BaseContainer => FDataContainer;
        public IIOContainer[] AssociatedContainers => new IIOContainer[]{ FBinSizeContainer };

        public MultiDimInStream(IIOFactory factory, InputAttribute attribute)
        {
            FDataContainer = factory.CreateIOContainer<IInStream<T>>(attribute.DecreaseBinSizeWrapCount(), false);
            FBinSizeContainer = factory.CreateIOContainer<IInStream<int>>(attribute.GetBinSizeInputAttribute(FDataContainer), false);
            FDataStream = FDataContainer.IOObject;
            FBinSizeStream = (IntInStream)FBinSizeContainer.IOObject;
        }
        
        public void Dispose()
        {
            FDataContainer.Dispose();
            FBinSizeContainer.Dispose();
        }

        protected override void BufferIncreased(IInStream<T>[] oldBuffer, IInStream<T>[] newBuffer)
        {
            Array.Copy(oldBuffer, newBuffer, oldBuffer.Length);
            for (int i = oldBuffer.Length; i < newBuffer.Length; i++)
            {
                newBuffer[i] = new InnerStream(FDataStream);
            }
        }
        
        public override bool Sync()
        {
            // Sync source
            IsChanged = FDataStream.Sync() | FBinSizeStream.Sync();
            
            if (IsChanged)
            {
                int dataLength = FDataStream.Length;
                int binSizeLength = FBinSizeStream.Length;
                int binSizeSum = 0;
                
                foreach (var binSize in FBinSizeStream)
                {
                    binSizeSum += SpreadUtils.NormalizeBinSize(dataLength, binSize);
                }
                
                int binTimes = SpreadUtils.DivByBinSize(dataLength, binSizeSum);
                binTimes = binTimes > 0 ? binTimes : 1;
                Length = binTimes * binSizeLength;

                var binSizeBuffer = MemoryPool<int>.GetArray();
                try
                {
                    using (var binSizeReader = FBinSizeStream.GetCyclicReader())
                    {
                        var numSlicesToWrite = Length;
                        var offsetIntoDataStream = 0;
                        while (numSlicesToWrite > 0)
                        {
                            var numSlicesToRead = Math.Min(numSlicesToWrite, binSizeBuffer.Length);
                            binSizeReader.Read(binSizeBuffer, 0, numSlicesToRead);
                            var offset = Length - numSlicesToWrite;
                            for (int i = offset; i < offset + numSlicesToRead; i++)
                            {
                                var binSize = SpreadUtils.NormalizeBinSize(dataLength, binSizeBuffer[i - offset]);
                                var innerStream = this[i] as InnerStream;
                                // Inner stream will use cyclic reader when offset + length > length of data stream.
                                innerStream.Offset = offsetIntoDataStream;
                                innerStream.Length = binSize;
                                offsetIntoDataStream += binSize;
                            }
                            numSlicesToWrite -= numSlicesToRead;
                        }
                    }
                }
                finally
                {
                    MemoryPool<int>.PutArray(binSizeBuffer);
                }
            }

            return base.Sync();
        }
    }
}
