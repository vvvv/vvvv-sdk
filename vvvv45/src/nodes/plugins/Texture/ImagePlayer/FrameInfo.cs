using System;
using System.Threading;

namespace VVVV.Nodes.ImagePlayer
{
    public class FrameInfo : IEquatable<FrameInfo>, IDisposable
    {
        private readonly string FFilename;
        private readonly int FBufferSize;
        private readonly Lazy<CancellationTokenSource> FCancellationTokenSource;
        
        public FrameInfo(string filename, int bufferSize)
        {
            FFilename = filename;
            FBufferSize = bufferSize;
            
            FCancellationTokenSource = new Lazy<CancellationTokenSource>(() => new CancellationTokenSource());
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
