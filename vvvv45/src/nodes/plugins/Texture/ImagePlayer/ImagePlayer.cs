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
using VVVV.Core.Logging;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V2.EX9;
using VVVV.Utils.VMath;
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
        public const int DEFAULT_BUFFER_SIZE = 32 * 1024;
        
        private volatile List<string> FFiles = new List<string>();
        
        private readonly int FThreadsIO;
        private readonly int FThreadsTexture;
        private readonly ILogger FLogger;
        private readonly TexturePool FTexturePool;
        private readonly MemoryPool FMemoryPool;
        private readonly BufferBlock<FrameInfo> FFrameInfoBuffer;
        private readonly TransformBlock<FrameInfo, Tuple<FrameInfo, Stream>> FFilePreloader;
        private readonly TransformBlock<Tuple<FrameInfo, Stream>, Frame> FFramePreloader;
        private readonly LinkedList<FrameInfo> FScheduledFrameInfos = new LinkedList<FrameInfo>();
        private readonly Dictionary<FrameInfo, Frame> FPreloadedFrames = new Dictionary<FrameInfo, Frame>();
        private readonly IDXDeviceService FDeviceService;
        
        public ImagePlayer(
            int threadsIO,
            int threadsTexture,
            ILogger logger,
            MemoryPool memoryPool,
            TexturePool texturePool,
            IDXDeviceService deviceService
           )
        {
            FThreadsIO = threadsIO;
            FThreadsTexture = threadsTexture;
            FLogger = logger;

            FTexturePool = texturePool;
            FMemoryPool = memoryPool;
            FDeviceService = deviceService;
            
            FFrameInfoBuffer = new BufferBlock<FrameInfo>();
            
            var filePreloaderOptions = new ExecutionDataflowBlockOptions()
            {
                MaxDegreeOfParallelism = threadsIO <= 0 ? DataflowBlockOptions.Unbounded : threadsIO,
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

            FDeviceService.DeviceDisabled += HandleDeviceDisabledOrRemoved;
            FDeviceService.DeviceRemoved += HandleDeviceDisabledOrRemoved;

            Directories = new Spread<string>(0);
            Filemasks = new Spread<string>(0);
        }

        void HandleDeviceDisabledOrRemoved(object sender, DeviceEventArgs e)
        {
            var device = e.Device;
            if (FTexturePool.HasTextureForDevice(device))
            {
                FTexturePool.Release(device);
            }
        }

        public void Dispose()
        {
            FDeviceService.DeviceDisabled -= HandleDeviceDisabledOrRemoved;
            FDeviceService.DeviceRemoved -= HandleDeviceDisabledOrRemoved;

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
                FFiles = ScanDirectories(Directories, Filemasks);
            }
            finally
            {
                Flush();
                ClearPreloadedFrames();
            }
        }

        Task FScanTask;
        public void ScanDirectoriesAsync()
        {
            FFiles = new List<string>();
            // Wait till an ongoing directory scan is complete
            if (FScanTask != null)
            {
                FScanTask.Wait();
            }
            FScanTask = ScanDirectoriesAsync(Directories, Filemasks)
                .ContinueWith(t =>
                    {
                        FFiles = t.Result;
                        FScanTask = null;
                    }
                );
        }

        static List<string> ScanDirectories(ISpread<string> directories, ISpread<string> filemasks)
        {
            var files = new List<string>();
            for (int i = 0; i < directories.SliceCount; i++)
            {
                try
                {
                    var directoryInfo = new DirectoryInfo(directories[i]);
                    var filesInDir = directoryInfo.EnumerateFiles(filemasks[i])
                        .Where(f => !f.Attributes.HasFlag(FileAttributes.Hidden))
                        .OrderBy(f => f.Name)
                        .Select(f => f.FullName);
                    files.AddRange(filesInDir);
                }
                catch (DirectoryNotFoundException)
                {
                }
            }
            return files;
        }

        static Task<List<string>> ScanDirectoriesAsync(ISpread<string> directories, ISpread<string> filemasks)
        {
            return Task.Factory.StartNew(
                () => ScanDirectories(directories, filemasks),
                CancellationToken.None, 
                TaskCreationOptions.None,
                TaskScheduler.Default
            );
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
            if (frameInfo != null)
                FPreloadedFrames.Remove(frameInfo);
            frameInfo.Dispose();
            frame.Dispose();
        }
        
        private FrameInfo CreateFrameInfo(string filename, int bufferSize, EX9.Format preferedFormat)
        {
            return new FrameInfo(filename, bufferSize, (SharpDX.Direct3D9.Format)preferedFormat);
        }
        
        private static Frame GetEmptyFrame()
        {
            return new Frame(GetEmptyFrameInfo(), (fi, dev) => { return null; });
        }
        
        private static FrameInfo GetEmptyFrameInfo()
        {
            return new FrameInfo(string.Empty, DEFAULT_BUFFER_SIZE, SharpDX.Direct3D9.Format.Unknown);
        }

        private bool IsScheduled(FrameInfo frameInfo)
        {
            return FScheduledFrameInfos.Any(fi => fi == frameInfo && !fi.IsCanceled);
        }

        private bool IsPreloaded(FrameInfo frameInfo)
        {
            return FPreloadedFrames.ContainsKey(frameInfo);
        }
        
        public ISpread<Frame> Preload(
            ISpread<int> visibleFrameIndices,
            ISpread<int> preloadFrameNrs,
            int bufferSize,
            EX9.Format preferedFormat,
            out int frameCount,
            out double durationIO,
            out double durationTexture,
            out int unusedFrames,
            ref ISpread<bool> loadedFrames)
        {
            var files = FFiles;
            frameCount = files.Count;
            durationIO = 0.0;
            durationTexture = 0.0;
            unusedFrames = 0;
            
            // Nothing to do
            if (frameCount == 0)
            {
                loadedFrames.SliceCount = 0;
                return new Spread<Frame>(0);
            }

            // Map frame numbers to file names
            var preloadFiles = preloadFrameNrs.Select(
                frameNr => 
                {
                    var fileName = files[VMath.Zmod(frameNr, files.Count)];
                    return CreateFrameInfo(fileName, bufferSize, preferedFormat);
                })
                .ToArray();

            // Dispose previously loaded frames
            foreach (var file in FPreloadedFrames.Keys.ToArray())
            {
                if (!preloadFiles.Contains(file))
                {
                    var frame = FPreloadedFrames[file];
                    Dispose(frame);
                }
            }

            // Ensure that there are as much dequeues as enqueues (or we'll run out of memory)
            while (FScheduledFrameInfos.Count > preloadFrameNrs.SliceCount)
            {
                var frame = Dequeue();
                var frameInfo = frame.Metadata;
                if (!preloadFiles.Contains(frameInfo) || frameInfo.IsCanceled)
                {
                    // Not needed anymore
                    Dispose(frame);
                    unusedFrames++;
                }
                else
                {
                    FPreloadedFrames.Add(frameInfo, frame);
                }
            }
            
            // Cancel unused scheduled frames
            foreach (var frameInfo in FScheduledFrameInfos.Where(fi => !fi.IsCanceled))
            {
                if (!preloadFiles.Contains(frameInfo))
                {
                    frameInfo.Cancel();
                }
            }

            // Schedule new frames
            foreach (var file in preloadFiles)
            {
                if (!IsScheduled(file) && !IsPreloaded(file))
                {
                    Enqueue(file);
                }
            }

            // Write the "is loaded" state
            loadedFrames.SliceCount = preloadFrameNrs.SliceCount;
            for (int i = 0; i < loadedFrames.SliceCount; i++)
            {
                var file = preloadFiles[i];
                if (!IsPreloaded(file))
                {
                    var frameInfo = FScheduledFrameInfos.First(fi => fi == file);
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
            var visibleFiles = preloadFiles.Length > 0 
                ? visibleFrameIndices.Select(i => preloadFiles[VMath.Zmod(i, preloadFiles.Length)]) 
                : Enumerable.Empty<FrameInfo>();
            foreach (var file in visibleFiles)
            {
                while (!IsPreloaded(file))
                {
                    var frame = Dequeue();
                    var frameInfo = frame.Metadata;
                    if (!preloadFiles.Contains(frameInfo) || frameInfo.IsCanceled)
                    {
                        // Not needed anymore
                        Dispose(frame);
                        unusedFrames++;
                    }
                    else
                    {
                        FPreloadedFrames.Add(frameInfo, frame);
                    }
                }
                var visibleFrame = FPreloadedFrames[file];
                var visibleFrameInfo = visibleFrame.Metadata;
                durationIO += visibleFrameInfo.DurationIO;
                durationTexture += visibleFrameInfo.DurationTexture;
                visibleFrames.Add(visibleFrame);
            }

            // Release textures of non visible frames
            foreach (var frame in FPreloadedFrames.Values.Where(f => !visibleFrames.Contains(f)))
            {
                frame.Dispose();
            }
            
            return visibleFrames;
        }

        private EX9.Texture CreateTexture(FrameInfo frameInfo, EX9.Device device)
        {
            using (var sharpDxDevice = new SharpDX.Direct3D9.Device(device.ComPointer))
            {
                ((SharpDX.IUnknown)sharpDxDevice).AddReference(); // Dispose will call release
                var decoder = frameInfo.Decoder;
                if (decoder != null)
                {
                    using (var texture = decoder.Decode(sharpDxDevice))
                    {
                        ((SharpDX.IUnknown)texture).AddReference(); // Dispose will call release
                        return SlimDX.Direct3D9.Texture.FromPointer(texture.NativePointer);
                    }
                }
                else
                {
                    return null;
                }
            }
        }

        private void UpdateTexture(FrameInfo frameInfo, EX9.Texture texture)
        {

        }

        private void DestroyTexture(FrameInfo frameInfo, EX9.Texture texture, DestroyReason reason)
        {
            if (texture != null)
            {
                // Put the texture back in the pool
                FTexturePool.PutTexture(texture);
                if (reason == DestroyReason.DeviceLost)
                {
                    // Release this device if it got lost
                    FTexturePool.Release(texture.Device);
                }
            }
        }

        private SharpDX.Direct3D9.Texture CreateTextureForDecoder(SharpDX.Direct3D9.Device device, int width, int height, int levels, SharpDX.Direct3D9.Format format, SharpDX.Direct3D9.Usage usage)
        {
            var pool = SharpDX.Direct3D9.Pool.Default;            
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
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            
            var filename = frameInfo.Filename;
            var bufferSize = frameInfo.BufferSize;
            var cancellationToken = frameInfo.Token;
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
            
            stopwatch.Stop();
            frameInfo.DurationIO = stopwatch.Elapsed.TotalSeconds;
            
            return Tuple.Create(frameInfo, memoryStream);
        }
        
        private Frame PreloadFrame(Tuple<FrameInfo, Stream> tuple)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            
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
                frameInfo.Decoder = FrameDecoder.Create(frameInfo.Filename, CreateTextureForDecoder, FMemoryPool, stream, frameInfo.PreferedFormat);
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
                if (stream != null && cleanupStream)
                {
                    FMemoryPool.StreamPool.PutStream(stream);
                }
            }
            
            stopwatch.Stop();
            frameInfo.DurationTexture = stopwatch.Elapsed.TotalSeconds;
            
            return frame;
        }
    }
}
