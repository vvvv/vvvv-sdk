using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using iop = System.Runtime.InteropServices;
using com = System.Runtime.InteropServices.ComTypes;

namespace System.IO
{
    class ComStream : System.IO.Stream
    {
        private readonly com.IStream source;
        private IntPtr mInt64; 

        public ComStream(com.IStream source)
        {
            this.source = source;
            this.mInt64 = iop.Marshal.AllocCoTaskMem(8);
        }

        ~ComStream()
        {
            Dispose(false);
        }

        protected override void Dispose(bool disposing)
        {
            if (this.mInt64 != null)
            {
                iop.Marshal.FreeCoTaskMem(this.mInt64);
                this.mInt64 = IntPtr.Zero;
            }
            base.Dispose(disposing);
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

        public override long Position
        {
            get 
            {
                source.Seek(0, (int)SeekOrigin.Current, this.mInt64);
                return iop.Marshal.ReadInt64(this.mInt64);
            }
            set
            {
                source.Seek(value, (int)SeekOrigin.Begin, this.mInt64);
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (offset != 0) throw new NotImplementedException();
            this.source.Read(buffer, count, this.mInt64);
            return iop.Marshal.ReadInt32(this.mInt64);
        }

        public override long Seek(long offset, System.IO.SeekOrigin origin)
        {
            this.source.Seek(offset, (int)origin, this.mInt64);
            return iop.Marshal.ReadInt64(this.mInt64);
        }

        public override void SetLength(long value)
        {
            this.source.SetSize(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (offset != 0) throw new NotImplementedException();
            this.source.Write(buffer, count, IntPtr.Zero);
        }
    }
}
