using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.IO
{
    public class AggregatedStream : ComStream
    {
        private readonly List<Tuple<long, Stream>> cache = new List<Tuple<long,Stream>>();
        private long length;

        public AggregatedStream(IEnumerable<Stream> streams)
        {
            foreach (var s in streams)
            {
                var t = Tuple.Create(s.Length, s);
                this.length += t.Item1;
                this.cache.Add(t);
            }
        }

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return true; }
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override void Flush()
        {
            throw new NotSupportedException();
        }

        public override long Length
        {
            get { return this.length; }
        }

        public override long Position
        {
            get;
            set;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            long streamOffset = 0;
            var totalBytesRead = 0;
            foreach (var tuple in this.cache)
            {
                var streamLength = tuple.Item1;
                var stream = tuple.Item2;
                if ((streamOffset <= Position) && (Position < streamOffset + streamLength))
                {
                    stream.Position = Position - streamOffset;
                    var bytesToRead = count - totalBytesRead;
                    while (bytesToRead > 0)
                    {
                        var bytesRead = stream.Read(buffer, offset + totalBytesRead, bytesToRead);
                        if (bytesRead == 0) break;
                        Position += bytesRead;
                        bytesToRead -= bytesRead;
                        totalBytesRead += bytesRead;
                    }
                }
                streamOffset += streamLength;
            }
            return totalBytesRead;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    Position = offset;
                    break;
                case SeekOrigin.Current:
                    Position += offset;
                    break;
                case SeekOrigin.End:
                    Position = Length - offset;
                    break;
                default:
                    throw new NotImplementedException();
            }
            return Position;
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }
    }
}
