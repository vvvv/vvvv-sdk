using System.Runtime.InteropServices.ComTypes;
using Marshal = System.Runtime.InteropServices.Marshal;

namespace System.IO
{
    public class MemoryComStream : MemoryStream, IStream
    {
        public MemoryComStream() : base() { }
        public MemoryComStream(int capacity) : base(capacity) { }
        public MemoryComStream(byte[] buffer) : base(buffer) { }
        public MemoryComStream(byte[] buffer, bool writable) : base(buffer, writable) { }
        public MemoryComStream(byte[] buffer, int index, int count) : base(buffer, index, count) { }
        public MemoryComStream(byte[] buffer, int index, int count, bool writable) : base(buffer, index, count, writable) { }
        public MemoryComStream(byte[] buffer, int index, int count, bool writable, bool publiclyVisible) : base(buffer, index, count, writable, publiclyVisible) { }

        void IStream.Clone(out IStream ppstm)
        {
            throw new NotImplementedException();
        }

        void IStream.Commit(int grfCommitFlags)
        {
            throw new NotImplementedException();
        }

        void IStream.CopyTo(IStream pstm, long cb, IntPtr pcbRead, IntPtr pcbWritten) => this.CopyTo(pstm, cb, pcbRead, pcbWritten);

        void IStream.LockRegion(long libOffset, long cb, int dwLockType)
        {
            throw new NotImplementedException();
        }

        void IStream.Read(byte[] pv, int cb, IntPtr pcbRead)
        {
            int read = Read(pv, 0, cb);
            if (pcbRead != IntPtr.Zero)
                Marshal.WriteInt32(pcbRead, read);
        }

        void IStream.Revert()
        {
            throw new NotImplementedException();
        }

        void IStream.Seek(long dlibMove, int dwOrigin, IntPtr plibNewPosition)
        {
            var origin = (SeekOrigin)dwOrigin;
            var pos = Seek(dlibMove, origin);
            if (plibNewPosition != IntPtr.Zero)
                Marshal.WriteInt64(plibNewPosition, pos);
        }

        void IStream.SetSize(long libNewSize)
        {
            SetLength(libNewSize);
        }

        void IStream.Stat(out STATSTG pstatstg, int grfStatFlag)
        {
            pstatstg = new STATSTG() { cbSize = Length };
        }

        void IStream.UnlockRegion(long libOffset, long cb, int dwLockType)
        {
            throw new NotImplementedException();
        }

        void IStream.Write(byte[] pv, int cb, IntPtr pcbWritten)
        {
            Write(pv, 0, cb);
            if (pcbWritten != IntPtr.Zero)
                Marshal.WriteInt32(pcbWritten, cb);
        }
    }

    public abstract class ComStream : Stream, IStream
    {
        void IStream.Clone(out IStream ppstm)
        {
            throw new NotImplementedException();
        }

        void IStream.Commit(int grfCommitFlags)
        {
            throw new NotImplementedException();
        }

        void IStream.CopyTo(IStream pstm, long cb, IntPtr pcbRead, IntPtr pcbWritten) => this.CopyTo(pstm, cb, pcbRead, pcbWritten);

        void IStream.LockRegion(long libOffset, long cb, int dwLockType)
        {
            throw new NotImplementedException();
        }

        void IStream.Read(byte[] pv, int cb, IntPtr pcbRead)
        {
            int read = Read(pv, 0, cb);
            if (pcbRead != IntPtr.Zero)
                Marshal.WriteInt32(pcbRead, read);
        }

        void IStream.Revert()
        {
            throw new NotImplementedException();
        }

        void IStream.Seek(long dlibMove, int dwOrigin, IntPtr plibNewPosition)
        {
            var origin = (SeekOrigin)dwOrigin;
            var pos = Seek(dlibMove, origin);
            if (plibNewPosition != IntPtr.Zero)
                Marshal.WriteInt64(plibNewPosition, pos);
        }

        void IStream.SetSize(long libNewSize)
        {
            SetLength(libNewSize);
        }

        void IStream.Stat(out STATSTG pstatstg, int grfStatFlag)
        {
            pstatstg = new STATSTG() { cbSize = Length };
        }

        void IStream.UnlockRegion(long libOffset, long cb, int dwLockType)
        {
            throw new NotImplementedException();
        }

        void IStream.Write(byte[] pv, int cb, IntPtr pcbWritten)
        {
            Write(pv, 0, cb);
            if (pcbWritten != IntPtr.Zero)
                Marshal.WriteInt32(pcbWritten, cb);
        }
    }
}
