using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using iop = System.Runtime.InteropServices;
using com = System.Runtime.InteropServices.ComTypes;
using win32 = VVVV.Utils.Win32;

namespace System.IO
{
    class ComStream : System.IO.Stream
    {
        private readonly win32.IStream source;
        private IntPtr mInt64;

        public ComStream(win32.IStream source)
        {
            if (source == null)
                throw new ArgumentNullException("source");
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

        public bool Equals(ComStream stream)
        {
            if (stream == null)
                return false;
            return stream.source == this.source;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ComStream);
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

        public override long Position
        {
            get 
            {
                source.Seek(0, SeekOrigin.Current, this.mInt64);
                return iop.Marshal.ReadInt64(this.mInt64);
            }
            set
            {
                source.Seek(value, SeekOrigin.Begin, this.mInt64);
            }
        }

        public override unsafe int Read(byte[] buffer, int offset, int count)
        {
            fixed (byte* pBuffer = &buffer[offset])
            {
                this.source.Read(new IntPtr(pBuffer), count, this.mInt64);
            }
            return iop.Marshal.ReadInt32(this.mInt64);
        }

        public override long Seek(long offset, System.IO.SeekOrigin origin)
        {
            this.source.Seek(offset, origin, this.mInt64);
            return iop.Marshal.ReadInt64(this.mInt64);
        }

        public override void SetLength(long value)
        {
            this.source.SetSize(value);
        }

        public override unsafe void Write(byte[] buffer, int offset, int count)
        {
            fixed (byte* pBuffer = &buffer[offset])
            {
                this.source.Write(new IntPtr(pBuffer), count, IntPtr.Zero);
            }
        }
    }
}
