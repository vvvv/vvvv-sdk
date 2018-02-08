using SharpDX.Direct3D9;
using System;
using System.Threading;

namespace VVVV.Nodes.ImagePlayer
{
    class FrameInfo : IEquatable<FrameInfo>, IDisposable
    {
        public readonly string Filename;
        public readonly int BufferSize;
        public readonly Format PreferedFormat;
        public double DurationIO;
        public double DurationTexture;
        private readonly Lazy<CancellationTokenSource> FCancellationTokenSource;
        
        public FrameInfo(string filename, int bufferSize, Format preferedFormat)
        {
            Filename = filename;
            BufferSize = bufferSize;
            PreferedFormat = preferedFormat;
            
            FCancellationTokenSource = new Lazy<CancellationTokenSource>(() => new CancellationTokenSource());
        }
        
        public bool IsLoaded
        {
            get
            {
                return DurationTexture > 0 && !IsCanceled;
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
            return this.Filename == other.Filename && this.PreferedFormat == other.PreferedFormat;
        }
        
        public override int GetHashCode()
        {
            int hashCode = 0;
            unchecked {
                if (Filename != null)
                    hashCode += 1000000009 * Filename.GetHashCode();
                hashCode += PreferedFormat.GetHashCode();
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
            return string.Format("[Filename={0}, PreferedFormat={1}]", Filename, PreferedFormat);
        }
    }
}
