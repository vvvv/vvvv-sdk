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
using SlimDX.Direct3D9;
using VVVV.Core.Logging;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V2.EX9;
using VVVV.Utils.VMath;
using VVVV.Utils.Win32;
using TextureResource = VVVV.PluginInterfaces.V2.EX9.DXResource<SlimDX.Direct3D9.Texture, VVVV.Nodes.ImagePlayer.Frame>;

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
        public const double DEFAULT_FPS = 25.0;
        
        private string FDirectory = string.Empty;
        private string FFilemask = DEFAULT_FILEMASK;
        private double FFps = DEFAULT_FPS;
        private string[] FFiles = new string[0];
        private Frame FCurrentFrame;
        
        private readonly int FQueueSize;
        private readonly List<Device> FDevices = new List<Device>();
        private readonly ILogger FLogger;
        private readonly ObjectPool<Stream> FStreamPool;
        private readonly IOTaskScheduler FIOTaskScheduler;
        private readonly TexturePool FTexturePool;
        private readonly BufferBlock<FrameInfo> FFrameInfoBuffer;
        private readonly TransformBlock<FrameInfo, Tuple<FrameInfo, Stream>> FFilePreloader;
        private readonly TransformBlock<Tuple<FrameInfo, Stream>, Frame> FFramePreloader;
//        private readonly BufferBlock<Frame> FCancledFrameBuffer;
        private readonly BufferBlock<Frame> FFrameBuffer;
        private readonly Dictionary<int, FrameInfo> FFrameInfos = new Dictionary<int, FrameInfo>();
        private readonly Dictionary<int, FrameInfo> FScheduledFrameInfos = new Dictionary<int, FrameInfo>();
        
        public ImagePlayer(int queueSize, ILogger logger)
        {
            FQueueSize = queueSize;
            FLogger = logger;
            
            FCurrentFrame = new Frame(new FrameInfo(this, -1, string.Empty), Enumerable.Empty<Texture>(), null);

            FStreamPool = new ObjectPool<Stream>(() => new MemoryStream());
            FTexturePool = new TexturePool();
            FIOTaskScheduler = new IOTaskScheduler();
            
            FFrameInfoBuffer = new BufferBlock<FrameInfo>();
            
            var filePreloaderOptions = new ExecutionDataflowBlockOptions()
            {
                MaxDegreeOfParallelism = DataflowBlockOptions.Unbounded,
                TaskScheduler = FIOTaskScheduler,
                BoundedCapacity = FQueueSize + 1
            };
            
            FFilePreloader = new TransformBlock<FrameInfo, Tuple<FrameInfo, Stream>>(
                (Func<FrameInfo, Tuple<FrameInfo, Stream>>) PreloadFile,
                filePreloaderOptions
               );
            
            var framePreloaderOptions = new ExecutionDataflowBlockOptions()
            {
                MaxDegreeOfParallelism = DataflowBlockOptions.Unbounded,
                BoundedCapacity = FQueueSize + 1
            };
            
            FFramePreloader = new TransformBlock<Tuple<FrameInfo, Stream>, Frame>(
                (Func<Tuple<FrameInfo, Stream>, Frame>) PreloadFrame,
                framePreloaderOptions
               );
            
//            FCancledFrameBuffer = new BufferBlock<Frame>();
            
            FFrameBuffer = new BufferBlock<Frame>();
            
            var linkOptions = new DataflowLinkOptions()
            {
                PropagateCompletion = true
            };
            
            FFrameInfoBuffer.LinkTo(FFilePreloader, linkOptions);
            FFilePreloader.LinkTo(FFramePreloader, linkOptions);
            FFramePreloader.LinkTo(FFrameBuffer, linkOptions);
//            FFramePreloader.LinkTo(FCancledFrameBuffer, linkOptions, frame => frame.Info.IsCanceled);
            
//            FFrameInfoBuffer.Completion.ContinueWith(t => FFilePreloader.Complete());
//            FFilePreloader.Completion.ContinueWith(t => FFramePreloader.Complete());
//            FFramePreloader.Completion.ContinueWith(t => FFrameBuffer.Complete());
        }
        
        public void Dispose()
        {
            Flush();
            
            FCurrentFrame.Dispose();
            
            // Mark head of pipeline as completed
            FFrameInfoBuffer.Complete();
            
            try
            {
                // Wait for last element in pipeline till completed
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
        
        public int QueueSize
        {
            get
            {
                return FQueueSize;
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
        
        public void Flush()
        {
            var stallingFrameInfo = new FrameInfo(this, -1, string.Empty);
            stallingFrameInfo.Cancel();
            Enqueue(stallingFrameInfo);
            Dequeue(stallingFrameInfo).Dispose();
        }
        
        private void Enqueue(FrameInfo frameInfo)
        {
            if (!FScheduledFrameInfos.ContainsKey(frameInfo.FrameNr))
            {
                if (FFrameInfoBuffer.Post(frameInfo))
                {
                    FScheduledFrameInfos.Add(frameInfo.FrameNr, frameInfo);
                }
                else
                {
                    throw new InternalBufferOverflowException(
                        string.Format("Enqueue for {0} failed!", frameInfo));
                }
            }
        }
        
        private Frame Dequeue(FrameInfo frameInfoToDequeue)
        {
            Frame frame = null;
            
            FrameInfo frameInfo = null;
            while (frameInfo != frameInfoToDequeue)
            {
                frame = FFrameBuffer.Receive();
                frameInfo = frame.Info;
                FScheduledFrameInfos.Remove(frameInfo.FrameNr);
                if (frameInfo != frameInfoToDequeue)
                {
                    frame.Dispose();
                    UnusedFrames++;
                }
            }
            
            return frame;
        }
        
        private void Cancel(FrameInfo frameInfo)
        {
            frameInfo.Cancel();
            var frame = Dequeue(frameInfo);
            frame.Dispose();
            FScheduledFrameInfos.Remove(frameInfo.FrameNr);
            FFrameInfos.Remove(frameInfo.FrameNr);
        }
        
        private FrameInfo CreateFrameInfo(int frameNr)
        {
            FrameInfo frameInfo = null;
            
            frameNr = VMath.Zmod(frameNr, FFiles.Length);
            if (!FFrameInfos.TryGetValue(frameNr, out frameInfo))
            {
                frameInfo = new FrameInfo(this, frameNr, FFiles[frameNr]);
                FFrameInfos[frameNr] = frameInfo;
            }
            
            return frameInfo;
        }
        
        internal void DestroyFrameInfo(FrameInfo frameInfo)
        {
            FFrameInfos.Remove(frameInfo.FrameNr);
        }
        
        private readonly Spread<TextureResource> FVisibleResources = new Spread<TextureResource>(0);
        
        public ISpread<TextureResource> Preload(
            ISpread<int> visibleFrameNrs,
            ISpread<int> preloadFrameNrs)
        {
            // Nothing to do
            if (FFiles.Length == 0)
            {
                return new Spread<TextureResource>(0);
            }
            
            // Mark all scheduled frames as unused
            foreach (var frameInfo in FScheduledFrameInfos.Values)
            {
                frameInfo.IsInUse = false;
            }
            
            var frameInfosToPreload =
                from frameNr in visibleFrameNrs.Concat(preloadFrameNrs)
                select CreateFrameInfo(frameNr);
            
            // Preload frames
            foreach (var frameInfo in frameInfosToPreload)
            {
                // Mark frame as used
                frameInfo.IsInUse = true;
            }
            
            // Cancel unused frames
            foreach (var frameInfo in FScheduledFrameInfos.Values.ToArray())
            {
                if (!frameInfo.IsInUse && !frameInfo.IsCanceled)
                {
                    Cancel(frameInfo);
                }
            }
            
            // Preload frames
            foreach (var frameInfo in frameInfosToPreload)
            {
                // Schedule it
                Enqueue(frameInfo);
            }
            
            // Dispose unused texture resources
            for (int i = visibleFrameNrs.SliceCount; i < FVisibleResources.SliceCount; i++)
            {
                FVisibleResources[i].Dispose();
                FVisibleResources[i] = null;
            }
            
            FVisibleResources.SliceCount = visibleFrameNrs.SliceCount;
            
            var visibleFrames = visibleFrameNrs.Select(frameNr => CreateFrameInfo(frameNr)).ToArray();
            for (int i = 0; i < FVisibleResources.SliceCount; i++)
            {
                var frameInfo = visibleFrames[i];
                var resource = FVisibleResources[i];
                if (resource != null && resource.Metadata.Info != frameInfo)
                {
                    resource.Dispose();
                    resource = null;
                }
                if (resource == null)
                {
                    var frame = Dequeue(frameInfo);
                    resource = new TextureResource(frame, CreateTexture, UpdateTexture, DestroyTexture);
                    FVisibleResources[i] = resource;
                }
            }
            
            return FVisibleResources;
        }
        
        private Texture CreateTexture(Frame frame, Device device)
        {
            lock (FDevices)
            {
                if (!FDevices.Contains(device))
                {
                    FDevices.Add(device);
                }
            }
            
            return frame.GetTexture(device);
        }
        
        private void UpdateTexture(Frame frame, Texture texture)
        {
            
        }
        
        private void DestroyTexture(Frame frame, Texture texture, bool onlyUnmanaged)
        {
            if (!onlyUnmanaged)
            {
                lock (FDevices)
                {
                    FDevices.Remove(texture.Device);
                }
            }
            
            frame.Dispose();
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
                // Mark this frame as canceled
                frameInfo.Cancel();
                
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
            var textures = new List<Texture>();
            
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                var decoder = FrameDecoder.Create(frameInfo, FTexturePool, stream);
                
                Device[] devices = null;
                lock (FDevices)
                {
                    devices = FDevices.ToArray();
                }
                
                foreach (var device in devices)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var texture = decoder.Decode(device);
                    textures.Add(texture);
                }
            }
            catch (OperationCanceledException)
            {
                // That's fine
            }
            catch (Exception e)
            {
                // Mark this frame as canceled
                frameInfo.Cancel();
                
                // Do some cleanup
                foreach (var texture in textures)
                {
                    FTexturePool.PutTexture(texture);
                }
                textures.Clear();
                
                // Log the exception
                FLogger.Log(e);
            }
            finally
            {
                // We're done with this frame, put used memory stream back in the pool
                FStreamPool.PutObject(stream);
            }
            
            timer.Stop();
            frameInfo.DurationTexture = timer.Duration;
            
            return new Frame(frameInfo, textures, FTexturePool);
        }
    }
}
