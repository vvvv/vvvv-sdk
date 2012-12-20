using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using VVVV.Utils.Streams;
using iop = System.Runtime.InteropServices;
using com = System.Runtime.InteropServices.ComTypes;

namespace System.IO
{
    class ComIStream : com.IStream
    {
        private readonly Stream source;

        public ComIStream(Stream source)
        {
            this.source = source;
        }

        public void Clone(out com.IStream ppstm)
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

        public void CopyTo(com.IStream pstm, long cb, IntPtr pcbRead, IntPtr pcbWritten)
        {
            if (!this.source.CanRead) throw new NotSupportedException("Source stream doesn't support reading.");
            var destination = new ComStream(pstm);
            if (!destination.CanWrite) throw new NotSupportedException("Destination stream doesn't support writing.");
            var buffer = new byte[4096];
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

        public void LockRegion(long libOffset, long cb, int dwLockType)
        {
            throw new NotImplementedException();
        }

        public void Read(byte[] pv, int cb, IntPtr pcbRead)
        {
            var bytesRead = this.source.Read(pv, 0, cb);
            iop.Marshal.WriteInt64(pcbRead, bytesRead);
        }

        public void Seek(long dlibMove, int dwOrigin, IntPtr plibNewPosition)
        {
            iop.Marshal.WriteInt64(plibNewPosition, this.source.Seek(dlibMove, (SeekOrigin)dwOrigin));
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

        public void Write(byte[] pv, int cb, IntPtr pcbWritten)
        {
            var previousPosition = this.source.Position;
            this.source.Write(pv, 0, cb);
            iop.Marshal.WriteInt64(pcbWritten, this.source.Position - previousPosition);
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
