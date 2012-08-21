using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VVVV.Utils.Streams
{
    public class RangeStream<T> : IInStream<T>
    {
        public class RangeStreamReader : IStreamReader<T>
        {
            private readonly RangeStream<T> rangeStream;
            private readonly CyclicStreamReader<T> sourceReader;

            public RangeStreamReader(RangeStream<T> rangeStream)
            {
                // TODO: Complete member initialization
                this.rangeStream = rangeStream;
                this.sourceReader = rangeStream.source.GetCyclicReader();
                this.sourceReader.Position = rangeStream.offset;
            }

            public T Read(int stride = 1)
            {
                if (Eos) throw new InvalidOperationException();
                position += stride;
                return sourceReader.Read(stride);
            }

            public int Read(T[] buffer, int offset, int length, int stride = 1)
            {
                length = StreamUtils.GetNumSlicesAhead(this, offset, length, stride);
                position += length;
                return sourceReader.Read(buffer, offset, length, stride);
            }

            public bool Eos
            {
                get { return Position >= Length; }
            }

            private int position;
            public int Position
            {
                get
                {
                    return position;
                }
                set
                {
                    position = value;
                    sourceReader.Position = rangeStream.offset + value;
                }
            }

            public int Length
            {
                get { return rangeStream.Length; }
            }

            public void Dispose()
            {
                sourceReader.Dispose();
            }

            public T Current
            {
                get { return sourceReader.Current; }
            }

            object System.Collections.IEnumerator.Current
            {
                get { return Current; }
            }

            public bool MoveNext()
            {
                return sourceReader.MoveNext();
            }

            public void Reset()
            {
                Position = 0;
            }
        }

        private readonly IInStream<T> source;
        private readonly int offset;
        private readonly int count;

        public RangeStream(IInStream<T> source, int offset, int count)
        {
            // TODO: Complete member initialization
            this.source = source;
            this.offset = offset;
            this.count = source.Length > 0 ? count : 0;
        }

        public RangeStreamReader GetReader()
        {
            return new RangeStreamReader(this);
        }

        IStreamReader<T> IInStream<T>.GetReader() { return GetReader(); }

        public int Length
        {
            get { return count; }
        }

        public object Clone()
        {
            return new RangeStream<T>(source, offset, count);
        }

        public bool Sync()
        {
            return source.Sync();
        }

        public bool IsChanged
        {
            get { return source.IsChanged; }
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
