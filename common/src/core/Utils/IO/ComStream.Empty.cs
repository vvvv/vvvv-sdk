using System.Runtime.InteropServices.ComTypes;
using Marshal = System.Runtime.InteropServices.Marshal;

namespace System.IO
{
    public class EmptyComStream : Stream, IStream
    {
        public static readonly EmptyComStream Instance = new EmptyComStream();

        private EmptyComStream() { }

        public override bool CanRead => true;
        public override bool CanSeek => true;
        public override bool CanWrite => false;
        public override long Length => 0;

        public override long Position
        {
            get
            {
                return 0;
            }
            set { }
        }

        public override void Close()
        {
            // Do nothing
        }

        protected override void Dispose(bool disposing)
        {
            // Do nothing
        }

        public override void Flush() { }

        public override int Read(byte[] buffer, int offset, int count) => 0;

        public override long Seek(long offset, SeekOrigin origin) => 0;

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

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
            if (pcbRead != IntPtr.Zero)
                Marshal.WriteInt32(pcbRead, 0);
        }

        void IStream.Revert()
        {
            throw new NotImplementedException();
        }

        void IStream.Seek(long dlibMove, int dwOrigin, IntPtr plibNewPosition)
        {
            if (plibNewPosition != IntPtr.Zero)
                Marshal.WriteInt64(plibNewPosition, 0);
        }

        void IStream.SetSize(long libNewSize)
        {
            throw new NotSupportedException();
        }

        void IStream.Stat(out STATSTG pstatstg, int grfStatFlag)
        {
            pstatstg = new STATSTG() { cbSize = 0 };
        }

        void IStream.UnlockRegion(long libOffset, long cb, int dwLockType)
        {
            throw new NotImplementedException();
        }

        void IStream.Write(byte[] pv, int cb, IntPtr pcbWritten)
        {
            throw new NotSupportedException();
        }
    }
}
