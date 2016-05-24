#region usings
using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using System.IO;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

using VVVV.Core.Logging;
using VVVV.Utils.Win32;
using VVVV.Utils.Imaging;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
#endregion usings

namespace VVVV.Nodes.Capture
{
    public enum CaptureState { Idle, Capturing, Writing }

    struct Frame
    {
        public Image Image;
        public float Delay;
    }

    #region PluginInfo
    [PluginInfo(Name = "Capture",
                Category = "System",
                Help = "Captures a window to an animated GIF",
                Tags = "gif",
                AutoEvaluate = true)]
    #endregion PluginInfo	
    public class CaptureNode : IPluginEvaluate
    {
        #region fields & pins
        [Input("Handle", IsSingle = true)]
        public ISpread<uint> FInput;

        [Input("Frame Count", IsSingle = true, MinValue = -1, DefaultValue = -1)]
        public ISpread<int> FFrameCount;

        [Input("Capture", IsSingle = true)]
        public ISpread<bool> FCapture;

        [Input("Delay in Milliseconds", IsSingle = true, MinValue = 0.01, MaxValue = 1, DefaultValue = 0.02)]
        public ISpread<double> FDelay;

        [Input("Filename", StringType = StringType.Filename, IsSingle = true)]
        public ISpread<string> FFilename;

        [Input("Auto Filename", IsSingle = true)]
        public ISpread<bool> FAutoFilename;

        [Output("Frame Count", IsSingle = true)]
        public ISpread<int> FFrameCountOut;

        [Output("Saving File", IsSingle = true)]
        public ISpread<bool> FWriting;

        [Import()]
        public ILogger FLogger;

        [Import()]
        public IHDEHost FHDEHost;

        double FLastFrameTime;

        CaptureState FCaptureState;
        ScreenCapture FScreenCapture = new ScreenCapture();
        List<Frame> FFrames = new List<Frame>();

        Gif FGIF = new Gif();
        #endregion fields & pins

        //called when data for any output pin is requested
        public async void Evaluate(int SpreadMax)
        {
            switch (FCaptureState)
            {
                case CaptureState.Idle:
                    {
                        if (FCapture[0])
                            FCaptureState = CaptureState.Capturing;
                        break;
                    }
                case CaptureState.Capturing:
                    {
                        if (FFrameCount[0] < 0)
                        {
                            //write file when capture stops
                            if (!FCapture[0])
                                FCaptureState = CaptureState.Writing;
                        }
                        else if (FFrames.Count >= FFrameCount[0])
                            FCaptureState = CaptureState.Writing;
                        break;
                    }
            }

            if (FCaptureState == CaptureState.Capturing)
            {
                try
                {
                    int p = unchecked((int)FInput[0]);
                    IntPtr handle = new IntPtr(p);

                    var img = FScreenCapture.CaptureWindow(handle);
                    var delay = FHDEHost.FrameTime - FLastFrameTime;

                    FFrames.Add(new Frame() { Image = img, Delay = (float)delay });
                }
                catch (Exception e)
                {
                    //in case of exception assume out of memory (even if no OOM exception occurs)
                    //free some memory by removing the last recorded frames for Saving not to fail
                    var freeCount = Math.Min(FFrames.Count, 10);
                    for (int i = FFrames.Count - freeCount; i < FFrames.Count; i++)
                        FFrames[i].Image.Dispose();
                    FFrames = FFrames.SkipLast(freeCount).ToList();

                    //then try saving the captured stuff
                    FCaptureState = CaptureState.Writing;
                }
            }
            else if (FCaptureState == CaptureState.Writing)
            {
                if (!FWriting[0])
                {
                    FWriting[0] = true;
                    await Task.Run(() => SaveFileAsync());
                    FWriting[0] = false;
                    FCaptureState = CaptureState.Idle;
                }
            }

            FLastFrameTime = FHDEHost.FrameTime;
            FFrameCountOut[0] = FFrames.Count;
        }

        async void SaveFileAsync()
        {
            var filename = FFilename[0];
            if (FAutoFilename[0])
            {
                var path = Path.GetDirectoryName(filename);
                filename = Path.GetFileNameWithoutExtension(filename);
                var now = DateTime.Now;
                filename += "_" + now.ToShortDateString() + "-" + now.ToLongTimeString().Replace(":", ".");
                filename = Path.Combine(path, filename + ".gif");
                //FLogger.Log(LogType.Debug, filename);
            }

            foreach (var frame in FFrames)
                FGIF.AddFrame(frame.Image, Math.Round(FDelay[0] * 100)); //gif cannot handle arbitrary framedelays, use userinput here

            FFrames.Clear();

            var fileStream = new FileStream(filename, FileMode.OpenOrCreate);
            FGIF.Save(fileStream);

            fileStream.Dispose();
            FGIF.Clear();
        }
    }
}
