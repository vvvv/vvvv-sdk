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
using VVVV.Utils.Win32;

namespace VVVV.Nodes.ImagePlayer
{
    public class FramePreloader : IDisposable
    {
        class InternalFrame : Frame
        {
            private readonly Dictionary<Device, Texture> FTextures = new Dictionary<Device, Texture>();
            private readonly Device[] FDevices;
            private readonly FramePreloader FFactory;
            private readonly string FFilename;
            private readonly HiPerfTimer FTimer = new HiPerfTimer();
            private MemoryPool.Memory FMemory;
            //                        private Stream FStream;
            
            public InternalFrame(FramePreloader factory, string filename, int frameNr, Device[] devices)
            {
                FFactory = factory;
                FFilename = filename;
                FrameNr = frameNr;
                FDevices = devices;
            }
            
            public string Filename
            {
                get
                {
                    return FFilename;
                }
            }
            
            public override void DoIO()
            {
                FTimer.Start();
                
                Contract.Requires(FMemory != null);
                
                Token.ThrowIfCancellationRequested();
                
                using (var fileStream = new FileStream(FFilename, FileMode.Open, FileAccess.Read))
                {
                    FMemory = MemoryPool.GetStream((int) fileStream.Length);
                    
                    var memoryStream = FMemory.Stream;
                    var buffer = new byte[4096];
                    while (fileStream.Position < fileStream.Length)
                    {
                        Token.ThrowIfCancellationRequested();
                        
                        int numBytesRead = fileStream.Read(buffer, 0, buffer.Length);
                        memoryStream.Write(buffer, 0, numBytesRead);
                    }
                    //                    fileStream.CopyTo(FMemory.Memory);
                }
                
                FTimer.Stop();
                
                DurationIO = FTimer.Duration;
                //                using (var fileStream = new FileStream(FFilename, FileMode.Open, FileAccess.Read))
                //                {
                //                    FStream = new MemoryStream((int) fileStream.Length);
                //                    fileStream.CopyTo(FStream);
                //                }
                //                                FStream = new FileStream(FFilename, FileMode.Open, FileAccess.Read);
            }
            
            public override void DoLoad()
            {
                foreach (var device in FDevices)
                {
                    FTextures[device] = CreateTextureForDevice(device);
                }
            }
            
            private Texture CreateTextureForDevice(Device device)
            {
                FTimer.Start();
                
                Contract.Requires(FMemory != null);
                
                Token.ThrowIfCancellationRequested();
                
                var stream = FMemory.Stream;
                //                                var stream = FStream;
                
                stream.Position = 0;
                
                var info = ImageInformation.FromStream(stream, true);
                
                Token.ThrowIfCancellationRequested();

                var result = Texture.FromStream(
                    device,
                    stream,
                    info.Width,
                    info.Height,
                    info.MipLevels,
                    Usage.None & ~Usage.AutoGenerateMipMap,
                    info.Format,
                    Pool.Default,
                    Filter.None,
                    Filter.None,
                    0);
                
                FTimer.Stop();
                
                DurationTexture += FTimer.Duration;
                
                return result;
            }
            
            public override Texture GetTexture(Device device)
            {
                Contract.Requires(FMemory != null);
                
                Texture result = null;
                if (!FTextures.TryGetValue(device, out result))
                {
                    result = CreateTextureForDevice(device);
                    FTextures[device] = result;
                }
                
                return result;
            }
            
            public override void Dispose()
            {
                FFactory.RemoveFrame(this);
                
                if (FMemory != null)
                {
                    FMemory.Dispose();
                    FMemory = null;
                }
                //                                if (FStream != null)
                //                                {
                //                                    FStream.Dispose();
                //                                }
                
                foreach (var texture in FTextures.Values)
                {
                    texture.Dispose();
                }
                
                FTextures.Clear();
                
                base.Dispose();
            }
            
            public override string ToString()
            {
                return string.Format("Frame {0} ({1})", FrameNr, Filename);
            }

        }
        
        class EmptyInternalFrame : Frame
        {
            public EmptyInternalFrame()
            {
                FrameNr = -1;
            }
            
            public override Texture GetTexture(Device device)
            {
                return null;
            }
            
            public override void DoIO()
            {
                // Do nothing
            }
            
            public override void DoLoad()
            {
                // Do nothing
            }
        }
        
        private readonly Dictionary<string, Frame> FCreatedFrames = new Dictionary<string, Frame>();
        private readonly CancellationTokenSource FCancellationTokenSource;
        private readonly CancellationToken FCancellationToken;
        private readonly int FPreloadCount;
        private readonly Task FIOTask;
        private readonly Task FPreloadTask;
        private readonly BlockingCollection<Frame> FRequestQueue;
        private readonly BlockingCollection<Frame> FIOQueue;
        private readonly BlockingCollection<Frame> FPreloadQueue;
        private readonly IEnumerable<Device> FDevices;
        private readonly ILogger FLogger;
        
        public FramePreloader(int preloadCount, IEnumerable<Device> devices, ILogger logger)
        {
            FPreloadCount = preloadCount;
            FDevices = devices;
            FLogger = logger;
            
            FRequestQueue = new BlockingCollection<Frame>(preloadCount + 1);
            FIOQueue = new BlockingCollection<Frame>();
            FPreloadQueue = new BlockingCollection<Frame>();
            
            FCancellationTokenSource = new CancellationTokenSource();
            FCancellationToken = FCancellationTokenSource.Token;
            
            FIOTask = Task.Factory.StartNew(ProcessRequestQueue, FCancellationToken);
            FPreloadTask = Task.Factory.StartNew(ProcessIOQueue, FCancellationToken);
        }
        
        public void Dispose()
        {
            foreach (var frame in WorkQueue)
            {
                frame.Cancel();
            }
            
            FCancellationTokenSource.Cancel();
            
            try
            {
                try
                {
                    FIOTask.Wait();
                }
                catch (AggregateException)
                {
                    // Should only contain the OperationCanceledException exception.
                    if (FIOTask.Exception != null)
                    {
                        throw FIOTask.Exception;
                    }
                }
                
                try
                {
                    FPreloadTask.Wait();
                }
                catch (AggregateException)
                {
                    // Should only contain the OperationCanceledException exception.
                    if (FPreloadTask.Exception != null)
                    {
                        throw FPreloadTask.Exception;
                    }
                }
            }
            finally
            {
                // Dispose remaining frames
                foreach (var frame in WorkQueue)
                {
                    frame.Dispose();
                }
                
                FRequestQueue.Dispose();
                FIOQueue.Dispose();
                FPreloadQueue.Dispose();
                
                FIOTask.Dispose();
                FPreloadTask.Dispose();
                
                FCancellationTokenSource.Dispose();
            }
        }
        
        public static Frame EmptyFrame
        {
            get
            {
                return new EmptyInternalFrame();
            }
        }
        
        public int PreloadCount
        {
            get
            {
                return FPreloadCount;
            }
        }
        
        public bool Underflow
        {
            get
            {
                return FPreloadQueue.Count == 0;
            }
        }
        
        public bool Overflow
        {
            get
            {
                return FPreloadQueue.Count == FPreloadCount;
            }
        }
        
        public Frame CreateFrame(string filename, int frameNr)
        {
            Frame result = null;
            
            lock (FCreatedFrames)
            {
                if (!FCreatedFrames.TryGetValue(filename, out result))
                {
                    result = new InternalFrame(this, filename, frameNr, FDevices.ToArray());
                    FCreatedFrames[filename] = result;
                }
            }
            
            return result;
        }
        
        public void Preload(Frame frame)
        {
            if (!frame.Scheduled)
            {
                FRequestQueue.Add(frame);
                frame.Scheduled = true;
            }
        }
        
        public int WaitForFrame(Frame frame)
        {
            Contract.Requires(frame != null, "Frame to wait for must not be null.");
            
            int unusedFrames = 0;
            Frame preloadedFrame = null;
            while (preloadedFrame != frame)
            {
                preloadedFrame = FPreloadQueue.Take(FCancellationToken);
                if (preloadedFrame != frame)
                {
                    // Oops, seems we did too much work.
                    preloadedFrame.Dispose();
                    unusedFrames++;
                }
            }
            return unusedFrames;
        }
        
        public IEnumerable<Frame> WorkQueue
        {
            get
            {
                lock (FCreatedFrames)
                {
                    return FCreatedFrames.Values.ToArray();
                }
            }
        }
        
        private void RemoveFrame(InternalFrame frame)
        {
            lock (FCreatedFrames)
            {
                FCreatedFrames.Remove(frame.Filename);
            }
        }
        
        private readonly HiPerfTimer FTimer1 = new HiPerfTimer();
        private void ProcessRequestQueue()
        {
            foreach (var frame in FRequestQueue.GetConsumingEnumerable(FCancellationToken))
            {
                try
                {
                    frame.DoIO();
                }
                catch (OperationCanceledException)
                {
                    frame.Dispose();
                    continue;
                }
                catch (Exception e)
                {
                    FLogger.Log(e);
                }
                
                FIOQueue.Add(frame);
            }
        }
        
        private readonly HiPerfTimer FTimer2 = new HiPerfTimer();
        private void ProcessIOQueue()
        {
            foreach (var frame in FIOQueue.GetConsumingEnumerable(FCancellationToken))
            {
                try
                {
                    frame.DoLoad();
                }
                catch (OperationCanceledException)
                {
                    frame.Dispose();
                    continue;
                }
                catch (Exception e)
                {
                    FLogger.Log(e);
                }
                
                FPreloadQueue.Add(frame);
            }
        }
    }
}
