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
using VVVV.Utils.VMath;
using VVVV.Utils.Win32;

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
        private Frame FCurrentFrame = new Frame(new FrameInfo(-1, string.Empty), Enumerable.Empty<Texture>(), null);
        
        private readonly int FPreloadCount;
        private readonly IEnumerable<Device> FDevices;
        private readonly ILogger FLogger;
        private readonly ObjectPool<Stream> FStreamPool;
        private readonly IOTaskScheduler FIOTaskScheduler;
        private readonly TexturePool FTexturePool;
        private readonly BufferBlock<FrameInfo> FFrameInfoBuffer;
        private readonly TransformBlock<FrameInfo, Tuple<FrameInfo, Stream>> FFilePreloader;
        private readonly TransformBlock<Tuple<FrameInfo, Stream>, Frame> FFramePreloader;
        private readonly BufferBlock<Frame> FFrameBuffer;
        private readonly LinkedList<FrameInfo> FScheduledFrameInfos = new LinkedList<FrameInfo>();
        
        public ImagePlayer(int preloadCount, IEnumerable<Device> devices, ILogger logger)
        {
            FPreloadCount = preloadCount;
            FDevices = devices;
            FLogger = logger;

            FStreamPool = new ObjectPool<Stream>(() => new MemoryStream());
            FTexturePool = new TexturePool();
            FIOTaskScheduler = new IOTaskScheduler();
            
            FFrameInfoBuffer = new BufferBlock<FrameInfo>();
            
            var filePreloaderOptions = new ExecutionDataflowBlockOptions()
            {
                MaxDegreeOfParallelism = DataflowBlockOptions.Unbounded,
                TaskScheduler = FIOTaskScheduler,
                BoundedCapacity = FPreloadCount + 1
            };
            
            FFilePreloader = new TransformBlock<FrameInfo, Tuple<FrameInfo, Stream>>(
                (Func<FrameInfo, Tuple<FrameInfo, Stream>>) PreloadFile,
                filePreloaderOptions
               );
            
            var framePreloaderOptions = new ExecutionDataflowBlockOptions()
            {
                MaxDegreeOfParallelism = DataflowBlockOptions.Unbounded,
                BoundedCapacity = FPreloadCount + 1
            };
            
            FFramePreloader = new TransformBlock<Tuple<FrameInfo, Stream>, Frame>(
                (Func<Tuple<FrameInfo, Stream>, Frame>) PreloadFrame,
                framePreloaderOptions
               );
            
            FFrameBuffer = new BufferBlock<Frame>();
            
            FFrameInfoBuffer.LinkTo(FFilePreloader);
            FFilePreloader.LinkTo(FFramePreloader);
            FFramePreloader.LinkTo(FFrameBuffer);
            
            FFrameInfoBuffer.Completion.ContinueWith(t => FFilePreloader.Complete());
            FFilePreloader.Completion.ContinueWith(t => FFramePreloader.Complete());
            FFramePreloader.Completion.ContinueWith(t => FFrameBuffer.Complete());
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
        
        public int PreloadCount
        {
            get
            {
                return FPreloadCount;
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
            var stallingFrameInfo = new FrameInfo(-1, string.Empty);
            stallingFrameInfo.Cancel();
            Enqueue(stallingFrameInfo);
            Dequeue(stallingFrameInfo).Dispose();
        }
        
        private void Enqueue(FrameInfo frameInfo)
        {
            if (!FScheduledFrameInfos.Contains(frameInfo))
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
        }
        
        private Frame Dequeue(FrameInfo frameInfoToDequeue)
        {
            Frame result = null;
            
            FrameInfo frameInfo = null;
            while (frameInfo != frameInfoToDequeue)
            {
                result = FFrameBuffer.Receive();
                frameInfo = result.Info;
                FScheduledFrameInfos.Remove(frameInfo);
                if (frameInfo != frameInfoToDequeue)
                {
                    result.Dispose();
                    UnusedFrames++;
                }
            }
            
            return result;
        }
        
        public void Seek(double time)
        {
            // Nothing to do
            if (FFiles.Length == 0)
            {
                return;
            }
            
            // Compute and normalize the frame number, as it can be any value
            int newFrameNr = VMath.Zmod((int) Math.Floor(time * FPS), FFiles.Length);
            
            // Cancel scheduled frames with a lower frame number
            foreach (var frameInfo in FScheduledFrameInfos)
            {
                if (frameInfo.FrameNr < newFrameNr)
                {
                    frameInfo.Cancel();
                }
            }
            
            var frameInfosToPreload =
                from frameNr in Enumerable.Range(newFrameNr, FPreloadCount + 1)
                let normalizedFrameNr = VMath.Zmod(frameNr, FFiles.Length)
                select new FrameInfo(normalizedFrameNr, FFiles[normalizedFrameNr]);
            
            foreach (var frameInfo in frameInfosToPreload)
            {
                Enqueue(frameInfo);
            }
            
            var newFrameInfo = frameInfosToPreload.First();
            
            if (newFrameInfo != FCurrentFrame.Info)
            {
                FCurrentFrame.Dispose();
                FCurrentFrame = Dequeue(newFrameInfo);
            }
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
