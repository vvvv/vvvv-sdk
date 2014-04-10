using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using VVVV.Utils.Streams;
using iop = System.Runtime.InteropServices;
using com = System.Runtime.InteropServices.ComTypes;
using win32 = VVVV.Utils.Win32;

namespace System.IO
{
    class ComIStream : win32.IStream
    {
        private readonly Stream source;

        public ComIStream(Stream source)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            this.source = source;
        }

        public void Clone(out win32.IStream ppstm)
        {
            ppstm = new ComIStream(this.source);
        }

        public void Commit(int grfCommitFlags)
        {
            if (grfCommitFlags == 0)
                this.source.Flush();
            else
                throw new NotSupportedException();
        }

        public void CopyTo(win32.IStream pstm, long cb, IntPtr pcbRead, IntPtr pcbWritten)
        {
            if (!this.source.CanRead) throw new NotSupportedException("Source stream doesn't support reading.");
            using (var destination = new ComStream(pstm))
            {
                if (!destination.CanWrite) throw new NotSupportedException("Destination stream doesn't support writing.");
                var buffer = MemoryPool<byte>.GetArray();
                try
                {
                    var totalBytesRead = 0L;
                    var totalBytesWritten = 0L;
                    var chunkLength = buffer.Length;
                    if (cb < chunkLength) chunkLength = (int)cb;
                    while (totalBytesWritten < cb)
                    {
                        // Read
                        var bytesRead = this.source.Read(buffer, 0, chunkLength);
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
                    iop.Marshal.WriteInt64(pcbRead, totalBytesRead);
                    iop.Marshal.WriteInt64(pcbWritten, totalBytesWritten);
                }
                finally
                {
                    MemoryPool<byte>.PutArray(buffer);
                }
            }
        }

        public void LockRegion(long libOffset, long cb, int dwLockType)
        {
            throw new NotImplementedException();
        }

        public void Read(IntPtr pv, int cb, IntPtr pcbRead)
        {
            var buffer = MemoryPool<byte>.GetArray();
            try
            {
                var totalBytesRead = 0;
                while (totalBytesRead < cb)
                {
                    var bytesToRead = Math.Min(buffer.Length, cb - totalBytesRead);
                    var bytesRead = this.source.Read(buffer, 0, bytesToRead);
                    if (bytesRead == 0) break;
                    iop.Marshal.Copy(buffer, 0, pv + totalBytesRead, bytesRead);
                    totalBytesRead += bytesRead;
                }
                iop.Marshal.WriteInt32(pcbRead, totalBytesRead);
            }
            finally
            {
                MemoryPool<byte>.PutArray(buffer);
            }
        }

        public void Seek(long dlibMove, SeekOrigin dwOrigin, IntPtr plibNewPosition)
        {
            iop.Marshal.WriteInt64(plibNewPosition, this.source.Seek(dlibMove, dwOrigin));
        }

        public void Stat(out com.STATSTG pstatstg, int grfStatFlag)
        {
            pstatstg = new System.Runtime.InteropServices.ComTypes.STATSTG()
            {
                cbSize = this.source.Length
            };
        }

        public void UnlockRegion(long libOffset, long cb, int dwLockType)
        {
            throw new NotImplementedException();
        }

        public void Write(IntPtr pv, int cb, IntPtr pcbWritten)
        {
            var buffer = MemoryPool<byte>.GetArray();
            try
            {
                var totalBytesWritten = 0;
                while (totalBytesWritten < cb)
                {
                    var bytesToWrite = Math.Min(buffer.Length, cb - totalBytesWritten);
                    iop.Marshal.Copy(pv, buffer, 0, bytesToWrite);
                    this.source.Write(buffer, 0, bytesToWrite);
                    totalBytesWritten += bytesToWrite;
                }
                iop.Marshal.WriteInt32(pcbWritten, totalBytesWritten);
            }
            finally
            {
                MemoryPool<byte>.PutArray(buffer);
            }
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
}
