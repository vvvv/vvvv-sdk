using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using iop = System.Runtime.InteropServices;
using com = System.Runtime.InteropServices.ComTypes;

namespace System.IO
{
    public class AdapterComStream : com.IStream
    {
        private readonly Stream source;

        public AdapterComStream(Stream source)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            this.source = source;
        }

        public bool Equals(AdapterComStream stream)
        {
            if (stream == null)
                return false;
            return stream.source == this.source;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as AdapterComStream);
        }

        public override int GetHashCode()
        {
            return this.source.GetHashCode();
        }

        public void Clone(out com.IStream ppstm)
        {
            ppstm = new AdapterComStream(this.source);
        }

        public void Commit(int grfCommitFlags)
        {
            if (grfCommitFlags == 0)
                this.source.Flush();
            else
                throw new NotSupportedException();
        }

        public void CopyTo(com.IStream pstm, long cb, IntPtr pcbRead, IntPtr pcbWritten) => this.source.CopyTo(pstm, cb, pcbRead, pcbWritten);

        public void LockRegion(long libOffset, long cb, int dwLockType)
        {
            throw new NotImplementedException();
        }

        public void Read(byte[] buffer, int cb, IntPtr pcbRead)
        {
            int read = this.source.Read(buffer, 0, cb);
            if (pcbRead != IntPtr.Zero)
                iop.Marshal.WriteInt32(pcbRead, read);
        }

        public void Seek(long dlibMove, int dwOrigin, IntPtr plibNewPosition)
        {
            var origin = (SeekOrigin)dwOrigin;
            var pos = this.source.Seek(dlibMove, origin);
            if (plibNewPosition != IntPtr.Zero)
                iop.Marshal.WriteInt64(plibNewPosition, pos);
        }

        public void Stat(out com.STATSTG pstatstg, int grfStatFlag)
        {
            pstatstg = new com.STATSTG() { cbSize = this.source.Length };
        }

        public void UnlockRegion(long libOffset, long cb, int dwLockType)
        {
            throw new NotImplementedException();
        }

        public void Write(byte[] buffer, int cb, IntPtr pcbWritten)
        {
            this.source.Write(buffer, 0, cb);
            if (pcbWritten != null)
                iop.Marshal.WriteInt32(pcbWritten, cb);
        }

        public void Revert()
        {
            throw new NotImplementedException();
        }

        public void SetSize(long libNewSize)
        {
            this.source.SetLength(libNewSize);
        }
    }

    public class ComAdapterStream : Stream
    {
        private readonly com.IStream source;

        public ComAdapterStream(com.IStream source)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            this.source = source;
        }

        public bool Equals(ComAdapterStream stream)
        {
            if (stream == null)
                return false;
            return stream.source == this.source;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ComAdapterStream);
        }

        public override int GetHashCode()
        {
            return this.source.GetHashCode();
        }

        public override bool CanRead { get { return true; } }
        public override bool CanSeek { get { return true; } }
        public override bool CanWrite { get { return true; } }

        public override void Flush()
        {
            source.Commit(0);
        }

        public override long Length
        {
            get
            {
                com.STATSTG stats;
                source.Stat(out stats, 1);
                return stats.cbSize;
            }
        }

        public override unsafe long Position
        {
            get
            {
                long position;
                var positionPtr = new IntPtr(&position);
                source.Seek(0, (int)SeekOrigin.Current, positionPtr);
                return position;
            }
            set
            {
                long position;
                var positionPtr = new IntPtr(&position);
                source.Seek(value, (int)SeekOrigin.Begin, positionPtr);
            }
        }

        public override unsafe int Read(byte[] buffer, int offset, int count)
        {
            long readCount;
            var readCountPtr = new IntPtr(&readCount);
            if (offset == 0)
                this.source.Read(buffer, count, readCountPtr);
            else
            {
                var tmp = new byte[count];
                this.source.Read(tmp, count, readCountPtr);
                for (int i = 0; i < readCount; i++)
                    buffer[offset + i] = tmp[i];
            }
            return (int)readCount;
        }

        public override unsafe long Seek(long offset, SeekOrigin origin)
        {
            long position;
            var positionPtr = new IntPtr(&position);
            this.source.Seek(offset, (int)origin, positionPtr);
            return position;
        }

        public override void SetLength(long value)
        {
            this.source.SetSize(value);
        }

        public override unsafe void Write(byte[] buffer, int offset, int count)
        {
            if (offset == 0)
                this.source.Write(buffer, count, IntPtr.Zero);
            else
            {
                var tmp = new byte[count];
                for (int i = 0; i < tmp.Length; i++)
                    tmp[i] = buffer[offset + i];
                this.source.Write(tmp, count, IntPtr.Zero);
            }
        }
    }

    public static class ComStreamExtensions
    {
        public static void CopyTo(this Stream source, com.IStream pstm, long cb, IntPtr pcbRead, IntPtr pcbWritten)
        {
            if (!source.CanRead) throw new NotSupportedException("Source stream doesn't support reading.");
            using (var destination = new ComAdapterStream(pstm))
            {
                if (!destination.CanWrite) throw new NotSupportedException("Destination stream doesn't support writing.");
                var buffer = new byte[4096];
                var totalBytesRead = 0L;
                var totalBytesWritten = 0L;
                var chunkLength = buffer.Length;
                if (cb < chunkLength) chunkLength = (int)cb;
                while (totalBytesWritten < cb)
                {
                    // Read
                    var bytesRead = source.Read(buffer, 0, chunkLength);
                    if (bytesRead == 0) break;
                    totalBytesRead += bytesRead;
                    // Write
                    var previousPosition = destination.Position;
                    destination.Write(buffer, 0, bytesRead);
                    var bytesWritten = destination.Position - previousPosition;
                    if (bytesWritten == 0) break;
                    totalBytesWritten += bytesWritten;

                    if (bytesRead != bytesWritten) break;
                }
                if (pcbRead != IntPtr.Zero)
                    iop.Marshal.WriteInt64(pcbRead, totalBytesRead);
                if (pcbWritten != IntPtr.Zero)
                    iop.Marshal.WriteInt64(pcbWritten, totalBytesWritten);
            }
        }
    }
}
