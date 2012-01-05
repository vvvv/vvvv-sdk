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
            private Stream FStream;
            
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
            
            public override void LoadFromDisk()
            {
                using (var fileStream = new FileStream(FFilename, FileMode.Open, FileAccess.Read))
                {
                    FMemoryStream = new MemoryStream((int) fileStream.Length);
                    fileStream.CopyTo(FMemoryStream);
                }
//                FStream = new FileStream(FFilename, FileMode.Open, FileAccess.Read);
            }
            
            public override void CreateTextures()
            {
                foreach (var device in FDevices)
                {
                    FTextures[device] = CreateTextureForDevice(device);
                }
            }
            
            private Texture CreateTextureForDevice(Device device)
            {
                FStream.Position = 0;
                
                var info = ImageInformation.FromStream(FStream, true);

                return Texture.FromStream(
                    device,
                    FStream,
                    info.Width,
                    info.Height,
                    info.MipLevels,
                    Usage.None & ~Usage.AutoGenerateMipMap,
                    info.Format,
                    Pool.Default,
                    Filter.None,
                    Filter.None,
                    0);
            }
            
            public override Texture GetTexture(Device device)
            {
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
                if (FStream != null)
                {
                    FStream.Dispose();
                }
                foreach (var texture in FTextures.Values)
                {
                    texture.Dispose();
                }
                FTextures.Clear();
                FFactory.Destroy(this);
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
            
            public override void LoadFromDisk()
            {
                // Do nothing
            }
            
            public override void CreateTextures()
            {
                // Do nothing
            }
            
            public override void Dispose()
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
            FIOQueue = new BlockingCollection<Frame>(preloadCount + 1);
            FPreloadQueue = new BlockingCollection<Frame>(preloadCount + 1);
            
            FCancellationTokenSource = new CancellationTokenSource();
            FCancellationToken = FCancellationTokenSource.Token;
            
            FIOTask = Task.Factory.StartNew(ProcessRequestQueue, FCancellationToken);
            FPreloadTask = Task.Factory.StartNew(ProcessIOQueue, FCancellationToken);
        }
        
        public void Dispose()
        {
            FCancellationTokenSource.Cancel();
            try
            {
                Task.WaitAll(FIOTask, FPreloadTask);
            }
            catch (AggregateException)
            {
                // Should only contain the OperationCanceledException exception.
                if (FIOTask.Exception != null)
                {
                    throw FIOTask.Exception;
                }
                if (FPreloadTask.Exception != null)
                {
                    throw FPreloadTask.Exception;
                }
            }
            finally
            {
                foreach (var frame in FCreatedFrames.Values.ToArray())
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
        
        public Frame Preload(string filename, int frameNr)
        {
            Frame result = null;
            
            if (!FCreatedFrames.TryGetValue(filename, out result))
            {
                result = new InternalFrame(this, filename, frameNr, FDevices.ToArray());
                FCreatedFrames[filename] = result;
                FRequestQueue.Add(result);
            }
            
            Debug.Assert(frameNr == result.FrameNr, "Invalid frame number.");
            
            return result;
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
        
        private void Destroy(InternalFrame frame)
        {
            FCreatedFrames.Remove(frame.Filename);
        }
        
        private void ProcessRequestQueue()
        {
            foreach (var frame in FRequestQueue.GetConsumingEnumerable(FCancellationToken))
            {
                try 
                {
                    frame.LoadFromDisk();
                } 
                catch (Exception e)
                {
                    FLogger.Log(e);
                }
                finally
                {
                    FIOQueue.Add(frame);
                }
            }
        }
        
        private void ProcessIOQueue()
        {
            foreach (var frame in FIOQueue.GetConsumingEnumerable(FCancellationToken))
            {
                try 
                {
                    frame.CreateTextures();
                } 
                catch (Exception e)
                {
                    FLogger.Log(e);
                }
                finally
                {
                    FPreloadQueue.Add(frame);
                }
            }
        }
    }
}
