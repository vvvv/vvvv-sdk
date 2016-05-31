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

        [Input("Report Progress", IsSingle = true)]
        public ISpread<bool> FReportProgress;

        [Input("Auto Open", IsSingle = true)]
        public ISpread<bool> FAutoOpen;
        
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

        IntPtr FHandleToCapture;
        string FCurrentFilename;

        Gif FGIF = new Gif();
        Form FProgressBar = new Form();
        Label FProgressLabel = new Label();
        #endregion fields & pins

        public CaptureNode()
        {
            FProgressBar.FormBorderStyle = FormBorderStyle.None;
            FProgressBar.Size = new Size(0, 0);

            FProgressLabel.Padding = new Padding(4);
            FProgressLabel.AutoSize = true;
            FProgressLabel.Parent = FProgressBar;            
        }

        //called when data for any output pin is requested
        public async void Evaluate(int SpreadMax)
        {
            switch (FCaptureState)
            {
                case CaptureState.Idle:
                    {
                        if (FCapture[0])
                        {
                            FCaptureState = CaptureState.Capturing;
                            int p = unchecked((int)FInput[0]);
                            FHandleToCapture = new IntPtr(p);
                        }
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
                    var img = FScreenCapture.CaptureWindow(FHandleToCapture);
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
                    if (FFrames.Count > 0)
                        FCaptureState = CaptureState.Writing;
                    else
                        FCaptureState = CaptureState.Idle;
                }
            }
            else if (FCaptureState == CaptureState.Writing)
            {
                if (!FWriting[0])
                {
                    FCurrentFilename = FFilename[0];
                    if (FAutoFilename[0])
                    {
                        var path = Path.GetDirectoryName(FCurrentFilename);
                        FCurrentFilename = Path.GetFileNameWithoutExtension(FCurrentFilename);
                        var now = DateTime.Now;
                        FCurrentFilename += "_" + now.ToShortDateString() + "-" + now.ToLongTimeString().Replace(":", ".");
                        FCurrentFilename = Path.Combine(path, FCurrentFilename + ".gif");
                        //FLogger.Log(LogType.Debug, filename);
                    }

                    FWriting[0] = true;
                    await Task.Run(() => SaveFileAsync(FCurrentFilename));
                    FWriting[0] = false;
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
                case CaptureState.Idle: FProgressBar.Hide(); break;
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
                        FProgressLabel.Text = "Writing File: " + FCurrentFilename;
                        FProgressLabel.BackColor = Color.LightSalmon;
                        break;
                    }
            }

            FProgressBar.ClientSize = new Size(FProgressLabel.Size.Width, FProgressLabel.Size.Height - 4);
        }

        async void SaveFileAsync(string filename)
        {
            foreach (var frame in FFrames)
                FGIF.AddFrame(frame.Image, Math.Round(FDelay[0] * 100)); //gif cannot handle arbitrary framedelays, use userinput here

            FFrames.Clear();

            var fileStream = new FileStream(filename, FileMode.OpenOrCreate);
            FGIF.Save(fileStream);

            if (FAutoOpen[0])
                Process.Start(filename);

            fileStream.Dispose();
            FGIF.Clear();
        }
    }
}
