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
            private readonly CancellationTokenSource FCancellationTokenSource = new CancellationTokenSource();
            private readonly Task FTask;
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
                
                factory.FCancellationToken.Register(Cancel);
                FTask = new Task(Load, Token);
            }
            
            protected CancellationToken Token
            {
                get
                {
                    return FCancellationTokenSource.Token;
                }
            }
            
            public string Filename
            {
                get
                {
                    return FFilename;
                }
            }
            
            private bool FIsStarted;
            public override bool IsStarted
            {
                get
                {
                    return FIsStarted;
                }
            }
            
            public override bool IsCanceled
            {
                get
                {
                    return FTask.IsCanceled;
                }
            }
            
            public override void Start()
            {
                FIsStarted = true;
                FTask.Start();
            }
            
            public override void Wait(CancellationToken token)
            {
                FTask.Wait(token);
            }
            
            public override void Cancel()
            {
                FCancellationTokenSource.Cancel();
            }
            
            private void Load()
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
                
                foreach (var texture in FTextures.Values)
                {
                    texture.Dispose();
                }
                
                FTextures.Clear();
                FTask.Dispose();
                FCancellationTokenSource.Dispose();
            }
            
            public override string ToString()
            {
                return string.Format("Frame {0} ({1})", FrameNr, Filename);
            }

        }
        
        class EmptyInternalFrame : Frame
        {
            private bool FIsStarted;
            private bool FIsCanceled;
            
            public EmptyInternalFrame()
            {
                FrameNr = -1;
            }
            
            public override bool IsStarted
            {
                get
                {
                    return FIsStarted;
                }
            }
            
            public override bool IsCanceled
            {
                get
                {
                    return FIsCanceled;
                }
            }
            
            public override Texture GetTexture(Device device)
            {
                return null;
            }
            
            public override void Start()
            {
                FIsStarted = true;
            }
            
            public override void Wait(CancellationToken token)
            {
                // Do nothing
            }
            
            public override void Cancel()
            {
                FIsCanceled = true;
            }
            
            public override void Dispose()
            {
                // Do nothing
            }
        }
        
        private readonly Dictionary<string, Frame> FCreatedFrames = new Dictionary<string, Frame>();
        private readonly CancellationTokenSource FCancellationTokenSource;
        private readonly CancellationToken FCancellationToken;
        private readonly IEnumerable<Device> FDevices;
        private readonly ILogger FLogger;
        private readonly TaskScheduler FTaskScheduler;
        
        public FramePreloader(IEnumerable<Device> devices, ILogger logger)
        {
            FDevices = devices;
            FLogger = logger;
            
            FCancellationTokenSource = new CancellationTokenSource();
            FCancellationToken = FCancellationTokenSource.Token;
            
            FTaskScheduler = TaskScheduler.Default;
        }
        
        public void Dispose()
        {
            FCancellationTokenSource.Cancel();
            
            foreach (var frame in WorkQueue)
            {
                try
                {
                    frame.Wait(CancellationToken.None);
                }
                catch (AggregateException)
                {
                    // TODO: Should only contain the OperationCanceledException exception.
                }
                catch (Exception e)
                {
                    FLogger.Log(e);
                }
            }
            
            foreach (var frame in WorkQueue)
            {
                frame.Dispose();
            }
            
            FCancellationTokenSource.Dispose();
        }
        
        public static Frame EmptyFrame
        {
            get
            {
                return new EmptyInternalFrame();
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
    }
}
