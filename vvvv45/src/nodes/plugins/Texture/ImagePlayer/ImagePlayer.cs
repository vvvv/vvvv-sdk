using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SlimDX.Direct3D9;
using VVVV.Core.Logging;
using VVVV.Utils.VMath;

namespace VVVV.Nodes.ImagePlayer
{
    /// <summary>
    /// Given a directory containing a sequence of images this class will
    /// try to preload the next couple of images based on the given frame
    /// number in order to provide fast access.
    /// </summary>
    public class ImagePlayer : IDisposable
    {
        public const string DEFAULT_FILEMASK = "*.dds";
        public const double DEFAULT_FPS = 25.0;
        
        private string FDirectory = string.Empty;
        private string FFilemask = DEFAULT_FILEMASK;
        private double FFps = DEFAULT_FPS;
        private string[] FFiles = new string[0];
        private FramePreloader FFramePreloader;
        private Frame FCurrentFrame = FramePreloader.EmptyFrame;
        private readonly IEnumerable<Device> FDevices;
        private readonly ILogger FLogger;
        
        public ImagePlayer(IEnumerable<Device> devices, ILogger logger)
        {
            FDevices = devices;
            FLogger = logger;
            FFramePreloader = new FramePreloader(0, devices, logger);
        }
        
        public void Dispose()
        {
            FFramePreloader.Dispose();
            FFramePreloader = null;
        }
        
        public string Directory
        {
            get
            {
                return FDirectory;
            }
            set
            {
                Contract.Requires(System.IO.Directory.Exists(value));
                
                if (value != FDirectory)
                {
                    FDirectory = value;
                    Reload();
                }
            }
        }
        
        public string Filemask
        {
            get
            {
                return FFilemask;
            }
            set
            {
                Contract.Requires(value != null);
                
                if (value != FFilemask)
                {
                    FFilemask = value;
                    Reload();
                }
            }
        }
        
        public void Reload()
        {
            FFiles = System.IO.Directory.GetFiles(FDirectory, FFilemask);
        }
        
        public double FPS
        {
            get
            {
                return FFps;
            }
            set
            {
                Contract.Requires(value >= 0.0);
                
                FFps = value;
            }
        }
        
        public int PreloadCount
        {
            get
            {
                return FFramePreloader.PreloadCount;
            }
            set
            {
                Contract.Requires(value >= 0);
                
                if (value != FFramePreloader.PreloadCount)
                {
                    FFramePreloader.Dispose();
                    FFramePreloader = new FramePreloader(value, FDevices, FLogger);
                }
            }
        }
        
        public bool Underflow
        {
            get
            {
                return FFramePreloader.Underflow;
            }
        }
        
        public bool Overflow
        {
            get
            {
                return FFramePreloader.Overflow;
            }
        }
        
        public Frame CurrentFrame
        {
            get
            {
                return FCurrentFrame;
            }
        }
        
        public int UnusedFrames
        {
            get;
            private set;
        }
        
        private int FFrameNr = 0;
        public void Seek(double time)
        {
            // Nothing to do
            if (FFiles.Length == 0)
            {
                return;
            }
            
            // Compute and normalize the frame number, as it can be any value
            int frameNr = VMath.Zmod((int) Math.Floor(time * FPS), FFiles.Length);
            //            int frameNr = VMath.Zmod(FFrameNr++, FFiles.Length);

            // Mark all frames as unused
            foreach (var frame in FFramePreloader.WorkQueue)
            {
                frame.Used = false;
            }
            
            // Mark frames we actually need as used
            var framesToPreload = new LinkedList<Frame>();
            for (int i = 0; i <= FFramePreloader.PreloadCount; i++)
            {
                int nextFrameNr = VMath.Zmod(frameNr + i, FFiles.Length);
                var frame = FFramePreloader.CreateFrame(FFiles[nextFrameNr], nextFrameNr);
                frame.Used = true;
                framesToPreload.AddLast(frame);
            }

            // Cancle unsed frames
            foreach (var frame in FFramePreloader.WorkQueue)
            {
                if (!frame.Used)
                {
                    frame.Cancel();
                }
            }
            
            // Preload frames
            foreach (var frame in framesToPreload)
            {
                FFramePreloader.Preload(frame);
            }
            
            var newFrame = framesToPreload.First.Value;

            // Nothing to do if current frame has same frame number
            if (newFrame != FCurrentFrame)
            {
                // Dispose the old frame
                if (!FCurrentFrame.Disposed)
                {
                    FCurrentFrame.Dispose();
                }
                
                // Wait till new frame is loaded
                UnusedFrames += FFramePreloader.WaitForFrame(newFrame);
                
                // Set new frame as current frame
                FCurrentFrame = newFrame;
            }
        }
    }
}
