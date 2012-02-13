using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading;
using VVVV.Core.Logging;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V2.EX9;
using EX9 = SlimDX.Direct3D9;
using Frame = VVVV.PluginInterfaces.V2.EX9.DXResource<SlimDX.Direct3D9.Texture, VVVV.Nodes.ImagePlayer.FrameInfo>;

namespace VVVV.Nodes.ImagePlayer
{
    [PluginInfo(Name = "Player", Category = "EX9.Texture")]
    public class PlayerNode : IPluginEvaluate, IDisposable
    {
        [Input("Directory", StringType = StringType.Directory)]
        public ISpread<string> FDirectoryIn;
        
        [Input("Filemask", DefaultString = ImagePlayer.DEFAULT_FILEMASK)]
        public ISpread<string> FFilemaskIn;
        
        [Input("Buffer Size IO", DefaultValue = ImagePlayer.DEFAULT_BUFFER_SIZE, Visibility = PinVisibility.Hidden)]
        public ISpread<int> FBufferSizeIn;
        
        [Input("Visible Frames")]
        public ISpread<ISpread<int>> FVisibleFramesIn;
        
        [Input("Preload Frames")]
        public ISpread<ISpread<int>> FPreloadFramesIn;
        
        [Input("Reload")]
        public ISpread<bool> FReloadIn;
        
        [Config("Threads IO", DefaultValue = 1.0, MinValue = -1.0)]
        public ISpread<int> FThreadsIOConfig;
        
        [Config("Threads Texture", DefaultValue = 2.0, MinValue = -1.0)]
        public ISpread<int> FThreadsTextureConfig;
        
        [Output("Texture")]
        public ISpread<ISpread<Frame>> FTextureOut;
        
        [Output("Duration IO", Visibility = PinVisibility.OnlyInspector)]
        public ISpread<double> FDurationIOOut;
        
        [Output("Duration Texture", Visibility = PinVisibility.OnlyInspector)]
        public ISpread<double> FDurationTextureOut;
        
        [Output("Unused Frames Count", Visibility = PinVisibility.OnlyInspector)]
        public ISpread<int> FUnusedFramesOut;
        
        private readonly ISpread<ImagePlayer> FImagePlayers = new Spread<ImagePlayer>(0);
        private readonly ILogger FLogger;
        private readonly IDXDeviceService FDeviceService;
        
        [ImportingConstructor]
        public PlayerNode(IPluginHost pluginHost, ILogger logger, IDXDeviceService deviceService)
        {
            FLogger = logger;
            FDeviceService = deviceService;
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
            
            int previosSliceCount = FImagePlayers.SliceCount;
            
            // Dispose unused image players
            for (int i = spreadMax; i < FImagePlayers.SliceCount ; i++)
            {
                FImagePlayers[i].Dispose();
            }
            
            FImagePlayers.SliceCount = spreadMax;
            FTextureOut.SliceCount = spreadMax;
            FDurationIOOut.SliceCount = spreadMax;
            FDurationTextureOut.SliceCount = spreadMax;
            FUnusedFramesOut.SliceCount = spreadMax;
            
            // Create new image players
            for (int i = previosSliceCount; i < spreadMax; i++)
            {
                FImagePlayers[i] = new ImagePlayer(FThreadsIOConfig[i], FThreadsTextureConfig[i], FLogger, FDeviceService);
            }
            
            for (int i = 0; i < spreadMax; i++)
            {
                var imagePlayer = FImagePlayers[i];
                
                if (imagePlayer != null && (imagePlayer.ThreadsIO != FThreadsIOConfig[i] || imagePlayer.ThreadsTexture != FThreadsTextureConfig[i]))
                {
                    imagePlayer.Dispose();
                    imagePlayer = null;
                }
                
                if (imagePlayer == null)
                {
                    imagePlayer = new ImagePlayer(FThreadsIOConfig[i], FThreadsTextureConfig[i], FLogger, FDeviceService);
                    FImagePlayers[i] = imagePlayer;
                }
                
                imagePlayer.Directory = FDirectoryIn[i];
                imagePlayer.Filemask = FFilemaskIn[i];
                
                if (FReloadIn[i])
                {
                    imagePlayer.Reload();
                }
                
                double durationIO = 0.0;
                double durationTexture = 0.0;
                int unusedFrames = 0;
                FTextureOut[i] = imagePlayer.Preload(
                    FVisibleFramesIn[i],
                    FPreloadFramesIn[i],
                    FBufferSizeIn[i],
                    out durationIO,
                    out durationTexture,
                    out unusedFrames);
                
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
        }
    }
}
