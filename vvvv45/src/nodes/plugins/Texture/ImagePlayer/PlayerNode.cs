using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading;
using SlimDX.Direct3D9;
using VVVV.Core.Logging;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V2.EX9;

namespace VVVV.Nodes.ImagePlayer
{
    [PluginInfo(Name = "Player", Category = "EX9.Texture")]
    public class PlayerNode : IPluginEvaluate, IDisposable, IPluginDXTexture2
    {
        [Input("Directory", StringType = StringType.Directory)]
        public ISpread<string> FDirectoryIn;
        
        [Input("Filemask", DefaultString = ImagePlayer.DEFAULT_FILEMASK)]
        public ISpread<string> FFilemaskIn;
        
        [Input("FPS", DefaultValue = 25.0)]
        public ISpread<double> FFpsIn;
        
        [Input("Preload Count", MinValue = 1)]
        public ISpread<int> FPreloadCountIn;
        
        [Input("Reload")]
        public ISpread<bool> FReloadIn;
        
        [Input("Time")]
        public ISpread<double> FTimeIn;
        
        [Output("Unused Frames")]
        public ISpread<int> FUnusedFramesOut;
        
        [Output("Duration IO")]
        public ISpread<double> FDurationIOOut;
        
        [Output("Duration Texture")]
        public ISpread<double> FDurationTextureOut;
        
        public IDXTextureOut FTextureOut;
        
        private readonly ISpread<ImagePlayer> FImagePlayers = new Spread<ImagePlayer>(0);
        private readonly List<Device> FDevices = new List<Device>();
        private readonly ILogger FLogger;
        
        [ImportingConstructor]
        public PlayerNode(IPluginHost pluginHost, ILogger logger)
        {
            FLogger = logger;
            
            pluginHost.CreateTextureOutput("Texture", TSliceMode.Dynamic, TPinVisibility.True, out FTextureOut);
            FTextureOut.Order = -1;
            
            SlimDX.Configuration.EnableObjectTracking = true;
        }
        
        public void Evaluate(int spreadMax)
        {
            int previosSliceCount = FImagePlayers.SliceCount;
            
            // Dispose unused image players
            for (int i = spreadMax; i < FImagePlayers.SliceCount ; i++)
            {
                FImagePlayers[i].Dispose();
            }
            
            FImagePlayers.SliceCount = spreadMax;
            FTextureOut.SliceCount = spreadMax;
            FUnusedFramesOut.SliceCount = spreadMax;
            FDurationIOOut.SliceCount = spreadMax;
            FDurationTextureOut.SliceCount = spreadMax;
            
            // Create new image players
            for (int i = previosSliceCount; i < spreadMax; i++)
            {
                FImagePlayers[i] = new ImagePlayer(FPreloadCountIn[i], FDevices, FLogger);
            }
            
            for (int i = 0; i < spreadMax; i++)
            {
                var imagePlayer = FImagePlayers[i];
                
                if (imagePlayer != null && imagePlayer.PreloadCount != FPreloadCountIn[i])
                {
                    imagePlayer.Dispose();
                    imagePlayer = null;
                }
                
                if (imagePlayer == null)
                {
                    imagePlayer = new ImagePlayer(FPreloadCountIn[i], FDevices, FLogger);
                    FImagePlayers[i] = imagePlayer;
                }
                
                imagePlayer.Directory = FDirectoryIn[i];
                imagePlayer.Filemask = FFilemaskIn[i];
                imagePlayer.FPS = FFpsIn[i];
                
                if (FReloadIn[i])
                {
                    imagePlayer.Reload();
                }
                imagePlayer.Seek(FTimeIn[i]);
                
                FUnusedFramesOut[i] = imagePlayer.UnusedFrames;
                FDurationIOOut[i] = imagePlayer.CurrentFrame.Info.DurationIO;
                FDurationTextureOut[i] = imagePlayer.CurrentFrame.Info.DurationTexture;
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
        
        Texture IPluginDXTexture2.GetTexture(IDXTextureOut pin, Device device, int slice)
        {
            return FImagePlayers[slice].CurrentFrame.GetTexture(device);
        }
        
        void IPluginDXResource.UpdateResource(IPluginOut pin, Device device)
        {
            lock (FDevices)
            {
                if (!FDevices.Contains(device))
                {
                    FDevices.Add(device);
                }
            }
        }
        
        void IPluginDXResource.DestroyResource(IPluginOut pin, Device device, bool onlyUnManaged)
        {
            foreach (var imagePlayer in FImagePlayers)
            {
                if (imagePlayer != null)
                {
                    imagePlayer.Dispose();
                }
            }
            FImagePlayers.SliceCount = 0;
            
            lock (FDevices)
            {
                FDevices.Remove(device);
            }
            
            FLogger.Log(LogType.Debug, SlimDX.ObjectTable.ReportLeaks());
        }
    }
}
