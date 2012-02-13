using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Threading.Tasks.Schedulers;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using VVVV.Core.Logging;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V2.EX9;
using VVVV.Utils.VMath;
using VVVV.Utils.Win32;
using EX9 = SlimDX.Direct3D9;
using Frame = VVVV.PluginInterfaces.V2.EX9.DXResource<SlimDX.Direct3D9.Texture, VVVV.Nodes.ImagePlayer.FrameInfo>;

namespace VVVV.Nodes.ImagePlayer
{
    /// <summary>
    /// Given a directory containing a sequence of images this class will
    /// try to preload the next couple of images based on the given frame
    /// number in order to provide fast access.
    /// </summary>
    class ImagePlayer : IDisposable
    {
        public const string DEFAULT_FILEMASK = "*";
        public const int DEFAULT_BUFFER_SIZE = 0x2000;
        
        private string FDirectory = string.Empty;
        private string FFilemask = DEFAULT_FILEMASK;
        private string[] FFiles = new string[0];
        private int FUnusedFrames;
        
        private readonly int FThreadsIO;
        private readonly int FThreadsTexture;
        private readonly List<EX9.Device> FDevices = new List<EX9.Device>();
        private readonly ILogger FLogger;
        private readonly IDXDeviceService FDeviceService;
        private readonly ObjectPool<MemoryStream> FStreamPool;
        private readonly TexturePool FTexturePool;
        private readonly MemoryPool FMemoryPool;
        private readonly BufferBlock<FrameInfo> FFrameInfoBuffer;
        private readonly TransformBlock<FrameInfo, Tuple<FrameInfo, MemoryStream>> FFilePreloader;
        private readonly TransformBlock<Tuple<FrameInfo, MemoryStream>, Frame> FFramePreloader;
        private readonly BufferBlock<Frame> FFrameBuffer;
        private readonly LinkedList<FrameInfo> FScheduledFrameInfos = new LinkedList<FrameInfo>();
        private readonly Spread<Frame> FVisibleFrames = new Spread<Frame>(0);
        
        public ImagePlayer(
            int threadsIO, 
            int threadsTexture, 
            ILogger logger, 
            IDXDeviceService deviceService, 
            IOTaskScheduler ioTaskScheduler,
            MemoryPool memoryPool,
            ObjectPool<MemoryStream> streamPool
           )
        {
            FThreadsIO = threadsIO;
            FThreadsTexture = threadsTexture;
            FLogger = logger;
            FDeviceService = deviceService;
            
            FDeviceService.DeviceDisabled += HandleDeviceDisabled;
            FDeviceService.DeviceRemoved += HandleDeviceRemoved;
            
            FStreamPool = streamPool;
            FTexturePool = new TexturePool();
            FMemoryPool = memoryPool;
            
            FFrameInfoBuffer = new BufferBlock<FrameInfo>();
            
            var filePreloaderOptions = new ExecutionDataflowBlockOptions()
            {
                MaxDegreeOfParallelism = threadsIO <= 0 ? DataflowBlockOptions.Unbounded : threadsIO,
                TaskScheduler = ioTaskScheduler,
                BoundedCapacity = threadsIO <= 0 ? DataflowBlockOptions.Unbounded : threadsIO
            };
            
            FFilePreloader = new TransformBlock<FrameInfo, Tuple<FrameInfo, MemoryStream>>(
                (Func<FrameInfo, Tuple<FrameInfo, MemoryStream>>) PreloadFile,
                filePreloaderOptions
               );
            
            var framePreloaderOptions = new ExecutionDataflowBlockOptions()
            {
                MaxDegreeOfParallelism = threadsTexture <= 0 ? DataflowBlockOptions.Unbounded : threadsTexture,
                BoundedCapacity = threadsTexture <= 0 ? DataflowBlockOptions.Unbounded : threadsTexture
            };
            
            FFramePreloader = new TransformBlock<Tuple<FrameInfo, MemoryStream>, Frame>(
                (Func<Tuple<FrameInfo, MemoryStream>, Frame>) PreloadFrame,
                framePreloaderOptions
               );
            
            FFrameBuffer = new BufferBlock<Frame>();
            
            var linkOptions = new DataflowLinkOptions()
            {
                PropagateCompletion = true
            };
            
            FFrameInfoBuffer.LinkTo(FFilePreloader, linkOptions);
            FFilePreloader.LinkTo(FFramePreloader, linkOptions);
            FFramePreloader.LinkTo(FFrameBuffer, linkOptions);
        }

        public void Dispose()
        {
            // Cancel all scheduled frames
            foreach (var frameInfo in FScheduledFrameInfos)
            {
                frameInfo.Cancel();
            }
            
            // Dispose all visible frames
            foreach (var frame in FVisibleFrames)
            {
                frame.Dispose();
                frame.Metadata.Dispose();
            }
            
            // Flush the pipeline
            Flush();
            
            // Mark head of pipeline as completed
            FFrameInfoBuffer.Complete();
            
            try
            {
                // Wait for last elements in pipeline till completed
                FFrameBuffer.Completion.Wait();
            }
            catch (AggregateException)
            {
                // TODO: Check if these are only exceptions of type OperationCancledException
            }
            finally
            {
                foreach (var stream in FStreamPool.ToArrayAndClear())
                {
                    stream.Dispose();
                }
                
                FTexturePool.Dispose();
            }
            
            FDeviceService.DeviceDisabled -= HandleDeviceDisabled;
            FDeviceService.DeviceRemoved -= HandleDeviceRemoved;
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
        
        public int ThreadsIO
        {
            get
            {
                return FThreadsIO;
            }
        }
        
        public int ThreadsTexture
        {
            get
            {
                return FThreadsTexture;
            }
        }
        
        private void Enqueue(FrameInfo frameInfo)
        {
            if (FFrameInfoBuffer.Post(frameInfo))
            {
                FScheduledFrameInfos.AddLast(frameInfo);
            }
            else
            {
                throw new InternalBufferOverflowException(
                    string.Format("Enqueue for {0} failed!", frameInfo));
            }
        }
        
        private Frame Dequeue(FrameInfo frameInfoToDequeue)
        {
            Frame frame = null;
            
            FrameInfo frameInfo = null;
            while (frameInfo != frameInfoToDequeue)
            {
                frame = Dequeue();
                frameInfo = frame.Metadata;
                
                if (frameInfo != frameInfoToDequeue)
                {
                    frame.Dispose();
                    frameInfo.Dispose();
                    FUnusedFrames++;
                }
            }
            
            return frame;
        }
        
        private Frame Dequeue()
        {
            FScheduledFrameInfos.RemoveFirst();
            return FFrameBuffer.Receive();
        }
        
        private FrameInfo CreateFrameInfo(int frameNr, int bufferSize)
        {
            frameNr = VMath.Zmod(frameNr, FFiles.Length);
            return new FrameInfo(frameNr, FFiles[frameNr], bufferSize);
        }
        
        private static Frame GetEmptyFrame()
        {
            return new Frame(GetEmptyFrameInfo(), (fi, dev) => { return null; });
        }
        
        private static FrameInfo GetEmptyFrameInfo()
        {
            return new FrameInfo(-1, string.Empty, DEFAULT_BUFFER_SIZE);
        }
        
        public ISpread<Frame> Preload(
            ISpread<int> visibleFrameNrs,
            ISpread<int> preloadFrameNrs,
            int bufferSize,
            out double durationIO,
            out double durationTexture,
            out int unusedFrames)
        {
            durationIO = 0.0;
            durationTexture = 0.0;
            FUnusedFrames = 0;
            
            // Nothing to do
            if (FFiles.Length == 0)
            {
                unusedFrames = FUnusedFrames;
                return new Spread<Frame>(0);
            }
            
            // Dispose old visible frames
            FVisibleFrames.Resize(
                visibleFrameNrs.SliceCount,
                frame =>
                {
                    frame.Dispose();
                    frame.Metadata.Dispose();
                }
               );
            
            var visibleFramesEnumerator = FVisibleFrames.GetEnumerator();
            for (int i = 0; i < visibleFrameNrs.SliceCount; i++)
            {
                var newFrameInfo = CreateFrameInfo(visibleFrameNrs[i], bufferSize);
                
                while (visibleFramesEnumerator.MoveNext())
                {
                    var currentFrame = visibleFramesEnumerator.Current ?? GetEmptyFrame();
                    var currentFrameInfo = currentFrame.Metadata;
                    
                    Debug.Assert(!currentFrameInfo.IsCanceled);
                    if (currentFrameInfo == newFrameInfo)
                    {
                        FVisibleFrames[i] = currentFrame;
                        durationIO += currentFrameInfo.DurationIO;
                        durationTexture += currentFrameInfo.DurationTexture;
                        newFrameInfo.Dispose();
                        newFrameInfo = null;
                        break;
                    }
                    else
                    {
                        currentFrame.Dispose();
                        currentFrameInfo.Dispose();
                    }
                }
                
                if (newFrameInfo != null)
                {
                    while (FScheduledFrameInfos.Count > 0)
                    {
                        var frame = Dequeue();
                        var frameInfo = frame.Metadata;
                        
                        if (frameInfo == newFrameInfo && !frameInfo.IsCanceled)
                        {
                            FVisibleFrames[i] = frame;
                            durationIO += frameInfo.DurationIO;
                            durationTexture += frameInfo.DurationTexture;
                            newFrameInfo.Dispose();
                            newFrameInfo = null;
                            break;
                        }
                        else
                        {
                            frame.Dispose();
                            frameInfo.Dispose();
                            FUnusedFrames++;
                        }
                    }
                }
                
                if (newFrameInfo != null)
                {
                    Enqueue(newFrameInfo);
                    FVisibleFrames[i] = Dequeue(newFrameInfo);
                    durationIO += newFrameInfo.DurationIO;
                    durationTexture += newFrameInfo.DurationTexture;
                    newFrameInfo = null;
                }
                
                if (newFrameInfo != null)
                {
                    newFrameInfo.Dispose();
                }
            }
            
            var canceledFrameInfos = FScheduledFrameInfos.TakeWhile(frameInfo => frameInfo.IsCanceled);
            foreach (var frameInfo in canceledFrameInfos.ToArray())
            {
                var frame = Dequeue();
                frame.Dispose();
                frame.Metadata.Dispose();
                FUnusedFrames++;
            }
            
            var scheduledFrameInfosEnumerator = FScheduledFrameInfos.Where(frameInfo => !frameInfo.IsCanceled).GetEnumerator();
            for (int i = 0; i < preloadFrameNrs.SliceCount; i++)
            {
                var newFrameInfo = CreateFrameInfo(preloadFrameNrs[i], bufferSize);
                
                while (scheduledFrameInfosEnumerator.MoveNext())
                {
                    var scheduledFrameInfo = scheduledFrameInfosEnumerator.Current;
                    
                    if (scheduledFrameInfo == newFrameInfo)
                    {
                        newFrameInfo.Dispose();
                        newFrameInfo = null;
                        break;
                    }
                    
                    scheduledFrameInfo.Cancel();
                }
                
                if (newFrameInfo != null)
                {
                    Enqueue(newFrameInfo);
                }
            }
            
            unusedFrames = FUnusedFrames;
            
            return FVisibleFrames;
        }
        
        private EX9.Texture CreateTexture(FrameInfo frameInfo, EX9.Device device, FrameDecoder decoder)
        {
            lock (FDevices)
            {
                if (!FDevices.Contains(device))
                {
                    FDevices.Add(device);
                }
            }
            
            if (decoder == null)
            {
                using (var stream = new FileStream(frameInfo.Filename, FileMode.Open, FileAccess.Read))
                {
                    decoder = FrameDecoder.Create(frameInfo.Filename, FTexturePool, FMemoryPool, stream);
                    return decoder.Decode(device);
                }
            }
            
            return decoder.Decode(device);
        }
        
        private void UpdateTexture(FrameInfo frameInfo, EX9.Texture texture)
        {
            
        }
        
        private void DestroyTexture(FrameInfo frameInfo, EX9.Texture texture, DestroyReason reason)
        {
            switch (reason)
            {
                case DestroyReason.Dispose:
                    FTexturePool.PutTexture(texture);
                    break;
                case DestroyReason.DeviceLost:
                    Flush();
                    FDevices.Remove(texture.Device);
                    FTexturePool.PutTexture(texture);
                    FTexturePool.Release(texture.Device);
                    break;
            }
        }
        
        private void HandleDeviceDisabled(object sender, DeviceEventArgs e)
        {
            ReleaseDevice(e.Device);
        }
        
        private void HandleDeviceRemoved(object sender, DeviceEventArgs e)
        {
            ReleaseDevice(e.Device);
        }
        
        private void ReleaseDevice(EX9.Device device)
        {
            bool doRelease = false;
            lock (FDevices)
            {
                doRelease = FDevices.Contains(device);
            }
            if (doRelease)
            {
                Flush();
                FDevices.Remove(device);
                FTexturePool.Release(device);
            }
        }
        
        private void Flush()
        {
            var emptyFrameInfo = GetEmptyFrameInfo();
            emptyFrameInfo.Cancel();
            Enqueue(emptyFrameInfo);
            var emptyFrame = Dequeue(emptyFrameInfo);
            emptyFrame.Dispose();
            emptyFrameInfo.Dispose();
        }
        
        private Tuple<FrameInfo, MemoryStream> PreloadFile(FrameInfo frameInfo)
        {
            // TODO: Consider using CopyStreamToStreamAsync from TPL extensions
            var timer = new HiPerfTimer();
            timer.Start();
            
            var filename = frameInfo.Filename;
            var bufferSize = frameInfo.BufferSize;
            var cancellationToken = frameInfo.Token;
            var memoryStream = FStreamPool.GetObject();
            var buffer = FMemoryPool.GetMemory(bufferSize);
            
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                
                using (var fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize))
                {
                    memoryStream.Position = 0;
                    if (fileStream.Length > memoryStream.Capacity)
                    {
                        memoryStream.Capacity = (int) fileStream.Length;
                    }
                    if (fileStream.Length != memoryStream.Length)
                    {
                        memoryStream.SetLength(fileStream.Length);
                    }
                    
                    while (fileStream.Position < fileStream.Length)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        int numBytesRead = fileStream.Read(buffer, 0, buffer.Length);
                        memoryStream.Write(buffer, 0, numBytesRead);
                    }
                    
                    memoryStream.Position = 0;
                }
            }
            catch (OperationCanceledException)
            {
                // That's fine
            }
            catch (Exception e)
            {
                // Log the exception
                FLogger.Log(e);
            }
            finally
            {
                // Put the buffer back in the pool so other blocks can use it
                FMemoryPool.PutMemory(buffer);
            }
            
            timer.Stop();
            frameInfo.DurationIO = timer.Duration;
            
            return Tuple.Create(frameInfo, memoryStream);
        }
        
        private Frame PreloadFrame(Tuple<FrameInfo, MemoryStream> tuple)
        {
            var timer = new HiPerfTimer();
            timer.Start();
            
            var frameInfo = tuple.Item1;
            var stream = tuple.Item2;
            var cancellationToken = frameInfo.Token;
            FrameDecoder decoder = null;
            
            var frame = new Frame(
                frameInfo,
                (fi, device) =>
                {
                    return CreateTexture(fi, device, decoder);
                },
                UpdateTexture,
                DestroyTexture);
            
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                decoder = FrameDecoder.Create(frameInfo.Filename, FTexturePool, FMemoryPool, stream);
                
                EX9.Device[] devices = null;
                lock (FDevices)
                {
                    devices = FDevices.ToArray();
                }
                
                foreach (var device in devices)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    frame.UpdateResource(device);
                }
            }
            catch (OperationCanceledException)
            {
                // That's fine
            }
            catch (Exception e)
            {
                // Log the exception
                FLogger.Log(e);
            }
            finally
            {
                decoder = null;
                // We're done with this frame, put used memory stream back in the pool
                FStreamPool.PutObject(stream);
            }
            
            timer.Stop();
            frameInfo.DurationTexture = timer.Duration;
            
            return frame;
        }
    }
}
