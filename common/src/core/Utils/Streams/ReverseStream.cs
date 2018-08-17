using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace VVVV.Utils.Streams
{
    public class ReverseStream<T> : IInStream<T>
    {
        public class ReverseStreamReader : IStreamReader<T>
        {
            private ReverseStream<T> FReverseStream;
            private IStreamReader<T> FSourceReader;

            public ReverseStreamReader(ReverseStream<T> reverseStream)
            {
                FReverseStream = reverseStream;
                var sourceStream = reverseStream.FSource;
                FSourceReader = sourceStream.GetReader();
                Reset();
            }

            public T Read(int stride = 1)
            {
                if (Eos) throw new InvalidOperationException();
                Position += stride;
                var result = FSourceReader.Read(stride);
                Position += stride;
                return result;
            }

            public int Read(T[] buffer, int offset, int length, int stride = 1)
            {
                int numSlicesToRead = StreamUtils.GetNumSlicesAhead(this, offset, length, stride);
                Position += numSlicesToRead * stride;
                FSourceReader.Read(buffer, offset, numSlicesToRead, stride);
                Position += numSlicesToRead * stride;
                int i = offset, j = offset + numSlicesToRead - 1;
                while (i < j)
                {
                    var t = buffer[i];
                    buffer[i] = buffer[j];
                    buffer[j] = t;
                    i += 1;
                    j -= 1;
                }
                return numSlicesToRead;
            }

            public bool Eos
            {
                get { return Position >= Length; }
            }

            public int Position
            {
                get
                {
                    return FSourceReader.Length - FSourceReader.Position;
                }
                set
                {
                    FSourceReader.Position = FSourceReader.Length - value;
                }
            }

            public int Length
            {
                get { return FReverseStream.Length; }
            }

            public void Dispose()
            {
                FSourceReader.Dispose();
                FSourceReader = null;
                FReverseStream = null;
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

        public ReverseStream(IInStream<T> source)
        {
            FSource = source;
        }

        public ReverseStreamReader GetReader()
        {
            return new ReverseStreamReader(this);
        }

        IStreamReader<T> IInStream<T>.GetReader() { return GetReader(); }

        public int Length
        {
            get { return FSource.Length; }
        }

        public object Clone()
        {
            return new ReverseStream<T>(FSource);
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
