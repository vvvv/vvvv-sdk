using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SlimDX.Direct3D9;

namespace VVVV.Nodes.ImagePlayer
{
    public abstract class Frame : IDisposable
    {
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
        
        public abstract bool IsStarted
        {
            get;
        }
        
        public abstract bool IsCanceled
        {
            get;
        }
        
        public abstract Texture GetTexture(Device device);
        
        public abstract void Start();
        
        public abstract void Cancel();
        
        public abstract void Wait(CancellationToken token);   
        
        public abstract void Dispose();
        
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
