using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using SlimDX.Direct3D9;

namespace VVVV.Nodes.ImagePlayer
{
    public abstract class Frame : IDisposable
    {
        private readonly CancellationTokenSource FCancellationTokenSource = new CancellationTokenSource();
        
        public int FrameNr
        {
            get;
            protected set;
        }
        
        public bool Used
        {
            get;
            set;
        }
        
        public bool Scheduled
        {
            get;
            set;
        }
        
        public bool Disposed
        {
            get;
            private set;
        }
        
        protected CancellationToken Token
        {
            get
            {
                return FCancellationTokenSource.Token;
            }
        }
        
        public abstract Texture GetTexture(Device device);

        public abstract void DoIO();
        
        public abstract void DoLoad();
        
        public void Cancel()
        {
            FCancellationTokenSource.Cancel();
        }
        
        public virtual void Dispose()
        {
            if (Disposed)
            {
                throw new ObjectDisposedException(this.ToString());
            }
            
            Disposed = true;
            FCancellationTokenSource.Dispose();
        }
        
        public double DurationIO
        {
            get;
            protected set;
        }
        
        public double DurationTexture
        {
            get;
            protected set;
        }
    }
}
