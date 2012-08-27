using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.IO.MemoryMappedFiles;
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
using Frame = VVVV.PluginInterfaces.V2.EX9.TextureResource<VVVV.Nodes.ImagePlayer.FrameInfo>;

namespace VVVV.Nodes.ImagePlayer
{
    /// <summary>
    /// Given a directory containing a sequence of images this class will
    /// try to preload the next couple of images based on the given frame
    /// number in order to provide fast access.
    /// </summary>
    class ImagePlayer : IDisposable
    {
        public const string DEFAULT_FILEMASK = "*.*";
        public const int DEFAULT_BUFFER_SIZE = 0xFF00;
        
        private string[] FFiles = new string[0];
        
        private readonly int FThreadsIO;
        private readonly int FThreadsTexture;
        private readonly ILogger FLogger;
        private readonly ObjectPool<MemoryStream> FStreamPool;
        private readonly TexturePool FTexturePool;
        private readonly MemoryPool FMemoryPool;
        private readonly BufferBlock<FrameInfo> FFrameInfoBuffer;
        private readonly TransformBlock<FrameInfo, Tuple<FrameInfo, Stream>> FFilePreloader;
        private readonly TransformBlock<Tuple<FrameInfo, Stream>, Frame> FFramePreloader;
        private readonly LinkedList<FrameInfo> FScheduledFrameInfos = new LinkedList<FrameInfo>();
        private readonly Dictionary<string, Frame> FPreloadedFrames = new Dictionary<string, Frame>();
        
        public ImagePlayer(
            int threadsIO,
            int threadsTexture,
            ILogger logger,
            IOTaskScheduler ioTaskScheduler,
            MemoryPool memoryPool,
            ObjectPool<MemoryStream> streamPool
           )
        {
            FThreadsIO = threadsIO;
            FThreadsTexture = threadsTexture;
            FLogger = logger;
            
            FStreamPool = streamPool;
            FTexturePool = new TexturePool();
            FMemoryPool = memoryPool;
            
            FFrameInfoBuffer = new BufferBlock<FrameInfo>();
            
            var filePreloaderOptions = new ExecutionDataflowBlockOptions()
            {
                MaxDegreeOfParallelism = threadsIO <= 0 ? DataflowBlockOptions.Unbounded : threadsIO,
                TaskScheduler = ioTaskScheduler,
                BoundedCapacity = threadsIO <= 0 ? DataflowBlockOptions.Unbounded : 2 * threadsIO
            };
            
            FFilePreloader = new TransformBlock<FrameInfo, Tuple<FrameInfo, Stream>>(
                (Func<FrameInfo, Tuple<FrameInfo, Stream>>) PreloadFile,
                filePreloaderOptions
               );
            
            var framePreloaderOptions = new ExecutionDataflowBlockOptions()
            {
                MaxDegreeOfParallelism = threadsTexture <= 0 ? DataflowBlockOptions.Unbounded : threadsTexture
            };
            
            FFramePreloader = new TransformBlock<Tuple<FrameInfo, Stream>, Frame>(
                (Func<Tuple<FrameInfo, Stream>, Frame>) PreloadFrame,
                framePreloaderOptions
               );
            
            var linkOptions = new DataflowLinkOptions()
            {
                PropagateCompletion = true
            };
            
            FFrameInfoBuffer.LinkTo(FFilePreloader, linkOptions);
            FFilePreloader.LinkTo(FFramePreloader, linkOptions);
        }

        public void Dispose()
        {
            // Cancel all scheduled frames
            foreach (var frameInfo in FScheduledFrameInfos)
            {
                frameInfo.Cancel();
            }
            
            // Dispose all visible frames
            ClearPreloadedFrames();
            
            // Flush the pipeline
            Flush();
            
            // Mark head of pipeline as completed
            FFrameInfoBuffer.Complete();
            
            try
            {
                // Wait for last elements in pipeline till completed
                FFramePreloader.Completion.Wait();
            }
            catch (AggregateException)
            {
                // TODO: Check if these are only exceptions of type OperationCancledException
            }
            finally
            {
                FTexturePool.Dispose();
            }
        }
        
        public ISpread<string> Directories
        {
            get;
            set;
        }
        
        public ISpread<string> Filemasks
        {
            get;
            set;
        }
        
        public void Reload()
        {
            try
            {
                var files = new Spread<string>[Directories.SliceCount];
                int maxCount = 1;
                for (int i = 0; i < files.Length; i++)
                {
                    files[i] = System.IO.Directory.GetFiles(Directories[i], Filemasks[i]).OrderBy(f => f).ToSpread();
                    maxCount = maxCount.CombineWith(files[i]);
                }
                FFiles = new string[files.Length * maxCount];
                for (int i = 0; i < files.Length; i++)
                {
                    int k = i;
                    for (int j = 0; j < maxCount; j++)
                    {
                        FFiles[k] = files[i][j];
                        k += files.Length;
                    }
                }
            }
            catch (DirectoryNotFoundException)
            {
                FFiles = new string[0];
            }
            finally
            {
                Flush();
                ClearPreloadedFrames();
                FTexturePool.Clear();
            }
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
                    Dispose(frame);
                }
            }
            
            return frame;
        }
        
        private Frame Dequeue()
        {
            FScheduledFrameInfos.RemoveFirst();
            return FFramePreloader.Receive();
        }

        private void Dispose(Frame frame)
        {
            var frameInfo = frame.Metadata;
            var filename = frameInfo.Filename;
            if (!string.IsNullOrEmpty(filename))
                FPreloadedFrames.Remove(filename);
            frameInfo.Dispose();
            frame.Dispose();
        }
        
        private FrameInfo CreateFrameInfo(string filename, int bufferSize)
        {
            return new FrameInfo(filename, bufferSize);
        }
        
        private static Frame GetEmptyFrame()
        {
            return new Frame(GetEmptyFrameInfo(), (fi, dev) => { return null; });
        }
        
        private static FrameInfo GetEmptyFrameInfo()
        {
            return new FrameInfo(string.Empty, DEFAULT_BUFFER_SIZE);
        }

        private bool IsScheduled(string file)
        {
            return FScheduledFrameInfos.Any(frameInfo => frameInfo.Filename == file && !frameInfo.IsCanceled);
        }

        private bool IsPreloaded(string file)
        {
            return FPreloadedFrames.ContainsKey(file);
        }
        
        public ISpread<Frame> Preload(
            ISpread<int> visibleFrameIndices,
            ISpread<int> preloadFrameNrs,
            int bufferSize,
            out int frameCount,
            out double durationIO,
            out double durationTexture,
            out int unusedFrames,
            ref ISpread<bool> loadedFrames)
        {
            frameCount = FFiles.Length;
            durationIO = 0.0;
            durationTexture = 0.0;
            unusedFrames = 0;
            
            // Nothing to do
            if (FFiles.Length == 0)
            {
                loadedFrames.SliceCount = 0;
                return new Spread<Frame>(0);
            }

            // Map frame numbers to file names
            var preloadFiles = preloadFrameNrs.Select(frameNr => FFiles[VMath.Zmod(frameNr, FFiles.Length)]).ToArray();
            
            // Cancel unused scheduled frames
            foreach (var frameInfo in FScheduledFrameInfos.Where(fi => !fi.IsCanceled))
            {
                if (!preloadFiles.Contains(frameInfo.Filename))
                {
                    frameInfo.Cancel();
                }
            }

            // Schedule new frames
            foreach (var file in preloadFiles)
            {
                if (!IsScheduled(file) && !IsPreloaded(file))
                {
                    var frameInfo = CreateFrameInfo(file, bufferSize);
                    Enqueue(frameInfo);
                }
            }

            // Write the "is loaded" state
            loadedFrames.SliceCount = preloadFrameNrs.SliceCount;
            for (int i = 0; i < loadedFrames.SliceCount; i++)
            {
                var file = preloadFiles[i];
                if (!IsPreloaded(file))
                {
                    var frameInfo = FScheduledFrameInfos.First(fi => fi.Filename == file);
                    loadedFrames[i] = frameInfo.IsLoaded;
                }
                else
                {
                    loadedFrames[i] = true;
                }
            }

            // Wait for the visible frames (and dipose unused ones)
            var visibleFrames = new Spread<Frame>(0);
            // Map frame numbers to file names
            var visibleFiles = preloadFiles.Length > 0 ? visibleFrameIndices.Select(i => preloadFiles[VMath.Zmod(i, preloadFiles.Length)]) : Enumerable.Empty<string>();
            foreach (var file in visibleFiles)
            {
                while (!IsPreloaded(file))
                {
                    var frame = Dequeue();
                    var frameInfo = frame.Metadata;
                    if (!preloadFiles.Contains(frameInfo.Filename) || frameInfo.IsCanceled)
                    {
                        // Not needed anymore
                        Dispose(frame);
                        unusedFrames++;
                    }
                    else
                    {
                        FPreloadedFrames.Add(frameInfo.Filename, frame);
                    }
                }
                var visibleFrame = FPreloadedFrames[file];
                var visibleFrameInfo = visibleFrame.Metadata;
                durationIO += visibleFrameInfo.DurationIO;
                durationTexture += visibleFrameInfo.DurationTexture;
                visibleFrames.Add(visibleFrame);
            }

            // Ensure that there are as much dequeues as enqueues (or we'll run out of memory)
            while (FScheduledFrameInfos.Count > preloadFrameNrs.SliceCount)
            {
                var frame = Dequeue();
                var frameInfo = frame.Metadata;
                if (!preloadFiles.Contains(frameInfo.Filename) || frameInfo.IsCanceled)
                {
                    // Not needed anymore
                    Dispose(frame);
                    unusedFrames++;
                }
                else
                {
                    FPreloadedFrames.Add(frameInfo.Filename, frame);
                }
            }

            // Dispose previously loaded frames
            foreach (var file in FPreloadedFrames.Keys.ToArray())
            {
                if (!preloadFiles.Contains(file))
                {
                    var frame = FPreloadedFrames[file];
                    Dispose(frame);
                }
            }
            
            return visibleFrames;
        }

        private TSharp ToSharpDX<TSharp, TSlim>(TSlim slim)
            where TSharp : SharpDX.CppObject
            where TSlim : SlimDX.ComObject
        {
            var sharp = slim.Tag as TSharp;
            if (sharp == null)
            {
                sharp = SharpDX.CppObject.FromPointer<TSharp>(slim.ComPointer);
                slim.Tag = sharp;
            }
            return sharp;
        }

        private TSlim ToSlimDX<TSharp, TSlim>(TSharp sharp, Func<IntPtr, TSlim> constructor)
            where TSharp : SharpDX.CppObject
            where TSlim : SlimDX.ComObject
        {
            var slim = constructor(sharp.NativePointer);
            slim.Tag = sharp;
            return slim;
        }

        private EX9.Texture CreateTexture(FrameInfo frameInfo, EX9.Device device)
        {
            using (var sharpDxDevice = new SharpDX.Direct3D9.Device(device.ComPointer))
            {
                ((SharpDX.IUnknown)sharpDxDevice).AddReference(); // Dispose will call release
                using (var texture = frameInfo.Decoder.Decode(sharpDxDevice))
                {
                    ((SharpDX.IUnknown)texture).AddReference(); // Dispose will call release
                    return SlimDX.Direct3D9.Texture.FromPointer(texture.NativePointer);
                }
            }
        }

        private void UpdateTexture(FrameInfo frameInfo, EX9.Texture texture)
        {

        }

        private void DestroyTexture(FrameInfo frameInfo, EX9.Texture texture, DestroyReason reason)
        {
            // Put the texture back in the pool
            FTexturePool.PutTexture(texture);
            if (reason == DestroyReason.DeviceLost)
            {
                // Release this device if it got lost
                FTexturePool.Release(texture.Device);
            }
        }

        private SharpDX.Direct3D9.Texture CreateTextureForDecoder(SharpDX.Direct3D9.Device device, int width, int height, int levels, SharpDX.Direct3D9.Format format)
        {
            var pool = SharpDX.Direct3D9.Pool.Default;
            var usage = SharpDX.Direct3D9.Usage.Dynamic & ~SharpDX.Direct3D9.Usage.AutoGenerateMipMap;
            var texture = FTexturePool.GetTexture(EX9.Device.FromPointer(device.NativePointer), width, height, levels, (EX9.Usage)usage, (EX9.Format)format, (EX9.Pool)pool);
            return new SharpDX.Direct3D9.Texture(texture.ComPointer);
        }

        private void ClearPreloadedFrames()
        {
            foreach (var frame in FPreloadedFrames.Values)
            {
                frame.Metadata.Dispose();
                frame.Dispose();
            }
            FPreloadedFrames.Clear();
        }
        
        private void Flush()
        {
            foreach (var frameInfo in FScheduledFrameInfos)
            {
                frameInfo.Cancel();
            }
            var emptyFrameInfo = GetEmptyFrameInfo();
            emptyFrameInfo.Cancel();
            Enqueue(emptyFrameInfo);
            var emptyFrame = Dequeue(emptyFrameInfo);
            emptyFrame.Dispose();
            emptyFrameInfo.Dispose();
        }
        
        private Tuple<FrameInfo, Stream> PreloadFile(FrameInfo frameInfo)
        {
            // TODO: Consider using CopyStreamToStreamAsync from TPL extensions
            var timer = new HiPerfTimer();
            timer.Start();
            
            var filename = frameInfo.Filename;
            var bufferSize = frameInfo.BufferSize;
            var cancellationToken = frameInfo.Token;
            //var memoryStream = FStreamPool.GetObject();
            Stream memoryStream = null;
            var buffer = FMemoryPool.ManagedPool.GetMemory(bufferSize);

            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                
                using (var fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, FileOptions.SequentialScan))
                {
                    var length = fileStream.Length;
                    memoryStream = FMemoryPool.StreamPool.GetStream((int)length);
                    //memoryStream.Position = 0;
                    
                    while (fileStream.Position < length)
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
                FMemoryPool.ManagedPool.PutMemory(buffer);
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

            var frame = new Frame(
                frameInfo,
                CreateTexture,
                UpdateTexture,
                DestroyTexture);

            var cleanupStream = false;
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                frameInfo.Decoder = FrameDecoder.Create(frameInfo.Filename, CreateTextureForDecoder, FMemoryPool, stream);
            }
            catch (OperationCanceledException)
            {
                // That's fine, just make sure to cleanup
                cleanupStream = true;
            }
            catch (Exception e)
            {
                // Log the exception
                FLogger.Log(e);
                // And don't forget to cleanup
                cleanupStream = true;
            }
            finally
            {
                // We're done with this frame, put used memory stream back in the pool
                //FStreamPool.PutObject(stream);
                if (stream != null && cleanupStream)
                {
                    FMemoryPool.StreamPool.PutStream(stream);
                }
            }
            
            timer.Stop();
            frameInfo.DurationTexture = timer.Duration;
            
            return frame;
        }
    }
}
