using System;
using System.Collections.Generic;
using System.IO;
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
        
        public abstract Texture GetTexture(Device device);

        public abstract void LoadFromDisk();
        
        public abstract void CreateTextures();
        
        public abstract void Dispose();
    }
}
