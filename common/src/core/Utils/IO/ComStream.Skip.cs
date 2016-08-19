using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using VVVV.Utils.VMath;

namespace System.IO
{
    public class SkipStream : ComStream
    {
        private readonly Stream stream;
        private readonly long count;
        private long position;

        public SkipStream(Stream stream, long count)
        {
            Debug.Assert(count >= 0);
            this.stream = stream;
            this.count = count;
        }

        public override bool CanRead
        {
            get { return this.stream.CanRead; }
        }

        public override bool CanSeek
        {
            get { return this.stream.CanSeek; }
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override void Flush()
        {
            this.stream.Flush();
        }

        public override long Length
        {
            get { return Math.Max(this.stream.Length - this.count, 0); }
        }

        public override long Position
        {
            get { return this.position; }
            set { Seek(value, SeekOrigin.Begin); }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            this.stream.Position = this.position + this.count;
            var bytesRead = this.stream.Read(buffer, offset, count);
            this.Position += bytesRead;
            return bytesRead;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    this.position = VMath.Clamp(offset, 0, this.Length);
                    break;
                case SeekOrigin.Current:
                    this.position = VMath.Clamp(this.position + offset, 0, this.Length);
                    break;
                case SeekOrigin.End:
                    this.position = VMath.Clamp(this.Length - offset, 0, this.Length);
                    break;
            }
            return this.position;
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
