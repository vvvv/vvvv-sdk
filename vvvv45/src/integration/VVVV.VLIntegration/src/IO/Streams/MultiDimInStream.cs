using System;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils;
using VVVV.Utils.Streams;
using System.Collections.Generic;
using System.Collections;
using VVVV.Hosting.IO;
using System.Linq;

namespace VVVV.VL.Hosting.IO.Streams
{
    abstract class MultiDimInStream<T, U> : MemoryIOStream<U>, IDisposable, ICollection, IIOMultiPin
    {
        private readonly IIOContainer<IInStream<T>> FDataContainer;
        private readonly IIOContainer<IInStream<int>> FBinSizeContainer;
        protected readonly IInStream<T> FDataStream;
        private readonly IInStream<int> FBinSizeStream;
        
        public MultiDimInStream(IIOFactory factory, InputAttribute attribute)
        {
            FDataContainer = factory.CreateIOContainer<IInStream<T>>(attribute.DecreaseBinSizeWrapCount(), false);
            FBinSizeContainer = factory.CreateIOContainer<IInStream<int>>(attribute.GetBinSizeInputAttribute(FDataContainer), false);
            FDataStream = FDataContainer.IOObject;
            FBinSizeStream = FBinSizeContainer.IOObject;
        }
        
        public void Dispose()
        {
            FDataContainer.Dispose();
            FBinSizeContainer.Dispose();
        }

        public IIOContainer BaseContainer => FDataContainer;
        public IIOContainer[] AssociatedContainers => new IIOContainer[] { FBinSizeContainer };
        
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
                    using (var dataReader = FDataStream.GetReader())
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
                                Sync(i, offsetIntoDataStream, binSize, dataReader);
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

        protected abstract void Sync(int i, int offsetIntoDataStream, int binSize, IStreamReader<T> dataReader);

        #region ICollection Members

        void ICollection.CopyTo(Array array, int index)
        {
        }

        int ICollection.Count
        {
            get { return Length; }
        }

        bool ICollection.IsSynchronized
        {
            get { return false; }
        }

        object ICollection.SyncRoot
        {
            get { return this; }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }

    class MultiDimEnumerableInStream<T> : MultiDimInStream<T, IEnumerable<T>>
    {
        class InnerStream : IInStream<T>, ICollection
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

            #region ICollection Members

            void ICollection.CopyTo(Array array, int index)
            {
            }

            int ICollection.Count
            {
                get { return Length; }
            }

            bool ICollection.IsSynchronized
            {
                get { return false; }
            }

            object ICollection.SyncRoot
            {
                get { return this; }
            }

            #endregion
        }

        public MultiDimEnumerableInStream(IIOFactory factory, InputAttribute attribute) : base(factory, attribute)
        {
        }

        protected override void BufferIncreased(IEnumerable<T>[] oldBuffer, IEnumerable<T>[] newBuffer)
        {
            Array.Copy(oldBuffer, newBuffer, oldBuffer.Length);
            for (int i = oldBuffer.Length; i < newBuffer.Length; i++)
            {
                newBuffer[i] = new InnerStream(FDataStream);
            }
        }

        protected override void Sync(int i, int offsetIntoDataStream, int binSize, IStreamReader<T> dataReader)
        {
            var innerStream = this[i] as InnerStream;
            // Inner stream will use cyclic reader when offset + length > length of data stream.
            innerStream.Offset = offsetIntoDataStream;
            innerStream.Length = binSize;
        }
    }

    class MultiDimSpreadInStream<T> : MultiDimInStream<T, global::VL.Lib.Collections.Spread<T>>
    {
        public MultiDimSpreadInStream(IIOFactory factory, InputAttribute attribute) : base(factory, attribute)
        {
        }

        protected override void BufferIncreased(global::VL.Lib.Collections.Spread<T>[] oldBuffer, global::VL.Lib.Collections.Spread<T>[] newBuffer)
        {
            Array.Copy(oldBuffer, newBuffer, oldBuffer.Length);
            for (int i = oldBuffer.Length; i < newBuffer.Length; i++)
            {
                newBuffer[i] = global::VL.Lib.Collections.Spread<T>.Empty;
            }
        }

        protected override void Sync(int i, int offsetIntoDataStream, int binSize, IStreamReader<T> dataReader)
        {
            var spread = this[i];
            var builder = spread.ToBuilder();
            builder.Count = binSize;
            var slicesToRead = binSize;
            using (var buffer = MemoryPool<T>.GetBuffer())
            {
                var j = 0;
                dataReader.Position = offsetIntoDataStream;
                while (slicesToRead > 0)
                {
                    var slicesRead = dataReader.Read(buffer, 0, Math.Min(slicesToRead, buffer.Length));
                    if (slicesRead > 0)
                    {
                        for (int k = 0; k < slicesRead; k++)
                            builder[j++] = buffer[k];
                        slicesToRead -= slicesRead;
                    }
                    else
                        break;
                }
            }
            this[i] = builder.ToSpread();
        }
    }
}
