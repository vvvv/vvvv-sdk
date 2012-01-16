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
        
        [Input("Queue Size", DefaultValue = 4.0)]
        public ISpread<int> FQueueSizeIn;
        
        [Input("Visible Frames")]
        public ISpread<ISpread<int>> FVisibleFramesIn;
        
        [Input("Preload Frames")]
        public ISpread<ISpread<int>> FPreloadFramesIn;
        
        [Input("Reload")]
        public ISpread<bool> FReloadIn;
        
        //        [Output("Unused Frames")]
        //        public ISpread<int> FUnusedFramesOut;
//
        
        [Output("Texture")]
        public ISpread<ISpread<Frame>> FTextureOut;
        
        [Output("Duration IO")]
        public ISpread<ISpread<double>> FDurationIOOut;
        
        [Output("Duration Texture")]
        public ISpread<ISpread<double>> FDurationTextureOut;
        
        [Output("Scheduled Frames")]
        public ISpread<int> FScheduledFramesOut;
        
        [Output("Canceled Frames")]
        public ISpread<int> FCanceledFramesOut;
        
        private readonly ISpread<ImagePlayer> FImagePlayers = new Spread<ImagePlayer>(0);
        private readonly ILogger FLogger;
        
        [ImportingConstructor]
        public PlayerNode(IPluginHost pluginHost, ILogger logger)
        {
            FLogger = logger;
        }
        
        public void Evaluate(int spreadMax)
        {
            // SpreadMax is not correct (should be correct in streams branch).
            spreadMax = FDirectoryIn
                .CombineWith(FFilemaskIn)
                .CombineWith(FQueueSizeIn)
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
            //            FUnusedFramesOut.SliceCount = spreadMax;
            FDurationIOOut.SliceCount = spreadMax;
            FDurationTextureOut.SliceCount = spreadMax;
            FScheduledFramesOut.SliceCount = spreadMax;
            FCanceledFramesOut.SliceCount = spreadMax;
            
            // Create new image players
            for (int i = previosSliceCount; i < spreadMax; i++)
            {
                FImagePlayers[i] = new ImagePlayer(FQueueSizeIn[i], FLogger);
            }
            
            for (int i = 0; i < spreadMax; i++)
            {
                var imagePlayer = FImagePlayers[i];
                
                FQueueSizeIn[i] = Math.Max(1, FQueueSizeIn[i]);
                if (imagePlayer != null && imagePlayer.QueueSize != FQueueSizeIn[i])
                {
                    imagePlayer.Dispose();
                    imagePlayer = null;
                }
                
                if (imagePlayer == null)
                {
                    imagePlayer = new ImagePlayer(FQueueSizeIn[i], FLogger);
                    FImagePlayers[i] = imagePlayer;
                }
                
                imagePlayer.Directory = FDirectoryIn[i];
                imagePlayer.Filemask = FFilemaskIn[i];
                
                if (FReloadIn[i])
                {
                    imagePlayer.Reload();
                }
                
                var durationIO = FDurationIOOut[i];
                var durationTexture = FDurationTextureOut[i];
                int scheduledFrames = 0;
                int canceledFrames = 0;
                FTextureOut[i] = imagePlayer.Preload(
                    FVisibleFramesIn[i],
                    FPreloadFramesIn[i],
                    ref durationIO,
                    ref durationTexture,
                    out scheduledFrames,
                    out canceledFrames);
                
                FScheduledFramesOut[i] = scheduledFrames;
                FCanceledFramesOut[i] = canceledFrames;
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
