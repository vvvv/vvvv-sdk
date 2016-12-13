using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace VVVV.Utils.Streams
{
    public class RangeStream<T> : IInStream<T>
    {
        public class RangeStreamReader : IStreamReader<T>
        {
            private RangeStream<T> FRangeStream;
            private IStreamReader<T> FSourceReader;

            public RangeStreamReader(RangeStream<T> rangeStream)
            {
                FRangeStream = rangeStream;
                var sourceStream = rangeStream.FSource;
                FSourceReader = sourceStream.GetReader();
                FSourceReader.Position = rangeStream.FOffset;
            }

            public T Read(int stride = 1)
            {
                if (Eos) throw new InvalidOperationException();
                FPosition += stride;
                return FSourceReader.Read(stride);
            }

            public int Read(T[] buffer, int offset, int length, int stride = 1)
            {
                int numSlicesToRead = StreamUtils.GetNumSlicesAhead(this, offset, length, stride);
                FPosition += numSlicesToRead * stride;
                return FSourceReader.Read(buffer, offset, numSlicesToRead, stride);
            }

            public bool Eos
            {
                get { return Position >= Length; }
            }

            private int FPosition;
            public int Position
            {
                get
                {
                    return FPosition;
                }
                set
                {
                    FPosition = value;
                    FSourceReader.Position = FRangeStream.FOffset + value;
                }
            }

            public int Length
            {
                get { return FRangeStream.Length; }
            }

            public void Dispose()
            {
                FSourceReader.Dispose();
                FSourceReader = null;
                FRangeStream = null;
            }

            public T Current
            {
                get;
                private set;
            }

            object System.Collections.IEnumerator.Current
            {
                get { return Current; }
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

            public void Reset()
            {
                Position = 0;
            }
        }

        private readonly IInStream<T> FSource;
        private readonly int FOffset;
        private readonly int FCount;

        public RangeStream(IInStream<T> source, int offset, int count)
        {
            Debug.Assert(offset >= 0);
            Debug.Assert(count >= 0);
            Debug.Assert(offset + count <= source.Length || source is CyclicStream<T>);
            FSource = source;
            FOffset = offset;
            FCount = count;
        }

        public RangeStreamReader GetReader()
        {
            return new RangeStreamReader(this);
        }

        IStreamReader<T> IInStream<T>.GetReader() { return GetReader(); }

        public int Length
        {
            get { return FCount; }
        }

        public object Clone()
        {
            return new RangeStream<T>(FSource, FOffset, FCount);
        }

        public bool Sync()
        {
            return FSource.Sync();
        }

        public bool IsChanged
        {
            get { return FSource.IsChanged; }
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
}
