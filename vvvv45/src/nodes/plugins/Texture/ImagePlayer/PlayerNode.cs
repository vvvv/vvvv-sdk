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
        
        [Input("Preload Count", MinValue = 0)]
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
        private readonly Dictionary<int, Device> FDevices = new Dictionary<int, Device>();
        private readonly ILogger FLogger;
        
        [ImportingConstructor]
        public PlayerNode(IPluginHost pluginHost, ILogger logger)
        {
            FLogger = logger;
            
            pluginHost.CreateTextureOutput("Texture", TSliceMode.Dynamic, TPinVisibility.True, out FTextureOut);
            FTextureOut.Order = -1;
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
                FImagePlayers[i] = new ImagePlayer(FDevices.Values, FLogger);
            }
            
            for (int i = 0; i < spreadMax; i++)
            {
                var imagePlayer = FImagePlayers[i];
                imagePlayer.Directory = FDirectoryIn[i];
                imagePlayer.Filemask = FFilemaskIn[i];
                imagePlayer.FPS = FFpsIn[i];
                imagePlayer.PreloadCount = FPreloadCountIn[i];
                
                if (FReloadIn[i])
                {
                    imagePlayer.Reload();
                }
                imagePlayer.Seek(FTimeIn[i]);
                
                FUnusedFramesOut[i] = imagePlayer.UnusedFrames;
                FDurationIOOut[i] = imagePlayer.CurrentFrame.DurationIO;
                FDurationTextureOut[i] = imagePlayer.CurrentFrame.DurationTexture;
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
        }
        
        void IPluginDXTexture2.GetTexture(IDXTextureOut ForPin, int OnDevice, int Slice, out int texAddr)
        {
            var intPtr = new IntPtr(OnDevice);
            var device = Device.FromPointer(intPtr);
            var texture = FImagePlayers[Slice].CurrentFrame.GetTexture(device);
            if (texture != null)
            {
                texAddr = texture.ComPointer.ToInt32();
            }
            else
            {
                texAddr = IntPtr.Zero.ToInt32();
            }
        }
        
        void IPluginDXResource.UpdateResource(IPluginOut ForPin, int OnDevice)
        {
            var intPtr = new IntPtr(OnDevice);
            FDevices[OnDevice] = Device.FromPointer(intPtr);
        }
        
        void IPluginDXResource.DestroyResource(IPluginOut ForPin, int OnDevice, bool OnlyUnManaged)
        {
            FDevices.Remove(OnDevice);
        }
    }
}
