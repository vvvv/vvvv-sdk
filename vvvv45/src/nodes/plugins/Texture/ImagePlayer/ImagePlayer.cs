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
            
            // Nothing to do if current frame has same frame number
            if (FCurrentFrame.FrameNr == frameNr)
            {
                return;
            }
            
            // We need the frame difference for prediction
            int frameDifference = frameNr - CurrentFrame.FrameNr;
            
            // Dispose old frame
            FCurrentFrame.Dispose();
            
            // Setup this frame
            FCurrentFrame = FFramePreloader.Preload(FFiles[frameNr], frameNr);
            
            // Preload next frames
            for (int i = 0; i < FFramePreloader.PreloadCount; i++)
            {
                frameNr += frameDifference;
                frameNr = VMath.Zmod(frameNr, FFiles.Length);
                FFramePreloader.Preload(FFiles[frameNr], frameNr);
            }
            
            // Wait till frame is loaded
            UnusedFrames += FFramePreloader.WaitForFrame(FCurrentFrame);

//foreach (var file in FFiles)
//{
//    using (var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read))
//    {
//        using (var memoryStream = new MemoryStream())
//        {
//            fileStream.CopyTo(memoryStream);
//        }
//    }
//}
        }
    }
}
