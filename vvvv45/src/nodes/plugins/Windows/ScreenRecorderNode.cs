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
using System.Diagnostics;
using System.Windows.Forms;
#endregion usings

namespace VVVV.Nodes.Capture
{
    public enum CaptureState { Idle, Capturing, Writing }

    struct Frame
    {
        public Image Image;
        public float Delay;
    }

    public enum GifDitherType
    {
        None = 0,
        Solid = 1,
        Ordered4x4 = 2,
        Ordered8x8 = 3,
        Ordered16x16 = 4,
        Spiral4x4 = 5,
        Spiral8x8 = 6,
        DualSpiral4x4 = 7,
        DualSpiral8x8 = 8,
        ErrorDiffusion = 9,
    }
    public enum GifPaletteType
    {
        Optimal = 1,
        FixedBW = 2,
        FixedHalftone8 = 3,
        FixedHalftone27 = 4,
        FixedHalftone64 = 5,
        FixedHalftone125 = 6,
        FixedHalftone216 = 7,
        FixedHalftone252 = 8,
        FixedHalftone256 = 9
    }

    #region PluginInfo
    [PluginInfo(Name = "ScreenRecorder",
                Category = "Windows",
                Help = "Captures a window to an animated GIF",
                Tags = "gif, capture",
                AutoEvaluate = true)]
    #endregion PluginInfo	
    public class CaptureNode : IPluginEvaluate, IDisposable
    {
        #region fields & pins
        [Input("Handle", IsSingle = true)]
        public ISpread<uint> FInput;

        [Input("Show Cursor", IsSingle = true, DefaultBoolean = true)]
        public ISpread<bool> FShowCursor;

        [Input("Frame Count", IsSingle = true, MinValue = -1, DefaultValue = -1)]
        public ISpread<int> FFrameCount;

        [Input("Record", IsSingle = true)]
        public IDiffSpread<bool> FRecord;

        [Input("Delay in Seconds", IsSingle = true, MinValue = 0.01, MaxValue = 1, DefaultValue = 0.02)]
        public ISpread<float> FDelay;

        [Input("Palette Type", IsSingle = true, DefaultEnumEntry = "Optimal")]
        public ISpread<GifPaletteType> FPaletteType;

        [Input("Color Count", IsSingle = true, MinValue = 1, MaxValue = 256, DefaultValue = 256)]
        public ISpread<int> FColorCount;

        [Input("Dither Type", IsSingle = true, DefaultEnumEntry = "ErrorDiffusion")]
        public ISpread<GifDitherType> FDitherType;

        [Input("Filename", StringType = StringType.Filename, IsSingle = true)]
        public ISpread<string> FFilename;

        [Input("Auto Filename", IsSingle = true)]
        public ISpread<bool> FAutoFilename;

        [Input("Report Progress", IsSingle = true)]
        public ISpread<bool> FReportProgress;

        [Input("Auto Open", IsSingle = true)]
        public ISpread<bool> FAutoOpen;

        [Output("Recording")]
        public ISpread<bool> FIsRecording;

        [Output("Frame Count")]
        public ISpread<int> FFrameCountOut;

        [Output("Saving File")]
        public ISpread<bool> FIsWriting;

        [Output("Last Saved File", StringType = StringType.Filename)]
        public ISpread<string> FLastFile;

        [Import()]
        public ILogger FLogger;

        [Import()]
        public IHDEHost FHDEHost;

        double FLastFrameTime;

        CaptureState FCaptureState;
        ScreenCapture FScreenCapture = new ScreenCapture();
        List<Frame> FFrames = new List<Frame>();

        IntPtr FHandleToCapture;
        int FFramesToCapture;
        float FDelayTimeToCapture;
        string FCurrentFilename;

        Gif FGIF = new Gif();
        Form FProgressBar = new Form();
        Label FProgressLabel = new Label();
        double FCurrentTime;
        #endregion fields & pins

        public CaptureNode()
        {
            FProgressBar.FormBorderStyle = FormBorderStyle.None;
            FProgressBar.Size = new Size(0, 0);
            FProgressBar.TopMost = true;

            FProgressLabel.Padding = new Padding(4);
            FProgressLabel.AutoSize = true;
            FProgressLabel.Parent = FProgressBar;            
        }

        public void Dispose()
        {
            FProgressLabel.Dispose();
            FProgressBar.Dispose();
        }

        //called when data for any output pin is requested
        public async void Evaluate(int SpreadMax)
        {
            switch (FCaptureState)
            {
                case CaptureState.Idle:
                    {
                        if (FRecord[0])
                            if (FFrameCount[0] < 0 || (FFrameCount[0] > 0 && FRecord.IsChanged))
                            {
                                int p = unchecked((int)FInput[0]);
                                FHandleToCapture = new IntPtr(p);
                                
                                if (FHandleToCapture != IntPtr.Zero)
                                {
                                    FCaptureState = CaptureState.Capturing;
                                    FFramesToCapture = FFrameCount[0];
                                    FDelayTimeToCapture = FDelay[0];
                                    FIsRecording[0] = true;
                                    FCurrentTime = FHDEHost.FrameTime;

                                    //initialize TimeProvider
                                    FHDEHost.SetFrameTimeProvider(_ => FCurrentTime);
                                }                                
                            }
                        break;
                    }
                case CaptureState.Capturing:
                    {
                        if (FFramesToCapture < 0)
                        {
                            //write file when capture stops
                            if (!FRecord[0])
                                FCaptureState = CaptureState.Writing;
                        }
                        else if (FFrames.Count >= FFramesToCapture)
                            FCaptureState = CaptureState.Writing;
                        break;
                    }
            }

            if (FCaptureState == CaptureState.Capturing)
            {
                try
                {
                    var img = FScreenCapture.CaptureWindow(FHandleToCapture, true, null, FShowCursor[0]);
                    var delay = FHDEHost.FrameTime - FLastFrameTime;

                    FFrames.Add(new Frame() { Image = img, Delay = (float)delay });

                    //advance frame time (will be taken into account for next frame captured)
                    FCurrentTime += FDelayTimeToCapture;
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
                    if (FFrames.Count > 0)
                        FCaptureState = CaptureState.Writing;
                    else
                        FCaptureState = CaptureState.Idle;
                }
            }
            else if (FCaptureState == CaptureState.Writing)
            {
                if (!FIsWriting[0])
                {
                    //reset TimeProvider
                    FHDEHost.SetFrameTimeProvider((ITimeProvider)null);

                    //compute filename
                    FCurrentFilename = FFilename[0];
                    if (FAutoFilename[0])
                    {
                        FCurrentFilename += Path.HasExtension(FCurrentFilename) ? "" : "\\"; //add trailing slash if this is supposed to be a path
                        var path = Path.GetDirectoryName(FCurrentFilename);
                        FCurrentFilename = Path.GetFileNameWithoutExtension(FCurrentFilename);
                        var now = DateTime.Now;
                        FCurrentFilename += "_" + now.ToString("dd.MM.yyyy-HH.mm.ss");
                        FCurrentFilename = Path.Combine(path, FCurrentFilename + ".gif");
                        //FLogger.Log(LogType.Debug, filename);
                    }

                    FIsRecording[0] = false;
                    FIsWriting[0] = true;

                    //write the file
                    await Task.Run(() => SaveGIFAsync(FCurrentFilename));
                    FLastFile[0] = FCurrentFilename;
                    FIsWriting[0] = false;

                    FCaptureState = CaptureState.Idle;
                    FProgressBar.Hide();
                }
            }

            if (FReportProgress[0])
                ReportProgress();

            FLastFrameTime = FHDEHost.FrameTime;
            FFrameCountOut[0] = FFrames.Count;
        }

        void ReportProgress()
        {
            switch (FCaptureState)
            {
                case CaptureState.Idle: FProgressBar.Hide(); return;
                case CaptureState.Capturing:
                    {
                        if (!FProgressBar.Visible)
                        {
                            RECT windowRect;
                            User32.GetWindowRect(FHandleToCapture, out windowRect);
                            FProgressLabel.BackColor = Color.LightGreen;

                            FProgressBar.Show();
                            FProgressBar.SetDesktopLocation(windowRect.Left, windowRect.Bottom + 10);
                        }

                        FProgressLabel.Text = "Frame Count: " + FFrames.Count.ToString();
                        break;
                    }
                case CaptureState.Writing:
                    {
                        FProgressLabel.Text = FFrames.Count.ToString() + " frames. Writing File: " + FCurrentFilename;
                        FProgressLabel.BackColor = Color.LightSalmon;
                        break;
                    }
            }

            FProgressBar.ClientSize = new Size(FProgressLabel.Size.Width, FProgressLabel.Size.Height - 4);
        }

        async void SaveGIFAsync(string filename)
        {
            foreach (var frame in FFrames)
                FGIF.AddFrame(frame.Image, Math.Round(FDelayTimeToCapture * 100)); //gif cannot handle arbitrary framedelays, use userinput here

            try
            {
                var fileStream = new FileStream(filename, FileMode.OpenOrCreate);
                FGIF.Save(fileStream, (PaletteType)FPaletteType[0], (DitherType)FDitherType[0], FColorCount[0]);
                fileStream.Dispose();

                if (FAutoOpen[0])
                    Process.Start(filename);
            }
            catch
            {
                //silently fail
            }
            finally
            {
                FFrames.Clear();
                FGIF.Clear();
            }
        }
    }
}
