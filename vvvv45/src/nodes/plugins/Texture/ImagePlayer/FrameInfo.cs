using System;
using System.Threading;

namespace VVVV.Nodes.ImagePlayer
{
    class FrameInfo : IEquatable<FrameInfo>, IDisposable
    {
        static int liveCount;
        private readonly string FFilename;
        private readonly int FBufferSize;
        private readonly Lazy<CancellationTokenSource> FCancellationTokenSource;
        
        public FrameInfo(string filename, int bufferSize)
        {
            FFilename = filename;
            FBufferSize = bufferSize;
            
            FCancellationTokenSource = new Lazy<CancellationTokenSource>(() => new CancellationTokenSource());
            System.Threading.Interlocked.Increment(ref liveCount);
        }
        
        public string Filename
        {
            get
            {
                return FFilename;
            }
        }
        
        public bool IsLoaded
        {
            get
            {
                return DurationTexture > 0 && !IsCanceled;
            }
        }
        
        public int BufferSize
        {
            get
            {
                return FBufferSize;
            }
        }
        
        public bool IsCanceled
        {
            get
            {
                return FCancellationTokenSource.Value.IsCancellationRequested;
            }
        }
        
        public CancellationToken Token
        {
            get
            {
                return FCancellationTokenSource.Value.Token;
            }
        }
        
        public void Cancel()
        {
            if (!IsCanceled)
            {
                FCancellationTokenSource.Value.Cancel();
            }
        }
        
        public void Dispose()
        {
            if (FCancellationTokenSource.IsValueCreated)
            {
                FCancellationTokenSource.Value.Dispose();
            }
            if (Decoder != null)
            {
                Decoder.Dispose();
                Decoder = null;
            }
            System.Threading.Interlocked.Decrement(ref liveCount);
        }
        
        public double DurationIO
        {
            get;
            set;
        }
        
        public double DurationTexture
        {
            get;
            set;
        }

        public int Width { get { return Decoder != null ? Decoder.Width : -1; } }
        public int Height { get { return Decoder != null ? Decoder.Height : -1; } }

        public FrameDecoder Decoder
        {
            get;
            set;
        }
        
        #region Equals and GetHashCode implementation
        public override bool Equals(object obj)
        {
            FrameInfo other = obj as FrameInfo;
            if (other == null)
                return false;
            return Equals(other);
        }
        
        public bool Equals(FrameInfo other)
        {
            return this.FFilename == other.FFilename;
        }
        
        public override int GetHashCode()
        {
            int hashCode = 0;
            unchecked {
                if (FFilename != null)
                    hashCode += 1000000009 * FFilename.GetHashCode();
            }
            return hashCode;
        }
        
        public static bool operator ==(FrameInfo lhs, FrameInfo rhs)
        {
            if (ReferenceEquals(lhs, rhs))
                return true;
            if (ReferenceEquals(lhs, null) || ReferenceEquals(rhs, null))
                return false;
            return lhs.Equals(rhs);
        }
        
        public static bool operator !=(FrameInfo lhs, FrameInfo rhs)
        {
            return !(lhs == rhs);
        }
        #endregion

        public override string ToString()
        {
            return string.Format("[FFilename={0}]", FFilename);
        }
    }
}
