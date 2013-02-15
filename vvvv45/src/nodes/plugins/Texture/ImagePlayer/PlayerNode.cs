using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks.Schedulers;
using VVVV.Core.Logging;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V2.EX9;
using EX9 = SlimDX.Direct3D9;
using Frame = VVVV.PluginInterfaces.V2.EX9.TextureResource<VVVV.Nodes.ImagePlayer.FrameInfo>;

namespace VVVV.Nodes.ImagePlayer
{
    [PluginInfo(Name = "Player", Category = "EX9.Texture")]
    public class PlayerNode : IPluginEvaluate, IDisposable
    {
        [Input("Directory", StringType = StringType.Directory)]
        public IDiffSpread<ISpread<string>> FDirectoryIn;
        
        [Input("Filemask", DefaultString = ImagePlayer.DEFAULT_FILEMASK)]
        public IDiffSpread<ISpread<string>> FFilemaskIn;

        [Input("Reload", IsBang = true)]
        public ISpread<bool> FReloadIn;

        [Input("IO Buffer Size", DefaultValue = ImagePlayer.DEFAULT_BUFFER_SIZE, Visibility = PinVisibility.OnlyInspector)]
        public ISpread<int> FBufferSizeIn;

        [Input("Preload Frames")]
        public ISpread<ISpread<int>> FPreloadFramesIn;

        [Input("Visible Frame Indices")]
        public ISpread<ISpread<int>> FVisibleFramesIn;
        
        [Config("Threads IO", DefaultValue = 1.0, MinValue = -1.0)]
        public ISpread<int> FThreadsIOConfig;
        
        [Config("Threads Texture", DefaultValue = 2.0, MinValue = -1.0)]
        public ISpread<int> FThreadsTextureConfig;
        
        [Output("Texture")]
        ISpread<ISpread<Frame>> FTextureOut;

        [Output("Width")]
        ISpread<ISpread<int>> FTextureWidthOut;

        [Output("Height")]
        ISpread<ISpread<int>> FTextureHeightOut;
        
        [Output("Frame Count")]
        public ISpread<int> FFrameCountOut;
        
        [Output("Duration IO", Visibility = PinVisibility.Hidden)]
        public ISpread<double> FDurationIOOut;
        
        [Output("Duration Texture", Visibility = PinVisibility.Hidden)]
        public ISpread<double> FDurationTextureOut;
        
        [Output("Unused Frames Count", Visibility = PinVisibility.Hidden)]
        public ISpread<int> FUnusedFramesOut;
        
        [Output("Loaded")]
        public ISpread<ISpread<bool>> FLoadedOut;
        
        private readonly ISpread<ImagePlayer> FImagePlayers = new Spread<ImagePlayer>(0);
        private readonly ILogger FLogger;
        private readonly IOTaskScheduler FIOTaskScheduler = new IOTaskScheduler();
        private readonly MemoryPool FMemoryPool = new MemoryPool();
        private readonly IDXDeviceService FDeviceService;
        
        [ImportingConstructor]
        public PlayerNode(IPluginHost pluginHost, ILogger logger, IDXDeviceService deviceService)
        {
            FLogger = logger;
            FDeviceService = deviceService;
        }
        
        private ImagePlayer CreateImagePlayer(int index)
        {
        	return new ImagePlayer(
        		FThreadsIOConfig[index], 
        		FThreadsTextureConfig[index], 
        		FLogger, 
        		FIOTaskScheduler, 
        		FMemoryPool,
                FDeviceService
        	);
        }
        
        private void DestroyImagePlayer(ImagePlayer imagePlayer)
        {
        	imagePlayer.Dispose();
        }
        
        public void Evaluate(int spreadMax)
        {
            // SpreadMax is not correct (should be correct in streams branch).
            spreadMax = FDirectoryIn
                .CombineWith(FFilemaskIn)
                .CombineWith(FBufferSizeIn)
                .CombineWith(FVisibleFramesIn)
                .CombineWith(FPreloadFramesIn)
                .CombineWith(FReloadIn);
            
            FImagePlayers.Resize(spreadMax, CreateImagePlayer, DestroyImagePlayer);
            
            FImagePlayers.SliceCount = spreadMax;
            FTextureOut.SliceCount = spreadMax;
            FTextureWidthOut.SliceCount = spreadMax;
            FTextureHeightOut.SliceCount = spreadMax;
            FFrameCountOut.SliceCount = spreadMax;
            FDurationIOOut.SliceCount = spreadMax;
            FDurationTextureOut.SliceCount = spreadMax;
            FUnusedFramesOut.SliceCount = spreadMax;
            FLoadedOut.SliceCount = spreadMax;
            
            for (int i = 0; i < spreadMax; i++)
            {
                bool reload = FReloadIn[i] || FDirectoryIn.IsChanged || FFilemaskIn.IsChanged;
                
                var imagePlayer = FImagePlayers[i];
                
                if (imagePlayer.ThreadsIO != FThreadsIOConfig[i] || imagePlayer.ThreadsTexture != FThreadsTextureConfig[i])
                {
                	DestroyImagePlayer(imagePlayer);
                	imagePlayer = CreateImagePlayer(i);
                	reload = true;
                	FImagePlayers[i] = imagePlayer;
                }
                
                imagePlayer.Directories = FDirectoryIn[i];
                imagePlayer.Filemasks = FFilemaskIn[i];
                
                if (reload)
                {
                    imagePlayer.Reload();
                    FMemoryPool.Clear();
                }

                int frameCount = 0;
                double durationIO = 0.0;
                double durationTexture = 0.0;
                int unusedFrames = 0;
                var loadedFrames = FLoadedOut[i];
                
                var frame = imagePlayer.Preload(
                    FVisibleFramesIn[i],
                    FPreloadFramesIn[i],
                    FBufferSizeIn[i],
                    out frameCount,
                    out durationIO,
                    out durationTexture,
                    out unusedFrames,
                    ref loadedFrames);
                
                FTextureOut[i] = frame;
                FTextureWidthOut[i] = frame.Select(f => f.Metadata.Width).ToSpread();
                FTextureHeightOut[i] = frame.Select(f => f.Metadata.Height).ToSpread();
                FFrameCountOut[i] = frameCount;
                FDurationIOOut[i] = durationIO;
                FDurationTextureOut[i] = durationTexture;
                FUnusedFramesOut[i] = unusedFrames;
            }
        }
        
        public void Dispose()
        {
            foreach (var imagePlayer in FImagePlayers)
            {
                if (imagePlayer != null)
                {
                    imagePlayer.Dispose();
                }
            }
            FImagePlayers.SliceCount = 0;
            FIOTaskScheduler.Dispose();
            FMemoryPool.Dispose();
        }
    }
}
