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
    public class ImagePlayer : IDisposable
    {
        public const string DEFAULT_FILEMASK = "*";
        
        private string FDirectory = string.Empty;
        private string FFilemask = DEFAULT_FILEMASK;
        private string[] FFiles = new string[0];
        
        private readonly int FQueueSize;
        private readonly List<EX9.Device> FDevices = new List<EX9.Device>();
        private readonly ILogger FLogger;
        private readonly ObjectPool<Stream> FStreamPool;
        private readonly IOTaskScheduler FIOTaskScheduler;
        private readonly TexturePool FTexturePool;
        private readonly BufferBlock<FrameInfo> FFrameInfoBuffer;
        private readonly TransformBlock<FrameInfo, Tuple<FrameInfo, Stream>> FFilePreloader;
        private readonly TransformBlock<Tuple<FrameInfo, Stream>, Frame> FFramePreloader;
        private readonly BufferBlock<Frame> FFrameBuffer;
        private readonly LinkedList<FrameInfo> FScheduledFrameInfos = new LinkedList<FrameInfo>();
        private readonly Spread<Frame> FVisibleFrames = new Spread<Frame>(0);
        
        public ImagePlayer(int queueSize, ILogger logger)
        {
            FQueueSize = queueSize;
            FLogger = logger;
            
            FStreamPool = new ObjectPool<Stream>(() => new MemoryStream());
            FTexturePool = new TexturePool();
            FIOTaskScheduler = new IOTaskScheduler();
            
            FFrameInfoBuffer = new BufferBlock<FrameInfo>();
            
            var filePreloaderOptions = new ExecutionDataflowBlockOptions()
            {
                MaxDegreeOfParallelism = DataflowBlockOptions.Unbounded,
                TaskScheduler = FIOTaskScheduler,
                BoundedCapacity = Math.Max(1, FQueueSize / 2)
            };
            
            FFilePreloader = new TransformBlock<FrameInfo, Tuple<FrameInfo, Stream>>(
                (Func<FrameInfo, Tuple<FrameInfo, Stream>>) PreloadFile,
                filePreloaderOptions
               );
            
            var framePreloaderOptions = new ExecutionDataflowBlockOptions()
            {
                MaxDegreeOfParallelism = DataflowBlockOptions.Unbounded,
                BoundedCapacity = FQueueSize
            };
            
            FFramePreloader = new TransformBlock<Tuple<FrameInfo, Stream>, Frame>(
                (Func<Tuple<FrameInfo, Stream>, Frame>) PreloadFrame,
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
            
            // Stall the pipeline
            Stall();
            
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
                FIOTaskScheduler.Dispose();
            }
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
        
        public int QueueSize
        {
            get
            {
                return FQueueSize;
            }
        }
        
        public int UnusedFrames
        {
            get;
            private set;
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
                    UnusedFrames++;
                }
            }
            
            return frame;
        }
        
        private Frame Dequeue()
        {
            FScheduledFrameInfos.RemoveFirst();
            return FFrameBuffer.Receive();
        }
        
        private FrameInfo CreateFrameInfo(int frameNr)
        {
            frameNr = VMath.Zmod(frameNr, FFiles.Length);
            return new FrameInfo(frameNr, FFiles[frameNr]);
        }
        
        private static Frame GetEmptyFrame()
        {
            return new Frame(GetEmptyFrameInfo(), (fi, dev) => { return null; });
        }
        
        private static FrameInfo GetEmptyFrameInfo()
        {
            return new FrameInfo(-1, string.Empty);
        }
        
        public ISpread<Frame> Preload(
            ISpread<int> visibleFrameNrs,
            ISpread<int> preloadFrameNrs,
            ref ISpread<double> durationIO,
            ref ISpread<double> durationTexture,
            out int scheduledFrameCount,
            out int canceledFrameCount)
        {
            // Nothing to do
            if (FFiles.Length == 0)
            {
                scheduledFrameCount = 0;
                canceledFrameCount = 0;
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
            
            durationIO.SliceCount = visibleFrameNrs.SliceCount;
            durationTexture.SliceCount = visibleFrameNrs.SliceCount;
            
            var visibleFramesEnumerator = FVisibleFrames.GetEnumerator();
            for (int i = 0; i < visibleFrameNrs.SliceCount; i++)
            {
                var newFrameInfo = CreateFrameInfo(visibleFrameNrs[i]);
                
                while (visibleFramesEnumerator.MoveNext())
                {
                    var currentFrame = visibleFramesEnumerator.Current ?? GetEmptyFrame();
                    var currentFrameInfo = currentFrame.Metadata;
                    
                    Debug.Assert(!currentFrameInfo.IsCanceled);
                    if (currentFrameInfo == newFrameInfo)
                    {
                        FVisibleFrames[i] = currentFrame;
                        durationIO[i] = currentFrameInfo.DurationIO;
                        durationTexture[i] = currentFrameInfo.DurationTexture;
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
                            durationIO[i] = frameInfo.DurationIO;
                            durationTexture[i] = frameInfo.DurationTexture;
                            newFrameInfo.Dispose();
                            newFrameInfo = null;
                            break;
                        }
                        else
                        {
                            frame.Dispose();
                            frameInfo.Dispose();
                        }
                    }
                }
                
                if (newFrameInfo != null)
                {
                    Enqueue(newFrameInfo);
                    FVisibleFrames[i] = Dequeue(newFrameInfo);
                    durationIO[i] = newFrameInfo.DurationIO;
                    durationTexture[i] = newFrameInfo.DurationTexture;
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
            }
            
            var scheduledFrameInfosEnumerator = FScheduledFrameInfos.Where(frameInfo => !frameInfo.IsCanceled).GetEnumerator();
            for (int i = 0; i < preloadFrameNrs.SliceCount; i++)
            {
                var newFrameInfo = CreateFrameInfo(preloadFrameNrs[i]);
                
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
            
            scheduledFrameCount = FScheduledFrameInfos.Where(frameInfo => !frameInfo.IsCanceled).Count();
            canceledFrameCount = FScheduledFrameInfos.Where(frameInfo => frameInfo.IsCanceled).Count();
            
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
                    decoder = FrameDecoder.Create(frameInfo.Filename, FTexturePool, stream);
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
                    Stall();
                    
                    lock (FDevices)
                    {
                        FDevices.Remove(texture.Device);
                    }
                    
                    FTexturePool.PutTexture(texture);
                    FTexturePool.Release(texture.Device);
                    break;
            }
        }
        
        private void Stall()
        {
            var dummyFrameInfo = GetEmptyFrameInfo();
            dummyFrameInfo.Cancel();
            Enqueue(dummyFrameInfo);
            var dummyFrame = Dequeue(dummyFrameInfo);
            dummyFrame.Dispose();
            dummyFrameInfo.Dispose();
        }
        
        private Tuple<FrameInfo, Stream> PreloadFile(FrameInfo frameInfo)
        {
            // TODO: Consider using CopyStreamToStreamAsync from TPL extensions
            var timer = new HiPerfTimer();
            timer.Start();
            
            var filename = frameInfo.Filename;
            var cancellationToken = frameInfo.Token;
            var memoryStream = FStreamPool.GetObject();
            var buffer = MemoryPool.GetMemory(0x2000);
            
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                
                using (var fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read))
                {
                    memoryStream.Position = 0;
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
                MemoryPool.PutMemory(buffer);
            }
            
            timer.Stop();
            frameInfo.DurationIO = timer.Duration;
            
            return Tuple.Create(frameInfo, memoryStream);
        }
        
        private Frame PreloadFrame(Tuple<FrameInfo, Stream> tuple)
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

                decoder = FrameDecoder.Create(frameInfo.Filename, FTexturePool, stream);
                
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
